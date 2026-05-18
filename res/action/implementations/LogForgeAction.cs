using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class LogForgeAction : IAction
{
    public string ActionId => "logforge";
    public string DisplayName => "Rewrite Log";
    public int ApCost => 1;
    public TargetType Scope => TargetType.Single;
    public List<IActionCondition> Conditions { get; }

    public LogForgeAction()
    {
        Conditions = new()
        {
            new MinApCondition(ApCost),
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
            return new ActionResult(false, "Cannot rewrite log");

        context.Actor.ConsumeAp(ApCost);
        return new ActionResult(true, $"Rewrote traces around {context.TargetNode?.Name}");
    }
}
