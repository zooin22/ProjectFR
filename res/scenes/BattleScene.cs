using Godot;
using ProjectFR.Action;
using ProjectFR.Battle;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;
using ProjectFR.Infiltration;
using ProjectFR.Mission;
using ProjectFR.Systems;

namespace ProjectFR.Scenes;

public partial class BattleScene : Control
{
    private BattleManager _battleManager = null!;
    private BattleDungeon _dungeon = null!;
    private InfiltrationManager _infiltrationManager = null!;
    private ActionRegistry _actionRegistry = null!;
    private readonly Dictionary<string, Button> _actionButtons = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ActorState> _nodeActors = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _executedPlayerActions = new();

    private Tree _folderTree = null!;
    private Tree _fileTree = null!;
    private ItemList _explorerFieldList = null!;
    private Label _pathLabel = null!;
    private Label _missionLabel = null!;
    private Label _turnStateLabel = null!;
    private ProgressBar _traceBar = null!;
    private Label _traceLabel = null!;
    private Label _playerStatusLabel = null!;
    private Label _explorerStateLabel = null!;
    private Label _cursorStatusLabel = null!;
    private Label _fieldSecurityLabel = null!;
    private Label _inspectorNameLabel = null!;
    private Label _inspectorTypeLabel = null!;
    private Label _inspectorPathLabel = null!;
    private Label _inspectorStatsLabel = null!;
    private Label _inspectorStatusLabel = null!;
    private Label _inspectorHintLabel = null!;
    private Label _securityAgentsLabel = null!;
    private ProgressBar _targetHpBar = null!;
    private ProgressBar _targetApBar = null!;
    private RichTextLabel _operationLogLabel = null!;
    private RichTextLabel _consoleFeedLabel = null!;
    private RichTextLabel _commandQueueLabel = null!;
    private Label _consoleHintLabel = null!;
    private Control _battleEndOverlay = null!;
    private Label _battleEndTitleLabel = null!;
    private Label _battleEndSummaryLabel = null!;
    private Label _battleEndStatsLabel = null!;
    private Button _restartBattleButton = null!;
    private Button _backToLobbyButton = null!;
    private Button _consoleHelpButton = null!;
    private Button _consoleScanButton = null!;
    private Button _consoleStatusButton = null!;
    private Button _consoleUpButton = null!;
    private Button _consoleClearButton = null!;
    private Button _clipboardWindowButton = null!;
    private Button _moveCursorButton = null!;
    private Button _executeQueueButton = null!;
    private Button _extractButton = null!;
    private Button _clearQueueButton = null!;
    private PopupMenu _explorerContextMenu = null!;
    private Control _clipboardWindow = null!;
    private Button _clipboardWindowCloseButton = null!;
    private Label _clipboardWindowStatusLabel = null!;
    private RichTextLabel _clipboardWindowItemsLabel = null!;

    private readonly Dictionary<long, string> _contextActionMap = new();

    private readonly Dictionary<string, Texture2D> _nodeIcons = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<SecurityAgentType, Texture2D> _agentIcons = new();

    private MissionData _currentMission = null!;
    private MissionProgress _missionProgress = null!;
    private MissionResult? _missionResult;
    private CampaignModifiers _campaignModifiers = null!;
    private int _effectiveTurnLimit;
    private bool _missionResolved;
    private string? _selectedNodePath;
    private string? _dragSourceNodePath;
    private string? _dragHoverTargetPath;

    public override void _Ready()
    {
        InitializeUi();
        InitializeBattle();
        UpdateUi();
        RunSmokeTestIfRequested();
    }

    private void InitializeUi()
    {
        _folderTree = GetNode<Tree>("RootMargin/MainVBox/MainSplit/FolderTreePanel/FolderTreeMargin/FolderTreeVBox/FolderTree");
        _fileTree = GetNode<Tree>("RootMargin/MainVBox/MainSplit/ExplorerPanel/ExplorerMargin/ExplorerVBox/FileListTree");
        _explorerFieldList = GetNode<ItemList>("RootMargin/MainVBox/MainSplit/ExplorerPanel/ExplorerMargin/ExplorerVBox/ExplorerFieldList");
        _pathLabel = GetNode<Label>("RootMargin/MainVBox/TopPanel/TopMargin/TopVBox/AddressRow/PathLabel");
        _missionLabel = GetNode<Label>("RootMargin/MainVBox/TopPanel/TopMargin/TopVBox/MissionLabel");
        _turnStateLabel = GetNode<Label>("RootMargin/MainVBox/TopPanel/TopMargin/TopVBox/StatusRow/TurnStateLabel");
        _traceBar = GetNode<ProgressBar>("RootMargin/MainVBox/TopPanel/TopMargin/TopVBox/StatusRow/TraceRow/TraceBar");
        _traceLabel = GetNode<Label>("RootMargin/MainVBox/TopPanel/TopMargin/TopVBox/StatusRow/TraceRow/TraceLabel");
        _explorerStateLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/ExplorerPanel/ExplorerMargin/ExplorerVBox/ExplorerSubToolbar/ExplorerStateLabel");
        _cursorStatusLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/ExplorerPanel/ExplorerMargin/ExplorerVBox/FieldStatusRow/CursorStatusLabel");
        _fieldSecurityLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/ExplorerPanel/ExplorerMargin/ExplorerVBox/FieldStatusRow/FieldSecurityLabel");
        _playerStatusLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/PlayerStatusLabel");
        _inspectorNameLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/PreviewFrame/PreviewMargin/PreviewVBox/InspectorNameLabel");
        _inspectorTypeLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/PreviewFrame/PreviewMargin/PreviewVBox/InspectorTypeLabel");
        _inspectorPathLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/PreviewFrame/PreviewMargin/PreviewVBox/InspectorPathLabel");
        _inspectorStatsLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/InspectorStatsLabel");
        _inspectorStatusLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/InspectorStatusLabel");
        _inspectorHintLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/InspectorHintLabel");
        _securityAgentsLabel = GetNode<Label>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/SecurityAgentsLabel");
        _targetHpBar = GetNode<ProgressBar>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/TargetHpBar");
        _targetApBar = GetNode<ProgressBar>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/TargetApBar");
        _operationLogLabel = GetNode<RichTextLabel>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/OperationLogLabel");
        _commandQueueLabel = GetNode<RichTextLabel>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueLabel");
        _consoleFeedLabel = GetNode<RichTextLabel>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/ConsolePanel/ConsoleMargin/ConsoleVBox/ConsoleFeedLabel");
        _consoleHintLabel = GetNode<Label>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/ConsoleHintLabel");
        _battleEndOverlay = GetNode<Control>("BattleEndOverlay");
        _battleEndTitleLabel = GetNode<Label>("BattleEndOverlay/OverlayCenter/BattleEndPanel/BattleEndMargin/BattleEndVBox/BattleEndTitleLabel");
        _battleEndSummaryLabel = GetNode<Label>("BattleEndOverlay/OverlayCenter/BattleEndPanel/BattleEndMargin/BattleEndVBox/BattleEndSummaryLabel");
        _battleEndStatsLabel = GetNode<Label>("BattleEndOverlay/OverlayCenter/BattleEndPanel/BattleEndMargin/BattleEndVBox/BattleEndStatsLabel");
        _restartBattleButton = GetNode<Button>("BattleEndOverlay/OverlayCenter/BattleEndPanel/BattleEndMargin/BattleEndVBox/BattleEndButtonRow/RestartBattleButton");
        _backToLobbyButton = GetNode<Button>("BattleEndOverlay/OverlayCenter/BattleEndPanel/BattleEndMargin/BattleEndVBox/BattleEndButtonRow/BackToLobbyButton");
        _consoleHelpButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/ConsolePanel/ConsoleMargin/ConsoleVBox/ConsoleTopRow/UtilityRow/HelpButton");
        _consoleScanButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/ConsolePanel/ConsoleMargin/ConsoleVBox/ConsoleTopRow/UtilityRow/ScanButton");
        _consoleStatusButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/ConsolePanel/ConsoleMargin/ConsoleVBox/ConsoleTopRow/UtilityRow/StatusButton");
        _consoleUpButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/ConsolePanel/ConsoleMargin/ConsoleVBox/ConsoleTopRow/UtilityRow/UpButton");
        _consoleClearButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/ConsolePanel/ConsoleMargin/ConsoleVBox/ConsoleTopRow/UtilityRow/ClearButton");
        _clipboardWindowButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/ClipboardWindowButton");
        _moveCursorButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/MoveCursorButton");
        _executeQueueButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/ExecuteQueueButton");
        _extractButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/ExtractButton");
        _clearQueueButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/ClearQueueButton");
        _explorerContextMenu = GetNode<PopupMenu>("ExplorerContextMenu");
        _clipboardWindow = GetNode<Control>("ClipboardWindow");
        _clipboardWindowCloseButton = GetNode<Button>("ClipboardWindow/ClipboardWindowMargin/ClipboardWindowVBox/ClipboardWindowHeaderRow/ClipboardWindowCloseButton");
        _clipboardWindowStatusLabel = GetNode<Label>("ClipboardWindow/ClipboardWindowMargin/ClipboardWindowVBox/ClipboardWindowStatusLabel");
        _clipboardWindowItemsLabel = GetNode<RichTextLabel>("ClipboardWindow/ClipboardWindowMargin/ClipboardWindowVBox/ClipboardWindowItemsLabel");

        LoadPlaceholderAssets();
        ConfigureTrees();

        _folderTree.ItemSelected += OnFolderTreeSelected;
        _fileTree.ItemSelected += OnFileTreeSelected;
        _fileTree.ItemActivated += OnFileTreeActivated;
        _explorerFieldList.ItemSelected += OnExplorerFieldSelected;
        _explorerFieldList.ItemActivated += OnExplorerFieldActivated;
        _explorerFieldList.GuiInput += OnExplorerFieldGuiInput;
        _restartBattleButton.Pressed += RestartBattle;
        _backToLobbyButton.Pressed += BackToLobby;
        _consoleHelpButton.Pressed += ShowConsoleHelp;
        _consoleScanButton.Pressed += () => AppendConsoleFeed(BuildVisibleNodeReport());
        _consoleStatusButton.Pressed += () => AppendConsoleFeed(BuildStatusSummary());
        _consoleUpButton.Pressed += NavigateUp;
        _consoleClearButton.Pressed += ClearConsoleFeed;
        _clipboardWindowButton.Pressed += ToggleClipboardWindow;
        _moveCursorButton.Pressed += QueueMoveCursorCommand;
        _executeQueueButton.Pressed += ExecuteQueuedCommands;
        _extractButton.Pressed += TryExtractMission;
        _clearQueueButton.Pressed += ClearQueuedCommands;
        _explorerContextMenu.IdPressed += OnExplorerContextMenuPressed;
        _clipboardWindowCloseButton.Pressed += CloseClipboardWindow;

        CreateActionButtons();
        AppendConsoleFeed("Explorer bridge online.");
        ShowConsoleHelp();
    }

