using Godot;
using ProjectFR.Action;
using ProjectFR.Action.Implementations;
using ProjectFR.Battle;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;
using ProjectFR.Systems;
using System.Threading.Tasks;

namespace ProjectFR.Scenes;

/// <summary>
/// Battle scene controller - manages the UI and game flow during battle
/// </summary>
public partial class BattleScene : Control
{
    private BattleManager _battleManager = null!;
    private ActionRegistry _actionRegistry = null!;
    private Label _playerHpLabel = null!;
    private Label _playerApLabel = null!;
    private ProgressBar _playerHpBar = null!;
    private ProgressBar _playerApBar = null!;
    private Label _playerStatusLabel = null!;
    private ItemList _enemyList = null!;
    private Label _selectedEnemyNameLabel = null!;
    private Label _selectedEnemyTypeLabel = null!;
    private Label _selectedEnemyPathLabel = null!;
    private Label _selectedEnemyStatsLabel = null!;
    private Label _selectedEnemyStatusLabel = null!;
    private ProgressBar _selectedEnemyHpBar = null!;
    private ProgressBar _selectedEnemyApBar = null!;
    private RichTextLabel _battleLogLabel = null!;
    private GridContainer _actionButtonsContainer = null!;
    private Label _turnCounterLabel = null!;
    private Control _battleEndOverlay = null!;
    private Label _battleEndTitleLabel = null!;
    private Label _battleEndSummaryLabel = null!;
    private Button _restartBattleButton = null!;
    private Button _backToMenuButton = null!;
    private Label _battleEndStatsLabel = null!;
    private readonly Dictionary<string, Button> _actionButtons = new();
    private readonly List<string> _executedPlayerActions = new();
    private readonly Dictionary<string, NodeData> _enemyNodes = new();

    public override void _Ready()
    {
        InitializeUI();
        InitializeBattle();
        UpdateUI();
        RunSmokeTestIfRequested();
    }

