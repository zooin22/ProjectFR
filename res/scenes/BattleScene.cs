using Godot;
using ProjectFR.Action;
using ProjectFR.Action.Implementations;
using ProjectFR.Battle;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;
using System.Threading.Tasks;

namespace ProjectFR.Scenes;

/// <summary>
/// Battle scene controller - manages the UI and game flow during battle
/// </summary>
public partial class BattleScene : Node
{
    private BattleManager _battleManager = null!;
    private ActionRegistry _actionRegistry = null!;
    private Label _playerHpLabel = null!;
    private Label _playerApLabel = null!;
    private ItemList _enemyList = null!;
    private RichTextLabel _battleLogLabel = null!;
    private GridContainer _actionButtonsContainer = null!;
    private Label _turnCounterLabel = null!;
    private readonly Dictionary<string, Button> _actionButtons = new();
    private readonly Dictionary<string, NodeData> _enemyNodes = new();

    /// <summary>
    /// Called when the scene is ready - initializes UI and battle
    /// </summary>
    public override void _Ready()
    {
        InitializeUI();
        InitializeBattle();
        UpdateUI();
        RunSmokeTestIfRequested();
    }

    /// <summary>
    /// Initializes all UI elements from the scene tree
    /// </summary>
    private void InitializeUI()
    {
        _playerHpLabel = GetNode<Label>("VBoxContainer/PlayerInfoPanel/PlayerInfoMargin/HBoxContainer/HPLabel");
        _playerApLabel = GetNode<Label>("VBoxContainer/PlayerInfoPanel/PlayerInfoMargin/HBoxContainer/APLabel");
        _enemyList = GetNode<ItemList>("VBoxContainer/HBoxContainer/EnemyPanel/EnemyMargin/EnemyVBox/EnemyList");
        _battleLogLabel = GetNode<RichTextLabel>("VBoxContainer/HBoxContainer/BattleLogPanel/BattleLogMargin/BattleLogVBox/BattleLog");
        _actionButtonsContainer = GetNode<GridContainer>("VBoxContainer/ActionsPanel/ActionsMargin/ActionsVBox/GridContainer");
        _turnCounterLabel = GetNode<Label>("VBoxContainer/TopPanel/TopMargin/TopVBox/TurnCounterLabel");

        _enemyList.SelectMode = ItemList.SelectModeEnum.Single;
        _enemyList.ItemSelected += OnEnemySelected;

        CreateActionButtons();
    }

    /// <summary>
    /// Creates action buttons dynamically from the action registry
    /// </summary>
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