    private void LoadPlaceholderAssets()
    {
        _nodeIcons["folder"] = GD.Load<Texture2D>("res://res/assets/ui/file_folder.svg");
        _nodeIcons["text file"] = GD.Load<Texture2D>("res://res/assets/ui/file_text.svg");
        _nodeIcons["log file"] = GD.Load<Texture2D>("res://res/assets/ui/file_log.svg");
        _nodeIcons["archive"] = GD.Load<Texture2D>("res://res/assets/ui/file_archive.svg");
        _nodeIcons["archive boss"] = GD.Load<Texture2D>("res://res/assets/ui/file_archive.svg");
        _nodeIcons["executable"] = GD.Load<Texture2D>("res://res/assets/ui/file_executable.svg");
        _nodeIcons["encrypted"] = GD.Load<Texture2D>("res://res/assets/ui/file_encrypted.svg");
        _nodeIcons["temp file"] = GD.Load<Texture2D>("res://res/assets/ui/file_temp.svg");
        _nodeIcons["shortcut"] = GD.Load<Texture2D>("res://res/assets/ui/file_shortcut.svg");
        _nodeIcons["special file"] = GD.Load<Texture2D>("res://res/assets/ui/file_encrypted.svg");
        _nodeIcons["default"] = GD.Load<Texture2D>("res://res/assets/ui/file_text.svg");

        _agentIcons[SecurityAgentType.GuardScanner] = GD.Load<Texture2D>("res://res/assets/agents/guard_scanner.svg");
        _agentIcons[SecurityAgentType.AntivirusHeavy] = GD.Load<Texture2D>("res://res/assets/agents/antivirus_heavy.svg");
        _agentIcons[SecurityAgentType.IndexerScout] = GD.Load<Texture2D>("res://res/assets/agents/indexer_scout.svg");
        _agentIcons[SecurityAgentType.BackupRepairer] = GD.Load<Texture2D>("res://res/assets/agents/backup_repairer.svg");
        _agentIcons[SecurityAgentType.FirewallSentinel] = GD.Load<Texture2D>("res://res/assets/agents/firewall_sentinel.svg");
        _agentIcons[SecurityAgentType.AiMonitor] = GD.Load<Texture2D>("res://res/assets/agents/ai_monitor.svg");
    }

    private void ConfigureTrees()
    {
        _folderTree.Columns = 1;
        _folderTree.HideRoot = true;
        _folderTree.SelectMode = Tree.SelectModeEnum.Single;

        _fileTree.Columns = 5;
        _fileTree.HideRoot = true;
        _fileTree.SelectMode = Tree.SelectModeEnum.Single;
        _fileTree.ColumnTitlesVisible = true;
        _fileTree.SetColumnTitle(0, "Name");
        _fileTree.SetColumnTitle(1, "Type");
        _fileTree.SetColumnTitle(2, "Security");
        _fileTree.SetColumnTitle(3, "Threat");
        _fileTree.SetColumnTitle(4, "Status/Size");
        _fileTree.SetColumnExpand(0, true);
        _fileTree.SetColumnExpand(1, false);
        _fileTree.SetColumnExpand(2, false);
        _fileTree.SetColumnExpand(3, false);
        _fileTree.SetColumnExpand(4, false);

        _explorerFieldList.IconMode = ItemList.IconModeEnum.Top;
        _explorerFieldList.SelectMode = ItemList.SelectModeEnum.Single;
        _explorerFieldList.FixedColumnWidth = 128;
        _explorerFieldList.FixedIconSize = new Vector2I(48, 48);
        _explorerFieldList.MaxTextLines = 2;
        _explorerFieldList.SameColumnWidth = true;
    }

    private void CreateActionButtons()
    {
        _actionRegistry = new ActionRegistry();
        var container = GetNode<GridContainer>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandDeckGrid");
        foreach (var action in _actionRegistry.GetAllActions())
        {
            var button = new Button
            {
                Text = $"> {action.ActionId.ToUpperInvariant()}\nAP {action.ApCost}",
                CustomMinimumSize = new Vector2(132, 48),
                TooltipText = ActionMetadata.GetTooltipText(action.ActionId)
            };
            button.Pressed += () => QueueMouseCommand(action.ActionId);
            container.AddChild(button);
            _actionButtons[action.ActionId] = button;
        }
    }

    private void InitializeBattle()
    {
        CampaignState.EnsureInitialized();
        CampaignState.BeginSelectedMission();
        _currentMission = CampaignState.CurrentMission ?? CampaignState.GetSelectedMission();
        _campaignModifiers = CampaignState.GetModifiers();
        _effectiveTurnLimit = Math.Max(3, _currentMission.TurnLimit - _campaignModifiers.HeatTurnPenalty);
        _missionProgress = new MissionProgress(_currentMission);
        _missionResult = null;
        _missionResolved = false;

        _dungeon = BattleFactory.CreateDefaultDungeon();
        BuildActorIndex(_dungeon.Root);

        _infiltrationManager = new InfiltrationManager(_currentMission);
        _infiltrationManager.Initialize(_dungeon.Root.Path, EnumerateKnownNodes(_dungeon.Root));
        SeedSecurityAgents();

        _battleManager = new BattleManager(BattleFactory.CreateDefaultPlayer())
        {
            EndBattleWhenEnemiesCleared = false
        };
        _battleManager.StartBattle();
        _battleManager.AddLog($"Mission accepted: {_currentMission.Title} / Client: {_currentMission.Client.Name}");
        _battleManager.AddLog($"Faction: {_currentMission.Client.Faction} / { _campaignModifiers.Summary }");

        LoadContainer(_dungeon.Root, isFirstContainer: true, reason: "Connected to root explorer.");
    }

    private void BuildActorIndex(ContainerNode container)
    {
        foreach (var child in container.Children)
        {
            _nodeActors[child.Path] = BattleFactory.CreateActorForNode(child, _campaignModifiers);
            if (child is ContainerNode nested)
            {
                BuildActorIndex(nested);
            }
        }
    }

