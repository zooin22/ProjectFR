using ProjectFR.Action.Implementations;

namespace ProjectFR.Action;

public class ActionRegistry
{
    private Dictionary<string, IAction> _actions = new();

    public ActionRegistry()
    {
        RegisterDefaultActions();
    }

    private void RegisterDefaultActions()
    {
        Register(new OpenAction());
        Register(new DeleteAction());
        Register(new InspectAction());
        Register(new CopyAction());
        Register(new CutAction());
        Register(new PasteAction());
        Register(new CleanAction());
        Register(new QuarantineAction());
        Register(new CompressAction());
        Register(new LogForgeAction());
        Register(new SearchAction());
    }

    public void Register(IAction action)
    {
        _actions[action.ActionId] = action;
    }

    public IAction? GetAction(string actionId)
    {
        return _actions.TryGetValue(actionId, out var action) ? action : null;
    }

    public List<IAction> GetAllActions()
    {
        return _actions.Values.ToList();
    }

    public IAction? GetRandomAction()
    {
        var actions = GetAllActions();
        return actions.Count > 0 ? actions[Random.Shared.Next(actions.Count)] : null;
    }

    public List<IAction> GetExecutableActions(ActionContext context)
    {
        return _actions.Values.Where(a => a.CanExecute(context)).ToList();
    }
}
