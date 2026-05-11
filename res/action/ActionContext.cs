using ProjectFR.Data;
using ProjectFR.Data.Nodes;
using ProjectFR.Systems;

namespace ProjectFR.Action;

public class ActionContext
{
    public ActorState Actor { get; set; }
    public ActorState? Target { get; set; }
    public NodeData? TargetNode { get; set; }
    public List<ActorState>? AllActors { get; set; }
    public ClipboardSystem? Clipboard { get; set; }
    public StatusEffectSystem? StatusEffects { get; set; }

    public ActionContext(ActorState actor)
    {
        Actor = actor;
    }

    public void SetTarget(ActorState? target, NodeData? node = null)
    {
        Target = target;
        TargetNode = node;
    }
}
