using Godot;
using ProjectFR.Action;
using ProjectFR.Battle;
using ProjectFR.Core;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;
using ProjectFR.Infiltration;
using ProjectFR.Mission;
using ProjectFR.Skills;
using ProjectFR.Systems;

namespace ProjectFR.Scenes;

public partial class BattleScene : Control
{
    private const int ExplorerFieldColumnWidth = 128;
    private const int ExplorerFieldIconSize = 48;
    private const int ActionButtonWidth = 132;
    private const int ActionButtonHeight = 48;

    // TODO(battle-removal): BattleManager surface still load-bearing — port before deleting:
    //   HP display + StatusEffects (conditions + inspector), Enemies (ActionContext.AllActors),
    //   Clipboard (ClipboardSystem ↔ InfiltrationState.Clipboard), BattleLog (UI),
    //   FinishBattle/LoadEncounter (core game loop).
    //   Decoupled so far: action-button gate (RunStatus), turn-label (RunStatus),
    //   player AP (CursorAgent.ActionPoints gates all operations and drives ActionContext AP checks).
    private BattleManager _battleManager = null!;
    private BattleDungeon _dungeon = null!;
    private InfiltrationManager _infiltrationManager = null!;
    private ActionRegistry _actionRegistry = null!;
    private SkillCatalog _skillCatalog = null!;
    private SkillExecutor _skillExecutor = null!;
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
    private RichTextLabel _securitySectionLabel = null!;
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
    private Button _tempWindowButton = null!;
    private Button _moveCursorButton = null!;
    private Button _executeQueueButton = null!;
    private Button _extractButton = null!;
    private Button _clearQueueButton = null!;
    private PopupMenu _explorerContextMenu = null!;
    private Control _clipboardWindow = null!;
    private Button _clipboardWindowCloseButton = null!;
    private Label _clipboardWindowStatusLabel = null!;
    private RichTextLabel _clipboardWindowItemsLabel = null!;
    private Button _storeInPouchButton = null!;
    private Button _restoreFromPouchButton = null!;
    private Control _tempWindow = null!;
    private Button _tempWindowCloseButton = null!;
    private Label _tempWindowStatusLabel = null!;
    private ItemList _tempWindowItemList = null!;
    private Button _logWindowButton = null!;
    private Control _logWindow = null!;
    private Button _logWindowCloseButton = null!;
    private Label _logWindowStatusLabel = null!;
    private ItemList _logWindowItemList = null!;
    private Button _logWindowForgeButton = null!;
    private int _selectedLogEntryIndex = -1;

    private readonly Dictionary<long, string> _contextActionMap = new();

