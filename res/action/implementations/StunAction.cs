using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class StunAction : ActionBase
{
    public override string ActionId => ActionIds.Stun;
    public override string DisplayName => "Stun";
    public override int ApCost => ActionConstants.StunActionApCost;
    public override TargetType Scope => TargetType.Single;

    public StunAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.StunActionApCost),
            new TargetAliveCondition()
        };
    }

    public override ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot stun");

        if (context.Target == null)
            return new ActionResult(false, "No target");

        context.ConsumeAp(ApCost);
        context.Target.TakeDamage(ActionConstants.StunDamage);

        // Security agent DisabledTurns is applied by BattleScene.ProcessCompletedOperations
        // when the Stun FileOperation completes.
        return new ActionResult(true, $"Stunned {context.TargetNode?.Name ?? "target"} for {ActionConstants.StunDamage} damage");
    }
}