    private void InitializeUI()
    {
        _playerHpLabel = GetNode<Label>("VBoxContainer/PlayerInfoPanel/PlayerInfoMargin/PlayerInfoVBox/PlayerHeader/HPLabel");
        _playerApLabel = GetNode<Label>("VBoxContainer/PlayerInfoPanel/PlayerInfoMargin/PlayerInfoVBox/PlayerHeader/APLabel");
        _playerHpBar = GetNode<ProgressBar>("VBoxContainer/PlayerInfoPanel/PlayerInfoMargin/PlayerInfoVBox/PlayerBars/HpBar");
        _playerApBar = GetNode<ProgressBar>("VBoxContainer/PlayerInfoPanel/PlayerInfoMargin/PlayerInfoVBox/PlayerBars/ApBar");
        _playerStatusLabel = GetNode<Label>("VBoxContainer/PlayerInfoPanel/PlayerInfoMargin/PlayerInfoVBox/PlayerStatusLabel");
        _enemyList = GetNode<ItemList>("VBoxContainer/ContentRow/EnemyPanel/EnemyMargin/EnemyVBox/EnemyList");
        _selectedEnemyNameLabel = GetNode<Label>("VBoxContainer/ContentRow/EnemyDetailPanel/EnemyDetailMargin/EnemyDetailVBox/SelectedEnemyNameLabel");
        _selectedEnemyTypeLabel = GetNode<Label>("VBoxContainer/ContentRow/EnemyDetailPanel/EnemyDetailMargin/EnemyDetailVBox/SelectedEnemyTypeLabel");
        _selectedEnemyPathLabel = GetNode<Label>("VBoxContainer/ContentRow/EnemyDetailPanel/EnemyDetailMargin/EnemyDetailVBox/SelectedEnemyPathLabel");
        _selectedEnemyStatsLabel = GetNode<Label>("VBoxContainer/ContentRow/EnemyDetailPanel/EnemyDetailMargin/EnemyDetailVBox/SelectedEnemyStatsLabel");
        _selectedEnemyStatusLabel = GetNode<Label>("VBoxContainer/ContentRow/EnemyDetailPanel/EnemyDetailMargin/EnemyDetailVBox/SelectedEnemyStatusLabel");
        _selectedEnemyHpBar = GetNode<ProgressBar>("VBoxContainer/ContentRow/EnemyDetailPanel/EnemyDetailMargin/EnemyDetailVBox/SelectedEnemyHpBar");
        _selectedEnemyApBar = GetNode<ProgressBar>("VBoxContainer/ContentRow/EnemyDetailPanel/EnemyDetailMargin/EnemyDetailVBox/SelectedEnemyApBar");
        _battleLogLabel = GetNode<RichTextLabel>("VBoxContainer/ContentRow/BattleLogPanel/BattleLogMargin/BattleLogVBox/BattleLog");
        _actionButtonsContainer = GetNode<GridContainer>("VBoxContainer/ActionsPanel/ActionsMargin/ActionsVBox/GridContainer");
        _turnCounterLabel = GetNode<Label>("VBoxContainer/TopPanel/TopMargin/TopVBox/TurnCounterLabel");
        _battleEndOverlay = GetNode<Control>("BattleEndOverlay");
        _battleEndTitleLabel = GetNode<Label>("BattleEndOverlay/OverlayCenter/BattleEndPanel/BattleEndMargin/BattleEndVBox/BattleEndTitleLabel");
        _battleEndSummaryLabel = GetNode<Label>("BattleEndOverlay/OverlayCenter/BattleEndPanel/BattleEndMargin/BattleEndVBox/BattleEndSummaryLabel");
        _battleEndStatsLabel = GetNode<Label>("BattleEndOverlay/OverlayCenter/BattleEndPanel/BattleEndMargin/BattleEndVBox/BattleEndStatsLabel");
        _restartBattleButton = GetNode<Button>("BattleEndOverlay/OverlayCenter/BattleEndPanel/BattleEndMargin/BattleEndVBox/BattleEndButtonRow/RestartBattleButton");
        _backToMenuButton = GetNode<Button>("BattleEndOverlay/OverlayCenter/BattleEndPanel/BattleEndMargin/BattleEndVBox/BattleEndButtonRow/BackToMenuButton");

        _enemyList.SelectMode = ItemList.SelectModeEnum.Single;
        _enemyList.ItemSelected += OnEnemySelected;
        _restartBattleButton.Pressed += RestartBattle;
        _backToMenuButton.Pressed += BackToMenu;

        CreateActionButtons();
    }

    private void CreateActionButtons()
    {
        _actionRegistry = new ActionRegistry();
        var actions = _actionRegistry.GetAllActions();

        foreach (var action in actions)
        {
            var button = new Button
            {
                Text = $"{action.DisplayName}\nAP {action.ApCost}",
                CustomMinimumSize = new Vector2(140, 48),
                TooltipText = GetTooltipText(action.ActionId)
            };
            button.Pressed += () => OnActionButtonPressed(action.ActionId);
            _actionButtonsContainer.AddChild(button);
            _actionButtons[action.ActionId] = button;
        }
    }

    private void InitializeBattle()
    {
        var player = new ActorState(
            maxHp: BattleConstants.DefaultPlayerMaxHp,
            maxAp: BattleConstants.DefaultPlayerMaxAp,
            attackPower: BattleConstants.DefaultPlayerAttackPower,
            displayName: BattleConstants.PlayerDisplayName
        );
        _battleManager = new BattleManager(player);

        AddDummyEnemy(
            new ActorState(
                maxHp: BattleConstants.DefaultEnemy1MaxHp,
                maxAp: BattleConstants.DefaultEnemy1MaxAp,
                attackPower: BattleConstants.DefaultEnemy1AttackPower,
                displayName: "Readme.txt"
            ),
            new FileNode("Readme.txt", "res://dummy/readme.txt", 4)
        );

        AddDummyEnemy(
            new ActorState(
                maxHp: BattleConstants.DefaultEnemy2MaxHp,
                maxAp: BattleConstants.DefaultEnemy2MaxAp,
                attackPower: BattleConstants.DefaultEnemy2AttackPower,
                displayName: "BuildCache"
            ),
            new FolderNode("BuildCache", "res://dummy/buildcache")
        );

        AddDummyEnemy(
            new ActorState(
                maxHp: BattleConstants.DefaultEnemy3MaxHp,
                maxAp: BattleConstants.DefaultEnemy3MaxAp,
                attackPower: BattleConstants.DefaultEnemy3AttackPower,
                displayName: "Boss.zip"
            ),
            new SpecialFileNode("Boss.zip", "res://dummy/boss.zip", 16)
        );

        _battleManager.StartBattle();
        _executedPlayerActions.Clear();
    }

