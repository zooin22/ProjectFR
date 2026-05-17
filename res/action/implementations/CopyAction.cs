using ProjectFR.Action.Conditions;
using ProjectFR.Data;

namespace ProjectFR.Action.Implementations;

public class CopyAction : IAction
{
    public string ActionId => "copy";
    public string DisplayName => "Copy (Ctrl+C)";
    public int ApCost => ActionConstants.CopyActionApCost;
    public TargetType Scope => TargetType.Single;
    public List<IActionCondition> Conditions { get; }

    public CopyAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.CopyActionApCost)
        };
    }

    public bool CanExecute(ActionContext context)
    {
        return Conditions.All(c => c.Check(context));
    }

    public ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot copy");

        if (context.TargetNode == null)
            return new ActionResult(false, "No target to copy");

        context.Actor.ConsumeAp(ApCost);

        if (context.Clipboard != null)
        {
            context.Clipboard.Copy(context.TargetNode);
            return new ActionResult(true, $"Copied {context.TargetNode.Name} to clipboard");
        }

        return new ActionResult(false, "Clipboard unavailable");
    }
}
