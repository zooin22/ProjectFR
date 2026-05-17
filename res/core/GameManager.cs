using Godot;
using ProjectFR.Battle;
using ProjectFR.Data;
using ProjectFR.Systems;

namespace ProjectFR.Core;

/// <summary>
/// Global game manager singleton - manages game state and battle lifecycle
/// </summary>
public partial class GameManager : Node
{
    /// <summary>
    /// Singleton instance of the GameManager
    /// </summary>
    public static GameManager Instance { get; private set; } = null!;

    /// <summary>
    /// Current active battle, or null if no battle is in progress
    /// </summary>
    public BattleManager? CurrentBattle { get; private set; }
    
    /// <summary>
    /// Global clipboard system shared across battles
    /// </summary>
    public ClipboardSystem Clipboard { get; } = new();
    
    /// <summary>
    /// Global status effect system shared across battles
    /// </summary>
    public StatusEffectSystem StatusEffects { get; } = new();

    /// <summary>
    /// Called when the node enters the scene tree
    /// </summary>
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

    /// <summary>
    /// Starts a new battle with default player and enemies
    /// </summary>
    public void StartNewBattle()
    {
        CurrentBattle = new BattleManager(BattleFactory.CreateDefaultPlayer());
        CurrentBattle.StartBattle();
    }

    /// <summary>
    /// Ends the current battle
    /// </summary>
    public void EndCurrentBattle()
    {
        CurrentBattle = null;
    }
}
