using Godot;

namespace ProjectFR.Scenes;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        var startButton = GetNode<Button>("RootMargin/MainVBox/HeroPanel/HeroMargin/HeroVBox/PrimaryButtonRow/StartBattleButton");
        startButton.Pressed += OnStartBattlePressed;

        if (HasAutomationArg("--projectfr-autostart-battle"))
        {
            CallDeferred(MethodName.OnStartBattlePressed);
        }
    }

    private void OnStartBattlePressed()
    {
        GetTree().ChangeSceneToFile("res://res/scenes/BattleScene.tscn");
    }

    private static bool HasAutomationArg(string arg)
    {
        return OS.GetCmdlineUserArgs().Contains(arg);
    }
}
