using ProjectFR.Data;

namespace ProjectFR.Action;

public interface IAction
{
    string ActionId { get; }
    string DisplayName { get; }
    int ApCost { get; }
    TargetType Scope { get; }
    List<IActionCondition> Conditions { get; }

    bool CanExecute(ActionContext context);
    ActionResult Execute(ActionContext context);
}
