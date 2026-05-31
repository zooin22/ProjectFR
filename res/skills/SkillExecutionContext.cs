using ProjectFR.Action;
using ProjectFR.Data.Nodes;

namespace ProjectFR.Skills;

public sealed class SkillExecutionContext
{
    public SkillDefinition Definition { get; init; } = null!;
    public ActionContext ActionContext { get; init; } = null!;
    public NodeData TargetNode { get; init; } = null!;

    public required Action<NodeData> PerformSearchResponse { get; init; }
    public required Action<NodeData, string, int, string> RevealPouchMask { get; init; }
    public required Func<string, bool> IsPermissionLocked { get; init; }
    public required Action<string, string, int, int> GrantPermissionOverride { get; init; }
    public required Action<string> AppendConsoleFeed { get; init; }
}
