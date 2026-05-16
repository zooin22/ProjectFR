using Godot;
using ProjectFR.Action;
using ProjectFR.Data;

namespace ProjectFR.Data.Actions;

[GlobalClass]
public partial class ContextActionData : Resource
{
    [Export]
    public string ActionId { get; set; } = "";

    [Export]
    public string DisplayName { get; set; } = "";

    [Export]
    public int ApCost { get; set; } = 1;

    [Export]
    public TargetType Scope { get; set; } = TargetType.Single;

    [Export]
    public Godot.Collections.Array<Resource> Conditions { get; set; } = new();

    public override void _ValidateProperty(Godot.Collections.Dictionary property)
    {
        base._ValidateProperty(property);
    }
}
