namespace ProjectFR.Infiltration;

public enum SecurityBehaviorStatus
{
    Success,
    Failure,
    Running
}

public abstract class SecurityBehaviorNode
{
    public abstract SecurityBehaviorStatus Tick(SecurityBehaviorContext context);
}

public sealed class SecuritySequenceNode(params SecurityBehaviorNode[] children) : SecurityBehaviorNode
{
    public override SecurityBehaviorStatus Tick(SecurityBehaviorContext context)
    {
        foreach (var child in children)
        {
            var status = child.Tick(context);
            if (status != SecurityBehaviorStatus.Success)
                return status;
        }

        return SecurityBehaviorStatus.Success;
    }
}

public sealed class SecuritySelectorNode(params SecurityBehaviorNode[] children) : SecurityBehaviorNode
{
    public override SecurityBehaviorStatus Tick(SecurityBehaviorContext context)
    {
        foreach (var child in children)
        {
            var status = child.Tick(context);
            if (status != SecurityBehaviorStatus.Failure)
                return status;
        }

        return SecurityBehaviorStatus.Failure;
    }
}

public sealed class SecurityConditionNode(Func<SecurityBehaviorContext, bool> predicate) : SecurityBehaviorNode
{
    public override SecurityBehaviorStatus Tick(SecurityBehaviorContext context)
    {
        return predicate(context) ? SecurityBehaviorStatus.Success : SecurityBehaviorStatus.Failure;
    }
}

public sealed class SecurityActionNode(Func<SecurityBehaviorContext, SecurityBehaviorStatus> action) : SecurityBehaviorNode
{
    public override SecurityBehaviorStatus Tick(SecurityBehaviorContext context)
    {
        return action(context);
    }
}
