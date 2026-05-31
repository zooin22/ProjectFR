namespace ProjectFR.Skills;

public sealed class SkillExecutor
{
    private readonly SkillCatalog _catalog;

    public SkillExecutor(SkillCatalog catalog)
    {
        _catalog = catalog;
    }

    public SkillDefinition? GetDefinition(string actionId)
    {
        return _catalog.Get(actionId);
    }

    public bool TryExecutePostActionBehavior(string actionId, SkillExecutionContext context)
    {
        var definition = _catalog.Get(actionId);
        if (definition == null || string.IsNullOrWhiteSpace(definition.BehaviorKey))
            return false;

        var behavior = SkillBehaviorFactory.Create(definition.BehaviorKey);
        if (behavior == null)
            return false;

        var status = behavior.Tick(context);
        return status == SkillNodeStatus.Success;
    }
}
