using Godot;
using ProjectFR.Core;

namespace ProjectFR.Scenes;

public partial class TitleScene : Control
{
    public override void _Ready()
    {
        DebugLog.Initialize();
        DebugLog.Info(nameof(TitleScene), "_Ready enter");

        try
        {
            if (HasAutomationArg("--projectfr-smoke-test-menu-flow"))
            {
                DebugLog.Info(nameof(TitleScene), "automation -> main menu");
                CallDeferred(MethodName.GoToMainMenu);
                return;
            }

            if (HasAutomationArg("--projectfr-autostart-battle") || HasAutomationArg("--projectfr-smoke-test"))
            {
                DebugLog.Info(nameof(TitleScene), "automation -> battle scene");
                CallDeferred(MethodName.GoToBattleScene);
                return;
            }

            var startButton = GetNode<Button>("RootMargin/MainVBox/ButtonRow/StartButton");
            var quitButton = GetNode<Button>("RootMargin/MainVBox/ButtonRow/QuitButton");

            startButton.Pressed += OnStartPressed;
            quitButton.Pressed += OnQuitPressed;
            DebugLog.Info(nameof(TitleScene), "_Ready exit");
        }
        catch (Exception exception)
        {
            DebugLog.Exception(nameof(TitleScene), exception, "_Ready failed");
            throw;
        }
    }

    private void OnStartPressed()
    {
        DebugLog.Info(nameof(TitleScene), "start pressed -> main menu");
        GoToMainMenu();
    }

    private void OnQuitPressed()
    {
        DebugLog.Info(nameof(TitleScene), "quit pressed");
        GetTree().Quit();
    }

    private void GoToBattleScene()
    {
        DebugLog.Info(nameof(TitleScene), "ChangeSceneToFile -> BattleScene.tscn");
        GetTree().ChangeSceneToFile("res://res/scenes/BattleScene.tscn");
    }

    private void GoToMainMenu()
    {
        DebugLog.Info(nameof(TitleScene), "ChangeSceneToFile -> main.tscn");
        GetTree().ChangeSceneToFile("res://res/scenes/main.tscn");
    }

    private static bool HasAutomationArg(string arg)
    {
        return OS.GetCmdlineUserArgs().Contains(arg);
    }
}