    private readonly Dictionary<string, Texture2D> _nodeIcons = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Texture2D> _badgeIcons = new(StringComparer.OrdinalIgnoreCase);
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
        DebugLog.Initialize();
        DebugLog.Info(nameof(BattleScene), "_Ready enter");
        try
        {
            InitializeUi();
            InitializeBattle();
            DebugLog.Info(nameof(BattleScene), $"battle initialized :: mission={_currentMission.Id} :: turnLimit={_effectiveTurnLimit}");
            UpdateUi();
            RunSmokeTestIfRequested();
            DebugLog.Info(nameof(BattleScene), "_Ready exit");
        }
        catch (Exception exception)
        {
            DebugLog.Exception(nameof(BattleScene), exception, "_Ready failed");
            throw;
        }
    }

    private void InitializeUi()
    {
        CacheUiReferences();
        LoadPlaceholderAssets();
        ConfigureTrees();
        BindUiEvents();
        CreateActionButtons();
        InitializeUiState();
    }

    private void CacheUiReferences()
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
        _securitySectionLabel = GetNode<RichTextLabel>("RootMargin/MainVBox/MainSplit/InspectorPanel/InspectorMargin/InspectorVBox/SecuritySectionLabel");
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
        _tempWindowButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/TempWindowButton");
        _moveCursorButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/MoveCursorButton");
        _executeQueueButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/ExecuteQueueButton");
        _extractButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/ExtractButton");
        _clearQueueButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/ClearQueueButton");
        _explorerContextMenu = GetNode<PopupMenu>("ExplorerContextMenu");
        _clipboardWindow = GetNode<Control>("ClipboardWindow");
        _clipboardWindowCloseButton = GetNode<Button>("ClipboardWindow/ClipboardWindowMargin/ClipboardWindowVBox/ClipboardWindowHeaderRow/ClipboardWindowCloseButton");
        _clipboardWindowStatusLabel = GetNode<Label>("ClipboardWindow/ClipboardWindowMargin/ClipboardWindowVBox/ClipboardWindowStatusLabel");
        _clipboardWindowItemsLabel = GetNode<RichTextLabel>("ClipboardWindow/ClipboardWindowMargin/ClipboardWindowVBox/ClipboardWindowItemsLabel");
        _storeInPouchButton = GetNode<Button>("ClipboardWindow/ClipboardWindowMargin/ClipboardWindowVBox/ClipboardWindowActionRow/StoreInPouchButton");
        _restoreFromPouchButton = GetNode<Button>("ClipboardWindow/ClipboardWindowMargin/ClipboardWindowVBox/ClipboardWindowActionRow/RestoreFromPouchButton");
        _tempWindow = GetNode<Control>("TempWindow");
        _tempWindowCloseButton = GetNode<Button>("TempWindow/TempWindowMargin/TempWindowVBox/TempWindowHeaderRow/TempWindowCloseButton");
        _tempWindowStatusLabel = GetNode<Label>("TempWindow/TempWindowMargin/TempWindowVBox/TempWindowStatusLabel");
        _tempWindowItemList = GetNode<ItemList>("TempWindow/TempWindowMargin/TempWindowVBox/TempWindowItemList");
        _logWindowButton = GetNode<Button>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandQueuePanel/CommandQueueMargin/CommandQueueVBox/CommandQueueButtonRow/LogWindowButton");
        _logWindow = GetNode<Control>("LogWindow");
        _logWindowCloseButton = GetNode<Button>("LogWindow/LogWindowMargin/LogWindowVBox/LogWindowHeaderRow/LogWindowCloseButton");
        _logWindowStatusLabel = GetNode<Label>("LogWindow/LogWindowMargin/LogWindowVBox/LogWindowStatusLabel");
        _logWindowItemList = GetNode<ItemList>("LogWindow/LogWindowMargin/LogWindowVBox/LogWindowItemList");
        _logWindowForgeButton = GetNode<Button>("LogWindow/LogWindowMargin/LogWindowVBox/LogWindowActionRow/LogWindowForgeButton");
    }

    private void BindUiEvents()
    {
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
        _tempWindowButton.Pressed += ToggleTempWindow;
        _moveCursorButton.Pressed += QueueMoveCursorCommand;
        _executeQueueButton.Pressed += ExecuteQueuedCommands;
        _extractButton.Pressed += TryExtractMission;
        _clearQueueButton.Pressed += ClearQueuedCommands;
        _explorerContextMenu.IdPressed += OnExplorerContextMenuPressed;
        _clipboardWindowCloseButton.Pressed += CloseClipboardWindow;
        _tempWindowCloseButton.Pressed += CloseTempWindow;
        _tempWindowItemList.ItemSelected += OnTempWindowItemSelected;
        _storeInPouchButton.Pressed += StoreSelectedClipboardEntryInPouch;
        _restoreFromPouchButton.Pressed += RestoreSelectedPouchEntryToClipboard;
        _logWindowButton.Pressed += ToggleLogWindow;
        _logWindowCloseButton.Pressed += CloseLogWindow;
        _logWindowItemList.ItemSelected += OnLogWindowItemSelected;
        _logWindowForgeButton.Pressed += QueueLogForgeFromLogWindow;
    }

    private void InitializeUiState()
    {
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

        _badgeIcons["watch"] = GD.Load<Texture2D>("res://res/assets/ui/badge_watch.svg");
        _badgeIcons["lock"] = GD.Load<Texture2D>("res://res/assets/ui/badge_lock.svg");
        _badgeIcons["override"] = GD.Load<Texture2D>("res://res/assets/ui/badge_override.svg");
        _badgeIcons["tracked"] = GD.Load<Texture2D>("res://res/assets/ui/badge_tracked.svg");
        _badgeIcons["pressure"] = GD.Load<Texture2D>("res://res/assets/ui/badge_pressure.svg");

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
        _explorerFieldList.FixedColumnWidth = ExplorerFieldColumnWidth;
        _explorerFieldList.FixedIconSize = new Vector2I(ExplorerFieldIconSize, ExplorerFieldIconSize);
        _explorerFieldList.MaxTextLines = 2;
        _explorerFieldList.SameColumnWidth = true;
    }

    private void CreateActionButtons()
    {
        _actionRegistry = new ActionRegistry();
        _skillCatalog = SkillCatalog.CreateDefault(_actionRegistry);
        _skillExecutor = new SkillExecutor(_skillCatalog);
        var container = GetNode<GridContainer>("RootMargin/MainVBox/ActionPanel/ActionMargin/ActionVBox/CommandDeckGrid");
        foreach (var action in _actionRegistry.GetAllActions())
        {
            var definition = _skillCatalog.Get(action.ActionId);
            var button = new Button
            {
                Text = $"> {(definition?.DisplayName ?? action.ActionId).ToUpperInvariant()}\nAP {action.ApCost}",
                CustomMinimumSize = new Vector2(ActionButtonWidth, ActionButtonHeight),
                TooltipText = definition?.Description ?? ActionMetadata.GetTooltipText(action.ActionId)
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
        _effectiveTurnLimit = Math.Max(InfiltrationTuning.MinimumTurnLimit, _currentMission.TurnLimit - _campaignModifiers.HeatTurnPenalty);
        _missionProgress = new MissionProgress(_currentMission);
        _missionResult = null;
        _missionResolved = false;

        _dungeon = BattleFactory.CreateDefaultDungeon(_currentMission);
        BuildActorIndex(_dungeon.Root);

        _infiltrationManager = new InfiltrationManager(_currentMission);
        _infiltrationManager.Initialize(_dungeon.Root.Path, EnumerateKnownNodes(_dungeon.Root));
        SeedSecurityAgents();

        _battleManager = new BattleManager(BattleFactory.CreateDefaultPlayer())
        {
            EndBattleWhenEnemiesCleared = false
        };
        _battleManager.StartBattle();
        AddOperationLog($"Mission accepted: {_currentMission.Title} / Client: {_currentMission.Client.Name}");
        AddOperationLog($"Faction: {_currentMission.Client.Faction} / { _campaignModifiers.Summary }");

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
        var targetPath = _currentMission.TargetPath;
        var agents = new List<SecurityAgent>
        {
            new(SecurityAgentType.IndexerScout, "Indexer Scout",
                BattleConstants.RootReadmePath,
                new[] { BattleConstants.RootReadmePath, BattleConstants.RootBuildCachePath }),
            new(SecurityAgentType.GuardScanner, "Guard Scanner",
                BattleConstants.RootBuildCachePath,
                new[] { BattleConstants.RootBuildCachePath, BattleConstants.CacheTempPath }),
            new(SecurityAgentType.FirewallSentinel, "Firewall Sentinel",
                BattleConstants.CacheAssetsPath,
                new[] { BattleConstants.CacheAssetsPath }),
            new(SecurityAgentType.AntivirusHeavy, "Antivirus Heavy",
                BattleConstants.BossZipPath,
                new[] { BattleConstants.BossZipPath }),
            new(SecurityAgentType.BackupRepairer, "Backup Repairer",
                BattleConstants.SystemLogPath,
                new[] { BattleConstants.SystemLogPath, BattleConstants.RootBuildCachePath }),
            new(SecurityAgentType.AiMonitor, "AI Monitor",
                BattleConstants.BossZipPath,
                new[] { BattleConstants.CacheTempPath, BattleConstants.BossZipPath })
        };
        EnsureMissionTargetPatrolled(agents, targetPath);
        foreach (var agent in agents)
            _infiltrationManager.AddSecurityAgent(agent);
    }

    private static void EnsureMissionTargetPatrolled(List<SecurityAgent> agents, string targetPath)
    {
        if (string.IsNullOrWhiteSpace(targetPath))
            return;
        if (agents.Any(a => a.PatrolRoute.Contains(targetPath, StringComparer.OrdinalIgnoreCase)))
            return;

        SecurityAgent? bestFit = null;
        var bestPrefixLength = 0;
        foreach (var agent in agents)
        {
            foreach (var routePath in agent.PatrolRoute)
            {
                var trimmed = routePath.TrimEnd('/');
                if (targetPath.StartsWith(trimmed + '/', StringComparison.OrdinalIgnoreCase)
                    && trimmed.Length > bestPrefixLength)
                {
                    bestFit = agent;
                    bestPrefixLength = trimmed.Length;
                }
            }
        }
        bestFit?.PatrolRoute.Add(targetPath);
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
        AddOperationLog($"Theme: {metadata.ThemeName}");
        AddOperationLog($"Event: {metadata.EventSummary}");
        AddOperationLog($"Objective: {_currentMission.ObjectiveType} {_currentMission.TargetPath} before turn {_effectiveTurnLimit}");

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
        if (_infiltrationManager.State.RunStatus != RunStatus.Active)
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

        var cursor = _infiltrationManager.State.CursorAgent;
        var previewContext = new ActionContext(cursor.ActionPoints, _infiltrationManager.State.Clipboard.Count)
        {
            Target = targetActor,
            TargetNode = selectedNode,
            StatusEffects = _battleManager.StatusEffects,
            AllActors = _battleManager.Enemies
        };

        if (actionId.Equals(ActionIds.Open, StringComparison.OrdinalIgnoreCase)
            && selectedNode is ContainerNode
            && _infiltrationManager.IsPermissionLocked(selectedNode.Path))
        {
            AppendConsoleFeed($"blocked :: open :: permission locked :: {selectedNode.Name}");
            return;
        }

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
            if (!ExecuteQueuedCommandEntry(entry))
            {
                continue;
            }

            _infiltrationManager.AdvanceTurn();
            ApplyBackupRepairerRestores();
            ApplyDetectionDamage();
            ProcessCompletedOperations();
            ApplyMissionFailureChecks();
            if (_infiltrationManager.State.RunStatus != RunStatus.Active)
                break;
            var remaining = _effectiveTurnLimit - _infiltrationManager.State.TurnCount;
            if (remaining <= 3 && remaining >= 0)
                AppendConsoleFeed($"⚠ {remaining}턴 남음 :: 추적도 임계 접근");
        }

        // Drain any in-flight deferred operations that need extra ticks to finish.
        // Without this, a Copy (2 ticks) queued alone would stay Running and never sync to clipboard.
        const int MaxDrainTurns = 3;
        for (var drainPass = 0; drainPass < MaxDrainTurns && _infiltrationManager.State.RunStatus == RunStatus.Active; drainPass++)
        {
            if (!_infiltrationManager.State.ActiveOperations.Any(op => op.Status == OperationStatus.Running && !op.CompletionHandled))
                break;
            _infiltrationManager.AdvanceTurn();
            ApplyBackupRepairerRestores();
            ApplyDetectionDamage();
            ProcessCompletedOperations();
            ApplyMissionFailureChecks();
            if (_infiltrationManager.State.RunStatus == RunStatus.Active)
            {
                var drainRemaining = _effectiveTurnLimit - _infiltrationManager.State.TurnCount;
                if (drainRemaining <= 3 && drainRemaining >= 0)
                    AppendConsoleFeed($"⚠ {drainRemaining}턴 남음 :: 추적도 임계 접근");
            }
        }

        _infiltrationManager.ClearQueue();
        ApplyMissionFailureChecks();
        if (_infiltrationManager.State.RunStatus != RunStatus.Active)
        {
            OnBattleEnd();
        }

        UpdateUi();
    }

    private bool ExecuteQueuedCommandEntry(CommandQueueEntry entry)
    {
        var selectedNode = _dungeon.GetNode(entry.PrimaryTargetPath);
        if (selectedNode == null)
        {
            AppendConsoleFeed($"missing target :: {entry.PrimaryTargetPath}");
            return false;
        }

        var actionIdForAp = MapOperationTypeToActionId(entry.OperationType);
        var apCost = actionIdForAp != null ? (_actionRegistry.GetAction(actionIdForAp)?.ApCost ?? 0) : 0;
        var cursor = _infiltrationManager.State.CursorAgent;
        if (apCost > 0 && cursor.ActionPoints < apCost)
        {
            AppendConsoleFeed($"ap insufficient :: {entry.OperationType} :: {apCost}AP needed :: {cursor.ActionPoints}AP remaining");
            return false;
        }

        if (!IsDeferredOperation(entry.OperationType)
            && !CanExecuteImmediateOperation(entry.OperationType, selectedNode))
        {
            return false;
        }

        _selectedNodePath = selectedNode.Path;
        var requiredTicks = GetRequiredTicksForOperation(entry.OperationType, selectedNode);
        var operation = new FileOperation(entry.OperationType, selectedNode.Path, requiredTicks, entry.SecondaryTargetPath)
        {
            NodeKind = ResolveExplorerNodeKind(selectedNode),
            NodeSize = selectedNode.Size
        };
        _infiltrationManager.StartOperation(operation);

        if (entry.OperationType == OperationType.MoveCursor)
        {
            AppendConsoleFeed($"cursor route :: {selectedNode.Name}");
            return true;
        }

        if (entry.OperationType == OperationType.Move)
        {
            AppendConsoleFeed($"move route :: {selectedNode.Name}");
            return true;
        }

        if (!IsDeferredOperation(entry.OperationType))
        {
            ExecuteImmediateAction(entry.OperationType, selectedNode);
            operation.MarkCompletionHandled();
        }
        else
        {
            cursor.ActionPoints = Math.Max(0, cursor.ActionPoints - apCost);
            AppendConsoleFeed($"op start :: {entry.OperationType} -> {selectedNode.Name} :: {requiredTicks}T");
        }

        return true;
    }

    private bool CanExecuteImmediateOperation(OperationType operationType, NodeData selectedNode)
    {
        var actionId = MapOperationTypeToActionId(operationType);
        if (actionId == null)
        {
            AppendConsoleFeed($"unsupported op :: {operationType}");
            return false;
        }

        if (!_nodeActors.TryGetValue(selectedNode.Path, out var targetActor) || !targetActor.IsAlive)
        {
            AppendConsoleFeed($"Target offline :: {selectedNode.Name}");
            return false;
        }

        var action = _actionRegistry.GetAction(actionId);
        if (action == null)
        {
            AppendConsoleFeed($"Unknown action :: {actionId}");
            return false;
        }

        if (actionId.Equals(ActionIds.Open, StringComparison.OrdinalIgnoreCase)
            && selectedNode is ContainerNode
            && _infiltrationManager.IsPermissionLocked(selectedNode.Path))
        {
            AppendConsoleFeed($"blocked :: open :: permission locked :: {selectedNode.Name}");
            return false;
        }

        var canExecCursor = _infiltrationManager.State.CursorAgent;
        var previewContext = new ActionContext(canExecCursor.ActionPoints, _infiltrationManager.State.Clipboard.Count)
        {
            Target = targetActor,
            TargetNode = selectedNode,
            StatusEffects = _battleManager.StatusEffects,
            AllActors = _battleManager.Enemies
        };

        if (!action.CanExecute(previewContext))
        {
            AppendConsoleFeed($"blocked :: {actionId}");
            return false;
        }

        return true;
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

        var execCursor = _infiltrationManager.State.CursorAgent;
        var context = new ActionContext(execCursor.ActionPoints, _infiltrationManager.State.Clipboard.Count)
        {
            Target = targetActor,
            TargetNode = selectedNode,
            Clipboard = _battleManager.Clipboard,
            StatusEffects = _battleManager.StatusEffects,
            AllActors = _battleManager.Enemies,
            ConsumeApCallback = ap => execCursor.ActionPoints = Math.Max(0, execCursor.ActionPoints - ap)
        };

        if (actionId.Equals(ActionIds.Open, StringComparison.OrdinalIgnoreCase)
            && selectedNode is ContainerNode
            && _infiltrationManager.IsPermissionLocked(selectedNode.Path))
        {
            AppendConsoleFeed($"blocked :: open :: permission locked :: {selectedNode.Name}");
            return;
        }

        var result = action.Execute(context);
        AddOperationLog($"Player: {result.Message}");
        if (result.Success)
        {
            SyncCursorApToPlayer();
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
            else if (actionId.Equals(ActionIds.Open, StringComparison.OrdinalIgnoreCase) && selectedNode is ContainerNode containerNode)
            {
                RevealContainer(containerNode);
            }
            else if (!TryExecuteSkillBehavior(actionId, selectedNode, context))
            {
                // no skill behavior registered; keep default action result only
            }

            if (actionId.Equals(ActionIds.Clean, StringComparison.OrdinalIgnoreCase)
                && _infiltrationManager.TryClearDetection($"Clean action on {selectedNode.Name}"))
            {
                AppendConsoleFeed($"detection cleared :: clean action :: {selectedNode.Name}");
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
                _executedPlayerActions.Add(ActionIds.LogForge);
                AddOperationLog($"Log rewritten: {node.Name}");
                foreach (var note in operation.CompletionNotes)
                    AppendConsoleFeed($"{note} :: {node.Name}");

                var logForgeMissionUpdate = _missionProgress.RegisterAction(ActionIds.LogForge, node.Path);
                if (!string.IsNullOrWhiteSpace(logForgeMissionUpdate))
                {
                    AddOperationLog(logForgeMissionUpdate);
                    AppendConsoleFeed(logForgeMissionUpdate);
                }
                if (_missionProgress.ObjectiveCompleted && !_infiltrationManager.State.ExitUnlocked)
                {
                    _infiltrationManager.UnlockExit($"Objective secured: {_currentMission.TargetPath}");
                    AppendConsoleFeed("objective complete :: extraction unlocked :: return to root and extract");
                }
            }
            else if (operation.Type == OperationType.Move)
            {
                var targetContainerPath = operation.SecondaryTargetPath;
                if (!string.IsNullOrWhiteSpace(targetContainerPath) && _dungeon.MoveNode(node.Path, targetContainerPath))
                {
                    AppendConsoleFeed($"move done :: {node.Name} -> {targetContainerPath}");
                    _infiltrationManager.AddTrace(InfiltrationTuning.MoveTraceIncrease, $"Moved {node.Name}");
                    ReloadCurrentContainer();
                }
            }
            else if (operation.Type == OperationType.Compress && !string.IsNullOrWhiteSpace(operation.SecondaryTargetPath))
            {
                if (_dungeon.MoveNode(node.Path, operation.SecondaryTargetPath))
                {
                    AppendConsoleFeed($"archive pack :: {node.Name} -> {operation.SecondaryTargetPath}");
                    _infiltrationManager.ReduceTrace(InfiltrationTuning.ArchiveTraceReduction, $"Archived {node.Name}");
                    ReloadCurrentContainer();
                }
            }
            else if (operation.Type == OperationType.Copy)
            {
                var copyBlocked = operation.CompletionNotes.Any(n => n.StartsWith("copy blocked"));
                if (copyBlocked)
                {
                    AppendConsoleFeed($"copy blocked :: clipboard full :: {node.Name}");
                    operation.MarkCompletionHandled();
                    continue;
                }

                _executedPlayerActions.Add(ActionIds.Copy);
                AppendConsoleFeed($"copy complete :: {node.Name}");

                var copyMissionUpdate = _missionProgress.RegisterAction(ActionIds.Copy, node.Path);
                if (!string.IsNullOrWhiteSpace(copyMissionUpdate))
                {
                    AddOperationLog(copyMissionUpdate);
                    AppendConsoleFeed(copyMissionUpdate);
                }
                if (_missionProgress.ObjectiveCompleted && !_infiltrationManager.State.ExitUnlocked)
                {
                    _infiltrationManager.UnlockExit($"Objective secured: {_currentMission.TargetPath}");
                    AppendConsoleFeed("objective complete :: extraction unlocked :: return to root and extract");
                }
            }
            else if (operation.Type == OperationType.Cut)
            {
                ExecuteImmediateAction(operation.Type, node);
                foreach (var note in operation.CompletionNotes)
                    AppendConsoleFeed($"{note} :: {node.Name}");
                if (operation.CompletionNotes.Count == 0)
                    AppendConsoleFeed($"cut complete :: {node.Name}");
            }
            else if (operation.Type == OperationType.Paste)
            {
                ExecuteImmediateAction(operation.Type, node);
                _battleManager.Clipboard.Clear();
                foreach (var note in operation.CompletionNotes)
                    AppendConsoleFeed($"{note} :: {node.Name}");
                if (operation.CompletionNotes.Count == 0)
                    AppendConsoleFeed($"paste complete :: {node.Name}");
            }
            else if (operation.Type == OperationType.MoveCursor)
            {
                // Cursor movement was already handled in InfiltrationManager.OnOperationCompleted.
            }
            else if (operation.Type == OperationType.Stun)
            {
                var agentsAtTarget = _infiltrationManager.SecurityAgents
                    .Where(a => string.Equals(a.CurrentNodePath, operation.TargetNodePath, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var agent in agentsAtTarget)
                    agent.DisabledTurns = InfiltrationTuning.StunDurationTurns;
                AppendConsoleFeed(agentsAtTarget.Count > 0
                    ? $"stun applied :: {node.Name} :: {agentsAtTarget.Count} agent(s) disabled {InfiltrationTuning.StunDurationTurns}T"
                    : $"stun no target :: {node.Name}");
            }
            else
            {
                var fallbackActionId = MapOperationTypeToActionId(operation.Type);
                if (fallbackActionId == null)
                    AppendConsoleFeed($"op unhandled :: {operation.Type} :: {node.Name}");
                else
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
        _infiltrationManager.AddTrace(InfiltrationTuning.SearchTraceIncrease, $"Search query executed at {selectedNode.Path}");
        _infiltrationManager.TriggerSearchSweep(selectedNode.Path);

        var targetNode = _dungeon.GetNode(_currentMission.TargetPath);
        if (targetNode == null)
        {
            AppendConsoleFeed("search result :: no signature resolved");
            return;
        }

        if (_infiltrationManager.IsNodeHiddenInPouch(targetNode.Path) && !_infiltrationManager.IsPouchMaskingBroken(targetNode.Path))
        {
            AppendConsoleFeed($"search result :: pouch-cached signature blurred :: {targetNode.Name}");
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

    private void RevealPouchMask(NodeData selectedNode, string traceReason, int traceIncrease, string label)
    {
        if (!_infiltrationManager.IsNodeHiddenInPouch(selectedNode.Path))
        {
            AppendConsoleFeed($"{label} :: no pouch mask on {selectedNode.Name}");
            return;
        }

        if (_infiltrationManager.IsPouchMaskingBroken(selectedNode.Path))
        {
            AppendConsoleFeed($"{label} :: pouch mask already broken :: {selectedNode.Name}");
            return;
        }

        _infiltrationManager.ExposePouchHiddenNode(selectedNode.Path, traceReason, traceIncrease);
        AppendConsoleFeed($"{label} :: pouch mask broken :: {selectedNode.Name}");
    }

    private void TryExtractMission()
    {
        if (_infiltrationManager.State.RunStatus != RunStatus.Active || _missionResolved)
            return;

        var escapeOnlyObjective = _currentMission.ObjectiveType == MissionObjectiveType.Escape;
        if (!escapeOnlyObjective && !_missionProgress.ObjectiveCompleted)
        {
            AppendConsoleFeed("extract blocked :: objective not completed");
            return;
        }

        if (!IsAtExtractionPoint())
        {
            AppendConsoleFeed("extract blocked :: return to root container first");
            return;
        }

        if (escapeOnlyObjective && !_infiltrationManager.State.ExitUnlocked)
            _infiltrationManager.UnlockExit("Escape objective: player reached root");

        if (!_infiltrationManager.TryEscape())
        {
            AppendConsoleFeed("extract blocked :: exit still locked");
            return;
        }

        var missionUpdate = escapeOnlyObjective
            ? _missionProgress.RegisterEscape(_dungeon.CurrentContainer.Path)
            : null;
        if (!string.IsNullOrWhiteSpace(missionUpdate))
        {
            _battleManager.AddLog(missionUpdate);
            AppendConsoleFeed(missionUpdate);
        }

        AppendConsoleFeed("extract success :: package delivered");
        OnBattleEnd();
        UpdateUi();
    }

    private bool IsAtExtractionPoint()
    {
        return string.Equals(_dungeon.CurrentContainer.Path, _dungeon.Root.Path, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDeferredOperation(OperationType operationType)
    {
        return operationType is OperationType.Copy or OperationType.Compress or OperationType.RewriteLog
            or OperationType.Cut or OperationType.Paste or OperationType.Stun;
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

    private static ExplorerNodeKind ResolveExplorerNodeKind(NodeData node)
    {
        return node switch
        {
            FolderNode => ExplorerNodeKind.Folder,
            ArchiveNode => ExplorerNodeKind.Archive,
            _ when node.UiTypeName.Contains("log", StringComparison.OrdinalIgnoreCase) => ExplorerNodeKind.Log,
            _ when node.UiTypeName.Contains("temp", StringComparison.OrdinalIgnoreCase) => ExplorerNodeKind.Temp,
            _ when node.UiTypeName.Contains("encrypt", StringComparison.OrdinalIgnoreCase) => ExplorerNodeKind.Encrypted,
            _ when node.UiTypeName.Contains("shortcut", StringComparison.OrdinalIgnoreCase) => ExplorerNodeKind.Shortcut,
            _ when node.UiTypeName.Contains("exec", StringComparison.OrdinalIgnoreCase) => ExplorerNodeKind.Executable,
            _ => ExplorerNodeKind.File
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

    private void ApplyBackupRepairerRestores()
    {
        foreach (var agent in _infiltrationManager.SecurityAgents
            .Where(a => a.AgentType == SecurityAgentType.BackupRepairer && a.DisabledTurns <= 0))
        {
            var path = agent.CurrentNodePath;
            if (!_dungeon.IsCleared(path))
                continue;

            if (!_dungeon.RestoreNode(path))
                continue;

            if (_nodeActors.TryGetValue(path, out var actor))
                actor.Heal(actor.MaxHp);

            _infiltrationManager.AddTrace(InfiltrationTuning.BackupRepairTraceIncrease, $"Backup Repairer restored node at {path}");
            AppendConsoleFeed($"node restored :: BackupRepairer :: {_dungeon.GetNode(path)?.Name ?? path}");
        }
    }

    private void ApplyDetectionDamage()
    {
        var damage = _infiltrationManager.State.LastTurnContactDamage;
        if (damage <= 0)
            return;

        var cursorPath = _infiltrationManager.State.CursorAgent.CurrentNodePath;
        var threateningAgents = _infiltrationManager.SecurityAgents
            .Where(a => a.AgentType is SecurityAgentType.GuardScanner or SecurityAgentType.AntivirusHeavy
                && string.Equals(a.CurrentNodePath, cursorPath, StringComparison.OrdinalIgnoreCase))
            .ToList();
        AppendConsoleFeed($"contact dmg :: {string.Join(", ", threateningAgents.Select(a => a.AgentType))} :: -{damage} HP");
        AddOperationLog($"Operator took {damage} contact damage from detected agents at {cursorPath}");
    }

    private void ApplyMissionFailureChecks()
    {
        if (_infiltrationManager.State.RunStatus != RunStatus.Active || _missionResolved)
            return;

        if (!_infiltrationManager.State.IsOperatorAlive)
        {
            _infiltrationManager.SetRunFailed("Operator eliminated. Mission failed.");
            return;
        }

        if (_infiltrationManager.State.TurnCount >= _effectiveTurnLimit)
        {
            _infiltrationManager.SetRunTimedOut();
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
            QueueSelectedCommand(ActionIds.Open);
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
            QueueSelectedCommand(ActionIds.Open);
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

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left }
            && !string.IsNullOrWhiteSpace(_dragSourceNodePath))
        {
            _dragSourceNodePath = null;
            _dragHoverTargetPath = null;
            UpdateUi();
            return;
        }

        if (@event is InputEventKey { Pressed: true, Echo: false } keyEvent && _infiltrationManager.State.RunStatus == RunStatus.Active)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Enter or Key.KpEnter:
                    if (_infiltrationManager.State.CommandQueue.Count > 0)
                        ExecuteQueuedCommands();
                    break;
                case Key.Delete:
                    if (GetSelectedNode() != null)
                        QueueSelectedCommand(ActionIds.Delete);
                    break;
                case Key.Backspace:
                    NavigateUp();
                    break;
                case Key.F5:
                    ReloadCurrentContainer();
                    UpdateUi();
                    break;
            }
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
            var title = GetContextActionTitle(actionId);
            _explorerContextMenu.AddItem(title, (int)menuId);
            _contextActionMap[menuId++] = actionId;
        }

        _explorerContextMenu.Position = DisplayServer.MouseGetPosition();
        _explorerContextMenu.Popup();
    }

    private IEnumerable<string> BuildContextActionIds(NodeData node)
    {
        var actor = _nodeActors.GetValueOrDefault(node.Path);
        var ctxCursor = _infiltrationManager.State.CursorAgent;
        var previewContext = new ActionContext(ctxCursor.ActionPoints, _infiltrationManager.State.Clipboard.Count)
        {
            Target = actor,
            TargetNode = node,
            StatusEffects = _battleManager.StatusEffects,
            AllActors = _battleManager.Enemies
        };

        var candidates = new List<string>();
        if (node is ContainerNode)
        {
            candidates.Add(ActionIds.Open);
        }

        candidates.AddRange(new[]
        {
            ActionIds.Inspect,
            ActionIds.Search,
            ActionIds.ShowHidden,
            ActionIds.PermissionOverride,
            ActionIds.Copy,
            ActionIds.Cut,
            ActionIds.Paste,
            ActionIds.Compress,
            ActionIds.LogForge,
            ActionIds.Stun,
            ActionIds.Delete
        });

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

    private string GetContextActionTitle(string actionId)
    {
        var definition = _skillCatalog.Get(actionId);
        if (!string.IsNullOrWhiteSpace(definition?.DisplayName))
            return definition.DisplayName;

        return actionId switch
        {
            ActionIds.Open => "Open",
            ActionIds.Inspect => "Properties",
            ActionIds.Search => "Search",
            ActionIds.ShowHidden => "Show Hidden",
            ActionIds.PermissionOverride => "Permission Override",
            ActionIds.Copy => "Copy",
            ActionIds.Cut => "Cut",
            ActionIds.Paste => "Paste",
            ActionIds.Delete => "Delete",
            ActionIds.Compress => "Compress",
            ActionIds.LogForge => "Rewrite Log",
            ActionIds.Stun => "Stun Agent",
            _ => actionId.ToUpperInvariant()
        };
    }

    private bool TryExecuteSkillBehavior(string actionId, NodeData selectedNode, ActionContext actionContext)
    {
        var definition = _skillExecutor.GetDefinition(actionId);
        if (definition == null)
            return false;

        return _skillExecutor.TryExecutePostActionBehavior(actionId, new SkillExecutionContext
        {
            Definition = definition,
            ActionContext = actionContext,
            TargetNode = selectedNode,
            PerformSearchResponse = PerformSearchResponse,
            RevealPouchMask = RevealPouchMask,
            IsPermissionLocked = path => _infiltrationManager.IsPermissionLocked(path),
            GrantPermissionOverride = (path, reason, traceIncrease, durationTurns) => _infiltrationManager.GrantPermissionOverride(path, reason, traceIncrease, durationTurns),
            AppendConsoleFeed = AppendConsoleFeed
        });
    }

    private void UpdateUi()
    {
        var metadata = _dungeon.GetCurrentMetadata();
        _pathLabel.Text = $"Path: {_dungeon.CurrentContainer.Path}";
        _missionLabel.Text = $"Mission: {_currentMission.Title} 쨌 Objective: {_currentMission.ObjectiveType} {_currentMission.TargetPath}\nTheme: {metadata.ThemeName} 쨌 {metadata.EventSummary}";
        _turnStateLabel.Text = $"Turn {_infiltrationManager.State.TurnCount}/{_effectiveTurnLimit} 쨌 {_infiltrationManager.State.RunStatus} 쨌 {_dungeon.GetProgressLabel()}";
        var approachingTurnLimit = _infiltrationManager.State.TurnCount >= _effectiveTurnLimit - 2;
        _turnStateLabel.Modulate = approachingTurnLimit ? new Color(1.0f, 0.35f, 0.35f, 1.0f) : Colors.White;
        _traceBar.MaxValue = _infiltrationManager.State.MaxTrace;
        _traceBar.Value = _infiltrationManager.State.Trace;
        _traceBar.Modulate = approachingTurnLimit ? new Color(1.0f, 0.35f, 0.35f, 1.0f) : Colors.White;
        _traceLabel.Text = $"TRACE {_infiltrationManager.State.Trace}/{_infiltrationManager.State.MaxTrace} 쨌 Alert {_infiltrationManager.State.AlertStage} 쨌 Heat {CampaignState.Heat} 쨌 {_campaignModifiers.Summary}";
        var cursorAgent = _infiltrationManager.State.CursorAgent;
        _playerStatusLabel.Text = $"Operator: HP {_infiltrationManager.State.OperatorHp}/{_infiltrationManager.State.OperatorMaxHp} 쨌 AP {cursorAgent.ActionPoints}/{cursorAgent.MaxActionPoints}\nStatus: {FormatStatusEffects(_battleManager.StatusEffects.GetEffects(_battleManager.Player.Id))} 쨌 {BuildWindowSummary()}";
        _explorerStateLabel.Text = BuildExplorerStateSummary();
        _cursorStatusLabel.Text = BuildCursorStatusSummary();
        _fieldSecurityLabel.Text = BuildFieldSecurityStatusLine();
        if (!string.IsNullOrWhiteSpace(_dragSourceNodePath))
        {
            _consoleHintLabel.Text = $"Dragging :: {_dragSourceNodePath} -> {(_dragHoverTargetPath ?? "(no drop target)")}";
        }

        RebuildFolderTree();
        RebuildFileTree();
        RebuildExplorerField();
        UpdateInspector();
        UpdateSecuritySection();
        UpdateActionButtons();
        UpdateOperationLog();
        UpdateCommandQueueUi();
        UpdateClipboardWindowUi();
        UpdateTempWindowUi();
        UpdateLogWindowUi();
        UpdateConsoleHint();
        UpdateBattleEndOverlay();
    }

    private void RebuildFolderTree()
    {
        _folderTree.Clear();
        var rootItem = _folderTree.CreateItem();
        BuildFolderTreeItem(rootItem, _dungeon.Root);
    }

    private void BuildFolderTreeItem(TreeItem parent, ContainerNode folder)
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

        foreach (var childContainer in folder.Children.OfType<ContainerNode>())
        {
            BuildFolderTreeItem(item, childContainer);
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
            item.SetText(4, $"{BuildNodeStatus(node, agents, monitored)} 쨌 {FormatSize(node.Size)}");
            item.SetTooltipText(0, BuildNodeTooltip(node, agents, monitored));
            item.SetCustomColor(0, monitored ? new Color(1.0f, 0.84f, 0.52f) : GetThreatColor(node));
            item.SetCustomColor(2, GetSecurityColor(agents, monitored));
            item.SetCustomColor(3, GetThreatColor(node));
            item.SetCustomColor(4, GetStatusColor(node, agents, monitored));

            if (agents.Count > 0)
            {
                item.SetIcon(2, ResolveAgentIcon(agents[0].AgentType));
            }

            var badgeIcon = ResolvePrimaryBadgeIcon(node.Path, monitored || agents.Count > 0);
            if (badgeIcon != null)
            {
                item.SetIcon(4, badgeIcon);
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
            rootItem.GetFirstChild()?.Select(0);
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
            if (agents.Count > 0)
            {
                primaryLine += " " + string.Join(" ", agents.Select(a => GetAgentTypeBadge(a.AgentType)));
            }

            if (isCursorHere)
            {
                primaryLine = $"> {primaryLine}";
            }

            var secondaryLine = agents.Count > 0
                ? $"SEC: {agents[0].DisplayName}{(monitored ? " 쨌 WATCH" : string.Empty)}"
                : monitored
                    ? $"{node.UiTypeName} 쨌 WATCH"
                    : node.UiTypeName;

            var badgeSummary = BuildCompactBadgeSummary(node.Path, monitored || agents.Count > 0);
            if (!string.IsNullOrWhiteSpace(badgeSummary))
            {
                secondaryLine = $"{secondaryLine} 쨌 {badgeSummary}";
            }

            var label = $"{primaryLine}\n{secondaryLine}";
            var itemIndex = _explorerFieldList.AddItem(label, icon);
            _explorerFieldList.SetItemMetadata(itemIndex, node.Path);
            _explorerFieldList.SetItemTooltip(itemIndex, BuildNodeTooltip(node, agents, monitored));
            _explorerFieldList.SetItemCustomFgColor(itemIndex, isCursorHere
                ? new Color(0.94f, 0.98f, 0.72f, 1f)
                : monitored
                    ? new Color(1.0f, 0.84f, 0.52f, 1f)
                    : GetThreatColor(node));

            var isDragHover = !string.IsNullOrWhiteSpace(_dragHoverTargetPath)
                && string.Equals(_dragHoverTargetPath, node.Path, StringComparison.OrdinalIgnoreCase);
            if (isDragHover)
            {
                _explorerFieldList.SetItemCustomBgColor(itemIndex, new Color(0.25f, 0.55f, 0.95f, 0.35f));
            }

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
        return $"Objects {nodeCount} 쨌 Queue {_infiltrationManager.State.CommandQueue.Count} 쨌 Active Ops {activeOps} 쨌 {BuildWindowSummary()}";
    }

    private string BuildWindowSummary()
    {
        var openWindows = _infiltrationManager.State.Windows.Where(window => window.IsOpen).ToList();
        if (openWindows.Count == 0)
            return "Windows none";

        var focused = openWindows.FirstOrDefault(window => window.IsFocused);
        var focusedName = focused?.WindowType.ToString() ?? "none";
        return $"Windows {openWindows.Count} 쨌 Focus {focusedName}";
    }

    private string BuildCursorStatusSummary()
    {
        var cursor = _infiltrationManager.State.CursorAgent;
        return $"Cursor Agent :: {cursor.CurrentNodePath} :: AP {cursor.ActionPoints}/{cursor.MaxActionPoints} :: Clipboard {_infiltrationManager.State.Clipboard.Count}/{cursor.ClipboardCapacity} :: Pouch {_infiltrationManager.State.PouchCache.Count}/{cursor.PouchCapacity}";
    }

    private string BuildFieldSecurityStatusLine()
    {
        var agentCount = _infiltrationManager.SecurityAgents.Count;
        var alertedCount = _infiltrationManager.SecurityAgents.Count(a => a.AwarenessStage > SecurityAwarenessStage.Passive);
        var activeOpCount = _infiltrationManager.State.ActiveOperations.Count(op => op.Status == OperationStatus.Running);
        return $"Security :: Alert {_infiltrationManager.State.AlertStage} :: Agents {agentCount} ({alertedCount} alerted) :: Ops {activeOpCount} active";
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

    private string BuildSelectedNodeBadgeSummary(NodeData? node)
    {
        if (node == null)
            return "Badges none";

        var badges = BuildSecurityBadges(node.Path, _infiltrationManager.IsNodeMonitored(node.Path));
        return string.IsNullOrWhiteSpace(badges)
            ? "Badges none"
            : $"Badges {badges}";
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
        var permissionLocked = _infiltrationManager.IsPermissionLocked(node.Path);
        var permissionOverridden = _infiltrationManager.HasPermissionOverride(node.Path);
        var permissionOverrideTurns = _infiltrationManager.GetPermissionOverrideTurns(node.Path);
        var trackedTurns = _infiltrationManager.GetTrackedPathTurns(node.Path);
        var scanPressureTurns = _infiltrationManager.GetScanPressureTurns(node.Path);
        var pouchHidden = _infiltrationManager.IsNodeHiddenInPouch(node.Path);
        var pouchBroken = _infiltrationManager.IsPouchMaskingBroken(node.Path);
        var pouchEligible = IsPouchEligible(node);
        IReadOnlyList<StatusEffectInstance> effects = actor != null
            ? _battleManager.StatusEffects.GetEffects(actor.Id)
            : Array.Empty<StatusEffectInstance>();
        _inspectorNameLabel.Text = node.Name;
        _inspectorTypeLabel.Text = $"Type: {node.UiTypeName}";
        _inspectorPathLabel.Text = $"Path: {node.Path}";
        _inspectorStatsLabel.Text = actor == null
            ? "Stats: offline"
            : $"Stats: HP {actor.CurrentHp}/{actor.MaxHp} 쨌 AP {actor.CurrentAp}/{actor.MaxAp} 쨌 ATK {actor.AttackPower}";
        _inspectorStatusLabel.Text = $"Threat: {node.CombatProfile.ThreatLabel} 쨌 Watch: {(monitored ? "Monitored" : "Low")} 쨌 Access: {BuildAccessLabel(permissionLocked, permissionOverridden, permissionOverrideTurns)} 쨌 Security FX: {BuildSecurityFxLabel(trackedTurns, scanPressureTurns)} 쨌 Pouch: {BuildPouchLabel(pouchHidden, pouchBroken, pouchEligible)} 쨌 Status: {FormatStatusEffects(effects)}";
        _inspectorHintLabel.Text = BuildInspectorHint(node, permissionLocked, permissionOverridden, permissionOverrideTurns, trackedTurns, scanPressureTurns, pouchHidden, pouchBroken, pouchEligible);
        _securityAgentsLabel.Text = agents.Count == 0
            ? (monitored ? "Security: monitored by nearby patrol" : "Security: none")
            : $"Security: {string.Join(", ", agents.Select(agent => $"{agent.DisplayName} ({agent.AgentType})"))}";

        _targetHpBar.MaxValue = actor?.MaxHp ?? 1;
        _targetHpBar.Value = actor?.CurrentHp ?? 0;
        _targetApBar.MaxValue = actor?.MaxAp ?? 1;
        _targetApBar.Value = actor?.CurrentAp ?? 0;
    }

    private void UpdateSecuritySection()
    {
        _securitySectionLabel.Clear();

        var agents = _infiltrationManager.SecurityAgents;
        if (agents.Count == 0)
        {
            _securitySectionLabel.AppendText("[color=#8b949e]Agents: none[/color]\n");
        }
        else
        {
            _securitySectionLabel.AppendText("[color=#f2cc60]Agents[/color]\n");
            foreach (var agent in agents)
            {
                var badge = GetAgentTypeBadge(agent.AgentType);
                var awareness = agent.AwarenessStage;
                var awarenessColor = awareness switch
                {
                    SecurityAwarenessStage.Passive => "#7ee787",
                    SecurityAwarenessStage.Suspicious => "#f2cc60",
                    SecurityAwarenessStage.ActiveScan => "#ffa657",
                    SecurityAwarenessStage.Quarantine or SecurityAwarenessStage.Purge => "#ff7b72",
                    _ => "#c9d1d9"
                };
                _securitySectionLabel.AppendText($"[color={awarenessColor}]{badge} {agent.DisplayName} [{awareness}] @ {agent.CurrentNodePath}[/color]\n");
            }
        }

        var activeOps = _infiltrationManager.State.ActiveOperations
            .Where(op => op.Status is OperationStatus.Running or OperationStatus.Queued)
            .ToList();

        if (activeOps.Count == 0)
        {
            _securitySectionLabel.AppendText("[color=#8b949e]Operations: none[/color]\n");
        }
        else
        {
            _securitySectionLabel.AppendText("[color=#58a6ff]Operations[/color]\n");
            foreach (var op in activeOps)
            {
                var percent = (int)(op.Progress * 100f);
                _securitySectionLabel.AppendText($"[color=#c9d1d9]{op.Type} :: {op.TargetNodePath} :: {percent}% :: {op.Status}[/color]\n");
            }
        }
    }

    private void UpdateActionButtons()
    {
        var node = GetSelectedNode();
        var actor = node != null ? _nodeActors.GetValueOrDefault(node.Path) : null;
        var btnCursor = _infiltrationManager.State.CursorAgent;
        var btnClipboardItemCount = _infiltrationManager.State.Clipboard.Count;

        foreach (var pair in _actionButtons)
        {
            var action = _actionRegistry.GetAction(pair.Key);
            if (action == null)
                continue;

            var previewContext = new ActionContext(btnCursor.ActionPoints, btnClipboardItemCount)
            {
                Target = actor,
                TargetNode = node,
                StatusEffects = _battleManager.StatusEffects,
                AllActors = _battleManager.Enemies
            };

            var canExecute = node != null
                && actor != null
                && actor.IsAlive
                && _infiltrationManager.State.RunStatus == RunStatus.Active
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
        foreach (var log in _infiltrationManager.State.EventLog.TakeLast(BattleConstants.UIBattleLogDisplayLines))
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

        var isRunEnded = _infiltrationManager.State.RunStatus != RunStatus.Active;
        _moveCursorButton.Disabled = isRunEnded || GetSelectedNode() == null;
        _clipboardWindowButton.Disabled = isRunEnded;
        _tempWindowButton.Disabled = isRunEnded;
        _logWindowButton.Disabled = isRunEnded;
        _executeQueueButton.Disabled = queue.Count == 0 || isRunEnded;
        var canExtract = !isRunEnded && IsAtExtractionPoint()
            && (_missionProgress.ObjectiveCompleted || _currentMission.ObjectiveType == MissionObjectiveType.Escape);
        _extractButton.Disabled = !canExtract;
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
        _storeInPouchButton.Disabled = true;
        _restoreFromPouchButton.Disabled = true;

        if (clipboardWindow == null || !clipboardWindow.IsOpen)
        {
            return;
        }

        _clipboardWindowStatusLabel.Text = $"Focus: {(clipboardWindow.IsFocused ? "Active" : "Standby")} 쨌 Bound: {clipboardWindow.BoundPath} 쨌 Pouch hides small files, reduces Trace, and blurs Search/Indexer scans";

        var selectedNode = GetSelectedNode();
        var selectedNodePath = selectedNode?.Path;
        var canStoreInPouch = selectedNode != null
            && selectedNode.Size <= _infiltrationManager.State.CursorAgent.PouchMaxFileSize
            && _infiltrationManager.State.Clipboard.Any(item => string.Equals(item.NodePath, selectedNodePath, StringComparison.OrdinalIgnoreCase));
        var canRestoreFromPouch = !string.IsNullOrWhiteSpace(selectedNodePath)
            && _infiltrationManager.State.PouchCache.Any(item => string.Equals(item.NodePath, selectedNodePath, StringComparison.OrdinalIgnoreCase));
        _storeInPouchButton.Disabled = !canStoreInPouch;
        _restoreFromPouchButton.Disabled = !canRestoreFromPouch;

        if (_infiltrationManager.State.Clipboard.Count == 0)
        {
            _clipboardWindowItemsLabel.AppendText("[color=#8b949e](clipboard empty)[/color]\n");
        }
        else
        {
            _clipboardWindowItemsLabel.AppendText("[color=#58a6ff]Clipboard[/color]\n");
            foreach (var item in _infiltrationManager.State.Clipboard)
            {
                var resolvedSize = item.Size > 0 ? item.Size : (_dungeon.GetNode(item.NodePath)?.Size ?? 0);
                var ghostTag = item.IsGhosted ? " 쨌 ghost" : string.Empty;
                _clipboardWindowItemsLabel.AppendText($"[color=#c9d1d9]- {item.NodeKind} :: {item.NodePath} 쨌 size {resolvedSize}{ghostTag}[/color]\n");
            }
        }

        _clipboardWindowItemsLabel.AppendText("\n[color=#f2cc60]Cheek Pouch Cache[/color]\n");
        if (_infiltrationManager.State.PouchCache.Count == 0)
        {
            _clipboardWindowItemsLabel.AppendText("[color=#8b949e](pouch empty)[/color]\n");
            return;
        }

        foreach (var item in _infiltrationManager.State.PouchCache)
        {
            var resolvedSize = item.Size > 0 ? item.Size : (_dungeon.GetNode(item.NodePath)?.Size ?? 0);
            var ghostTag = item.IsGhosted ? " 쨌 ghost" : string.Empty;
            _clipboardWindowItemsLabel.AppendText($"[color=#c9d1d9]- {item.NodeKind} :: {item.NodePath} 쨌 size {resolvedSize}{ghostTag}[/color]\n");
        }
    }

    private void ToggleClipboardWindow()
    {
        var existing = _infiltrationManager.State.Windows
            .FirstOrDefault(window => window.WindowType == ExplorerWindowType.Clipboard);

        if (existing?.IsOpen == true)
        {
            _infiltrationManager.CloseWindow(existing.WindowId);
            AppendConsoleFeed("window close :: clipboard");
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

    private void UpdateTempWindowUi()
    {
        var tempWindow = _infiltrationManager.State.Windows
            .FirstOrDefault(window => window.WindowType == ExplorerWindowType.Temp);

        _tempWindowButton.Text = tempWindow?.IsOpen == true ? "Temp *" : "Temp";
        _tempWindow.Visible = tempWindow?.IsOpen == true;
        _tempWindowItemList.Clear();
        _tempWindowStatusLabel.Text = "Temp window offline";

        if (tempWindow == null || !tempWindow.IsOpen)
        {
            return;
        }

        _tempWindowStatusLabel.Text = $"Focus: {(tempWindow.IsFocused ? "Active" : "Standby")} 쨌 Bound: {tempWindow.BoundPath} 쨌 클릭으로 노드 선택";

        var boundContainer = _dungeon.GetNode(tempWindow.BoundPath) as ContainerNode;
        if (boundContainer == null)
        {
            _tempWindowItemList.AddItem($"(bound path not a container: {tempWindow.BoundPath})");
            return;
        }

        var children = boundContainer.Children.Where(child => !_dungeon.IsCleared(child.Path)).ToList();
        if (children.Count == 0)
        {
            _tempWindowItemList.AddItem("(empty)");
            return;
        }

        foreach (var child in children)
        {
            var agents = GetAgentsForNode(child.Path);
            var monitored = _infiltrationManager.IsNodeMonitored(child.Path);
            var badges = BuildCompactBadgeSummary(child.Path, agents.Count > 0 || monitored);
            var badgePart = string.IsNullOrWhiteSpace(badges) ? string.Empty : $" | {badges}";
            var idx = _tempWindowItemList.AddItem($"{child.UiTypeName} :: {child.Name}{badgePart}");
            _tempWindowItemList.SetItemMetadata(idx, child.Path);
            if (string.Equals(_selectedNodePath, child.Path, StringComparison.OrdinalIgnoreCase))
                _tempWindowItemList.Select(idx);
        }
    }

    private void OnTempWindowItemSelected(long index)
    {
        var path = _tempWindowItemList.GetItemMetadata((int)index).AsString();
        if (string.IsNullOrWhiteSpace(path))
            return;

        _selectedNodePath = path;
        AppendConsoleFeed($"target locked :: {_dungeon.GetNode(path)?.Name ?? path} :: {path}");
        UpdateUi();
    }

    private void ToggleTempWindow()
    {
        var existing = _infiltrationManager.State.Windows
            .FirstOrDefault(window => window.WindowType == ExplorerWindowType.Temp);

        if (existing?.IsOpen == true)
        {
            _infiltrationManager.CloseWindow(existing.WindowId);
            AppendConsoleFeed("window close :: temp");
            UpdateUi();
            return;
        }

        _infiltrationManager.OpenWindow(
            ExplorerWindowType.Temp,
            "Temp Window",
            _dungeon.CurrentContainer.Path,
            traceModifier: 1);
        AppendConsoleFeed("window open :: temp");
        UpdateUi();
    }

    private void CloseTempWindow()
    {
        var existing = _infiltrationManager.State.Windows
            .FirstOrDefault(window => window.WindowType == ExplorerWindowType.Temp && window.IsOpen);
        if (existing == null)
            return;

        _infiltrationManager.CloseWindow(existing.WindowId);
        AppendConsoleFeed("window close :: temp");
        UpdateUi();
    }

    private void ToggleLogWindow()
    {
        var existing = _infiltrationManager.State.Windows
            .FirstOrDefault(window => window.WindowType == ExplorerWindowType.LogViewer);

        if (existing?.IsOpen == true)
        {
            _infiltrationManager.CloseLogViewerWindow();
            AppendConsoleFeed("window close :: log viewer");
            UpdateUi();
            return;
        }

        _infiltrationManager.OpenLogViewerWindow();
        _selectedLogEntryIndex = _infiltrationManager.State.EventLog.Count - 1;
        AppendConsoleFeed("window open :: log viewer");
        UpdateUi();
    }

    private void CloseLogWindow()
    {
        _infiltrationManager.CloseLogViewerWindow();
        AppendConsoleFeed("window close :: log viewer");
        UpdateUi();
    }

    private void OnLogWindowItemSelected(long index)
    {
        _selectedLogEntryIndex = (int)index;
        UpdateLogWindowForgeButton();
    }

    private void UpdateLogWindowUi()
    {
        var logWindow = _infiltrationManager.State.Windows
            .FirstOrDefault(window => window.WindowType == ExplorerWindowType.LogViewer);

        _logWindowButton.Text = logWindow?.IsOpen == true ? "Log *" : "Log";
        _logWindow.Visible = logWindow?.IsOpen == true;
        _logWindowItemList.Clear();
        _logWindowStatusLabel.Text = "Log window offline";
        _logWindowForgeButton.Disabled = true;

        if (logWindow == null || !logWindow.IsOpen)
            return;

        var entries = _infiltrationManager.State.EventLog;
        _logWindowStatusLabel.Text = $"Focus: {(logWindow.IsFocused ? "Active" : "Standby")} | Entries: {entries.Count}";

        foreach (var entry in entries)
        {
            var path = ExtractPathFromLogEntry(entry);
            var itemIndex = _logWindowItemList.AddItem(entry);
            _logWindowItemList.SetItemMetadata(itemIndex, path ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(path))
            {
                _logWindowItemList.SetItemCustomFgColor(itemIndex, new Color(0.49f, 0.76f, 1.0f));
            }
        }

        var count = _logWindowItemList.ItemCount;
        if (_selectedLogEntryIndex >= 0 && _selectedLogEntryIndex < count)
        {
            _logWindowItemList.Select(_selectedLogEntryIndex);
            _logWindowItemList.EnsureCurrentIsVisible();
        }
        else if (count > 0)
        {
            _selectedLogEntryIndex = count - 1;
            _logWindowItemList.Select(_selectedLogEntryIndex);
            _logWindowItemList.EnsureCurrentIsVisible();
        }

        UpdateLogWindowForgeButton();
    }

    private void UpdateLogWindowForgeButton()
    {
        var selected = _logWindowItemList.GetSelectedItems();
        if (selected.Length == 0 || _infiltrationManager.State.RunStatus != RunStatus.Active)
        {
            _logWindowForgeButton.Disabled = true;
            return;
        }

        var path = _logWindowItemList.GetItemMetadata(selected[0]).AsString();
        var nodeExists = !string.IsNullOrWhiteSpace(path) && _dungeon.GetNode(path) != null;
        _logWindowForgeButton.Disabled = !nodeExists;
    }

    private void QueueLogForgeFromLogWindow()
    {
        var selected = _logWindowItemList.GetSelectedItems();
        if (selected.Length == 0)
            return;

        var path = _logWindowItemList.GetItemMetadata(selected[0]).AsString();
        if (string.IsNullOrWhiteSpace(path))
        {
            AppendConsoleFeed("logforge :: no path in selected entry");
            return;
        }

        var node = _dungeon.GetNode(path);
        if (node == null)
        {
            AppendConsoleFeed($"logforge :: node not found :: {path}");
            return;
        }

        QueueOperationCommand(OperationType.RewriteLog, path, null, $"LOGFORGE -> {node.Name}");
    }

    private static string? ExtractPathFromLogEntry(string logEntry)
    {
        int idx = logEntry.IndexOf("res://", StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            idx = logEntry.IndexOf("root/", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        var end = idx;
        while (end < logEntry.Length && logEntry[end] != ' ' && logEntry[end] != '\t'
               && logEntry[end] != '\n' && logEntry[end] != '(' && logEntry[end] != ':')
            end++;
        var path = logEntry[idx..end].TrimEnd('/', ')', ',', '.', ';');
        return string.IsNullOrWhiteSpace(path) ? null : path;
    }

    private void StoreSelectedClipboardEntryInPouch()
    {
        var selectedNode = GetSelectedNode();
        if (selectedNode == null)
            return;

        if (selectedNode.Size > _infiltrationManager.State.CursorAgent.PouchMaxFileSize)
        {
            AppendConsoleFeed($"pouch store blocked :: {selectedNode.Name} :: size {selectedNode.Size} > {_infiltrationManager.State.CursorAgent.PouchMaxFileSize}");
            UpdateUi();
            return;
        }

        if (_infiltrationManager.TryMoveClipboardToPouch(selectedNode.Path, selectedNode.Size))
        {
            AppendConsoleFeed($"pouch store :: {selectedNode.Name} :: trace softened");
        }
        else
        {
            AppendConsoleFeed($"pouch store failed :: {selectedNode.Name}");
        }

        UpdateUi();
    }

    private void RestoreSelectedPouchEntryToClipboard()
    {
        var selectedNode = GetSelectedNode();
        if (selectedNode == null)
            return;

        if (_infiltrationManager.TryRestoreFromPouch(selectedNode.Path))
        {
            AppendConsoleFeed($"pouch restore :: {selectedNode.Name}");
        }
        else
        {
            AppendConsoleFeed($"pouch restore failed :: {selectedNode.Name}");
        }

        UpdateUi();
    }

    private void UpdateConsoleHint()
    {
        var node = GetSelectedNode();
        if (!string.IsNullOrWhiteSpace(_dragSourceNodePath))
        {
            return;
        }

        var hintCursor = _infiltrationManager.State.CursorAgent;
        _consoleHintLabel.Text = node == null
            ? "Select node -> queue command / drag to folder/archive"
            : $"Select node -> queue command / drag :: {node.Name} :: AP {hintCursor.ActionPoints}/{hintCursor.MaxActionPoints} :: objective {(_missionProgress.ObjectiveCompleted ? "done" : "pending")}";
    }

    private void UpdateBattleEndOverlay()
    {
        var isRunEnded = _infiltrationManager.State.RunStatus != RunStatus.Active;
        _battleEndOverlay.Visible = isRunEnded;
        if (!isRunEnded)
            return;

        var result = _missionResult;
        var didWin = result?.Success ?? _battleManager.IsPlayerAlive;
        _battleEndTitleLabel.Text = didWin ? "Mission Complete" : "Mission Failed";
        _battleEndSummaryLabel.Text = result?.Summary ?? (didWin ? "Extraction completed." : "Operation failed.");
        var turnsUsedStat = result?.TurnsUsed ?? _infiltrationManager.State.TurnCount;
        var factionRepLine = string.Empty;
        if (result != null)
        {
            var factionRep = CampaignState.GetFactionReputation(result.Mission.Client.FactionId);
            var repSign = result.ReputationDelta >= 0 ? "+" : "";
            factionRepLine = $"\n세력: {result.Mission.Client.Faction} ({result.Mission.Client.Name}) 쨌 평판 {repSign}{result.ReputationDelta} → {factionRep}";
        }
        _battleEndStatsLabel.Text = $"Turns {turnsUsedStat}/{_effectiveTurnLimit} 쨌 Actions {_executedPlayerActions.Count} 쨌 Unique {_executedPlayerActions.Distinct().Count()} 쨌 Cleared {_dungeon.ClearedNodeCount}/{_dungeon.TotalNodeCount}"
            + (result != null ? $"\nCredits {result.CreditsDelta:+#;-#;0} 쨌 Rep {result.ReputationDelta:+#;-#;0} 쨌 Heat {result.HeatDelta:+#;-#;0}" : string.Empty)
            + factionRepLine;
    }

    private void OnBattleEnd()
    {
        if (!_missionResolved)
        {
            var runStatus = _infiltrationManager.State.RunStatus;
            var extracted = runStatus == RunStatus.Escaped;
            _missionResult = _missionProgress.Resolve(_battleManager.IsPlayerAlive, extracted, _infiltrationManager.State.TurnCount, _effectiveTurnLimit, _infiltrationManager.State.Trace);
            CampaignState.ApplyMissionResult(_missionResult);
            var finishMessage = runStatus switch
            {
                RunStatus.Escaped => "Extraction complete. Package delivered.",
                RunStatus.TimedOut => $"Trace level critical. Turn limit {_effectiveTurnLimit} exceeded.",
                RunStatus.Failed => "Operator eliminated. Mission failed.",
                _ => "Run ended."
            };
            _battleManager.FinishBattle(finishMessage);
            AddOperationLog(_missionResult.Success
                ? $"Mission complete: {_missionResult.Summary}"
                : $"Mission failed: {_missionResult.Summary}");
            AddOperationLog($"Payout: {_missionResult.CreditsDelta:+#;-#;0}c / Rep {_missionResult.ReputationDelta:+#;-#;0} / Heat {_missionResult.HeatDelta:+#;-#;0}");
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
            var badges = BuildSecurityBadges(node.Path, agents.Count > 0);
            return $"[{index + 1}] {(string.IsNullOrWhiteSpace(badges) ? node.Name : $"{badges} {node.Name}")} :: {node.UiTypeName} :: {BuildNodeStatus(node, agents)} :: {node.CombatProfile.ThreatLabel} :: {agentText}";
        }));
    }

    private string BuildStatusSummary()
    {
        var selected = GetSelectedNode();
        return $"status :: path {_dungeon.CurrentContainer.Path} :: turn {_infiltrationManager.State.TurnCount}/{_effectiveTurnLimit} :: remaining {CountRemainingThreats()} :: selected {(selected?.Name ?? "none")} :: {BuildSelectedNodeBadgeSummary(selected)} :: {BuildWindowSummary()}";
    }

    private string BuildNodeStatus(NodeData node, IReadOnlyList<SecurityAgent>? agents = null, bool monitored = false)
    {
        if (!_nodeActors.TryGetValue(node.Path, out var actor) || !actor.IsAlive)
            return "cleared";

        var permissionLocked = _infiltrationManager.IsPermissionLocked(node.Path);
        var permissionOverridden = _infiltrationManager.HasPermissionOverride(node.Path);
        var permissionOverrideTurns = _infiltrationManager.GetPermissionOverrideTurns(node.Path);
        var trackedTurns = _infiltrationManager.GetTrackedPathTurns(node.Path);
        var scanPressureTurns = _infiltrationManager.GetScanPressureTurns(node.Path);
        var pouchHidden = _infiltrationManager.IsNodeHiddenInPouch(node.Path);
        var pouchBroken = _infiltrationManager.IsPouchMaskingBroken(node.Path);
        var accessSuffix = permissionLocked ? " locked" : permissionOverridden ? $" overridden-{permissionOverrideTurns}t" : string.Empty;
        var securityFxSuffix = trackedTurns > 0 ? $" tracked-{trackedTurns}t" : scanPressureTurns > 0 ? $" pressured-{scanPressureTurns}t" : string.Empty;
        var pouchSuffix = BuildNodePouchSuffix(node, pouchHidden, pouchBroken);

        if (node is ContainerNode container && container.Children.Count > 0)
            return ((agents?.Count ?? 0) > 0 || monitored ? "guarded sealed" : "sealed") + accessSuffix + securityFxSuffix + pouchSuffix;

        if ((agents?.Count ?? 0) > 0 || monitored)
            return (actor.CurrentHp < actor.MaxHp ? "guarded active" : "guarded") + accessSuffix + securityFxSuffix + pouchSuffix;

        return (actor.CurrentHp < actor.MaxHp ? "active" : "idle") + accessSuffix + securityFxSuffix + pouchSuffix;
    }

    private List<SecurityAgent> GetAgentsForNode(string nodePath)
    {
        return _infiltrationManager.SecurityAgents
            .Where(agent => string.Equals(agent.CurrentNodePath, nodePath, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static string GetAgentTypeBadge(SecurityAgentType agentType) => agentType switch
    {
        SecurityAgentType.IndexerScout => "[SCOUT]",
        SecurityAgentType.AiMonitor => "[AI]",
        SecurityAgentType.GuardScanner => "[GUARD]",
        SecurityAgentType.FirewallSentinel => "[FW]",
        SecurityAgentType.AntivirusHeavy => "[AV]",
        SecurityAgentType.BackupRepairer => "[BKUP]",
        _ => "[SEC]"
    };

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

    private string BuildDisplayName(NodeData node, IReadOnlyList<SecurityAgent> agents, bool monitored = false)
    {
        var badges = BuildSecurityBadges(node.Path, agents.Count > 0 || monitored);
        return string.IsNullOrWhiteSpace(badges)
            ? node.Name
            : $"{badges} {node.Name}";
    }

    private string BuildSecurityBadges(string nodePath, bool guarded)
    {
        var badges = new List<string>();

        if (guarded)
            badges.Add("[WATCH]");

        if (_infiltrationManager.IsPermissionLocked(nodePath))
        {
            badges.Add("[LOCK]");
        }
        else if (_infiltrationManager.HasPermissionOverride(nodePath))
        {
            badges.Add($"[OVERRIDE {_infiltrationManager.GetPermissionOverrideTurns(nodePath)}T]");
        }

        var trackedTurns = _infiltrationManager.GetTrackedPathTurns(nodePath);
        if (trackedTurns > 0)
            badges.Add($"[TRACKED {trackedTurns}T]");

        var scanPressureTurns = _infiltrationManager.GetScanPressureTurns(nodePath);
        if (scanPressureTurns > 0)
            badges.Add($"[PRESSURE {scanPressureTurns}T]");

        return string.Join(" ", badges);
    }

    private string BuildCompactBadgeSummary(string nodePath, bool guarded)
    {
        var parts = new List<string>();

        if (guarded)
            parts.Add("WATCH");

        if (_infiltrationManager.IsPermissionLocked(nodePath))
        {
            parts.Add("LOCK");
        }
        else if (_infiltrationManager.HasPermissionOverride(nodePath))
        {
            parts.Add($"OVR {_infiltrationManager.GetPermissionOverrideTurns(nodePath)}T");
        }

        var trackedTurns = _infiltrationManager.GetTrackedPathTurns(nodePath);
        if (trackedTurns > 0)
            parts.Add($"TRK {trackedTurns}T");

        var scanPressureTurns = _infiltrationManager.GetScanPressureTurns(nodePath);
        if (scanPressureTurns > 0)
            parts.Add($"PRS {scanPressureTurns}T");

        return string.Join(" / ", parts);
    }

    private Texture2D? ResolvePrimaryBadgeIcon(string nodePath, bool guarded)
    {
        if (_infiltrationManager.IsPermissionLocked(nodePath))
            return _badgeIcons.GetValueOrDefault("lock");

        if (_infiltrationManager.HasPermissionOverride(nodePath))
            return _badgeIcons.GetValueOrDefault("override");

        if (_infiltrationManager.GetTrackedPathTurns(nodePath) > 0)
            return _badgeIcons.GetValueOrDefault("tracked");

        if (_infiltrationManager.GetScanPressureTurns(nodePath) > 0)
            return _badgeIcons.GetValueOrDefault("pressure");

        if (guarded)
            return _badgeIcons.GetValueOrDefault("watch");

        return null;
    }

    private static string BuildSecuritySummary(IReadOnlyList<SecurityAgent> agents, bool monitored = false)
    {
        if (agents.Count == 0)
            return monitored ? "Nearby Watch" : "None";

        if (agents.Count == 1)
            return agents[0].DisplayName;

        return $"{agents[0].DisplayName} +{agents.Count - 1}";
    }

    private string BuildAccessLabel(bool permissionLocked, bool permissionOverridden, int permissionOverrideTurns)
    {
        if (permissionLocked)
            return "Locked";

        return permissionOverridden
            ? $"Overridden ({permissionOverrideTurns}T)"
            : "Open";
    }

    private static string BuildSecurityFxLabel(int trackedTurns, int scanPressureTurns)
    {
        if (trackedTurns > 0 && scanPressureTurns > 0)
            return $"Tracked ({trackedTurns}T) / Pressure ({scanPressureTurns}T)";

        if (trackedTurns > 0)
            return $"Tracked ({trackedTurns}T)";

        if (scanPressureTurns > 0)
            return $"Pressure ({scanPressureTurns}T)";

        return "None";
    }

    private static string BuildPouchLabel(bool pouchHidden, bool pouchBroken, bool pouchEligible)
    {
        if (pouchHidden)
            return pouchBroken ? "Pouch Exposed" : "Pouch Hidden";

        return pouchEligible ? "Pouch Safe" : "Oversize";
    }

    private string BuildTooltipPouchLabel(NodeData node, bool pouchHidden, bool pouchBroken, bool pouchEligible)
    {
        if (pouchHidden)
            return pouchBroken ? "Exposed" : "Hidden";

        return pouchEligible
            ? "Safe"
            : $"Oversize ({node.Size}/{_infiltrationManager.State.CursorAgent.PouchMaxFileSize})";
    }

    private string BuildNodePouchSuffix(NodeData node, bool pouchHidden, bool pouchBroken)
    {
        if (pouchHidden)
            return pouchBroken ? " pouch-exposed" : " pouch-hidden";

        return IsPouchEligible(node) ? " pouch-safe" : " oversize";
    }

    private string BuildInspectorHint(
        NodeData node,
        bool permissionLocked,
        bool permissionOverridden,
        int permissionOverrideTurns,
        int trackedTurns,
        int scanPressureTurns,
        bool pouchHidden,
        bool pouchBroken,
        bool pouchEligible)
    {
        if (trackedTurns > 0)
            return $"Security trace marker active for {trackedTurns} turn(s). Actions on this path add extra Trace. Rewrite Log can scrub it.";

        if (scanPressureTurns > 0)
            return $"Scan pressure active for {scanPressureTurns} turn(s). Hidden pouch routes are less reliable here. Rewrite Log can diffuse it.";

        if (permissionLocked)
            return "Firewall Sentinel lock active. Use Permission Override to force access.";

        if (permissionOverridden)
            return $"Permission Override window active for {permissionOverrideTurns} turn(s). Access will relock after it expires.";

        if (pouchHidden)
        {
            return pouchBroken
                ? "Pouch masking is broken. Search and Indexer scans can track this file again."
                : "Cheek pouch cache is masking this file from some Search/Indexer checks.";
        }

        if (!pouchEligible)
            return $"Too large for cheek pouch cache. Limit {_infiltrationManager.State.CursorAgent.PouchMaxFileSize}, file size {node.Size}.";

        if (node is ContainerNode container && container.Children.Count > 0)
            return node.CombatProfile.RevealSummary ?? "Open to reveal nested nodes.";

        return "Select a command from the deck below.";
    }

    private string BuildNodeTooltip(NodeData node, IReadOnlyList<SecurityAgent> agents, bool monitored = false)
    {
        var permissionLocked = _infiltrationManager.IsPermissionLocked(node.Path);
        var permissionOverridden = _infiltrationManager.HasPermissionOverride(node.Path);
        var permissionOverrideTurns = _infiltrationManager.GetPermissionOverrideTurns(node.Path);
        var trackedTurns = _infiltrationManager.GetTrackedPathTurns(node.Path);
        var scanPressureTurns = _infiltrationManager.GetScanPressureTurns(node.Path);
        var pouchHidden = _infiltrationManager.IsNodeHiddenInPouch(node.Path);
        var pouchBroken = _infiltrationManager.IsPouchMaskingBroken(node.Path);
        var pouchEligible = IsPouchEligible(node);
        var lines = new List<string>
        {
            node.Path,
            $"Type: {node.UiTypeName}",
            $"Threat: {node.CombatProfile.ThreatLabel}",
            $"Watch: {(monitored ? "Monitored" : "Low")}",
            $"Access: {BuildAccessLabel(permissionLocked, permissionOverridden, permissionOverrideTurns)}",
            $"Security FX: {BuildSecurityFxLabel(trackedTurns, scanPressureTurns)}",
            $"Pouch: {BuildTooltipPouchLabel(node, pouchHidden, pouchBroken, pouchEligible)}"
        };

        if (agents.Count > 0)
        {
            lines.Add($"Security: {string.Join(", ", agents.Select(agent => agent.DisplayName))}");
        }
        else if (monitored)
        {
            lines.Add("Security: nearby patrol coverage");
        }

        if (permissionLocked)
        {
            lines.Add("Firewall Sentinel lock active. Permission Override required for direct access.");
        }
        else if (permissionOverridden)
        {
            lines.Add($"Permission Override active for {permissionOverrideTurns} turn(s). Access will relock when it expires.");
        }

        if (trackedTurns > 0)
        {
            lines.Add($"Tracked path marker active for {trackedTurns} turn(s).");
        }

        if (scanPressureTurns > 0)
        {
            lines.Add($"Scan pressure active for {scanPressureTurns} turn(s). Pouch masking is weaker here.");
        }

        if (pouchHidden)
        {
            lines.Add(pouchBroken
                ? "Cheek pouch cache was exposed by Show Hidden / Permission Override."
                : "Cheek pouch cache blurs Search and Indexer scans.");
        }

        return string.Join("\n", lines);
    }

    private void SyncCursorApToPlayer()
    {
        _battleManager.Player.CurrentAp = _infiltrationManager.State.CursorAgent.ActionPoints;
    }

    private bool IsPouchEligible(NodeData node)
    {
        return node.Size <= _infiltrationManager.State.CursorAgent.PouchMaxFileSize;
    }

    private static string FormatSize(long size)
    {
        return size <= 0 ? "-" : $"{size} kb";
    }

    private static OperationType MapActionIdToOperationType(string actionId)
    {
        return actionId.ToLowerInvariant() switch
        {
            ActionIds.Open => OperationType.Access,
            ActionIds.Copy => OperationType.Copy,
            ActionIds.Cut => OperationType.Cut,
            ActionIds.Paste => OperationType.Paste,
            ActionIds.Move => OperationType.Move,
            ActionIds.Delete => OperationType.Delete,
            ActionIds.Compress => OperationType.Compress,
            ActionIds.Extract => OperationType.ExtractArchive,
            ActionIds.Inspect => OperationType.Properties,
            ActionIds.Search => OperationType.Search,
            ActionIds.Sort => OperationType.Sort,
            ActionIds.ShowHidden => OperationType.ShowHidden,
            ActionIds.LogForge => OperationType.RewriteLog,
            ActionIds.Quarantine => OperationType.Quarantine,
            ActionIds.Clean => OperationType.Clean,
            ActionIds.Inject => OperationType.Inject,
            ActionIds.Stun => OperationType.Stun,
            ActionIds.Decoy => OperationType.Decoy,
            ActionIds.PermissionOverride => OperationType.PermissionOverride,
            _ => OperationType.Access
        };
    }

    private static string? MapOperationTypeToActionId(OperationType operationType)
    {
        return operationType switch
        {
            OperationType.MoveCursor => null,
            OperationType.Access => ActionIds.Open,
            OperationType.Copy => ActionIds.Copy,
            OperationType.Cut => ActionIds.Cut,
            OperationType.Paste => ActionIds.Paste,
            OperationType.Move => ActionIds.Move,
            OperationType.Delete => ActionIds.Delete,
            OperationType.Compress => ActionIds.Compress,
            OperationType.ExtractArchive => ActionIds.Extract,
            OperationType.Properties => ActionIds.Inspect,
            OperationType.Search => ActionIds.Search,
            OperationType.ShowHidden => ActionIds.ShowHidden,
            OperationType.RewriteLog => ActionIds.LogForge,
            OperationType.Quarantine => ActionIds.Quarantine,
            OperationType.Clean => ActionIds.Clean,
            OperationType.Inject => ActionIds.Inject,
            OperationType.Stun => ActionIds.Stun,
            OperationType.Decoy => ActionIds.Decoy,
            OperationType.PermissionOverride => ActionIds.PermissionOverride,
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

    private void AddOperationLog(string msg)
    {
        _battleManager.AddLog(msg);
        _infiltrationManager.State.AddLog(msg);
    }

    private void AppendConsoleFeed(string text)
    {
        _consoleFeedLabel.AppendText($"[color=#7ee787]{text}[/color]\n");
        DebugLog.Trace("ConsoleFeed", text);
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
        AppendConsoleFeed("shortcuts :: Enter=Execute Queue | Del=Delete | Backspace=Navigate Up | F5=Refresh");

        if (DebugLog.Enabled)
        {
            AppendConsoleFeed($"debug log :: {DebugLog.CurrentLogPath}");
        }
    }

    private void RestartBattle()
    {
        DebugLog.Info(nameof(BattleScene), "ReloadCurrentScene");
        GetTree().ReloadCurrentScene();
    }

    private void BackToLobby()
    {
        DebugLog.Info(nameof(BattleScene), "ChangeSceneToFile -> main.tscn");
        GetTree().ChangeSceneToFile("res://res/scenes/main.tscn");
    }

    private async void RunSmokeTestIfRequested()
    {
        if (!HasAutomationArg("--projectfr-smoke-test") && !HasAutomationArg("--projectfr-smoke-test-menu-flow"))
            return;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        _selectedNodePath = BattleConstants.RootBuildCachePath;
        QueueSelectedCommand(ActionIds.Inspect);
        QueueSelectedCommand(ActionIds.Open);
        ExecuteQueuedCommands();
        _selectedNodePath = BattleConstants.CacheAssetsPath;
        QueueSelectedCommand(ActionIds.Open);
        ExecuteQueuedCommands();

        if (_currentMission.Id == "mission_extract_boss" && _currentMission.ObjectiveType == MissionObjectiveType.Extract)
        {
            // Golden path: Copy target → navigate to root → Extract.
            _selectedNodePath = BattleConstants.BossZipPath;
            QueueSelectedCommand(ActionIds.Copy);
            ExecuteQueuedCommands();

            if (_infiltrationManager.State.RunStatus == RunStatus.Active)
            {
                LoadContainer(_dungeon.Root, reason: "Smoke test navigating to root for extraction");
                TryExtractMission();

                var success = _missionResult?.Success ?? false;
                DebugLog.Info(nameof(BattleScene), $"smoke-test golden-path :: mission={_currentMission.Id} :: success={success} :: summary={_missionResult?.Summary ?? "pending"}");
                AppendConsoleFeed($"smoke-test :: golden-path :: {(success ? "PASS" : "FAIL")} :: {_missionResult?.Summary ?? "no result"}");
            }
            else
            {
                DebugLog.Info(nameof(BattleScene), "smoke-test golden-path :: battle ended before extract step");
                AppendConsoleFeed("smoke-test :: golden-path :: FAIL :: battle ended prematurely");
            }
        }
    }

    private static bool HasAutomationArg(string arg)
    {
        return OS.GetCmdlineUserArgs().Contains(arg);
    }
}