    private static IEnumerable<NodeData> EnumerateKnownNodes(ContainerNode container)
    {
        foreach (var child in container.Children)
        {
            yield return child;
            if (child is ContainerNode nested)
            {
                foreach (var nestedChild in EnumerateKnownNodes(nested))
                {
                    yield return nestedChild;
                }
            }
        }
    }

    private void SeedSecurityAgents()
    {
        _infiltrationManager.AddSecurityAgent(new SecurityAgent(
            SecurityAgentType.IndexerScout,
            "Indexer Scout",
            BattleConstants.RootReadmePath,
            new[] { BattleConstants.RootReadmePath, BattleConstants.RootBuildCachePath }));

        _infiltrationManager.AddSecurityAgent(new SecurityAgent(
            SecurityAgentType.GuardScanner,
            "Guard Scanner",
            BattleConstants.RootBuildCachePath,
            new[] { BattleConstants.RootBuildCachePath, BattleConstants.CacheTempPath }));

        _infiltrationManager.AddSecurityAgent(new SecurityAgent(
            SecurityAgentType.FirewallSentinel,
            "Firewall Sentinel",
            BattleConstants.CacheAssetsPath,
            new[] { BattleConstants.CacheAssetsPath }));

        _infiltrationManager.AddSecurityAgent(new SecurityAgent(
            SecurityAgentType.AntivirusHeavy,
            "Antivirus Heavy",
            BattleConstants.BossZipPath,
            new[] { BattleConstants.BossZipPath }));
    }

