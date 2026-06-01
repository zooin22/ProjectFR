using ProjectFR.Data;
using ProjectFR.Data.Nodes;
using ProjectFR.Systems;

namespace ProjectFR.Action;

public class ActionContext
{
    private readonly int _directAp;
    private readonly int _directClipboardItemCount;

    public ActorState? Actor { get; set; }
    public ActorState? Target { get; set; }
    public NodeData? TargetNode { get; set; }
    public List<ActorState>? AllActors { get; set; }
    public ClipboardSystem? Clipboard { get; set; }
    public StatusEffectSystem? StatusEffects { get; set; }
    public Action<int>? ConsumeApCallback { get; set; }

    public string? ActorId => Actor?.Id;
    public int CurrentAp => Actor?.CurrentAp ?? _directAp;
    public int ClipboardItemCount => Clipboard != null ? (Clipboard.HasContent ? 1 : 0) : _directClipboardItemCount;
    public bool ClipboardHasContent => ClipboardItemCount > 0;

    public ActionContext(ActorState actor)
    {
        Actor = actor;
    }

    public ActionContext(int currentAp, int clipboardItemCount)
    {
        _directAp = currentAp;
        _directClipboardItemCount = clipboardItemCount;
    }

    public void ConsumeAp(int amount)
    {
        if (ConsumeApCallback != null)
            ConsumeApCallback(amount);
        else
            Actor?.ConsumeAp(amount);
    }

    public void SetTarget(ActorState? target, NodeData? node = null)
    {
        Target = target;
        TargetNode = node;
    }
}
