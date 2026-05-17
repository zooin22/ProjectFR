using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Systems;

namespace ProjectFR.Action.Implementations;

public class CleanAction : IAction
{
    public string ActionId => "clean";
    public string DisplayName => "Clean (L)";
    public int ApCost => ActionConstants.CleanActionApCost;
    public TargetType Scope => TargetType.AoE;
    public List<IActionCondition> Conditions { get; }

    public CleanAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.CleanActionApCost)
        };
    }

    public bool CanExecute(ActionContext context)
    {
        return Conditions.All(c => c.Check(context));
    }

    public ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot execute Clean action");

        context.Actor.ConsumeAp(ApCost);
        int damage = ActionConstants.CleanDamage;

        if (context.AllActors != null)
        {
            int totalDamage = 0;
            foreach (var actor in context.AllActors)
            {
                if (actor != context.Actor)
                {
                    actor.TakeDamage(damage);
                    totalDamage += damage;
                }
            }

            if (context.StatusEffects != null)
            {
                context.StatusEffects.ClearEffects(context.Actor.Id);
            }

            return new ActionResult(true, $"Cleaned area dealing {totalDamage} damage and removing own status effects", totalDamage);
        }

        return new ActionResult(false, "Cannot execute AoE action");
    }
}