    private void LoadContainer(ContainerNode container, bool isFirstContainer = false, string? reason = null)
    {
        var previousPath = _dungeon.CurrentContainer.Path;
        _dungeon.EnterContainer(container.Path);
        if (!isFirstContainer)
        {
            var directJump = !string.Equals(previousPath, container.Path, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(_dungeon.GetParentContainer(container.Path)?.Path, previousPath, StringComparison.OrdinalIgnoreCase);
            _infiltrationManager.HandleFolderNavigation(container.Path, directJump);
        }
        else
        {
            _infiltrationManager.SetCurrentFolder(container.Path);
        }
        var encounter = BattleFactory.CreateEncounter(container, _campaignModifiers, node => !_dungeon.IsCleared(node.Path) && _nodeActors.GetValueOrDefault(node.Path)?.IsAlive == true);
        var encounterActors = encounter.Select(item => ReuseActor(item.NodeData.Path, item.Actor)).ToList();
        _battleManager.LoadEncounter(encounterActors, $"Entered {container.Path}", restorePlayerAp: !isFirstContainer);

        var metadata = _dungeon.GetCurrentMetadata();
        _battleManager.AddLog($"Theme: {metadata.ThemeName}");
        _battleManager.AddLog($"Event: {metadata.EventSummary}");
        _battleManager.AddLog($"Objective: {_currentMission.ObjectiveType} {_currentMission.TargetPath} before turn {_effectiveTurnLimit}");

        if (!string.IsNullOrWhiteSpace(reason))
        {
            AppendConsoleFeed(reason);
        }

        _selectedNodePath = GetFirstVisibleNodePath();
    }

    private ActorState ReuseActor(string path, ActorState fallback)
    {
        if (_nodeActors.TryGetValue(path, out var actor))
        {
            return actor;
        }

        _nodeActors[path] = fallback;
        return fallback;
    }

    private void QueueMouseCommand(string actionId)
    {
        AppendConsoleFeed($"> queue {actionId.ToUpperInvariant()}");
        QueueSelectedCommand(actionId);
    }

    private void QueueSelectedCommand(string actionId)
    {
        if (_battleManager.IsBattleEnd)
            return;

        var selectedNode = GetSelectedNode();
        if (selectedNode == null)
        {
            AppendConsoleFeed("No target selected.");
            return;
        }

        if (!_nodeActors.TryGetValue(selectedNode.Path, out var targetActor) || !targetActor.IsAlive)
        {
            AppendConsoleFeed($"Target offline :: {selectedNode.Name}");
            return;
        }

        var action = _actionRegistry.GetAction(actionId);
        if (action == null)
        {
            AppendConsoleFeed($"Unknown action :: {actionId}");
            return;
        }

        var previewContext = new ActionContext(_battleManager.Player)
        {
            Target = targetActor,
            TargetNode = selectedNode,
            Clipboard = _battleManager.Clipboard,
            StatusEffects = _battleManager.StatusEffects,
            AllActors = _battleManager.Enemies
        };

        if (!action.CanExecute(previewContext))
        {
            AppendConsoleFeed($"blocked :: {actionId}");
            return;
        }

        QueueOperationCommand(
            MapActionIdToOperationType(actionId),
            selectedNode.Path,
            null,
            $"{actionId.ToUpperInvariant()} -> {selectedNode.Name}");
    }

    private void ExecuteQueuedCommands()
    {
        if (_infiltrationManager.State.CommandQueue.Count == 0)
        {
            AppendConsoleFeed("queue empty");
            return;
        }

        var queued = _infiltrationManager.State.CommandQueue.OrderBy(entry => entry.Order).ToList();

        foreach (var entry in queued)
        {
            ExecuteQueuedCommandEntry(entry);
            _infiltrationManager.AdvanceTurn();
            ProcessCompletedOperations();
            if (_battleManager.IsBattleEnd)
                break;
        }

        _infiltrationManager.ClearQueue();
        ApplyMissionFailureChecks();
        if (_battleManager.IsBattleEnd)
        {
            OnBattleEnd();
        }

        UpdateUi();
    }

    private void ExecuteQueuedCommandEntry(CommandQueueEntry entry)
    {
        var selectedNode = _dungeon.GetNode(entry.PrimaryTargetPath);
        if (selectedNode == null)
        {
            AppendConsoleFeed($"missing target :: {entry.PrimaryTargetPath}");
            return;
        }

        _selectedNodePath = selectedNode.Path;
        var requiredTicks = GetRequiredTicksForOperation(entry.OperationType, selectedNode);
        var operation = new FileOperation(entry.OperationType, selectedNode.Path, requiredTicks, entry.SecondaryTargetPath);
        _infiltrationManager.StartOperation(operation);

        if (entry.OperationType == OperationType.MoveCursor)
        {
            AppendConsoleFeed($"cursor route :: {selectedNode.Name}");
            return;
        }

        if (entry.OperationType == OperationType.Move)
        {
            AppendConsoleFeed($"move route :: {selectedNode.Name}");
            return;
        }

        if (!IsDeferredOperation(entry.OperationType))
        {
            ExecuteImmediateAction(entry.OperationType, selectedNode);
            operation.MarkCompletionHandled();
        }
        else
        {
            AppendConsoleFeed($"op start :: {entry.OperationType} -> {selectedNode.Name} :: {requiredTicks}T");
        }
    }

    private void ExecuteImmediateAction(OperationType operationType, NodeData selectedNode)
    {
        var actionId = MapOperationTypeToActionId(operationType);
        if (actionId == null)
        {
            AppendConsoleFeed($"unsupported op :: {operationType}");
            return;
        }

        if (!_nodeActors.TryGetValue(selectedNode.Path, out var targetActor) || !targetActor.IsAlive)
        {
            AppendConsoleFeed($"Target offline :: {selectedNode.Name}");
            return;
        }

        var action = _actionRegistry.GetAction(actionId);
        if (action == null)
        {
            AppendConsoleFeed($"Unknown action :: {actionId}");
            return;
        }

        var context = new ActionContext(_battleManager.Player)
        {
            Target = targetActor,
            TargetNode = selectedNode,
            Clipboard = _battleManager.Clipboard,
            StatusEffects = _battleManager.StatusEffects,
            AllActors = _battleManager.Enemies
        };

        var result = _battleManager.PlayerAction(action, context);
        if (result.Success)
        {
            _executedPlayerActions.Add(actionId);
            AppendConsoleFeed($"exec :: {actionId} -> {selectedNode.Name}");

            var missionUpdate = _missionProgress.RegisterAction(actionId, selectedNode.Path);
            if (!string.IsNullOrWhiteSpace(missionUpdate))
            {
                _battleManager.AddLog(missionUpdate);
                AppendConsoleFeed(missionUpdate);
            }

            if (_missionProgress.ObjectiveCompleted && !_infiltrationManager.State.ExitUnlocked)
            {
                _infiltrationManager.UnlockExit($"Objective secured: {_currentMission.TargetPath}");
                AppendConsoleFeed("objective complete :: extraction unlocked :: return to root and extract");
            }

            if (!targetActor.IsAlive)
            {
                HandleNodeDefeated(selectedNode);
            }
            else if (actionId.Equals("open", StringComparison.OrdinalIgnoreCase) && selectedNode is ContainerNode containerNode)
            {
                RevealContainer(containerNode);
            }
            else if (actionId.Equals("search", StringComparison.OrdinalIgnoreCase))
            {
                PerformSearchResponse(selectedNode);
            }
        }
        else
        {
            AppendConsoleFeed($"blocked :: {actionId}");
        }
    }

    private void ProcessCompletedOperations()
    {
        foreach (var operation in _infiltrationManager.State.ActiveOperations
                     .Where(op => op.Status == OperationStatus.Completed && !op.CompletionHandled)
                     .ToList())
        {
            var node = _dungeon.GetNode(operation.TargetNodePath);
            if (node == null)
            {
                operation.MarkCompletionHandled();
                continue;
            }

            AppendConsoleFeed($"op done :: {operation.Type} -> {node.Name}");

            if (operation.Type == OperationType.RewriteLog)
            {
                _infiltrationManager.ReduceTrace(8, $"Log rewritten at {node.Path}");
                _battleManager.AddLog($"Log rewritten: {node.Name}");
            }
            else if (operation.Type == OperationType.Move)
            {
                var targetContainerPath = operation.SecondaryTargetPath;
                if (!string.IsNullOrWhiteSpace(targetContainerPath) && _dungeon.MoveNode(node.Path, targetContainerPath))
                {
                    AppendConsoleFeed($"move done :: {node.Name} -> {targetContainerPath}");
                    _infiltrationManager.AddTrace(4, $"Moved {node.Name}");
                    ReloadCurrentContainer();
                }
            }
            else if (operation.Type == OperationType.Compress && !string.IsNullOrWhiteSpace(operation.SecondaryTargetPath))
            {
                if (_dungeon.MoveNode(node.Path, operation.SecondaryTargetPath))
                {
                    AppendConsoleFeed($"archive pack :: {node.Name} -> {operation.SecondaryTargetPath}");
                    _infiltrationManager.ReduceTrace(2, $"Archived {node.Name}");
                    ReloadCurrentContainer();
                }
            }
            else
            {
                ExecuteImmediateAction(operation.Type, node);
            }

            operation.MarkCompletionHandled();
        }
    }

    private void QueueMoveCursorCommand()
    {
        var selectedNode = GetSelectedNode();
        if (selectedNode == null)
        {
            AppendConsoleFeed("move blocked :: no target selected");
            return;
        }

        QueueOperationCommand(OperationType.MoveCursor, selectedNode.Path, null, $"MOVE CURSOR -> {selectedNode.Name}");
    }

    private void QueueOperationCommand(OperationType operationType, string primaryTargetPath, string? secondaryTargetPath, string summary)
    {
        _infiltrationManager.QueueCommand(new CommandQueueEntry
        {
            OperationType = operationType,
            PrimaryTargetPath = primaryTargetPath,
            SecondaryTargetPath = secondaryTargetPath,
            Summary = summary
        });
        AppendConsoleFeed($"queued :: {summary.ToLowerInvariant()}");
        UpdateUi();
    }

    private void TryQueueDragDropOperation(string sourcePath, string targetPath)
    {
        if (string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
            return;

        var sourceNode = _dungeon.GetNode(sourcePath);
        var targetNode = _dungeon.GetNode(targetPath);
        if (sourceNode == null || targetNode == null)
            return;

        if (targetNode is not ContainerNode targetContainer)
        {
            AppendConsoleFeed("drop blocked :: target must be folder or archive");
            return;
        }

        if (targetNode is ArchiveNode)
        {
            QueueOperationCommand(OperationType.Compress, sourceNode.Path, targetContainer.Path, $"COMPRESS {sourceNode.Name} -> {targetContainer.Name}");
            return;
        }

        QueueOperationCommand(OperationType.Move, sourceNode.Path, targetContainer.Path, $"MOVE {sourceNode.Name} -> {targetContainer.Name}");
    }

    private void PerformSearchResponse(NodeData selectedNode)
    {
        _infiltrationManager.AddTrace(7, $"Search query executed at {selectedNode.Path}");

        foreach (var agent in _infiltrationManager.SecurityAgents.Where(agent => agent.AgentType is SecurityAgentType.IndexerScout or SecurityAgentType.AiMonitor))
        {
            agent.IsAlerted = true;
            agent.AwarenessStage = SecurityAwarenessStage.ActiveScan;
        }

        var targetNode = _dungeon.GetNode(_currentMission.TargetPath);
        if (targetNode == null)
        {
            AppendConsoleFeed("search result :: no signature resolved");
            return;
        }

        var targetParentPath = _dungeon.GetParentContainer(targetNode.Path)?.Path;
        var sameContainer = string.Equals(targetParentPath, _dungeon.CurrentContainer.Path, StringComparison.OrdinalIgnoreCase);
        var subtreeHit = selectedNode is ContainerNode && targetNode.Path.StartsWith(selectedNode.Path, StringComparison.OrdinalIgnoreCase);
        var exactHit = string.Equals(selectedNode.Path, targetNode.Path, StringComparison.OrdinalIgnoreCase);
        var currentFolderHit = targetNode.Path.StartsWith(_dungeon.CurrentContainer.Path, StringComparison.OrdinalIgnoreCase);

        if (exactHit)
        {
            AppendConsoleFeed($"search hit :: exact signature match :: {targetNode.Name}");
            _selectedNodePath = targetNode.Path;
            return;
        }

        if (sameContainer)
        {
            AppendConsoleFeed($"search hit :: target signature nearby in current container :: {targetNode.Name}");
            _selectedNodePath = targetNode.Path;
            return;
        }

        if (subtreeHit)
        {
            AppendConsoleFeed($"search hint :: signature found deeper under {selectedNode.Name}");
            return;
        }

        if (currentFolderHit)
        {
            AppendConsoleFeed($"search hint :: current folder contains deeper signature traces");
            return;
        }

        AppendConsoleFeed("search result :: no relevant signature in current route");
    }

    private void TryExtractMission()
    {
        if (_battleManager.IsBattleEnd || _missionResolved)
            return;

        if (!_missionProgress.ObjectiveCompleted)
        {
            AppendConsoleFeed("extract blocked :: objective not completed");
            return;
        }

        if (!IsAtExtractionPoint())
        {
            AppendConsoleFeed("extract blocked :: return to root container first");
            return;
        }

        if (!_infiltrationManager.TryEscape())
        {
            AppendConsoleFeed("extract blocked :: exit still locked");
            return;
        }

        AppendConsoleFeed("extract success :: package delivered");
        _battleManager.FinishBattle("Extraction complete. Package delivered.");
        OnBattleEnd();
        UpdateUi();
    }

    private bool IsAtExtractionPoint()
    {
        return string.Equals(_dungeon.CurrentContainer.Path, _dungeon.Root.Path, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDeferredOperation(OperationType operationType)
    {
        return operationType is OperationType.Copy or OperationType.Compress or OperationType.RewriteLog;
    }

    private static int GetRequiredTicksForOperation(OperationType operationType, NodeData node)
    {
        return operationType switch
        {
            OperationType.Copy => node.Size > 512 ? 3 : 2,
            OperationType.Compress => 2,
            OperationType.RewriteLog => 2,
            OperationType.Move => 1,
            _ => 1
        };
    }

    private void ClearQueuedCommands()
    {
        _infiltrationManager.ClearQueue();
        AppendConsoleFeed("queue cleared");
        UpdateUi();
    }

    private void HandleNodeDefeated(NodeData node)
    {
        _dungeon.MarkCleared(node);
        AppendConsoleFeed($"node cleared :: {node.Name}");

        if (_selectedNodePath == node.Path)
        {
            _selectedNodePath = GetFirstVisibleNodePath();
        }

        if (_dungeon.CurrentContainer.Children.Count == 0 && _dungeon.EnterParentOfCurrent())
        {
            LoadContainer(_dungeon.CurrentContainer, reason: "Current container empty. Returning to parent.");
        }
        else
        {
            ReloadCurrentContainer();
        }
    }

    private void RevealContainer(ContainerNode container)
    {
        var revealMessage = container.CombatProfile.RevealSummary
            ?? (container is ArchiveNode ? "Archive opened; contents spilled out." : "Container opened; children exposed.");
        AppendConsoleFeed(revealMessage);
        LoadContainer(container, reason: revealMessage);
    }

    private void ReloadCurrentContainer()
    {
        LoadContainer(_dungeon.CurrentContainer, reason: $"Refreshing {_dungeon.CurrentContainer.Path}");
    }

    private void NavigateUp()
    {
        if (!_dungeon.EnterParentOfCurrent())
        {
            AppendConsoleFeed("Already at root.");
            return;
        }

        LoadContainer(_dungeon.CurrentContainer, reason: $"Moved up to {_dungeon.CurrentContainer.Path}");
        UpdateUi();
    }

    private void ApplyMissionFailureChecks()
    {
        if (_battleManager.IsBattleEnd || _missionResolved)
            return;

        if (_battleManager.TurnCount > _effectiveTurnLimit)
        {
            _battleManager.FinishBattle($"Trace level critical. Turn limit {_effectiveTurnLimit} exceeded.");
        }
    }

    private int CountRemainingThreats()
    {
        return _nodeActors
            .Where(pair => !_dungeon.IsCleared(pair.Key))
            .Count(pair => pair.Value.IsAlive);
    }

    private void OnFolderTreeSelected()
    {
        var item = _folderTree.GetSelected();
        if (item == null)
            return;

        var path = item.GetMetadata(0).AsString();
        if (_dungeon.GetNode(path) is not ContainerNode container)
            return;

        LoadContainer(container, reason: $"Navigated to {path}");
        UpdateUi();
    }

    private void OnFileTreeSelected()
    {
        var item = _fileTree.GetSelected();
        if (item == null)
            return;

        _selectedNodePath = item.GetMetadata(0).AsString();
        var selectedNode = GetSelectedNode();
        if (selectedNode != null)
        {
            AppendConsoleFeed($"target locked :: {selectedNode.Name} :: {selectedNode.Path}");
        }

        UpdateUi();
    }

    private void OnFileTreeActivated()
    {
        var node = GetSelectedNode();
        if (node is ContainerNode)
        {
            QueueSelectedCommand("open");
        }
    }

    private void OnExplorerFieldSelected(long index)
    {
        var metadata = _explorerFieldList.GetItemMetadata((int)index);
        _selectedNodePath = metadata.AsString();
        var selectedNode = GetSelectedNode();
        if (selectedNode != null)
        {
            AppendConsoleFeed($"target locked :: {selectedNode.Name} :: {selectedNode.Path}");
        }

        UpdateUi();
    }

    private void OnExplorerFieldActivated(long index)
    {
        OnExplorerFieldSelected(index);
        var node = GetSelectedNode();
        if (node is ContainerNode)
        {
            QueueSelectedCommand("open");
        }
    }

    private void OnExplorerFieldGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            var itemIndex = _explorerFieldList.GetItemAtPosition(mouseButton.Position, true);

            if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Right)
            {
                if (itemIndex < 0)
                    return;

                _explorerFieldList.Select(itemIndex);
                var metadata = _explorerFieldList.GetItemMetadata(itemIndex);
                _selectedNodePath = metadata.AsString();
                ShowExplorerContextMenu();
                UpdateUi();
                return;
            }

            if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (itemIndex >= 0)
                {
                    _dragSourceNodePath = _explorerFieldList.GetItemMetadata(itemIndex).AsString();
                    _dragHoverTargetPath = null;
                }
                return;
            }

            if (!mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (!string.IsNullOrWhiteSpace(_dragSourceNodePath) && itemIndex >= 0)
                {
                    var dropPath = _explorerFieldList.GetItemMetadata(itemIndex).AsString();
                    TryQueueDragDropOperation(_dragSourceNodePath, dropPath);
                }

                _dragSourceNodePath = null;
                _dragHoverTargetPath = null;
                UpdateUi();
                return;
            }
        }

        if (@event is InputEventMouseMotion mouseMotion && !string.IsNullOrWhiteSpace(_dragSourceNodePath))
        {
            var itemIndex = _explorerFieldList.GetItemAtPosition(mouseMotion.Position, true);
            _dragHoverTargetPath = itemIndex >= 0
                ? _explorerFieldList.GetItemMetadata(itemIndex).AsString()
                : null;
            UpdateUi();
        }
    }

