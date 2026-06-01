using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Systems;

namespace ProjectFR.Action.Implementations;

public class CleanAction : ActionBase
{
    public override string ActionId => ActionIds.Clean;
    public override string DisplayName => "Clean (L)";
    public override int ApCost => ActionConstants.CleanActionApCost;
    public override TargetType Scope => TargetType.AoE;

    public CleanAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.CleanActionApCost)
        };
    }

    public override ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot execute Clean action");

        context.ConsumeAp(ApCost);
        int damage = ActionConstants.CleanDamage;

        if (context.AllActors != null)
        {
            int totalDamage = 0;
            foreach (var actor in context.AllActors)
            {
                actor.TakeDamage(damage);
                totalDamage += damage;
            }

            if (context.StatusEffects != null && context.ActorId != null)
            {
                context.StatusEffects.ClearEffects(context.ActorId);
            }

            return new ActionResult(true, $"Cleaned area dealing {totalDamage} damage and removing own status effects", totalDamage);
        }

        return new ActionResult(false, "Cannot execute AoE action");
    }
}
