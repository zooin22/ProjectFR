using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Systems;

namespace ProjectFR.Action.Implementations;

public class QuarantineAction : IAction
{
    public string ActionId => "quarantine";
    public string DisplayName => "Quarantine (Q)";
    public int ApCost => 2;
    public TargetType Scope => TargetType.Single;
    public List<IActionCondition> Conditions { get; }

    public QuarantineAction()
    {
        Conditions = new()
        {
            new MinApCondition(2),
            new TargetAliveCondition()
        };
    }

    public bool CanExecute(ActionContext context)
    {
        return Conditions.All(c => c.Check(context));
    }

    public ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot quarantine");

        if (context.Target == null || context.StatusEffects == null)
            return new ActionResult(false, "Invalid target or status effects system");

        context.Actor.ConsumeAp(ApCost);

        context.StatusEffects.AddEffect(
            context.Target.Id,
            StatusEffect.Quarantine,
            3
        );

        return new ActionResult(true, $"Quarantined {context.TargetNode?.Name} for 3 turns");
    }
}