    private void ShowExplorerContextMenu()
    {
        var node = GetSelectedNode();
        if (node == null)
            return;

        _contextActionMap.Clear();
        _explorerContextMenu.Clear();

        long menuId = 1;
        _explorerContextMenu.AddItem("Move Cursor", (int)menuId);
        _contextActionMap[menuId++] = "__move_cursor__";

        foreach (var actionId in BuildContextActionIds(node))
        {
            var title = actionId switch
            {
                "open" => "Open",
                "inspect" => "Properties",
                "search" => "Search",
                "copy" => "Copy",
                "delete" => "Delete",
                "compress" => "Compress",
                "logforge" => "Rewrite Log",
                _ => actionId.ToUpperInvariant()
            };
            _explorerContextMenu.AddItem(title, (int)menuId);
            _contextActionMap[menuId++] = actionId;
        }

        _explorerContextMenu.Position = DisplayServer.MouseGetPosition();
        _explorerContextMenu.Popup();
    }

    private IEnumerable<string> BuildContextActionIds(NodeData node)
    {
        var actor = _nodeActors.GetValueOrDefault(node.Path);
        var previewContext = new ActionContext(_battleManager.Player)
        {
            Target = actor,
            TargetNode = node,
            Clipboard = _battleManager.Clipboard,
            StatusEffects = _battleManager.StatusEffects,
            AllActors = _battleManager.Enemies
        };

        var candidates = new List<string>();
        if (node is ContainerNode)
        {
            candidates.Add("open");
        }

        candidates.AddRange(new[] { "inspect", "search", "copy", "compress", "logforge", "delete" });

        return candidates
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(actionId =>
            {
                var action = _actionRegistry.GetAction(actionId);
                return action != null && action.CanExecute(previewContext);
            });
    }

    private void OnExplorerContextMenuPressed(long id)
    {
        if (!_contextActionMap.TryGetValue(id, out var actionId))
            return;

        if (actionId == "__move_cursor__")
        {
            QueueMoveCursorCommand();
            return;
        }

        QueueSelectedCommand(actionId);
    }

    private void UpdateUi()
    {
        var metadata = _dungeon.GetCurrentMetadata();
        _pathLabel.Text = $"Path: {_dungeon.CurrentContainer.Path}";
        _missionLabel.Text = $"Mission: {_currentMission.Title} · Objective: {_currentMission.ObjectiveType} {_currentMission.TargetPath}\nTheme: {metadata.ThemeName} · {metadata.EventSummary}";
        _turnStateLabel.Text = $"Turn {_battleManager.TurnCount}/{_effectiveTurnLimit} · State {_battleManager.CurrentState} · {_dungeon.GetProgressLabel()}";
        _traceBar.MaxValue = 100;
        _traceBar.Value = Math.Min(100, CampaignState.Heat * 15 + _battleManager.TurnCount * 5);
        _traceLabel.Text = $"TRACE {CampaignState.Heat} · {_campaignModifiers.Summary}";
        _playerStatusLabel.Text = $"Operator: HP {_battleManager.Player.CurrentHp}/{_battleManager.Player.MaxHp} · AP {_battleManager.Player.CurrentAp}/{_battleManager.Player.MaxAp}\nStatus: {FormatStatusEffects(_battleManager.StatusEffects.GetEffects(_battleManager.Player.Id))} · {BuildWindowSummary()}";
        _explorerStateLabel.Text = BuildExplorerStateSummary();
        _cursorStatusLabel.Text = BuildCursorStatusSummary();
        _fieldSecurityLabel.Text = BuildFieldSecuritySummary();
        if (!string.IsNullOrWhiteSpace(_dragSourceNodePath))
        {
            _consoleHintLabel.Text = $"Dragging :: {_dragSourceNodePath} -> {(_dragHoverTargetPath ?? "(drop target 없음)")}";
        }

        RebuildFolderTree();
        RebuildFileTree();
        RebuildExplorerField();
        UpdateInspector();
        UpdateActionButtons();
        UpdateOperationLog();
        UpdateCommandQueueUi();
        UpdateClipboardWindowUi();
        UpdateConsoleHint();
        UpdateBattleEndOverlay();
    }

    private void RebuildFolderTree()
    {
        _folderTree.Clear();
        var rootItem = _folderTree.CreateItem();
        BuildFolderTreeItem(rootItem, _dungeon.Root);
    }

    private void BuildFolderTreeItem(TreeItem parent, FolderNode folder)
    {
        if (folder != _dungeon.Root && _dungeon.IsCleared(folder.Path))
            return;

        var item = _folderTree.CreateItem(parent);
        item.SetText(0, folder.Name);
        item.SetMetadata(0, folder.Path);
        item.SetIcon(0, ResolveNodeIcon(folder));
        item.SetTooltipText(0, folder.Path);
        item.Collapsed = false;

        if (string.Equals(folder.Path, _dungeon.CurrentContainer.Path, StringComparison.OrdinalIgnoreCase))
        {
            item.SetCustomColor(0, new Color(0.54f, 0.98f, 0.78f));
            item.Select(0);
        }

        foreach (var childFolder in folder.Children.OfType<FolderNode>())
        {
            BuildFolderTreeItem(item, childFolder);
        }
    }

