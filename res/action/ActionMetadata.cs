using Godot;
using ProjectFR.Action.Implementations;

namespace ProjectFR.Action;

public static class ActionMetadata
{
    public static string GetTooltipText(string actionId) => actionId switch
    {
        ActionIds.Open => "Basic attack",
        ActionIds.Inspect => "Free information check",
        ActionIds.Delete => "High damage single target",
        ActionIds.Copy => "Copy target to clipboard",
        ActionIds.Cut => "Damage and cut target to clipboard",
        ActionIds.Paste => "Paste from clipboard with bonus effect",
        ActionIds.Clean => "AoE damage + clear own status effects",
        ActionIds.Quarantine => $"Prevent enemy action for {ActionConstants.QuarantineEffectDuration} turns",
        ActionIds.Compress => $"Reduce enemy attack by {Math.Abs(ActionConstants.CompressAttackModifier)} for {ActionConstants.CompressEffectDuration} turns",
        ActionIds.LogForge => "Rewrite records to reduce trace after a delay",
        ActionIds.Search => "Scan for signatures and reveal clues, but raises trace",
        ActionIds.Sort => "Reorder listing to surface targets and reduce scanning friction",
        ActionIds.ShowHidden => "Show hidden layers and partially break pouch masking",
        ActionIds.Move => "Relocate target to another container path",
        ActionIds.Extract => "Extract packaged data at a valid extraction point",
        ActionIds.Inject => "Inject payload into the target for tactical disruption",
        ActionIds.Stun => "Stagger target actions for a short window",
        ActionIds.Decoy => "Deploy a decoy to redirect security attention",
        ActionIds.PermissionOverride => "Break a Firewall Sentinel lock to force access. Side effect: exposes any pouch-masked files and raises Trace.",
        _ => string.Empty
    };

    public static Color GetReadyColor(string actionId) => actionId switch
    {
        ActionIds.Open => new Color(0.47f, 0.78f, 1.0f),
        ActionIds.Inspect => new Color(0.76f, 0.67f, 1.0f),
        ActionIds.Delete => new Color(1.0f, 0.49f, 0.45f),
        ActionIds.Copy => new Color(0.47f, 0.9f, 0.72f),
        ActionIds.Cut => new Color(1.0f, 0.64f, 0.4f),
        ActionIds.Paste => new Color(0.98f, 0.79f, 0.37f),
        ActionIds.Clean => new Color(0.42f, 0.86f, 0.86f),
        ActionIds.Quarantine => new Color(0.83f, 0.59f, 1.0f),
        ActionIds.Compress => new Color(0.59f, 0.92f, 0.6f),
        ActionIds.LogForge => new Color(0.86f, 0.82f, 1.0f),
        ActionIds.Search => new Color(0.98f, 0.88f, 0.49f),
        ActionIds.Sort => new Color(0.78f, 0.86f, 0.98f),
        ActionIds.ShowHidden => new Color(0.72f, 0.89f, 1.0f),
        ActionIds.Move => new Color(0.64f, 0.9f, 0.78f),
        ActionIds.Extract => new Color(0.55f, 0.95f, 0.9f),
        ActionIds.Inject => new Color(0.99f, 0.63f, 0.63f),
        ActionIds.Stun => new Color(0.87f, 0.77f, 1.0f),
        ActionIds.Decoy => new Color(0.9f, 0.82f, 0.62f),
        ActionIds.PermissionOverride => new Color(1.0f, 0.72f, 0.52f),
        _ => Colors.White
    };
}
