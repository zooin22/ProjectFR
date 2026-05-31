using ProjectFR.Data;

namespace ProjectFR.Action;

public abstract class ActionBase : IAction
{
    public abstract string ActionId { get; }
    public abstract string DisplayName { get; }
    public abstract int ApCost { get; }
    public abstract TargetType Scope { get; }
    public List<IActionCondition> Conditions { get; protected set; } = new();

    public virtual bool CanExecute(ActionContext context)
    {
        return Conditions.All(c => c.Check(context));
    }

    public abstract ActionResult Execute(ActionContext context);
}