    private void RebuildFileTree()
    {
        _fileTree.Clear();
        var rootItem = _fileTree.CreateItem();
        var visibleNodes = _dungeon.GetCurrentEncounterNodes().ToList();
        if (visibleNodes.Count == 0)
        {
            var emptyItem = _fileTree.CreateItem(rootItem);
            emptyItem.SetText(0, "(empty)");
            return;
        }

        TreeItem? selectedItem = null;
        foreach (var node in visibleNodes)
        {
            var item = _fileTree.CreateItem(rootItem);
            var agents = GetAgentsForNode(node.Path);
            var monitored = _infiltrationManager.IsNodeMonitored(node.Path);
            item.SetMetadata(0, node.Path);
            item.SetText(0, BuildDisplayName(node, agents, monitored));
            item.SetIcon(0, ResolveNodeIcon(node));
            item.SetText(1, node.UiTypeName);
            item.SetText(2, BuildSecuritySummary(agents, monitored));
            item.SetText(3, node.CombatProfile.ThreatLabel);
            item.SetText(4, $"{BuildNodeStatus(node, agents, monitored)} · {FormatSize(node.Size)}");
            item.SetTooltipText(0, BuildNodeTooltip(node, agents, monitored));
            item.SetCustomColor(0, monitored ? new Color(1.0f, 0.84f, 0.52f) : GetThreatColor(node));
            item.SetCustomColor(2, GetSecurityColor(agents, monitored));
            item.SetCustomColor(3, GetThreatColor(node));
            item.SetCustomColor(4, GetStatusColor(node, agents, monitored));

            if (agents.Count > 0)
            {
                item.SetIcon(2, ResolveAgentIcon(agents[0].AgentType));
            }

            if (string.Equals(_selectedNodePath, node.Path, StringComparison.OrdinalIgnoreCase))
            {
                selectedItem = item;
            }
        }

        if (selectedItem != null)
        {
            selectedItem.Select(0);
        }
        else if (visibleNodes.Count > 0)
        {
            _selectedNodePath = visibleNodes[0].Path;
        }
    }

    private void RebuildExplorerField()
    {
        _explorerFieldList.Clear();
        var visibleNodes = _dungeon.GetCurrentEncounterNodes().ToList();
        for (var i = 0; i < visibleNodes.Count; i++)
        {
            var node = visibleNodes[i];
            var agents = GetAgentsForNode(node.Path);
            var monitored = _infiltrationManager.IsNodeMonitored(node.Path);
            var icon = ResolveNodeIcon(node);
            var isCursorHere = string.Equals(_infiltrationManager.State.CursorAgent.CurrentNodePath, node.Path, StringComparison.OrdinalIgnoreCase);
            var primaryLine = BuildDisplayName(node, agents, monitored);
            if (isCursorHere)
            {
                primaryLine = $"> {primaryLine}";
            }

            var secondaryLine = agents.Count > 0
                ? $"SEC: {agents[0].DisplayName}{(monitored ? " · WATCH" : string.Empty)}"
                : monitored
                    ? $"{node.UiTypeName} · WATCH"
                    : node.UiTypeName;

            var label = $"{primaryLine}\n{secondaryLine}";
            var itemIndex = _explorerFieldList.AddItem(label, icon);
            _explorerFieldList.SetItemMetadata(itemIndex, node.Path);
            _explorerFieldList.SetItemTooltip(itemIndex, BuildNodeTooltip(node, agents, monitored));
            _explorerFieldList.SetItemCustomFgColor(itemIndex, isCursorHere
                ? new Color(0.94f, 0.98f, 0.72f, 1f)
                : monitored
                    ? new Color(1.0f, 0.84f, 0.52f, 1f)
                    : GetThreatColor(node));

            if (string.Equals(_selectedNodePath, node.Path, StringComparison.OrdinalIgnoreCase))
            {
                _explorerFieldList.Select(itemIndex);
                _explorerFieldList.EnsureCurrentIsVisible();
            }
        }

        if (visibleNodes.Count > 0 && string.IsNullOrWhiteSpace(_selectedNodePath))
        {
            _selectedNodePath = visibleNodes[0].Path;
            _explorerFieldList.Select(0);
        }
    }

    private string BuildExplorerStateSummary()
    {
        var nodeCount = _dungeon.GetCurrentEncounterNodes().Count;
        var activeOps = _infiltrationManager.State.ActiveOperations.Count(op => op.Status == OperationStatus.Running);
        return $"Objects {nodeCount} · Queue {_infiltrationManager.State.CommandQueue.Count} · Active Ops {activeOps} · {BuildWindowSummary()}";
    }

    private string BuildWindowSummary()
    {
        var openWindows = _infiltrationManager.State.Windows.Where(window => window.IsOpen).ToList();
        if (openWindows.Count == 0)
            return "Windows none";

        var focused = openWindows.FirstOrDefault(window => window.IsFocused);
        var focusedName = focused?.WindowType.ToString() ?? "none";
        return $"Windows {openWindows.Count} · Focus {focusedName}";
    }

    private string BuildCursorStatusSummary()
    {
        var cursor = _infiltrationManager.State.CursorAgent;
        return $"Cursor Agent :: {cursor.CurrentNodePath} :: AP {cursor.ActionPoints}/{cursor.MaxActionPoints} :: Clipboard {_infiltrationManager.State.Clipboard.Count}/{cursor.ClipboardCapacity}";
    }

    private string BuildFieldSecuritySummary()
    {
        var agents = _infiltrationManager.GetVisibleSecurityAgents(_dungeon.CurrentContainer.Path)
            .Concat(string.IsNullOrWhiteSpace(_selectedNodePath)
                ? Enumerable.Empty<SecurityAgent>()
                : _infiltrationManager.GetMonitoringAgents(_selectedNodePath))
            .DistinctBy(agent => agent.Id)
            .ToList();

        if (agents.Count == 0)
        {
            return $"Security :: {_infiltrationManager.State.AlertStage} :: no visible patrol";
        }

        var summary = string.Join(" / ", agents.Select(agent => $"{agent.DisplayName} [{agent.AwarenessStage}]"));
        return $"Security :: {_infiltrationManager.State.AlertStage} :: {summary}";
    }

    private void UpdateInspector()
    {
        var node = GetSelectedNode();
        if (node == null)
        {
            _inspectorNameLabel.Text = "No target selected";
            _inspectorTypeLabel.Text = "Type: -";
            _inspectorPathLabel.Text = "Path: -";
            _inspectorStatsLabel.Text = "Stats: -";
            _inspectorStatusLabel.Text = "Status: -";
            _inspectorHintLabel.Text = "Choose a file or folder from the explorer pane.";
            _securityAgentsLabel.Text = "Security: none";
            _targetHpBar.MaxValue = 1;
            _targetHpBar.Value = 0;
            _targetApBar.MaxValue = 1;
            _targetApBar.Value = 0;
            return;
        }

        var actor = _nodeActors.GetValueOrDefault(node.Path);
        var agents = GetAgentsForNode(node.Path);
        var monitored = _infiltrationManager.IsNodeMonitored(node.Path);
        IReadOnlyList<StatusEffectInstance> effects = actor != null
            ? _battleManager.StatusEffects.GetEffects(actor.Id)
            : Array.Empty<StatusEffectInstance>();
        _inspectorNameLabel.Text = node.Name;
        _inspectorTypeLabel.Text = $"Type: {node.UiTypeName}";
        _inspectorPathLabel.Text = $"Path: {node.Path}";
        _inspectorStatsLabel.Text = actor == null
            ? "Stats: offline"
            : $"Stats: HP {actor.CurrentHp}/{actor.MaxHp} · AP {actor.CurrentAp}/{actor.MaxAp} · ATK {actor.AttackPower}";
        _inspectorStatusLabel.Text = $"Threat: {node.CombatProfile.ThreatLabel} · Watch: {(monitored ? "Monitored" : "Low")} · Status: {FormatStatusEffects(effects)}";
        _inspectorHintLabel.Text = node is ContainerNode container && container.Children.Count > 0
            ? node.CombatProfile.RevealSummary ?? "Open to reveal nested nodes."
            : "Select a command from the deck below.";
        _securityAgentsLabel.Text = agents.Count == 0
            ? (monitored ? "Security: monitored by nearby patrol" : "Security: none")
            : $"Security: {string.Join(", ", agents.Select(agent => $"{agent.DisplayName} ({agent.AgentType})"))}";

        _targetHpBar.MaxValue = actor?.MaxHp ?? 1;
        _targetHpBar.Value = actor?.CurrentHp ?? 0;
        _targetApBar.MaxValue = actor?.MaxAp ?? 1;
        _targetApBar.Value = actor?.CurrentAp ?? 0;
    }

