using ProjectFR.Systems;

namespace ProjectFR.Action.Conditions;

public class NotStatusCondition : IActionCondition
{
    public string ConditionId => "not_status";
    public StatusEffect StatusType { get; set; }

    public NotStatusCondition(StatusEffect statusType = StatusEffect.Quarantine)
    {
        StatusType = statusType;
    }

    public bool Check(ActionContext context)
    {
        if (context.StatusEffects == null || context.Actor == null)
            return true;

        return !context.StatusEffects.HasEffect(context.Actor.Id, StatusType);
    }
}
