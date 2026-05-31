using ProjectFR.Action;
using ProjectFR.Infiltration;

namespace ProjectFR.Skills;

public sealed class SkillDefinition
{
    public string ActionId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public OperationType OperationType { get; init; }
    public bool Queueable { get; init; } = true;
    public bool ShowInCommandDeck { get; init; } = true;
    public bool ShowInContextMenu { get; init; } = true;
    public string? BehaviorKey { get; init; }
    public string? BehaviorResourcePath { get; init; }

    public static SkillDefinition FromAction(IAction action, OperationType operationType, string? behaviorKey = null, string? behaviorResourcePath = null)
    {
        return new SkillDefinition
        {
            ActionId = action.ActionId,
            DisplayName = action.DisplayName,
            Description = ActionMetadata.GetTooltipText(action.ActionId),
            OperationType = operationType,
            BehaviorKey = behaviorKey,
            BehaviorResourcePath = behaviorResourcePath
        };
    }
}
