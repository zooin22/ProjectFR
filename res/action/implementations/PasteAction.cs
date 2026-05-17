using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;
using ProjectFR.Systems;

namespace ProjectFR.Action.Implementations;

/// <summary>
/// Paste action - applies effects based on clipboard content type
/// </summary>
public class PasteAction : IAction
{
    /// <summary>Gets the unique identifier for this action</summary>
    public string ActionId => "paste";
    
    /// <summary>Gets the display name for this action</summary>
    public string DisplayName => "Paste (Ctrl+V)";
    
    /// <summary>Gets the AP cost to execute this action</summary>
    public int ApCost => ActionConstants.PasteActionApCost;
    
    /// <summary>Gets the targeting scope of this action</summary>
    public TargetType Scope => TargetType.Single;
    
    /// <summary>Gets the list of conditions that must be met to execute this action</summary>
    public List<IActionCondition> Conditions { get; }

    /// <summary>
    /// Initializes a new instance of the PasteAction class
    /// </summary>
    public PasteAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.PasteActionApCost),
            new ClipboardNotEmptyCondition()
        };
    }

    /// <summary>
    /// Checks whether this action can be executed in the given context
    /// </summary>
    /// <param name="context">The action context to check</param>
    /// <returns>True if all conditions are met, false otherwise</returns>
    public bool CanExecute(ActionContext context)
    {
        return Conditions.All(c => c.Check(context));
    }

    /// <summary>
    /// Executes the paste action
    /// </summary>
    /// <param name="context">The action context containing actor and target</param>
    /// <returns>The result of the action execution</returns>
    public ActionResult Execute(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!CanExecute(context))
            return new ActionResult(false, "Cannot paste");

        ArgumentNullException.ThrowIfNull(context.Clipboard);
        ArgumentNullException.ThrowIfNull(context.Target);

        context.Actor.ConsumeAp(ApCost);
        var pastedNode = context.Clipboard.Paste();

        if (pastedNode == null)
            return new ActionResult(false, "Nothing to paste");

        return pastedNode switch
        {
            SpecialFileNode => ApplySpecialFileEffect(context, pastedNode),
            FileNode => ApplyFileEffect(context, pastedNode),
            FolderNode => ApplyFolderEffect(context, pastedNode),
            _ => new ActionResult(false, "Unknown clipboard content")
        };
    }

    /// <summary>
    /// Applies the effect of pasting a special file
    /// </summary>
    private ActionResult ApplySpecialFileEffect(ActionContext context, NodeData pastedNode)
    {
        int damage = (int)(ActionConstants.PasteSpecialFileBaseDamage * ActionConstants.PasteSpecialFileMultiplier);
        context.Target?.TakeDamage(damage);
        return new ActionResult(true, $"Pasted Special File dealing {damage} damage", damage);
    }

    /// <summary>
    /// Applies the effect of pasting a regular file
    /// </summary>
    private ActionResult ApplyFileEffect(ActionContext context, NodeData pastedNode)
    {
        context.Target?.TakeDamage(ActionConstants.PasteFileDamage);
        return new ActionResult(
            true,
            $"Pasted File dealing {ActionConstants.PasteFileDamage} damage",
            ActionConstants.PasteFileDamage
        );
    }

    /// <summary>
    /// Applies the effect of pasting a folder (applies quarantine status)
    /// </summary>
    private ActionResult ApplyFolderEffect(ActionContext context, NodeData pastedNode)
    {
        if (context.StatusEffects == null || context.Target == null)
            return new ActionResult(false, "Cannot apply status effect");

        context.StatusEffects.AddEffect(
            context.Target.Id,
            StatusEffect.Quarantine,
            ActionConstants.QuarantineEffectDuration
        );
        return new ActionResult(true, $"Pasted Folder and applied Quarantine ({ActionConstants.QuarantineEffectDuration} turns)");
    }
}

/// <summary>
/// Represents a special file node - a more dangerous file type
/// </summary>
public class SpecialFileNode : FileNode
{
    /// <summary>
    /// Initializes a new instance of the SpecialFileNode class
    /// </summary>
    /// <param name="name">The name of the file</param>
    /// <param name="path">The path to the file</param>
    /// <param name="size">The size of the file in bytes</param>
    public SpecialFileNode(string name, string path, long size = 0)
        : base(name, path, size)
    {
    }
}