    /// <summary>
    /// Initializes the battle with player and enemies
    /// </summary>
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
    }

    /// <summary>
    /// Adds a dummy enemy with associated node data
    /// </summary>
    /// <param name="enemy">The enemy actor state</param>
    /// <param name="nodeData">The file/folder node representing this enemy</param>
    private void AddDummyEnemy(ActorState enemy, NodeData nodeData)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        ArgumentNullException.ThrowIfNull(nodeData);
        
        _battleManager.AddEnemy(enemy);
        _enemyNodes[enemy.Id] = nodeData;
    }

    /// <summary>
    /// Process function called every frame - handles input
    /// </summary>
    /// <param name="delta">Time since last frame in seconds</param>
    public override void _Process(double delta)
    {
        HandleInput();
    }

    /// <summary>
    /// Handles keyboard input for action execution
    /// </summary>
    private void HandleInput()
    {
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

    /// <summary>
    /// Checks if a specified input action was just pressed
    /// </summary>
    /// <param name="actionName">The name of the input action to check</param>
    /// <returns>True if the action was just pressed, false otherwise</returns>
    private static bool ActionPressed(string actionName)
    {
        return InputMap.HasAction(actionName) && Input.IsActionJustPressed(actionName);
    }

    /// <summary>
    /// Called when an enemy is selected in the enemy list
    /// </summary>
    /// <param name="index">The index of the selected enemy</param>
    private void OnEnemySelected(long index)
    {
        UpdateActionButtons();
    }

    /// <summary>
    /// Called when an action button is pressed
    /// </summary>
    /// <param name="actionId">The ID of the action to execute</param>
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
        CleanupDefeatedEnemies();
        UpdateUI();

        if (_battleManager.IsBattleEnd)
        {
            OnBattleEnd();
        }
    }

    /// <summary>
    /// Gets the currently selected enemy from the list
    /// </summary>
    /// <returns>The selected enemy actor state, or null if none selected</returns>
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

    /// <summary>
    /// Removes defeated enemies from the tracking dictionary
    /// </summary>
    private void CleanupDefeatedEnemies()
    {
        var aliveIds = _battleManager.Enemies.Select(enemy => enemy.Id).ToHashSet();
        var removedIds = _enemyNodes.Keys.Where(id => !aliveIds.Contains(id)).ToList();
        foreach (var removedId in removedIds)
        {
            _enemyNodes.Remove(removedId);
        }
    }

    /// <summary>
    /// Updates all UI elements to reflect current battle state
    /// </summary>
    private void UpdateUI()
    {
        _playerHpLabel.Text = $"HP: {_battleManager.Player.CurrentHp}/{_battleManager.Player.MaxHp}";
        _playerApLabel.Text = $"AP: {_battleManager.Player.CurrentAp}/{_battleManager.Player.MaxAp}";
        _turnCounterLabel.Text = $"Turn: {_battleManager.TurnCount} / State: {_battleManager.CurrentState}";

        _enemyList.Clear();
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
            var display = $"{enemy.DisplayName} [{kind}] - HP {enemy.CurrentHp}/{enemy.MaxHp}, AP {enemy.CurrentAp}/{enemy.MaxAp}";
            _enemyList.AddItem(display);
        }

        if (_battleManager.Enemies.Count > 0)
        {
            _enemyList.Select(Mathf.Clamp(_enemyList.GetSelectedItems().FirstOrDefault(), 0, _battleManager.Enemies.Count - 1));
        }

        _battleLogLabel.Clear();
        foreach (var log in _battleManager.BattleLog.TakeLast(BattleConstants.UIBattleLogDisplayLines))
        {
            _battleLogLabel.AppendText(log + "\n");
        }

        if (_battleLogLabel.GetLineCount() > 0)
        {
            _battleLogLabel.ScrollToLine(_battleLogLabel.GetLineCount() - 1);
        }

        UpdateActionButtons();
    }

    /// <summary>
    /// Updates the enabled/disabled state of action buttons based on available actions
    /// </summary>
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

            pair.Value.Disabled = _battleManager.CurrentState != BattleState.PlayerTurn || !action.CanExecute(previewContext);
        }
    }

    /// <summary>
    /// Called when the battle ends to display victory/defeat message
    /// </summary>
    private void OnBattleEnd()
    {
        _battleManager.AddLog(_battleManager.IsPlayerAlive
            ? "Victory! File system clean."
            : "Defeat... System compromised.");
        UpdateUI();
    }

    /// <summary>
    /// Runs a deterministic smoke test when launched with automation arguments.
    /// </summary>
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
            ("Boss.zip", "compress"),
            ("Boss.zip", "delete"),
            ("Readme.txt", "cut"),
            ("BuildCache", "clean")
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

    private bool CanExecuteAction(string actionId)
    {
        return _actionButtons.TryGetValue(actionId, out var button) && !button.Disabled;
    }

    private static bool HasAutomationArg(string arg)
    {
        return OS.GetCmdlineUserArgs().Contains(arg);
    }

    /// <summary>
    /// Gets tooltip text for an action based on its ID
    /// </summary>
    /// <param name="actionId">The action ID</param>
    /// <returns>The tooltip text describing the action</returns>
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
