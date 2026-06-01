using ProjectFR.Data.Nodes;
using ProjectFR.Mission;
using System.Linq;

namespace ProjectFR.Infiltration;

/// <summary>
/// 1차 침투 런타임 관리자 초안.
/// 기존 BattleManager를 바로 제거하지 않고, 새 침투 구조를 옆에 세우기 위한 스캐폴딩이다.
/// </summary>
public sealed class InfiltrationManager
{
    public InfiltrationState State { get; } = new();
    public List<SecurityAgent> SecurityAgents { get; } = new();
    public MissionData Mission { get; }
    private readonly SecurityBehaviorExecutor _securityBehaviorExecutor = new();

    public InfiltrationManager(MissionData mission)
    {
        Mission = mission;
    }

    public void Initialize(string startFolderPath, IEnumerable<NodeData> knownNodes)
    {
        State.CurrentFolderPath = startFolderPath;
        State.CursorAgent.CurrentNodePath = startFolderPath;
        State.KnownNodePaths.Clear();
        State.Windows.Clear();
        State.Clipboard.Clear();
        State.PouchCache.Clear();
        State.ExposedPouchPaths.Clear();
        State.PermissionOverrideTurns.Clear();
        State.TrackedPathTurns.Clear();
        State.ForcedLockTurns.Clear();
        State.ScanPressureTurns.Clear();
        State.ActiveOperations.Clear();
        State.CommandQueue.Clear();
        State.RunStatus = RunStatus.Active;
        State.ObjectiveState = ObjectiveState.Revealed;
        State.OperatorHp = State.OperatorMaxHp;
        State.LastTurnContactDamage = 0;
        foreach (var node in knownNodes)
        {
            State.KnownNodePaths.Add(node.Path);
        }

        State.Windows.Add(new ExplorerWindowState
        {
            WindowType = ExplorerWindowType.Main,
            Title = "Main Infiltration Window",
            BoundPath = startFolderPath,
            IsOpen = true,
            IsFocused = true,
            SlotIndex = 0
        });

        State.AddLog($"Infiltration started: {Mission.Title}");
    }

    public void AddSecurityAgent(SecurityAgent agent)
    {
        SecurityAgents.Add(agent);
    }

