using ProjectFR.Action;
using ProjectFR.Data;
using ProjectFR.Systems;

namespace ProjectFR.Battle;

public enum BattleState
{
    PlayerTurn,
    EnemyTurn,
    EndTurn,
    BattleEnd
}

public class BattleManager
{
    public BattleState CurrentState { get; private set; } = BattleState.PlayerTurn;
    public ActorState Player { get; private set; }
    public List<ActorState> Enemies { get; private set; } = new();
    public int TurnCount { get; private set; } = 0;

    private ActionRegistry _actionRegistry;
    private StatusEffectSystem _statusEffects;
    private ClipboardSystem _clipboard;
    private List<string> _battleLog = new();

    public IReadOnlyList<string> BattleLog => _battleLog.AsReadOnly();

    public BattleManager(ActorState player)
    {
        Player = player;
        _actionRegistry = new ActionRegistry();
        _statusEffects = new StatusEffectSystem();
        _clipboard = new ClipboardSystem();
    }

    public void AddEnemy(ActorState enemy)
    {
        Enemies.Add(enemy);
    }

    public void StartBattle()
    {
        Player.RestoreAllAp();
        CurrentState = BattleState.PlayerTurn;
        TurnCount = 0;
        _battleLog.Clear();
        AddLog("Battle started!");
    }

    public void PlayerAction(IAction action, ActionContext context)
    {
        if (CurrentState != BattleState.PlayerTurn)
        {
            AddLog("Not player's turn!");
            return;
        }

        context.Clipboard = _clipboard;
        context.StatusEffects = _statusEffects;
        context.AllActors = Enemies;

        var result = action.Execute(context);
        AddLog($"Player: {result.Message}");

        if (!result.Success)
            return;

        EndPlayerTurn();
    }

    public void EndPlayerTurn()
    {
        CurrentState = BattleState.EnemyTurn;
        ProcessEnemyTurns();
    }

    private void ProcessEnemyTurns()
    {
        foreach (var enemy in Enemies)
        {
            if (!enemy.IsAlive)
                continue;

            if (_statusEffects.HasEffect(enemy.ToString() ?? "", StatusEffect.Quarantine))
            {
                AddLog($"{enemy} is quarantined and cannot act!");
                _statusEffects.UpdateDurations(enemy.ToString() ?? "");
                continue;
            }

            if (enemy.CurrentAp > 0)
            {
                PerformEnemyAction(enemy);
            }

            _statusEffects.UpdateDurations(enemy.ToString() ?? "");
        }

        CurrentState = BattleState.EndTurn;
        EndRound();
    }

    private void PerformEnemyAction(ActorState enemy)
    {
        var context = new ActionContext(enemy);
        context.Target = Player;
        context.Clipboard = _clipboard;
        context.StatusEffects = _statusEffects;
        context.AllActors = Enemies;

        var executableActions = _actionRegistry.GetExecutableActions(context);
        if (executableActions.Count == 0)
        {
            int damage = System.Math.Max(1, enemy.AttackPower + _statusEffects.GetAttackModifier(enemy.ToString() ?? ""));
            Player.TakeDamage(damage);
            AddLog($"{enemy} attacks dealing {damage} damage!");
            return;
        }

        var action = executableActions[Random.Shared.Next(executableActions.Count)];
        var result = action.Execute(context);
        AddLog($"{enemy}: {result.Message}");
    }

    private void EndRound()
    {
        TurnCount++;
        RemoveDeadEnemies();

        if (CheckBattleEnd())
        {
            CurrentState = BattleState.BattleEnd;
            return;
        }

        Player.RestoreAllAp();
        _statusEffects.UpdateDurations(Player.ToString() ?? "");

        CurrentState = BattleState.PlayerTurn;
        AddLog($"--- Turn {TurnCount} ---");
    }

    private void RemoveDeadEnemies()
    {
        Enemies.RemoveAll(e => !e.IsAlive);
    }

    private bool CheckBattleEnd()
    {
        if (!Player.IsAlive)
        {
            AddLog("Player defeated!");
            return true;
        }

        if (Enemies.Count == 0)
        {
            AddLog("All enemies defeated! Victory!");
            return true;
        }

        return false;
    }

    public void AddLog(string message)
    {
        _battleLog.Add(message);
        if (_battleLog.Count > 50)
        {
            _battleLog.RemoveAt(0);
        }
    }

    public bool IsPlayerAlive => Player.IsAlive;
    public bool HasEnemies => Enemies.Count > 0;
    public bool IsBattleEnd => CurrentState == BattleState.BattleEnd;
}
