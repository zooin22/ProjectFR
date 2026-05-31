namespace ProjectFR.Skills;

public enum SkillNodeStatus
{
    Success,
    Failure,
    Running
}

public abstract class SkillBehaviorNode
{
    public abstract SkillNodeStatus Tick(SkillExecutionContext context);
}

public sealed class SkillSequenceNode(params SkillBehaviorNode[] children) : SkillBehaviorNode
{
    public override SkillNodeStatus Tick(SkillExecutionContext context)
    {
        foreach (var child in children)
        {
            var status = child.Tick(context);
            if (status != SkillNodeStatus.Success)
                return status;
        }

        return SkillNodeStatus.Success;
    }
}

public sealed class SkillSelectorNode(params SkillBehaviorNode[] children) : SkillBehaviorNode
{
    public override SkillNodeStatus Tick(SkillExecutionContext context)
    {
        foreach (var child in children)
        {
            var status = child.Tick(context);
            if (status != SkillNodeStatus.Failure)
                return status;
        }

        return SkillNodeStatus.Failure;
    }
}

public sealed class SkillConditionNode(Func<SkillExecutionContext, bool> predicate) : SkillBehaviorNode
{
    public override SkillNodeStatus Tick(SkillExecutionContext context)
    {
        return predicate(context) ? SkillNodeStatus.Success : SkillNodeStatus.Failure;
    }
}

public sealed class SkillActionNode(Func<SkillExecutionContext, SkillNodeStatus> action) : SkillBehaviorNode
{
    public override SkillNodeStatus Tick(SkillExecutionContext context)
    {
        return action(context);
    }
}
