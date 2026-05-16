using Godot;

namespace ProjectFR.Scenes;

public partial class MainMenu : Node
{
    public override void _Ready()
    {
        var startButton = GetNode<Button>("SubViewport/Control/VBoxContainer/StartBattleButton");
        startButton.Pressed += OnStartBattlePressed;
    }

    private void OnStartBattlePressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/BattleScene.tscn");
    }
}
