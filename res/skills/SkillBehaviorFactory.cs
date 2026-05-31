using ProjectFR.Infiltration;

namespace ProjectFR.Skills;

public static class SkillBehaviorFactory
{
    public static SkillBehaviorNode? Create(string? behaviorKey)
    {
        return behaviorKey switch
        {
            SkillBehaviorKeys.Search => BuildSearchBehavior(),
            SkillBehaviorKeys.ShowHidden => BuildShowHiddenBehavior(),
            SkillBehaviorKeys.PermissionOverride => BuildPermissionOverrideBehavior(),
            _ => null
        };
    }

    private static SkillBehaviorNode BuildSearchBehavior()
    {
        return new SkillActionNode(context =>
        {
            context.PerformSearchResponse(context.TargetNode);
            return SkillNodeStatus.Success;
        });
    }

    private static SkillBehaviorNode BuildShowHiddenBehavior()
    {
        return new SkillActionNode(context =>
        {
            context.RevealPouchMask(
                context.TargetNode,
                "Show Hidden pierced cheek pouch masking",
                InfiltrationTuning.ShowHiddenPouchExposeTraceIncrease,
                context.Definition.ActionId);
            return SkillNodeStatus.Success;
        });
    }

    private static SkillBehaviorNode BuildPermissionOverrideBehavior()
    {
        return new SkillSequenceNode(
            new SkillSelectorNode(
                new SkillSequenceNode(
                    new SkillConditionNode(context => context.IsPermissionLocked(context.TargetNode.Path)),
                    new SkillActionNode(context =>
                    {
                        context.GrantPermissionOverride(
                            context.TargetNode.Path,
                            $"Permission Override forced access at {context.TargetNode.Path}",
                            InfiltrationTuning.PermissionOverrideTraceIncrease,
                            InfiltrationTuning.PermissionOverrideDurationTurns);
                        context.AppendConsoleFeed($"permission override :: access granted :: {context.TargetNode.Name} :: {InfiltrationTuning.PermissionOverrideDurationTurns}T");
                        return SkillNodeStatus.Success;
                    })
                ),
                new SkillActionNode(context =>
                {
                    context.AppendConsoleFeed($"permission override :: no lock present :: {context.TargetNode.Name}");
                    return SkillNodeStatus.Success;
                })
            ),
            new SkillActionNode(context =>
            {
                context.RevealPouchMask(
                    context.TargetNode,
                    "Permission Override exposed cheek pouch cache",
                    InfiltrationTuning.PermissionOverridePouchExposeTraceIncrease,
                    context.Definition.ActionId);
                return SkillNodeStatus.Success;
            })
        );
    }
}
