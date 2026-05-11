using Godot;
using ProjectFR.Battle;
using ProjectFR.Data;
using ProjectFR.Systems;

namespace ProjectFR.Core;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    public BattleManager? CurrentBattle { get; private set; }
    public ClipboardSystem Clipboard { get; } = new();
    public StatusEffectSystem StatusEffects { get; } = new();

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;
        SetProcessUnhandledInput(true);
    }

    public void StartNewBattle()
    {
        var player = new ActorState(maxHp: 30, maxAp: 5, attackPower: 4);
        CurrentBattle = new BattleManager(player);

        CurrentBattle.AddEnemy(new ActorState(maxHp: 10, maxAp: 3, attackPower: 2));
        CurrentBattle.AddEnemy(new ActorState(maxHp: 8, maxAp: 2, attackPower: 2));
        CurrentBattle.AddEnemy(new ActorState(maxHp: 15, maxAp: 4, attackPower: 3));

        CurrentBattle.StartBattle();
    }

    public void EndCurrentBattle()
    {
        CurrentBattle = null;
    }
}
