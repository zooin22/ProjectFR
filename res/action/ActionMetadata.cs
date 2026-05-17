using Godot;
using ProjectFR.Action.Implementations;

namespace ProjectFR.Action;

public static class ActionMetadata
{
    public static string GetTooltipText(string actionId) => actionId switch
    {
        "open" => "Basic attack",
        "inspect" => "Free information check",
        "delete" => "High damage single target",
        "copy" => "Copy target to clipboard",
        "cut" => "Damage and cut target to clipboard",
        "paste" => "Paste from clipboard with bonus effect",
        "clean" => "AoE damage + clear own status effects",
        "quarantine" => $"Prevent enemy action for {ActionConstants.QuarantineEffectDuration} turns",
        "compress" => $"Reduce enemy attack by {Math.Abs(ActionConstants.CompressAttackModifier)} for {ActionConstants.CompressEffectDuration} turns",
        _ => string.Empty
    };

    public static Color GetReadyColor(string actionId) => actionId switch
    {
        "open" => new Color(0.47f, 0.78f, 1.0f),
        "inspect" => new Color(0.76f, 0.67f, 1.0f),
        "delete" => new Color(1.0f, 0.49f, 0.45f),
        "copy" => new Color(0.47f, 0.9f, 0.72f),
        "cut" => new Color(1.0f, 0.64f, 0.4f),
        "paste" => new Color(0.98f, 0.79f, 0.37f),
        "clean" => new Color(0.42f, 0.86f, 0.86f),
        "quarantine" => new Color(0.83f, 0.59f, 1.0f),
        "compress" => new Color(0.59f, 0.92f, 0.6f),
        _ => Colors.White
    };
}
