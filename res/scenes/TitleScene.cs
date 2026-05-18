using Godot;

namespace ProjectFR.Scenes;

public partial class TitleScene : Control
{
    public override void _Ready()
    {
        if (HasAutomationArg("--projectfr-autostart-battle") || HasAutomationArg("--projectfr-smoke-test"))
        {
            CallDeferred(MethodName.GoToBattleScene);
            return;
        }

        var startButton = GetNode<Button>("RootMargin/MainVBox/ButtonRow/StartButton");
        var quitButton = GetNode<Button>("RootMargin/MainVBox/ButtonRow/QuitButton");

        startButton.Pressed += OnStartPressed;
        quitButton.Pressed += OnQuitPressed;
    }

    private void OnStartPressed()
    {
        GetTree().ChangeSceneToFile("res://res/scenes/main.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    private void GoToBattleScene()
    {
        GetTree().ChangeSceneToFile("res://res/scenes/BattleScene.tscn");
    }

    private static bool HasAutomationArg(string arg)
    {
        return OS.GetCmdlineUserArgs().Contains(arg);
    }
}
