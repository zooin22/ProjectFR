using Godot;
using ProjectFR.Action;
using ProjectFR.Action.Implementations;
using ProjectFR.Battle;
using ProjectFR.Data;

namespace ProjectFR.Scenes;

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

    private Dictionary<string, Button> _actionButtons = new();

    public override void _Ready()
    {
        InitializeUI();
        InitializeBattle();
        UpdateUI();
    }

    private void InitializeUI()
    {
        _playerHpLabel = GetNode<Label>("VBoxContainer/PlayerInfoPanel/HBoxContainer/HPLabel");
        _playerApLabel = GetNode<Label>("VBoxContainer/PlayerInfoPanel/HBoxContainer/APLabel");
        _enemyList = GetNode<ItemList>("VBoxContainer/HBoxContainer/EnemyList");
        _battleLogLabel = GetNode<RichTextLabel>("VBoxContainer/HBoxContainer/BattleLog");
        _actionButtonsContainer = GetNode<GridContainer>("VBoxContainer/ActionsPanel/GridContainer");
        _turnCounterLabel = GetNode<Label>("VBoxContainer/TurnCounterLabel");

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
                Text = action.DisplayName,
                CustomMinimumSize = new Vector2(120, 40)
            };
            button.Pressed += () => OnActionButtonPressed(action.ActionId);
            _actionButtonsContainer.AddChild(button);
            _actionButtons[action.ActionId] = button;
        }
    }

    private void InitializeBattle()
    {
        var player = new ActorState(maxHp: 30, maxAp: 5, attackPower: 4);
        _battleManager = new BattleManager(player);

        _battleManager.AddEnemy(new ActorState(maxHp: 10, maxAp: 3, attackPower: 2));
        _battleManager.AddEnemy(new ActorState(maxHp: 8, maxAp: 2, attackPower: 2));
        _battleManager.AddEnemy(new ActorState(maxHp: 15, maxAp: 4, attackPower: 3));

        _battleManager.StartBattle();
    }

    public override void _Process(double delta)
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.IsActionJustPressed("ui_open"))
            OnActionButtonPressed("open");
        else if (Input.IsActionJustPressed("ui_inspect"))
            OnActionButtonPressed("inspect");
        else if (Input.IsActionJustPressed("ui_delete"))
            OnActionButtonPressed("delete");
        else if (Input.IsActionJustPressed("ui_copy"))
            OnActionButtonPressed("copy");
        else if (Input.IsActionJustPressed("ui_cut"))
            OnActionButtonPressed("cut");
        else if (Input.IsActionJustPressed("ui_paste"))
            OnActionButtonPressed("paste");
        else if (Input.IsActionJustPressed("ui_clean"))
            OnActionButtonPressed("clean");
        else if (Input.IsActionJustPressed("ui_quarantine"))
            OnActionButtonPressed("quarantine");
        else if (Input.IsActionJustPressed("ui_compress"))
            OnActionButtonPressed("compress");
    }

    private void OnActionButtonPressed(string actionId)
    {
        if (_battleManager.CurrentState != BattleState.PlayerTurn)
        {
            GD.Print($"Not player's turn! Current state: {_battleManager.CurrentState}");
            return;
        }

        var action = _actionRegistry.GetAction(actionId);
        if (action == null)
        {
            GD.Print($"Action not found: {actionId}");
            return;
        }

        var target = _battleManager.Enemies.FirstOrDefault(e => e.IsAlive);
        if (target == null)
        {
            GD.Print("No alive enemies to target");
            return;
        }

        var context = new ActionContext(_battleManager.Player)
        {
            Target = target
        };

        _battleManager.PlayerAction(action, context);
        UpdateUI();

        if (_battleManager.IsBattleEnd)
        {
            OnBattleEnd();
        }
    }

    private void UpdateUI()
    {
        _playerHpLabel.Text = $"HP: {_battleManager.Player.CurrentHp}/{_battleManager.Player.MaxHp}";
        _playerApLabel.Text = $"AP: {_battleManager.Player.CurrentAp}/{_battleManager.Player.MaxAp}";
        _turnCounterLabel.Text = $"Turn: {_battleManager.TurnCount}";

        _enemyList.Clear();
        for (int i = 0; i < _battleManager.Enemies.Count; i++)
        {
            var enemy = _battleManager.Enemies[i];
            var display = $"Enemy {i + 1}: {enemy.CurrentHp}/{enemy.MaxHp} HP";
            _enemyList.AddItem(display);
        }

        _battleLogLabel.Clear();
        var recentLogs = _battleManager.BattleLog.TakeLast(10);
        foreach (var log in recentLogs)
        {
            _battleLogLabel.AppendText(log + "\n");
        }

        _battleLogLabel.ScrollToLine(_battleLogLabel.GetLineCount() - 1);
    }

    private void OnBattleEnd()
    {
        if (_battleManager.IsPlayerAlive)
        {
            GD.Print("Victory!");
        }
        else
        {
            GD.Print("Defeat!");
        }
    }
}
