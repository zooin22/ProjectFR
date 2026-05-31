using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Systems;

namespace ProjectFR.Action.Implementations;

public class QuarantineAction : ActionBase
{
    public override string ActionId => ActionIds.Quarantine;
    public override string DisplayName => "Quarantine (Q)";
    public override int ApCost => ActionConstants.QuarantineActionApCost;
    public override TargetType Scope => TargetType.Single;

    public QuarantineAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.QuarantineActionApCost),
            new TargetAliveCondition()
        };
    }

    public override ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot quarantine");

        if (context.Target == null || context.StatusEffects == null)
            return new ActionResult(false, "Invalid target or status effects system");

        context.Actor.ConsumeAp(ApCost);

        context.StatusEffects.AddEffect(
            context.Target.Id,
            StatusEffect.Quarantine,
            ActionConstants.QuarantineEffectDuration
        );

        return new ActionResult(true, $"Quarantined {context.TargetNode?.Name} for {ActionConstants.QuarantineEffectDuration} turns");
    }
}
