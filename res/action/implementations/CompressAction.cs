using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Systems;

namespace ProjectFR.Action.Implementations;

public class CompressAction : IAction
{
    public string ActionId => "compress";
    public string DisplayName => "Compress (M)";
    public int ApCost => ActionConstants.CompressActionApCost;
    public TargetType Scope => TargetType.Single;
    public List<IActionCondition> Conditions { get; }

    public CompressAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.CompressActionApCost),
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
            return new ActionResult(false, "Cannot compress");

        if (context.Target == null || context.StatusEffects == null)
            return new ActionResult(false, "Invalid target or status effects system");

        context.Actor.ConsumeAp(ApCost);

        context.StatusEffects.AddEffect(
            context.Target.Id,
            StatusEffect.Compressed,
            ActionConstants.CompressEffectDuration,
            ActionConstants.CompressAttackModifier
        );

        return new ActionResult(true, $"Compressed {context.TargetNode?.Name} reducing attack by {Math.Abs(ActionConstants.CompressAttackModifier)} for {ActionConstants.CompressEffectDuration} turns");
    }
}
