using Godot;
using ProjectFR.Action;
using ProjectFR.Action.Implementations;
using ProjectFR.Battle;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;

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
    private readonly Dictionary<string, Button> _actionButtons = new();
    private readonly Dictionary<string, NodeData> _enemyNodes = new();

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

        _enemyList.SelectMode = ItemList.SelectModeEnum.Single;
        _enemyList.ItemSelected += OnEnemySelected;

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
        var player = new ActorState(maxHp: 30, maxAp: 5, attackPower: 4, displayName: "Player.exe");
        _battleManager = new BattleManager(player);

        AddDummyEnemy(new ActorState(maxHp: 10, maxAp: 3, attackPower: 2, displayName: "Readme.txt"), new FileNode("Readme.txt", "res://dummy/readme.txt", 4));
        AddDummyEnemy(new ActorState(maxHp: 8, maxAp: 2, attackPower: 2, displayName: "BuildCache"), new FolderNode("BuildCache", "res://dummy/buildcache"));
        AddDummyEnemy(new ActorState(maxHp: 15, maxAp: 4, attackPower: 3, displayName: "Boss.zip"), new SpecialFileNode("Boss.zip", "res://dummy/boss.zip", 16));

        _battleManager.StartBattle();
    }

    private void AddDummyEnemy(ActorState enemy, NodeData nodeData)
    {
        _battleManager.AddEnemy(enemy);
        _enemyNodes[enemy.Id] = nodeData;
    }

    public override void _Process(double delta)
    {
        HandleInput();
    }

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

    private static bool ActionPressed(string actionName)
    {
        return InputMap.HasAction(actionName) && Input.IsActionJustPressed(actionName);
    }

    private void OnEnemySelected(long index)
    {
        UpdateActionButtons();
    }

    private void OnActionButtonPressed(string actionId)
    {
        if (_battleManager.CurrentState != BattleState.PlayerTurn)
        {
            _battleManager.AddLog($"지금은 플레이어 턴이 아님: {_battleManager.CurrentState}");
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
        foreach (var log in _battleManager.BattleLog.TakeLast(12))
        {
            _battleLogLabel.AppendText(log + "\n");
        }

        if (_battleLogLabel.GetLineCount() > 0)
        {
            _battleLogLabel.ScrollToLine(_battleLogLabel.GetLineCount() - 1);
        }

        UpdateActionButtons();
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

            pair.Value.Disabled = _battleManager.CurrentState != BattleState.PlayerTurn || !action.CanExecute(previewContext);
        }
    }

    private void OnBattleEnd()
    {
        _battleManager.AddLog(_battleManager.IsPlayerAlive ? "Victory! 더미 전투 클리어" : "Defeat... 다시 시도해봐");
        UpdateUI();
    }

    private static string GetTooltipText(string actionId) => actionId switch
    {
        "open" => "기본 공격",
        "inspect" => "무료 정보 확인",
        "delete" => "고데미지 단일기",
        "copy" => "대상을 클립보드에 복사",
        "cut" => "대미지 + 클립보드 저장",
        "paste" => "클립보드 내용으로 추가 효과",
        "clean" => "전체 2대미지 + 내 상태 해제",
        "quarantine" => "행동 봉쇄",
        "compress" => "공격력 감소",
        _ => string.Empty
    };
}
