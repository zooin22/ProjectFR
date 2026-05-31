using ProjectFR.Action.Conditions;
using ProjectFR.Data;
using ProjectFR.Data.Nodes;
using ProjectFR.Systems;

namespace ProjectFR.Action.Implementations;

/// <summary>
/// Paste action - applies effects based on clipboard content type
/// </summary>
public class PasteAction : ActionBase
{
    public override string ActionId => ActionIds.Paste;
    public override string DisplayName => "Paste (Ctrl+V)";
    public override int ApCost => ActionConstants.PasteActionApCost;
    public override TargetType Scope => TargetType.Single;

    public PasteAction()
    {
        Conditions = new()
        {
            new MinApCondition(ActionConstants.PasteActionApCost),
            new ClipboardNotEmptyCondition()
        };
    }

    public override ActionResult Execute(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!CanExecute(context))
            return new ActionResult(false, "Cannot paste");

        ArgumentNullException.ThrowIfNull(context.Clipboard);
        ArgumentNullException.ThrowIfNull(context.Target);

        var pastedNode = context.Clipboard.Paste();

        if (pastedNode == null)
            return new ActionResult(false, "Nothing to paste");

        context.Actor.ConsumeAp(ApCost);

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
