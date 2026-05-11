namespace ProjectFR.Action.Conditions;

public class ClipboardNotEmptyCondition : IActionCondition
{
    public string ConditionId => "clipboard_not_empty";

    public bool Check(ActionContext context)
    {
        return context.Clipboard != null && context.Clipboard.HasContent;
    }
}
