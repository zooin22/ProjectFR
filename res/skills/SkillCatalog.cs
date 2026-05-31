using ProjectFR.Action;
using ProjectFR.Infiltration;

namespace ProjectFR.Skills;

public sealed class SkillCatalog
{
    private readonly Dictionary<string, SkillDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public void Register(SkillDefinition definition)
    {
        _definitions[definition.ActionId] = definition;
    }

    public SkillDefinition? Get(string actionId)
    {
        return _definitions.GetValueOrDefault(actionId);
    }

    public IReadOnlyCollection<SkillDefinition> GetAll()
    {
        return _definitions.Values.ToList();
    }

    public static SkillCatalog CreateDefault(ActionRegistry actionRegistry)
    {
        var catalog = new SkillCatalog();
        foreach (var action in actionRegistry.GetAllActions())
        {
            catalog.Register(SkillDefinition.FromAction(
                action,
                MapActionIdToOperationType(action.ActionId),
                GetBehaviorKey(action.ActionId),
                GetBehaviorResourcePath(action.ActionId)));
        }

        return catalog;
    }

    private static string? GetBehaviorKey(string actionId)
    {
        return actionId switch
        {
            ActionIds.Search => SkillBehaviorKeys.Search,
            ActionIds.ShowHidden => SkillBehaviorKeys.ShowHidden,
            ActionIds.PermissionOverride => SkillBehaviorKeys.PermissionOverride,
            _ => null
        };
    }

    private static string? GetBehaviorResourcePath(string actionId)
    {
        return actionId switch
        {
            ActionIds.Search => "res://res/skills/behaviors/search.btres",
            ActionIds.ShowHidden => "res://res/skills/behaviors/show_hidden.btres",
            ActionIds.PermissionOverride => "res://res/skills/behaviors/permission_override.btres",
            _ => null
        };
    }

    private static OperationType MapActionIdToOperationType(string actionId)
    {
        return actionId switch
        {
            ActionIds.Open => OperationType.Access,
            ActionIds.Copy => OperationType.Copy,
            ActionIds.Cut => OperationType.Cut,
            ActionIds.Paste => OperationType.Paste,
            ActionIds.Move => OperationType.Move,
            ActionIds.Delete => OperationType.Delete,
            ActionIds.Compress => OperationType.Compress,
            ActionIds.Extract => OperationType.ExtractArchive,
            ActionIds.Inspect => OperationType.Properties,
            ActionIds.Search => OperationType.Search,
            ActionIds.Sort => OperationType.Sort,
            ActionIds.ShowHidden => OperationType.ShowHidden,
            ActionIds.LogForge => OperationType.RewriteLog,
            ActionIds.Quarantine => OperationType.Quarantine,
            ActionIds.Clean => OperationType.Clean,
            ActionIds.Inject => OperationType.Inject,
            ActionIds.Stun => OperationType.Stun,
            ActionIds.Decoy => OperationType.Decoy,
            ActionIds.PermissionOverride => OperationType.PermissionOverride,
            _ => OperationType.Access
        };
    }
}
