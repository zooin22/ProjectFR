using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;
using ProjectFR.Systems;

namespace ProjectFR.Action.Implementations;

public class PasteAction : IAction
{
    public string ActionId => "paste";
    public string DisplayName => "Paste (Ctrl+V)";
    public int ApCost => 2;
    public TargetType Scope => TargetType.Single;
    public List<IActionCondition> Conditions { get; }

    public PasteAction()
    {
        Conditions = new()
        {
            new MinApCondition(2),
            new ClipboardNotEmptyCondition()
        };
    }

    public bool CanExecute(ActionContext context)
    {
        return Conditions.All(c => c.Check(context));
    }

    public ActionResult Execute(ActionContext context)
    {
        if (!CanExecute(context))
            return new ActionResult(false, "Cannot paste");

        if (context.Clipboard == null || context.Target == null)
            return new ActionResult(false, "Invalid clipboard or target");

        context.Actor.ConsumeAp(ApCost);
        var pastedNode = context.Clipboard.Paste();

        if (pastedNode == null)
            return new ActionResult(false, "Nothing to paste");

        int damage = 0;
        string effectName = "";

        if (pastedNode is SpecialFileNode)
        {
            damage = (int)(10 * 1.5);
            context.Target.TakeDamage(damage);
            return new ActionResult(true, $"Pasted Special File dealing {damage} damage", damage);
        }
        else if (pastedNode is FileNode)
        {
            damage = 5;
            context.Target.TakeDamage(damage);
            return new ActionResult(true, $"Pasted File dealing {damage} damage", damage);
        }
        else if (pastedNode is FolderNode)
        {
            if (context.StatusEffects != null)
            {
                context.StatusEffects.AddEffect(context.Target.ToString() ?? "", StatusEffect.Quarantine, 3);
                return new ActionResult(true, "Pasted Folder and applied Quarantine (3 turns)");
            }
            return new ActionResult(false, "Cannot apply status effect");
        }

        return new ActionResult(false, "Unknown clipboard content");
    }
}

public class SpecialFileNode : FileNode
{
    public SpecialFileNode(string name, string path, long size = 0)
        : base(name, path, size)
    {
    }
}