    public ExplorerWindowState OpenWindow(ExplorerWindowType windowType, string title, string boundPath, int traceModifier = 0)
    {
        var existing = State.Windows.FirstOrDefault(window => window.WindowType == windowType && string.Equals(window.BoundPath, boundPath, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.IsOpen = true;
            FocusWindow(existing.WindowId);
            return existing;
        }

        foreach (var window in State.Windows)
        {
            window.IsFocused = false;
        }

        var state = new ExplorerWindowState
        {
            WindowType = windowType,
            Title = title,
            BoundPath = boundPath,
            IsOpen = true,
            IsFocused = true,
            SlotIndex = State.Windows.Count,
            TraceModifier = traceModifier
        };
        State.Windows.Add(state);
        if (traceModifier > 0)
        {
            AddTrace(traceModifier, $"Opened {title}");
        }
        State.AddLog($"Window opened: {title} @ {boundPath}");
        return state;
    }

    public void FocusWindow(string windowId)
    {
        foreach (var window in State.Windows)
        {
            window.IsFocused = string.Equals(window.WindowId, windowId, StringComparison.OrdinalIgnoreCase);
        }
    }

    public void CloseWindow(string windowId)
    {
        var window = State.Windows.FirstOrDefault(item => string.Equals(item.WindowId, windowId, StringComparison.OrdinalIgnoreCase));
        if (window == null || window.WindowType == ExplorerWindowType.Main)
            return;

        window.IsOpen = false;
        window.IsFocused = false;
        var mainWindow = State.Windows.FirstOrDefault(item => item.WindowType == ExplorerWindowType.Main);
        if (mainWindow != null)
        {
            mainWindow.IsFocused = true;
        }
        State.AddLog($"Window closed: {window.Title}");
    }

    public ExplorerWindowState OpenLogViewerWindow()
    {
        return OpenWindow(ExplorerWindowType.LogViewer, "Event Log", "system://event-log", traceModifier: 0);
    }

    public bool CloseLogViewerWindow()
    {
        var window = State.Windows.FirstOrDefault(w => w.WindowType == ExplorerWindowType.LogViewer && w.IsOpen);
        if (window == null) return false;
        CloseWindow(window.WindowId);
        return true;
    }

    public void AdvanceTurn()
    {
        State.TurnCount++;
        State.CursorAgent.RestoreActionPoints();
        TickOperations();
        ApplyMultiWindowParallelOperationTrace();
        TickPermissionOverrides();
        TickTurnDictionary(State.TrackedPathTurns, "Tracked path expired");
        TickTurnDictionary(State.ForcedLockTurns, "Forced lock expired");
        TickTurnDictionary(State.ScanPressureTurns, "Scan pressure expired");
        var wasDetected = State.CursorAgent.IsDetected;
        AdvanceSecurityAgents();
        if (!wasDetected && State.CursorAgent.IsDetected)
        {
            InterruptMonitoredOperationsOnDetection();
        }
        ApplyDetectionContactDamage();
        State.AddLog($"Turn advanced to {State.TurnCount}");
    }

    public void QueueCommand(CommandQueueEntry entry)
    {
        if (!TryValidateQueueEntry(entry, out var validationError))
        {
            State.AddLog($"Queue rejected: {validationError}");
            return;
        }

        entry.Order = State.CommandQueue.Count + 1;
        State.CommandQueue.Add(entry);
        State.AddLog($"Queued: {entry.Summary}");
    }

    public void ClearQueue()
    {
        State.CommandQueue.Clear();
        State.AddLog("Command queue cleared");
    }

    public void ExecuteQueuedCommands()
    {
        var startedCount = 0;
        var skippedCount = 0;

        foreach (var entry in State.CommandQueue.OrderBy(x => x.Order).ToList())
        {
            if (!TryValidateQueueEntry(entry, out var validationError))
            {
                skippedCount++;
                State.AddLog($"Queue skipped: {validationError}");
                continue;
            }

            var operation = CreateOperationFromQueueEntry(entry);
            StartOperation(operation);
            startedCount++;
        }

        State.CommandQueue.Clear();
        State.AddLog($"Command queue executed ({startedCount} started, {skippedCount} skipped)");
    }

    public void StartOperation(FileOperation operation)
    {
        operation.Start();
        State.ActiveOperations.Add(operation);
        State.AddLog($"Operation started: {operation.Type} @ {operation.TargetNodePath}");

        ApplyTrackedPathActionTrace(operation.TargetNodePath, operation.Type);

        if (GetMonitoringAgents(operation.TargetNodePath).Count > 0)
        {
            AddTrace(InfiltrationTuning.MonitoredOperationTraceIncrease, $"Monitored operation: {operation.Type} @ {operation.TargetNodePath}");
        }
    }

    private void ApplyMultiWindowParallelOperationTrace()
    {
        var runningPaths = State.ActiveOperations
            .Where(op => op.Status == OperationStatus.Running)
            .Select(op => op.TargetNodePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (runningPaths.Count == 0)
            return;

        var windowsWithOperation = State.Windows
            .Count(w => w.IsOpen && runningPaths.Any(path =>
                path.StartsWith(w.BoundPath.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)));

        var extraWindows = windowsWithOperation - 1;
        if (extraWindows <= 0)
            return;

        AddTrace(
            extraWindows * InfiltrationTuning.MultiWindowParallelOperationTraceCostPerWindow,
            $"Parallel operations across {windowsWithOperation} windows");
    }

    private void TickOperations()
    {
        foreach (var operation in State.ActiveOperations.Where(op => op.Status == OperationStatus.Running).ToList())
        {
            operation.Tick();
            if (operation.Status == OperationStatus.Completed)
            {
                OnOperationCompleted(operation);
                State.AddLog($"Operation completed: {operation.Type} @ {operation.TargetNodePath}");
            }
        }
    }

    public void AddTrace(int amount, string reason)
    {
        State.Trace = Math.Min(State.MaxTrace, State.Trace + amount);
        State.AddLog($"Trace +{amount}: {reason}");
        UpdateAlertStage();
    }

    public void ReduceTrace(int amount, string reason)
    {
        State.Trace = Math.Max(0, State.Trace - amount);
        State.AddLog($"Trace -{amount}: {reason}");
        UpdateAlertStage();
    }

    public bool TryCopyToClipboard(string nodePath, ExplorerNodeKind nodeKind, long size = 0)
    {
        if (State.Clipboard.Count >= State.CursorAgent.ClipboardCapacity)
        {
            State.AddLog("Clipboard full");
            return false;
        }

        State.Clipboard.Add(new ClipboardEntry
        {
            NodePath = nodePath,
            NodeKind = nodeKind,
            Size = size
        });
        State.AddLog($"Clipboard add: {nodePath}");
        return true;
    }

    public bool TryMoveClipboardToPouch(string nodePath, long size)
    {
        if (State.PouchCache.Count >= State.CursorAgent.PouchCapacity)
        {
            State.AddLog("Pouch cache full");
            return false;
        }

        if (size > State.CursorAgent.PouchMaxFileSize)
        {
            State.AddLog($"Pouch rejected oversized file: {nodePath} ({size})");
            return false;
        }

        var clipboardEntry = State.Clipboard.FirstOrDefault(entry => string.Equals(entry.NodePath, nodePath, StringComparison.OrdinalIgnoreCase));
        if (clipboardEntry == null)
        {
            State.AddLog($"Clipboard entry missing: {nodePath}");
            return false;
        }

        clipboardEntry.Size = size;
        clipboardEntry.IsGhosted = true;
        State.ExposedPouchPaths.Remove(nodePath);
        State.Clipboard.Remove(clipboardEntry);
        State.PouchCache.Add(clipboardEntry);
        ReduceTrace(InfiltrationTuning.PouchHideTraceReduction, $"Cheek pouch hid small file: {nodePath}");
        State.AddLog($"Pouch cache add: {nodePath}");
        return true;
    }

    public bool TryRestoreFromPouch(string nodePath)
    {
        if (State.Clipboard.Count >= State.CursorAgent.ClipboardCapacity)
        {
            State.AddLog("Clipboard full");
            return false;
        }

        var pouchEntry = State.PouchCache.FirstOrDefault(entry => string.Equals(entry.NodePath, nodePath, StringComparison.OrdinalIgnoreCase));
        if (pouchEntry == null)
        {
            State.AddLog($"Pouch cache entry missing: {nodePath}");
            return false;
        }

        pouchEntry.IsGhosted = false;
        State.ExposedPouchPaths.Remove(nodePath);
        State.PouchCache.Remove(pouchEntry);
        State.Clipboard.Add(pouchEntry);
        State.AddLog($"Pouch cache restore: {nodePath}");
        return true;
    }

    public void MoveCursor(string nodePath)
    {
        State.CursorAgent.CurrentNodePath = nodePath;
        State.AddLog($"Cursor moved: {nodePath}");

        ExecuteSecurityBehavior(
            SecurityBehaviorKeys.CursorCrossedMonitoredNode,
            nodePath,
            GetMonitoringAgents(nodePath),
            SecurityAwarenessStage.Suspicious,
            InfiltrationTuning.CursorMonitoredTraceIncrease,
            $"Cursor crossed monitored node: {nodePath}");
    }

    public void SetCurrentFolder(string folderPath)
    {
        State.CurrentFolderPath = folderPath;
    }

    public void HandleFolderNavigation(string folderPath, bool directJump = false)
    {
        State.CurrentFolderPath = folderPath;
        State.CursorAgent.CurrentNodePath = folderPath;
        State.AddLog($"Folder navigation: {folderPath}");

        var visibleAgents = GetVisibleSecurityAgents(folderPath);
        if (visibleAgents.Count == 0)
            return;

        var traceGain = directJump
            ? InfiltrationTuning.DirectFolderJumpTraceIncrease
            : InfiltrationTuning.FolderNavigationTraceIncrease;

        ExecuteSecurityBehavior(
            SecurityBehaviorKeys.FolderNavigation,
            folderPath,
            visibleAgents,
            directJump ? SecurityAwarenessStage.ActiveScan : SecurityAwarenessStage.Suspicious,
            traceGain,
            $"Navigated into monitored folder: {folderPath}",
            directJump);
    }

    public void TriggerSearchSweep(string nodePath)
    {
        var agents = SecurityAgents
            .Where(agent => agent.AgentType is SecurityAgentType.IndexerScout or SecurityAgentType.AiMonitor)
            .ToList();

        ExecuteSecurityBehavior(
            SecurityBehaviorKeys.SearchSweep,
            nodePath,
            agents,
            SecurityAwarenessStage.ActiveScan,
            0,
            $"Search sweep escalated at {nodePath}");
    }

    public List<SecurityAgent> GetVisibleSecurityAgents(string currentFolderPath)
    {
        return SecurityAgents
            .Where(agent => string.Equals(agent.CurrentNodePath, currentFolderPath, StringComparison.OrdinalIgnoreCase)
                || agent.PatrolRoute.Any(path => string.Equals(path, currentFolderPath, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public bool IsNodeHiddenInPouch(string nodePath)
    {
        return State.PouchCache.Any(entry => string.Equals(entry.NodePath, nodePath, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsPouchMaskingBroken(string nodePath)
    {
        return State.ExposedPouchPaths.Contains(nodePath);
    }

    public bool HasPermissionOverride(string nodePath)
    {
        return GetPermissionOverrideTurns(nodePath) > 0;
    }

    public int GetPermissionOverrideTurns(string nodePath)
    {
        return State.PermissionOverrideTurns.GetValueOrDefault(nodePath);
    }

    public bool IsPermissionLocked(string nodePath)
    {
        if (HasPermissionOverride(nodePath))
            return false;

        if (GetForcedLockTurns(nodePath) > 0)
            return true;

        return SecurityAgents.Any(agent => agent.AgentType == SecurityAgentType.FirewallSentinel
            && string.Equals(agent.CurrentNodePath, nodePath, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsPathTracked(string nodePath)
    {
        return GetTrackedPathTurns(nodePath) > 0;
    }

    public int GetTrackedPathTurns(string nodePath)
    {
        return State.TrackedPathTurns.GetValueOrDefault(nodePath);
    }

    public int GetForcedLockTurns(string nodePath)
    {
        return State.ForcedLockTurns.GetValueOrDefault(nodePath);
    }

    public bool HasScanPressure(string nodePath)
    {
        return GetScanPressureTurns(nodePath) > 0;
    }

    public int GetScanPressureTurns(string nodePath)
    {
        return State.ScanPressureTurns.GetValueOrDefault(nodePath);
    }

    public void MarkTrackedPath(string nodePath, int durationTurns, string reason)
    {
        State.TrackedPathTurns[nodePath] = Math.Max(1, durationTurns);
        State.AddLog($"Tracked path marked: {nodePath} ({State.TrackedPathTurns[nodePath]}T) :: {reason}");
    }

    public void ApplyForcedLock(string nodePath, int durationTurns, string reason)
    {
        State.ForcedLockTurns[nodePath] = Math.Max(1, durationTurns);
        State.AddLog($"Forced lock applied: {nodePath} ({State.ForcedLockTurns[nodePath]}T) :: {reason}");
    }

    public void ApplyScanPressure(string nodePath, int durationTurns, string reason)
    {
        State.ScanPressureTurns[nodePath] = Math.Max(1, durationTurns);
        State.AddLog($"Scan pressure applied: {nodePath} ({State.ScanPressureTurns[nodePath]}T) :: {reason}");
    }

    public bool ClearTrackedPath(string nodePath, string reason)
    {
        if (!State.TrackedPathTurns.Remove(nodePath))
            return false;

        State.AddLog($"Tracked path cleared: {nodePath} :: {reason}");
        return true;
    }

    public bool ClearScanPressure(string nodePath, string reason)
    {
        if (!State.ScanPressureTurns.Remove(nodePath))
            return false;

        State.AddLog($"Scan pressure cleared: {nodePath} :: {reason}");
        return true;
    }

    public void GrantPermissionOverride(string nodePath, string reason, int traceIncrease, int durationTurns = InfiltrationTuning.PermissionOverrideDurationTurns)
    {
        State.PermissionOverrideTurns[nodePath] = Math.Max(InfiltrationTuning.PermissionOverrideMinimumDurationTurns, durationTurns);
        AddTrace(traceIncrease, reason);
        State.AddLog($"Permission override granted: {nodePath} ({State.PermissionOverrideTurns[nodePath]}T)");
    }

    public void ExposePouchHiddenNode(string nodePath, string reason, int traceIncrease)
    {
        if (!IsNodeHiddenInPouch(nodePath))
            return;

        State.ExposedPouchPaths.Add(nodePath);
        AddTrace(traceIncrease, reason);
        State.AddLog($"Pouch exposed: {nodePath}");
    }

    public List<SecurityAgent> GetMonitoringAgents(string nodePath)
    {
        var pouchHidden = IsNodeHiddenInPouch(nodePath) && !IsPouchMaskingBroken(nodePath);
        var scanPressure = HasScanPressure(nodePath) || HasScanPressure(State.CurrentFolderPath);
        var pouchEntrySize = pouchHidden
            ? State.PouchCache.FirstOrDefault(e => string.Equals(e.NodePath, nodePath, StringComparison.OrdinalIgnoreCase))?.Size ?? 0
            : 0L;
        return SecurityAgents
            .Where(agent => string.Equals(agent.CurrentNodePath, nodePath, StringComparison.OrdinalIgnoreCase)
                || IsNodeInSight(agent, nodePath))
            .Where(agent =>
            {
                if (scanPressure || !pouchHidden)
                    return true;
                if (agent.AgentType == SecurityAgentType.IndexerScout)
                    return false;
                if (agent.AgentType == SecurityAgentType.AiMonitor)
                    return pouchEntrySize >= InfiltrationTuning.PouchSizeAiMonitorDetectionThreshold;
                return true;
            })
            .ToList();
    }

    public bool IsNodeMonitored(string nodePath)
    {
        return GetMonitoringAgents(nodePath).Count > 0;
    }

    private static bool IsNodeInSight(SecurityAgent agent, string nodePath)
    {
        if (agent.PatrolRoute.Count == 0)
            return false;

        var index = agent.PatrolRoute.FindIndex(path => string.Equals(path, nodePath, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return false;

        return Math.Abs(index - agent.PatrolIndex) <= agent.SightRange;
    }

    public void UnlockExit(string reason)
    {
        State.ExitUnlocked = true;
        State.RunStatus = RunStatus.ObjectiveCompleted;
        State.ObjectiveState = ObjectiveState.Completed;
        State.AddLog($"Exit unlocked: {reason}");
    }

    public bool TryEscape()
    {
        if (!State.ExitUnlocked)
        {
            State.AddLog("Escape blocked: exit locked");
            return false;
        }

        State.RunStatus = RunStatus.Escaped;
        State.AddLog("Escape successful");
        return true;
    }

    public void SetRunFailed(string reason)
    {
        if (State.RunStatus != RunStatus.Active) return;
        State.RunStatus = RunStatus.Failed;
        State.AddLog($"Run failed: {reason}");
    }

    public void SetRunTimedOut()
    {
        if (State.RunStatus != RunStatus.Active) return;
        State.RunStatus = RunStatus.TimedOut;
        State.AddLog("Run timed out: turn limit reached");
    }

    public bool TryClearDetection(string reason)
    {
        if (!State.CursorAgent.IsDetected)
            return false;
        if (State.Trace > InfiltrationTuning.DetectionClearTraceThreshold)
            return false;
        State.CursorAgent.IsDetected = false;
        State.AddLog($"Detection cleared: {reason}");
        return true;
    }

    private void TickPermissionOverrides()
    {
        foreach (var path in State.PermissionOverrideTurns.Keys.ToList())
        {
            var remaining = State.PermissionOverrideTurns[path] - 1;
            if (remaining <= 0)
            {
                State.PermissionOverrideTurns.Remove(path);
                State.AddLog($"Permission override expired: {path}");
                continue;
            }

            State.PermissionOverrideTurns[path] = remaining;
        }
    }

    private void TickTurnDictionary(Dictionary<string, int> turnsByPath, string expireLabel)
    {
        foreach (var path in turnsByPath.Keys.ToList())
        {
            var remaining = turnsByPath[path] - 1;
            if (remaining <= 0)
            {
                turnsByPath.Remove(path);
                State.AddLog($"{expireLabel}: {path}");
                continue;
            }

            turnsByPath[path] = remaining;
        }
    }

    private void ApplyTrackedPathActionTrace(string nodePath, OperationType operationType)
    {
        var targetTrackedTurns = GetTrackedPathTurns(nodePath);
        var folderTrackedTurns = GetTrackedPathTurns(State.CurrentFolderPath);
        if (targetTrackedTurns <= 0 && folderTrackedTurns <= 0)
            return;

        var trackedTurns = Math.Max(targetTrackedTurns, folderTrackedTurns);
        AddTrace(
            InfiltrationTuning.TrackedActionTraceBonus,
            $"Tracked path pressure: {operationType} @ {nodePath} ({trackedTurns}T)");
    }

    private void AdvanceSecurityAgents()
    {
        var activeTargets = State.ActiveOperations
            .Where(op => op.Status == OperationStatus.Running)
            .Select(op => op.TargetNodePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var cursorPath = State.CursorAgent.CurrentNodePath;
        var convergenceActive = State.AlertStage >= SecurityAwarenessStage.Quarantine
            && !string.IsNullOrWhiteSpace(cursorPath);

        foreach (var agent in SecurityAgents)
        {
            if (agent.DisabledTurns > 0)
            {
                agent.DisabledTurns--;
                continue;
            }

            if (convergenceActive)
            {
                var cursorIndex = agent.PatrolRoute.FindIndex(p =>
                    string.Equals(p, cursorPath, StringComparison.OrdinalIgnoreCase));
                if (cursorIndex >= 0)
                {
                    if (agent.PatrolIndex < cursorIndex) agent.PatrolIndex++;
                    else if (agent.PatrolIndex > cursorIndex) agent.PatrolIndex--;
                    agent.CurrentNodePath = agent.PatrolRoute[agent.PatrolIndex];
                }
                else
                {
                    agent.CurrentNodePath = cursorPath;
                }
                agent.AwarenessStage = State.AlertStage;
                continue;
            }

            if (agent.IsAlerted && activeTargets.Count > 0)
            {
                var targetPath = activeTargets.First();
                agent.CurrentNodePath = targetPath;
                agent.AwarenessStage = State.AlertStage >= SecurityAwarenessStage.ActiveScan
                    ? SecurityAwarenessStage.ActiveScan
                    : SecurityAwarenessStage.Suspicious;
                continue;
            }

            if (agent.PatrolRoute.Count > 0)
            {
                agent.PatrolIndex = (agent.PatrolIndex + 1) % agent.PatrolRoute.Count;
                agent.CurrentNodePath = agent.PatrolRoute[agent.PatrolIndex];
                agent.AwarenessStage = State.AlertStage;
            }
        }

        if (!State.CursorAgent.IsDetected && SecurityAgents.Any(agent =>
                agent.IsAlerted &&
                string.Equals(agent.CurrentNodePath, State.CursorAgent.CurrentNodePath, StringComparison.OrdinalIgnoreCase)))
        {
            State.CursorAgent.IsDetected = true;
            State.AddLog($"Cursor agent detected at {State.CursorAgent.CurrentNodePath}");
        }
    }

    private void ApplyDetectionContactDamage()
    {
        State.LastTurnContactDamage = 0;
        if (!State.CursorAgent.IsDetected)
            return;

        var cursorPath = State.CursorAgent.CurrentNodePath;
        var threateningCount = SecurityAgents.Count(a =>
            a.AgentType is SecurityAgentType.GuardScanner or SecurityAgentType.AntivirusHeavy
            && string.Equals(a.CurrentNodePath, cursorPath, StringComparison.OrdinalIgnoreCase));

        if (threateningCount == 0)
            return;

        var damage = InfiltrationTuning.DetectionContactDamage * threateningCount;
        State.TakeOperatorDamage(damage);
        State.LastTurnContactDamage = damage;
    }

    private void InterruptMonitoredOperationsOnDetection()
    {
        foreach (var operation in State.ActiveOperations
            .Where(op => op.Status == OperationStatus.Running && IsNodeMonitored(op.TargetNodePath))
            .ToList())
        {
            operation.Fail();
            State.AddLog($"Operation interrupted by detection: {operation.Type} @ {operation.TargetNodePath}");
        }
    }

    private static bool TryValidateQueueEntry(CommandQueueEntry entry, out string error)
    {
        if (string.IsNullOrWhiteSpace(entry.PrimaryTargetPath))
        {
            error = $"{entry.OperationType} has no primary target path";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private FileOperation CreateOperationFromQueueEntry(CommandQueueEntry entry)
    {
        var requiredTicks = entry.OperationType switch
        {
            OperationType.MoveCursor => 1,
            OperationType.Copy => 2,
            OperationType.Compress => 2,
            OperationType.RewriteLog => 2,
            OperationType.Delete => 1,
            OperationType.Stun => 1,
            _ => 1
        };

        return new FileOperation(entry.OperationType, entry.PrimaryTargetPath, requiredTicks, entry.SecondaryTargetPath);
    }

    private void OnOperationCompleted(FileOperation operation)
    {
        switch (operation.Type)
        {
            case OperationType.MoveCursor:
                MoveCursor(operation.TargetNodePath);
                break;
            case OperationType.Copy:
            {
                var ok = TryCopyToClipboard(operation.TargetNodePath, operation.NodeKind, operation.NodeSize);
                operation.CompletionNotes.Add(ok ? "copy complete" : "copy blocked :: clipboard full");
                break;
            }
            case OperationType.Cut:
            {
                var ok = TryCopyToClipboard(operation.TargetNodePath, operation.NodeKind, operation.NodeSize);
                operation.CompletionNotes.Add(ok ? "cut clipboard synced" : "cut blocked :: clipboard full");
                break;
            }
            case OperationType.Paste:
            {
                var pasted = State.Clipboard.FirstOrDefault(e =>
                    string.Equals(e.NodePath, operation.TargetNodePath, StringComparison.OrdinalIgnoreCase));
                if (pasted != null)
                {
                    State.Clipboard.Remove(pasted);
                    operation.CompletionNotes.Add("paste complete :: clipboard cleared");
                }
                break;
            }
            case OperationType.RewriteLog:
            {
                ReduceTrace(InfiltrationTuning.RewriteLogTraceReduction, $"Log rewritten at {operation.TargetNodePath}");
                var cleared = new List<string>();
                if (ClearTrackedPath(operation.TargetNodePath, "Rewrite Log scrubbed node route"))
                    cleared.Add("tracked");
                if (ClearTrackedPath(State.CurrentFolderPath, "Rewrite Log scrubbed current folder route"))
                    cleared.Add("folder-tracked");
                if (ClearScanPressure(operation.TargetNodePath, "Rewrite Log diffused node scan pressure"))
                    cleared.Add("pressure");
                if (ClearScanPressure(State.CurrentFolderPath, "Rewrite Log diffused folder scan pressure"))
                    cleared.Add("folder-pressure");
                if (cleared.Count > 0)
                    operation.CompletionNotes.Add($"log scrub :: cleared {string.Join(", ", cleared)}");
                if (TryClearDetection($"Rewrite log completed at {operation.TargetNodePath}"))
                    operation.CompletionNotes.Add("detection cleared :: log rewritten");
                break;
            }
        }
    }

    private void UpdateAlertStage()
    {
        State.AlertStage = State.Trace switch
        {
            >= 85 => SecurityAwarenessStage.Purge,
            >= 60 => SecurityAwarenessStage.Quarantine,
            >= 35 => SecurityAwarenessStage.ActiveScan,
            >= 15 => SecurityAwarenessStage.Suspicious,
            _ => SecurityAwarenessStage.Passive
        };
    }

    private bool ExecuteSecurityBehavior(
        string behaviorKey,
        string primaryPath,
        IReadOnlyList<SecurityAgent> agents,
        SecurityAwarenessStage awarenessStage,
        int traceAmount,
        string traceReason,
        bool directJump = false)
    {
        if (agents.Count == 0)
            return false;

        var executed = false;
        foreach (var agent in agents)
        {
            var resolvedKey = ResolveSecurityBehaviorKey(behaviorKey, agent.AgentType);
            var isObjectivePath = string.Equals(primaryPath, Mission.TargetPath, StringComparison.OrdinalIgnoreCase);
            var isObjectiveRoute = IsObjectiveRoute(primaryPath, Mission.TargetPath);
            var agentOnObjectiveRoute = IsObjectiveRoute(agent.CurrentNodePath, Mission.TargetPath);
            executed |= _securityBehaviorExecutor.TryExecute(resolvedKey, new SecurityBehaviorContext
            {
                PrimaryPath = primaryPath,
                Agent = agent,
                Agents = new[] { agent },
                CurrentFolderPath = State.CurrentFolderPath,
                CursorPath = State.CursorAgent.CurrentNodePath,
                ObjectivePath = Mission.TargetPath,
                IsObjectivePath = isObjectivePath,
                IsObjectiveRoute = isObjectiveRoute,
                AgentOnObjectiveRoute = agentOnObjectiveRoute,
                DirectJump = directJump,
                TraceAmount = traceAmount,
                TraceReason = traceReason,
                AwarenessStage = awarenessStage,
                AddTrace = AddTrace,
                AddLog = message => State.AddLog(message),
                AlertAgent = AlertAgent,
                MarkTrackedPath = MarkTrackedPath,
                ApplyForcedLock = ApplyForcedLock,
                ApplyScanPressure = ApplyScanPressure
            });
        }

        return executed;
    }

    private static void AlertAgent(SecurityAgent agent, SecurityAwarenessStage awarenessStage)
    {
        agent.IsAlerted = true;
        agent.AwarenessStage = awarenessStage;
    }

    private static string ResolveSecurityBehaviorKey(string behaviorKey, SecurityAgentType agentType)
    {
        return behaviorKey switch
        {
            SecurityBehaviorKeys.CursorCrossedMonitoredNode => agentType switch
            {
                SecurityAgentType.GuardScanner => SecurityBehaviorKeys.CursorCrossedGuardScanner,
                SecurityAgentType.IndexerScout => SecurityBehaviorKeys.CursorCrossedIndexerScout,
                SecurityAgentType.AiMonitor => SecurityBehaviorKeys.CursorCrossedAiMonitor,
                SecurityAgentType.FirewallSentinel => SecurityBehaviorKeys.CursorCrossedFirewallSentinel,
                SecurityAgentType.AntivirusHeavy => SecurityBehaviorKeys.CursorCrossedAntivirusHeavy,
                SecurityAgentType.BackupRepairer => SecurityBehaviorKeys.CursorCrossedBackupRepairer,
                _ => behaviorKey
            },
            SecurityBehaviorKeys.FolderNavigation => agentType switch
            {
                SecurityAgentType.GuardScanner => SecurityBehaviorKeys.FolderNavigationGuardScanner,
                SecurityAgentType.IndexerScout => SecurityBehaviorKeys.FolderNavigationIndexerScout,
                SecurityAgentType.AiMonitor => SecurityBehaviorKeys.FolderNavigationAiMonitor,
                SecurityAgentType.FirewallSentinel => SecurityBehaviorKeys.FolderNavigationFirewallSentinel,
                SecurityAgentType.AntivirusHeavy => SecurityBehaviorKeys.FolderNavigationAntivirusHeavy,
                SecurityAgentType.BackupRepairer => SecurityBehaviorKeys.FolderNavigationBackupRepairer,
                _ => behaviorKey
            },
            SecurityBehaviorKeys.SearchSweep => agentType switch
            {
                SecurityAgentType.IndexerScout => SecurityBehaviorKeys.SearchSweepIndexerScout,
                SecurityAgentType.AiMonitor => SecurityBehaviorKeys.SearchSweepAiMonitor,
                SecurityAgentType.AntivirusHeavy => SecurityBehaviorKeys.SearchSweepAntivirusHeavy,
                SecurityAgentType.BackupRepairer => SecurityBehaviorKeys.SearchSweepBackupRepairer,
                _ => behaviorKey
            },
            _ => behaviorKey
        };
    }

    private static bool IsObjectiveRoute(string path, string objectivePath)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(objectivePath))
            return false;

        if (string.Equals(path, objectivePath, StringComparison.OrdinalIgnoreCase))
            return true;

        return objectivePath.StartsWith(path.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(objectivePath.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase);
    }
}
