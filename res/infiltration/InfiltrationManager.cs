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
        State.ActiveOperations.Clear();
        State.CommandQueue.Clear();
        State.RunStatus = RunStatus.Active;
        State.ObjectiveState = ObjectiveState.Revealed;
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

    public void AdvanceTurn()
    {
        State.TurnCount++;
        State.CursorAgent.RestoreActionPoints();
        TickOperations();
        AdvanceSecurityAgents();
        State.AddLog($"Turn advanced to {State.TurnCount}");
    }

    public void QueueCommand(CommandQueueEntry entry)
    {
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
        foreach (var entry in State.CommandQueue.OrderBy(x => x.Order).ToList())
        {
            var operation = CreateOperationFromQueueEntry(entry);
            StartOperation(operation);
        }

        State.CommandQueue.Clear();
        State.AddLog("Command queue executed");
    }

    public void StartOperation(FileOperation operation)
    {
        operation.Start();
        State.ActiveOperations.Add(operation);
        State.AddLog($"Operation started: {operation.Type} @ {operation.TargetNodePath}");

        if (GetMonitoringAgents(operation.TargetNodePath).Count > 0)
        {
            AddTrace(4, $"Monitored operation: {operation.Type} @ {operation.TargetNodePath}");
        }
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

    public bool TryCopyToClipboard(string nodePath, ExplorerNodeKind nodeKind)
    {
        if (State.Clipboard.Count >= State.CursorAgent.ClipboardCapacity)
        {
            State.AddLog("Clipboard full");
            return false;
        }

        State.Clipboard.Add(new ClipboardEntry
        {
            NodePath = nodePath,
            NodeKind = nodeKind
        });
        State.AddLog($"Clipboard add: {nodePath}");
        return true;
    }

    public void MoveCursor(string nodePath)
    {
        State.CursorAgent.CurrentNodePath = nodePath;
        State.AddLog($"Cursor moved: {nodePath}");

        foreach (var agent in GetMonitoringAgents(nodePath))
        {
            agent.IsAlerted = true;
            agent.AwarenessStage = SecurityAwarenessStage.Suspicious;
            AddTrace(6, $"Cursor crossed monitored node: {nodePath}");
        }
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

        var traceGain = directJump ? 5 : 3;
        foreach (var agent in visibleAgents)
        {
            agent.IsAlerted = true;
            agent.AwarenessStage = directJump
                ? SecurityAwarenessStage.ActiveScan
                : SecurityAwarenessStage.Suspicious;
        }

        AddTrace(traceGain, $"Navigated into monitored folder: {folderPath}");
    }

    public List<SecurityAgent> GetVisibleSecurityAgents(string currentFolderPath)
    {
        return SecurityAgents
            .Where(agent => string.Equals(agent.CurrentNodePath, currentFolderPath, StringComparison.OrdinalIgnoreCase)
                || agent.PatrolRoute.Any(path => string.Equals(path, currentFolderPath, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public List<SecurityAgent> GetMonitoringAgents(string nodePath)
    {
        return SecurityAgents
            .Where(agent => string.Equals(agent.CurrentNodePath, nodePath, StringComparison.OrdinalIgnoreCase)
                || IsNodeInSight(agent, nodePath))
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

    private void AdvanceSecurityAgents()
    {
        var activeTargets = State.ActiveOperations
            .Where(op => op.Status == OperationStatus.Running)
            .Select(op => op.TargetNodePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var agent in SecurityAgents)
        {
            if (agent.DisabledTurns > 0)
            {
                agent.DisabledTurns--;
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
        if (operation.Type == OperationType.Copy)
        {
            TryCopyToClipboard(operation.TargetNodePath, ExplorerNodeKind.File);
        }
        else if (operation.Type == OperationType.MoveCursor)
        {
            MoveCursor(operation.TargetNodePath);
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
}
