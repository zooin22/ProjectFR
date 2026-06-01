using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Systems;

namespace ProjectFR.Action.Implementations;

public class CompressAction : ActionBase
{
    public override string ActionId => ActionIds.Compress;
    public override string DisplayName => "Compress (M)";
    public override int ApCost => ActionConstants.CompressActionApCost;
    public override TargetType Scope => TargetType.Single;

    public CompressAction()
    {
        Conditions = new()
        {
            new MinApCondition(ApCost),
            new TargetAliveCondition()
        };
    }

    public override ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot compress");

        if (context.Target == null)
            return new ActionResult(false, "No target");
        if (context.StatusEffects == null)
            return new ActionResult(false, "Status effects system not available");

        context.ConsumeAp(ApCost);

        context.StatusEffects.AddEffect(
            context.Target.Id,
            StatusEffect.Compressed,
            ActionConstants.CompressEffectDuration,
            ActionConstants.CompressAttackModifier
        );

        return new ActionResult(true, $"Compressed {context.TargetNode?.Name ?? context.Target.Id ?? "unknown"} reducing attack by {System.Math.Abs(ActionConstants.CompressAttackModifier)} for {ActionConstants.CompressEffectDuration} turns");
    }
}