    private void UpdateActionButtons()
    {
        var node = GetSelectedNode();
        var actor = node != null ? _nodeActors.GetValueOrDefault(node.Path) : null;

        foreach (var pair in _actionButtons)
        {
            var action = _actionRegistry.GetAction(pair.Key);
            if (action == null)
                continue;

            var previewContext = new ActionContext(_battleManager.Player)
            {
                Target = actor,
                TargetNode = node,
                Clipboard = _battleManager.Clipboard,
                StatusEffects = _battleManager.StatusEffects,
                AllActors = _battleManager.Enemies
            };

            var canExecute = node != null
                && actor != null
                && actor.IsAlive
                && _battleManager.CurrentState == BattleState.PlayerTurn
                && action.CanExecute(previewContext);

            pair.Value.Disabled = !canExecute;
            pair.Value.Modulate = canExecute
                ? GetThreatColor(node)
                : new Color(0.55f, 0.58f, 0.64f, 0.9f);
        }
    }

    private void UpdateOperationLog()
    {
        _operationLogLabel.Clear();
        foreach (var log in _battleManager.BattleLog.TakeLast(BattleConstants.UIBattleLogDisplayLines))
        {
            _operationLogLabel.AppendText($"[color=#c9d1d9]{log}[/color]\n");
        }

        foreach (var operation in _infiltrationManager.State.ActiveOperations.TakeLast(4))
        {
            var percent = (int)(operation.Progress * 100f);
            _operationLogLabel.AppendText($"[color=#7ee787]op :: {operation.Type} :: {percent}% :: {operation.Status}[/color]\n");
        }

        if (_operationLogLabel.GetLineCount() > 0)
        {
            _operationLogLabel.ScrollToLine(_operationLogLabel.GetLineCount() - 1);
        }
    }

    private void UpdateCommandQueueUi()
    {
        _commandQueueLabel.Clear();
        var queue = _infiltrationManager.State.CommandQueue;
        if (queue.Count == 0)
        {
            _commandQueueLabel.AppendText("[color=#8b949e](empty)[/color]\n");
        }
        else
        {
            foreach (var entry in queue.OrderBy(entry => entry.Order))
            {
                _commandQueueLabel.AppendText($"[color=#c9d1d9]{entry.Order}. {entry.Summary}[/color]\n");
            }
        }

        _moveCursorButton.Disabled = _battleManager.IsBattleEnd || GetSelectedNode() == null;
        _clipboardWindowButton.Disabled = _battleManager.IsBattleEnd;
        _executeQueueButton.Disabled = queue.Count == 0 || _battleManager.IsBattleEnd;
        _extractButton.Disabled = _battleManager.IsBattleEnd || !_missionProgress.ObjectiveCompleted || !IsAtExtractionPoint();
        _clearQueueButton.Disabled = queue.Count == 0;
    }

    private void UpdateClipboardWindowUi()
    {
        var clipboardWindow = _infiltrationManager.State.Windows
            .FirstOrDefault(window => window.WindowType == ExplorerWindowType.Clipboard);

        _clipboardWindowButton.Text = clipboardWindow?.IsOpen == true ? "Clipboard *" : "Clipboard";
        _clipboardWindow.Visible = clipboardWindow?.IsOpen == true;
        _clipboardWindowItemsLabel.Clear();
        _clipboardWindowStatusLabel.Text = "Clipboard window offline";

        if (clipboardWindow == null || !clipboardWindow.IsOpen)
        {
            return;
        }

        _clipboardWindowStatusLabel.Text = $"Focus: {(clipboardWindow.IsFocused ? "Active" : "Standby")} · Bound: {clipboardWindow.BoundPath}";

        if (_infiltrationManager.State.Clipboard.Count == 0)
        {
            _clipboardWindowItemsLabel.AppendText("[color=#8b949e](clipboard empty)[/color]\n");
            return;
        }

        foreach (var item in _infiltrationManager.State.Clipboard)
        {
            var ghostTag = item.IsGhosted ? " · ghost" : string.Empty;
            _clipboardWindowItemsLabel.AppendText($"[color=#c9d1d9]- {item.NodeKind} :: {item.NodePath}{ghostTag}[/color]\n");
        }
    }

    private void ToggleClipboardWindow()
    {
        var existing = _infiltrationManager.State.Windows
            .FirstOrDefault(window => window.WindowType == ExplorerWindowType.Clipboard);

        if (existing?.IsOpen == true)
        {
            _infiltrationManager.FocusWindow(existing.WindowId);
            AppendConsoleFeed("window focus :: clipboard");
            UpdateUi();
            return;
        }

        _infiltrationManager.OpenWindow(
            ExplorerWindowType.Clipboard,
            "Clipboard Window",
            _dungeon.CurrentContainer.Path,
            traceModifier: 1);
        AppendConsoleFeed("window open :: clipboard");
        UpdateUi();
    }

    private void CloseClipboardWindow()
    {
        var existing = _infiltrationManager.State.Windows
            .FirstOrDefault(window => window.WindowType == ExplorerWindowType.Clipboard && window.IsOpen);
        if (existing == null)
            return;

        _infiltrationManager.CloseWindow(existing.WindowId);
        AppendConsoleFeed("window close :: clipboard");
        UpdateUi();
    }

    private void UpdateConsoleHint()
    {
        var node = GetSelectedNode();
        if (!string.IsNullOrWhiteSpace(_dragSourceNodePath))
        {
            return;
        }

        _consoleHintLabel.Text = node == null
            ? "Select node -> queue command / drag to folder/archive"
            : $"Select node -> queue command / drag :: {node.Name} :: AP {_battleManager.Player.CurrentAp}/{_battleManager.Player.MaxAp} :: objective {(_missionProgress.ObjectiveCompleted ? "done" : "pending")}";
    }

    private void UpdateBattleEndOverlay()
    {
        _battleEndOverlay.Visible = _battleManager.IsBattleEnd;
        if (!_battleManager.IsBattleEnd)
            return;

        var result = _missionResult;
        var didWin = result?.Success ?? _battleManager.IsPlayerAlive;
        _battleEndTitleLabel.Text = didWin ? "Mission Complete" : "Mission Failed";
        _battleEndSummaryLabel.Text = result?.Summary ?? (didWin ? "Extraction completed." : "Operation failed.");
        _battleEndStatsLabel.Text = $"Actions {_executedPlayerActions.Count} · Unique {_executedPlayerActions.Distinct().Count()} · Cleared {_dungeon.ClearedNodeCount}/{_dungeon.TotalNodeCount}"
            + (result != null ? $"\nCredits {result.CreditsDelta:+#;-#;0} · Rep {result.ReputationDelta:+#;-#;0} · Heat {result.HeatDelta:+#;-#;0}" : string.Empty);
    }

    private void OnBattleEnd()
    {
        if (!_missionResolved)
        {
            var extracted = _infiltrationManager.State.RunStatus == RunStatus.Escaped;
            _missionResult = _missionProgress.Resolve(_battleManager.IsPlayerAlive, extracted, _battleManager.TurnCount, _effectiveTurnLimit);
            CampaignState.ApplyMissionResult(_missionResult);
            _battleManager.AddLog(_missionResult.Success
                ? $"Mission complete: {_missionResult.Summary}"
                : $"Mission failed: {_missionResult.Summary}");
            _battleManager.AddLog($"Payout: {_missionResult.CreditsDelta:+#;-#;0}c / Rep {_missionResult.ReputationDelta:+#;-#;0} / Heat {_missionResult.HeatDelta:+#;-#;0}");
            AppendConsoleFeed(_missionResult.Success
                ? $"mission-complete :: {_missionResult.Summary}"
                : $"mission-failed :: {_missionResult.Summary}");
            _missionResolved = true;
        }
    }

    private NodeData? GetSelectedNode()
    {
        if (string.IsNullOrWhiteSpace(_selectedNodePath))
            return null;

        return _dungeon.GetNode(_selectedNodePath);
    }

    private string? GetFirstVisibleNodePath()
    {
        return _dungeon.GetCurrentEncounterNodes().FirstOrDefault()?.Path;
    }