    private void AddDummyEnemy(ActorState enemy, NodeData nodeData)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        ArgumentNullException.ThrowIfNull(nodeData);

        _battleManager.AddEnemy(enemy);
        _enemyNodes[enemy.Id] = nodeData;
    }

    public override void _Process(double delta)
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (_battleEndOverlay.Visible)
            return;

        if (ActionPressed("ui_open"))
            OnActionButtonPressed("open");
        else if (ActionPressed("ui_inspect"))
            OnActionButtonPressed("inspect");
        else if (ActionPressed("ui_delete"))
            OnActionButtonPressed("delete");
        else if (ActionPressed("ui_copy"))
            OnActionButtonPressed("copy");
        else if (ActionPressed("ui_cut"))
            OnActionButtonPressed("cut");
        else if (ActionPressed("ui_paste"))
            OnActionButtonPressed("paste");
        else if (ActionPressed("ui_clean"))
            OnActionButtonPressed("clean");
        else if (ActionPressed("ui_quarantine"))
            OnActionButtonPressed("quarantine");
        else if (ActionPressed("ui_compress"))
            OnActionButtonPressed("compress");
    }

    private static bool ActionPressed(string actionName)
    {
        return InputMap.HasAction(actionName) && Input.IsActionJustPressed(actionName);
    }

    private void OnEnemySelected(long index)
    {
        UpdateSelectedEnemyPanel();
        UpdateActionButtons();
    }

    private void OnActionButtonPressed(string actionId)
    {
        ArgumentNullException.ThrowIfNull(actionId);

        if (_battleManager.CurrentState != BattleState.PlayerTurn)
        {
            _battleManager.AddLog($"Not player's turn: {_battleManager.CurrentState}");
            UpdateUI();
            return;
        }

        var action = _actionRegistry.GetAction(actionId);
        if (action == null)
        {
            _battleManager.AddLog($"Action not found: {actionId}");
            UpdateUI();
            return;
        }

        var target = GetSelectedEnemy() ?? _battleManager.Enemies.FirstOrDefault(e => e.IsAlive);
        if (target == null)
        {
            _battleManager.AddLog("No alive enemies to target");
            UpdateUI();
            return;
        }

        var context = new ActionContext(_battleManager.Player)
        {
            Target = target,
            TargetNode = _enemyNodes.GetValueOrDefault(target.Id)
        };

        _battleManager.PlayerAction(action, context);
        _executedPlayerActions.Add(actionId);
        CleanupDefeatedEnemies();
        UpdateUI();

        if (_battleManager.IsBattleEnd)
        {
            OnBattleEnd();
        }
    }

    private ActorState? GetSelectedEnemy()
    {
        var selected = _enemyList.GetSelectedItems();
        if (selected.Length == 0)
            return null;

        var enemyIndex = (int)selected[0];
        return enemyIndex >= 0 && enemyIndex < _battleManager.Enemies.Count
            ? _battleManager.Enemies[enemyIndex]
            : null;
    }

    private void CleanupDefeatedEnemies()
    {
        var aliveIds = _battleManager.Enemies.Select(enemy => enemy.Id).ToHashSet();
        var removedIds = _enemyNodes.Keys.Where(id => !aliveIds.Contains(id)).ToList();
        foreach (var removedId in removedIds)
        {
            _enemyNodes.Remove(removedId);
        }
    }

    private void UpdateUI()
    {
        _playerHpLabel.Text = $"HP: {_battleManager.Player.CurrentHp}/{_battleManager.Player.MaxHp}";
        _playerApLabel.Text = $"AP: {_battleManager.Player.CurrentAp}/{_battleManager.Player.MaxAp}";
        _playerHpBar.MaxValue = _battleManager.Player.MaxHp;
        _playerHpBar.Value = _battleManager.Player.CurrentHp;
        _playerHpBar.TooltipText = _playerHpLabel.Text;
        _playerApBar.MaxValue = _battleManager.Player.MaxAp;
        _playerApBar.Value = _battleManager.Player.CurrentAp;
        _playerApBar.TooltipText = _playerApLabel.Text;
        _turnCounterLabel.Text = $"Turn: {_battleManager.TurnCount} / State: {_battleManager.CurrentState}";
        _playerStatusLabel.Text = $"Status: {FormatStatusEffects(_battleManager.StatusEffects.GetEffects(_battleManager.Player.Id))}";

        var selectedEnemyId = GetSelectedEnemy()?.Id;

        _enemyList.Clear();
        var selectedIndex = -1;
        for (int i = 0; i < _battleManager.Enemies.Count; i++)
        {
            var enemy = _battleManager.Enemies[i];
            var node = _enemyNodes.GetValueOrDefault(enemy.Id);
            var kind = node switch
            {
                FolderNode => "Folder",
                SpecialFileNode => "Special",
                _ => "File"
            };
            var effects = _battleManager.StatusEffects.GetEffects(enemy.Id);
            var threat = GetThreatMarker(enemy, effects, node);
            var statusSuffix = effects.Count > 0 ? $" · {FormatCompactStatusEffects(effects)}" : string.Empty;
            var display = $"{threat} {enemy.DisplayName} [{kind}] - HP {enemy.CurrentHp}/{enemy.MaxHp}, AP {enemy.CurrentAp}/{enemy.MaxAp}{statusSuffix}";
            _enemyList.AddItem(display);
            ApplyEnemyListItemStyle(i, enemy, effects, node);

            if (enemy.Id == selectedEnemyId)
                selectedIndex = i;
        }

        if (_battleManager.Enemies.Count > 0)
        {
            if (selectedIndex < 0)
                selectedIndex = 0;

            _enemyList.Select(selectedIndex);
            HighlightSelectedEnemy(selectedIndex);
        }

        _battleLogLabel.Clear();
        foreach (var log in _battleManager.BattleLog.TakeLast(BattleConstants.UIBattleLogDisplayLines))
        {
            _battleLogLabel.AppendText(FormatBattleLog(log) + "\n");
        }

        if (_battleLogLabel.GetLineCount() > 0)
        {
            _battleLogLabel.ScrollToLine(_battleLogLabel.GetLineCount() - 1);
        }

        UpdateSelectedEnemyPanel();
        UpdateActionButtons();
        UpdateBattleEndOverlay();
    }

    private void UpdateSelectedEnemyPanel()
    {
        var selectedEnemy = GetSelectedEnemy() ?? _battleManager.Enemies.FirstOrDefault(e => e.IsAlive);
        var targetNode = selectedEnemy != null ? _enemyNodes.GetValueOrDefault(selectedEnemy.Id) : null;

        if (selectedEnemy == null || targetNode == null)
        {
            _selectedEnemyNameLabel.Text = "No target selected";
            _selectedEnemyTypeLabel.Text = "Type: -";
            _selectedEnemyPathLabel.Text = "Path: -";
            _selectedEnemyStatsLabel.Text = "ATK: - · Size: -";
            _selectedEnemyStatusLabel.Text = "Status: None";
            _selectedEnemyHpBar.MaxValue = 1;
            _selectedEnemyHpBar.Value = 0;
            _selectedEnemyApBar.MaxValue = 1;
            _selectedEnemyApBar.Value = 0;
            return;
        }

        var kind = targetNode switch
        {
            FolderNode => "Folder",
            SpecialFileNode => "Special File",
            _ => "File"
        };

        _selectedEnemyNameLabel.Text = selectedEnemy.DisplayName;
        _selectedEnemyTypeLabel.Text = $"Type: {kind}";
        _selectedEnemyPathLabel.Text = $"Path: {targetNode.Path}";
        _selectedEnemyStatsLabel.Text = $"ATK: {selectedEnemy.AttackPower} · Size: {targetNode.Size}";
        _selectedEnemyStatusLabel.Text = $"Status: {FormatStatusEffects(_battleManager.StatusEffects.GetEffects(selectedEnemy.Id))}";

        _selectedEnemyHpBar.MaxValue = selectedEnemy.MaxHp;
        _selectedEnemyHpBar.Value = selectedEnemy.CurrentHp;
        _selectedEnemyHpBar.TooltipText = $"HP: {selectedEnemy.CurrentHp}/{selectedEnemy.MaxHp}";

        _selectedEnemyApBar.MaxValue = selectedEnemy.MaxAp;
        _selectedEnemyApBar.Value = selectedEnemy.CurrentAp;
        _selectedEnemyApBar.TooltipText = $"AP: {selectedEnemy.CurrentAp}/{selectedEnemy.MaxAp}";
    }

    private void UpdateActionButtons()
    {
        var selectedEnemy = GetSelectedEnemy() ?? _battleManager.Enemies.FirstOrDefault(e => e.IsAlive);
        var targetNode = selectedEnemy != null ? _enemyNodes.GetValueOrDefault(selectedEnemy.Id) : null;

        foreach (var pair in _actionButtons)
        {
            var action = _actionRegistry.GetAction(pair.Key);
            if (action == null)
                continue;

            var previewContext = new ActionContext(_battleManager.Player)
            {
                Target = selectedEnemy,
                TargetNode = targetNode,
                Clipboard = _battleManager.Clipboard,
                StatusEffects = _battleManager.StatusEffects,
                AllActors = _battleManager.Enemies
            };

            var canExecute = _battleManager.CurrentState == BattleState.PlayerTurn && action.CanExecute(previewContext);
            pair.Value.Disabled = !canExecute;
            ApplyActionButtonStyle(pair.Value, action, canExecute);
        }
    }

    private void OnBattleEnd()
    {
        _battleManager.AddLog(_battleManager.IsPlayerAlive
            ? "Victory! File system clean."
            : "Defeat... System compromised.");
        UpdateUI();
    }

    private void UpdateBattleEndOverlay()
    {
        var isBattleEnd = _battleManager.IsBattleEnd;
        _battleEndOverlay.Visible = isBattleEnd;

        if (!isBattleEnd)
            return;

        var didWin = _battleManager.IsPlayerAlive;
        _battleEndTitleLabel.Text = didWin ? "Victory" : "Defeat";
        _battleEndSummaryLabel.Text = didWin
            ? $"{_battleManager.TurnCount}턴 만에 전투를 정리했어. 다시 플레이하거나 메뉴로 돌아갈 수 있어."
            : "플레이어가 쓰러졌어. 바로 재도전하거나 메뉴로 돌아갈 수 있어.";

        var totalActions = _executedPlayerActions.Count;
        var uniqueActions = _executedPlayerActions.Distinct().Count();
        var defeatedEnemies = Math.Max(0, 3 - _battleManager.Enemies.Count);
        _battleEndStatsLabel.Text = $"사용 액션 {totalActions}회 · 액션 종류 {uniqueActions}개 · 정리한 적 {defeatedEnemies}체";
    }

    private void RestartBattle()
    {
        GetTree().ReloadCurrentScene();
    }

    private void BackToMenu()
    {
        GetTree().ChangeSceneToFile("res://res/scenes/main.tscn");
    }

    private async void RunSmokeTestIfRequested()
    {
        if (!HasAutomationArg("--projectfr-smoke-test"))
            return;

        GD.Print("[ProjectFR] Smoke test starting.");

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        var scriptedSteps = new (string TargetName, string ActionId)[]
        {
            ("BuildCache", "copy"),
            ("Boss.zip", "paste"),
            ("Boss.zip", "delete"),
            ("Readme.txt", "delete")
        };

        var executedActions = new List<string>();

        foreach (var step in scriptedSteps)
        {
            if (!TrySelectEnemy(step.TargetName))
            {
                GD.PushError($"[ProjectFR] Smoke test target not found: {step.TargetName}");
                GetTree().Quit(1);
                return;
            }

            if (!CanExecuteAction(step.ActionId))
            {
                GD.PushError($"[ProjectFR] Smoke test action unavailable: {step.ActionId} on {step.TargetName}");
                GetTree().Quit(1);
                return;
            }

            executedActions.Add($"{step.ActionId}@{step.TargetName}");
            GD.Print($"[ProjectFR] Smoke action: {step.ActionId} -> {step.TargetName}");
            OnActionButtonPressed(step.ActionId);

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            if (!_battleManager.IsPlayerAlive)
            {
                GD.PushError("[ProjectFR] Smoke test ended with player defeat.");
                GetTree().Quit(1);
                return;
            }
        }

        GD.Print($"[ProjectFR] Smoke test finished. PlayerAlive={_battleManager.IsPlayerAlive}, RemainingEnemies={_battleManager.Enemies.Count}, Actions=[{string.Join(", ", executedActions)}]");
        GetTree().Quit(0);
    }

    private bool TrySelectEnemy(string displayName)
    {
        for (int index = 0; index < _battleManager.Enemies.Count; index++)
        {
            if (_battleManager.Enemies[index].DisplayName != displayName)
                continue;

            _enemyList.Select(index);
            OnEnemySelected(index);
            return true;
        }

        return false;
    }

    private void ApplyEnemyListItemStyle(int index, ActorState enemy, IReadOnlyList<StatusEffectInstance> effects, NodeData? node)
    {
        _enemyList.SetItemCustomFgColor(index, GetEnemyListColor(enemy, effects, node));
        _enemyList.SetItemTooltip(index, BuildEnemyTooltip(enemy, effects, node));
    }

    private void HighlightSelectedEnemy(int selectedIndex)
    {
        for (int i = 0; i < _battleManager.Enemies.Count; i++)
        {
            var enemy = _battleManager.Enemies[i];
            var node = _enemyNodes.GetValueOrDefault(enemy.Id);
            var effects = _battleManager.StatusEffects.GetEffects(enemy.Id);
            var color = GetEnemyListColor(enemy, effects, node);

            if (i == selectedIndex)
            {
                color = color.Lightened(0.2f);
            }

            _enemyList.SetItemCustomFgColor(i, color);
        }
    }

    private static string GetThreatMarker(ActorState enemy, IReadOnlyList<StatusEffectInstance> effects, NodeData? node)
    {
        if (effects.Any(effect => effect.Type == StatusEffect.Quarantine))
            return "◆";

        if (node is SpecialFileNode || enemy.AttackPower >= 3)
            return "▲";

        if (enemy.CurrentHp <= Math.Max(1, enemy.MaxHp / 3))
            return "▼";

        return "•";
    }

    private static Color GetEnemyListColor(ActorState enemy, IReadOnlyList<StatusEffectInstance> effects, NodeData? node)
    {
        if (effects.Any(effect => effect.Type == StatusEffect.Quarantine))
            return new Color(0.82f, 0.67f, 1.0f);

        if (effects.Any(effect => effect.Type == StatusEffect.Compressed))
            return new Color(0.55f, 0.88f, 0.66f);

        if (node is SpecialFileNode || enemy.AttackPower >= 3)
            return new Color(1.0f, 0.66f, 0.4f);

        if (enemy.CurrentHp <= Math.Max(1, enemy.MaxHp / 3))
            return new Color(0.98f, 0.47f, 0.45f);

        return new Color(0.83f, 0.87f, 0.92f);
    }

    private static string BuildEnemyTooltip(ActorState enemy, IReadOnlyList<StatusEffectInstance> effects, NodeData? node)
    {
        var type = node switch
        {
            FolderNode => "Folder",
            SpecialFileNode => "Special File",
            _ => "File"
        };

        return $"{enemy.DisplayName}\nType: {type}\nATK: {enemy.AttackPower}\nStatus: {FormatStatusEffects(effects)}";
    }

    private static string FormatStatusEffects(IReadOnlyList<StatusEffectInstance> effects)
    {
        if (effects.Count == 0)
            return "None";

        return string.Join(", ", effects.Select(FormatStatusEffect));
    }

    private static string FormatCompactStatusEffects(IReadOnlyList<StatusEffectInstance> effects)
    {
        if (effects.Count == 0)
            return string.Empty;

        return string.Join(", ", effects.Select(effect => $"{GetStatusShortName(effect.Type)} {effect.Duration}T"));
    }

    private static string FormatStatusEffect(StatusEffectInstance effect)
    {
        var suffix = effect.Magnitude != 0 ? $" {effect.Magnitude:+#;-#;0}" : string.Empty;
        return $"{GetStatusShortName(effect.Type)} {effect.Duration}T{suffix}";
    }

    private static string GetStatusShortName(StatusEffect effect) => effect switch
    {
        StatusEffect.Quarantine => "Quarantine",
        StatusEffect.Compressed => "Compressed",
        StatusEffect.Corrupted => "Corrupted",
        _ => effect.ToString()
    };

    private static string FormatBattleLog(string log)
    {
        var color = log switch
        {
            var text when text.StartsWith("Player:") => "#7ee787",
            var text when text.Contains("Victory") => "#79c0ff",
            var text when text.Contains("Defeat") || text.Contains("defeated") || text.Contains("compromised") => "#ff7b72",
            var text when text.StartsWith("--- Turn") => "#a5d6ff",
            var text when text.Contains("cannot act") => "#d2a8ff",
            var text when text.Contains("Action not found") || text.Contains("No alive enemies") || text.Contains("Not player's turn") => "#f2cc60",
            var text when text.Contains(":") => "#ffa657",
            _ => "#c9d1d9"
        };

        return $"[color={color}]{log}[/color]";
    }

    private static void ApplyActionButtonStyle(Button button, IAction action, bool canExecute)
    {
        button.Modulate = canExecute
            ? GetActionReadyColor(action.ActionId)
            : new Color(0.55f, 0.58f, 0.64f, 0.9f);
    }

    private static Color GetActionReadyColor(string actionId) => actionId switch
    {
        "open" => new Color(0.47f, 0.78f, 1.0f),
        "inspect" => new Color(0.76f, 0.67f, 1.0f),
        "delete" => new Color(1.0f, 0.49f, 0.45f),
        "copy" => new Color(0.47f, 0.9f, 0.72f),
        "cut" => new Color(1.0f, 0.64f, 0.4f),
        "paste" => new Color(0.98f, 0.79f, 0.37f),
        "clean" => new Color(0.42f, 0.86f, 0.86f),
        "quarantine" => new Color(0.83f, 0.59f, 1.0f),
        "compress" => new Color(0.59f, 0.92f, 0.6f),
        _ => Colors.White
    };

    private bool CanExecuteAction(string actionId)
    {
        return _actionButtons.TryGetValue(actionId, out var button) && !button.Disabled;
    }

    private static bool HasAutomationArg(string arg)
    {
        return OS.GetCmdlineUserArgs().Contains(arg);
    }

    private static string GetTooltipText(string actionId) => actionId switch
    {
        "open" => "Basic attack",
        "inspect" => "Free information check",
        "delete" => "High damage single target",
        "copy" => "Copy target to clipboard",
        "cut" => "Damage and cut target to clipboard",
        "paste" => "Paste from clipboard with bonus effect",
        "clean" => "AoE damage + clear own status effects",
        "quarantine" => "Prevent enemy action",
        "compress" => "Reduce enemy attack power",
        _ => string.Empty
    };
}
