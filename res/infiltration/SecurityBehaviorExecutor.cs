namespace ProjectFR.Infiltration;

public sealed class SecurityBehaviorExecutor
{
    public bool TryExecute(string behaviorKey, SecurityBehaviorContext context)
    {
        var behavior = SecurityBehaviorFactory.Create(behaviorKey);
        if (behavior == null)
            return false;

        return behavior.Tick(context) == SecurityBehaviorStatus.Success;
    }
}