    private string BuildVisibleNodeReport()
    {
        var nodes = _dungeon.GetCurrentEncounterNodes();
        if (nodes.Count == 0)
            return "scan :: empty container";

        return string.Join("\n", nodes.Select((node, index) =>
        {
            var agents = GetAgentsForNode(node.Path);
            var agentText = agents.Count == 0 ? "unguarded" : string.Join(", ", agents.Select(agent => agent.DisplayName));
            return $"[{index + 1}] {node.Name} :: {node.UiTypeName} :: {BuildNodeStatus(node, agents)} :: {node.CombatProfile.ThreatLabel} :: {agentText}";
        }));
    }

    private string BuildStatusSummary()
    {
        var selected = GetSelectedNode();
        return $"status :: path {_dungeon.CurrentContainer.Path} :: turn {_battleManager.TurnCount}/{_effectiveTurnLimit} :: remaining {CountRemainingThreats()} :: selected {(selected?.Name ?? "none")} :: {BuildWindowSummary()}";
    }

    private string BuildNodeStatus(NodeData node, IReadOnlyList<SecurityAgent>? agents = null, bool monitored = false)
    {
        if (!_nodeActors.TryGetValue(node.Path, out var actor) || !actor.IsAlive)
            return "cleared";

        if (node is ContainerNode container && container.Children.Count > 0)
            return (agents?.Count ?? 0) > 0 || monitored ? "guarded sealed" : "sealed";

        if ((agents?.Count ?? 0) > 0 || monitored)
            return actor.CurrentHp < actor.MaxHp ? "guarded active" : "guarded";

        return actor.CurrentHp < actor.MaxHp ? "active" : "idle";
    }

    private List<SecurityAgent> GetAgentsForNode(string nodePath)
    {
        return _infiltrationManager.SecurityAgents
            .Where(agent => string.Equals(agent.CurrentNodePath, nodePath, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private Texture2D ResolveNodeIcon(NodeData node)
    {
        var key = node.UiTypeName.ToLowerInvariant();
        if (_nodeIcons.TryGetValue(key, out var icon))
            return icon;

        if (node is FolderNode && _nodeIcons.TryGetValue("folder", out var folderIcon))
            return folderIcon;

        return _nodeIcons["default"];
    }

    private Texture2D? ResolveAgentIcon(SecurityAgentType agentType)
    {
        return _agentIcons.GetValueOrDefault(agentType);
    }

    private static string BuildDisplayName(NodeData node, IReadOnlyList<SecurityAgent> agents, bool monitored = false)
    {
        return agents.Count == 0 && !monitored
            ? node.Name
            : $"[Guarded] {node.Name}";
    }

    private static string BuildSecuritySummary(IReadOnlyList<SecurityAgent> agents, bool monitored = false)
    {
        if (agents.Count == 0)
            return monitored ? "Nearby Watch" : "None";

        if (agents.Count == 1)
            return agents[0].DisplayName;

        return $"{agents[0].DisplayName} +{agents.Count - 1}";
    }

    private string BuildNodeTooltip(NodeData node, IReadOnlyList<SecurityAgent> agents, bool monitored = false)
    {
        var lines = new List<string>
        {
            node.Path,
            $"Type: {node.UiTypeName}",
            $"Threat: {node.CombatProfile.ThreatLabel}",
            $"Watch: {(monitored ? "Monitored" : "Low")}"
        };

        if (agents.Count > 0)
        {
            lines.Add($"Security: {string.Join(", ", agents.Select(agent => agent.DisplayName))}");
        }
        else if (monitored)
        {
            lines.Add("Security: nearby patrol coverage");
        }

        return string.Join("\n", lines);
    }

    private static string FormatSize(long size)
    {
        return size <= 0 ? "-" : $"{size} kb";
    }

    private static OperationType MapActionIdToOperationType(string actionId)
    {
        return actionId.ToLowerInvariant() switch
        {
            "open" => OperationType.Access,
            "copy" => OperationType.Copy,
            "paste" => OperationType.Paste,
            "move" => OperationType.Move,
            "delete" => OperationType.Delete,
            "compress" => OperationType.Compress,
            "extract" => OperationType.ExtractArchive,
            "inspect" => OperationType.Properties,
            "search" => OperationType.Search,
            "sort" => OperationType.Sort,
            "hide" => OperationType.ShowHidden,
            "logforge" => OperationType.RewriteLog,
            "inject" => OperationType.Inject,
            "stun" => OperationType.Stun,
            "decoy" => OperationType.Decoy,
            "override" => OperationType.PermissionOverride,
            _ => OperationType.Access
        };
    }

    private static string? MapOperationTypeToActionId(OperationType operationType)
    {
        return operationType switch
        {
            OperationType.MoveCursor => null,
            OperationType.Access => "open",
            OperationType.Copy => "copy",
            OperationType.Paste => "paste",
            OperationType.Move => "move",
            OperationType.Delete => "delete",
            OperationType.Compress => "compress",
            OperationType.ExtractArchive => "extract",
            OperationType.Properties => "inspect",
            OperationType.RewriteLog => "logforge",
            OperationType.Inject => "inject",
            OperationType.Stun => "stun",
            OperationType.Decoy => "decoy",
            OperationType.PermissionOverride => "override",
            _ => null
        };
    }

    private static Color GetThreatColor(NodeData? node)
    {
        return node?.CombatProfile.ThreatLevel switch
        {
            NodeThreatLevel.Low => new Color(0.70f, 0.88f, 0.72f),
            NodeThreatLevel.Medium => new Color(0.94f, 0.83f, 0.47f),
            NodeThreatLevel.High => new Color(1.0f, 0.64f, 0.36f),
            NodeThreatLevel.Critical => new Color(1.0f, 0.42f, 0.42f),
            _ => new Color(0.80f, 0.86f, 0.92f)
        };
    }

    private static Color GetSecurityColor(IReadOnlyList<SecurityAgent> agents, bool monitored = false)
    {
        if (agents.Count == 0)
            return monitored ? new Color(0.96f, 0.80f, 0.48f) : new Color(0.72f, 0.78f, 0.84f);

        return agents.Any(agent => agent.AgentType == SecurityAgentType.AntivirusHeavy || agent.AgentType == SecurityAgentType.FirewallSentinel)
            ? new Color(1.0f, 0.55f, 0.50f)
            : new Color(0.49f, 0.76f, 1.0f);
    }

    private static Color GetStatusColor(NodeData node, IReadOnlyList<SecurityAgent> agents, bool monitored = false)
    {
        if (agents.Count > 0 || monitored)
            return new Color(0.96f, 0.89f, 0.56f);

        return node is ContainerNode
            ? new Color(0.76f, 0.84f, 0.96f)
            : new Color(0.74f, 0.78f, 0.84f);
    }

    private static string FormatStatusEffects(IReadOnlyList<StatusEffectInstance> effects)
    {
        if (effects.Count == 0)
            return "None";

        return string.Join(", ", effects.Select(effect => $"{effect.Type} {effect.Duration}T"));
    }

    private void AppendConsoleFeed(string text)
    {
        _consoleFeedLabel.AppendText($"[color=#7ee787]{text}[/color]\n");
        if (_consoleFeedLabel.GetLineCount() > 0)
        {
            _consoleFeedLabel.ScrollToLine(_consoleFeedLabel.GetLineCount() - 1);
        }
    }

    private void ClearConsoleFeed()
    {
        _consoleFeedLabel.Clear();
        AppendConsoleFeed("Console buffer cleared.");
    }

    private void ShowConsoleHelp()
    {
        AppendConsoleFeed("mouse-console :: click folder/file, then click command deck");
        AppendConsoleFeed("utility :: SCAN lists visible nodes / STATUS shows turn state / UP returns to parent");
        AppendConsoleFeed("open on folders or archives reveals nested children. Boss type is data-driven, not zip-locked.");
    }

    private void RestartBattle()
    {
        GetTree().ReloadCurrentScene();
    }

    private void BackToLobby()
    {
        GetTree().ChangeSceneToFile("res://res/scenes/main.tscn");
    }

    private async void RunSmokeTestIfRequested()
    {
        if (!HasAutomationArg("--projectfr-smoke-test"))
            return;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _selectedNodePath = BattleConstants.RootBuildCachePath;
        QueueSelectedCommand("inspect");
        QueueSelectedCommand("open");
        ExecuteQueuedCommands();
        _selectedNodePath = BattleConstants.CacheAssetsPath;
        QueueSelectedCommand("open");
        ExecuteQueuedCommands();
    }

    private static bool HasAutomationArg(string arg)
    {
        return OS.GetCmdlineUserArgs().Contains(arg);
    }
}
