class_name NotStatusCondition
extends ActionCondition

var status_type: int

func _init(p_status_type: int = 0) -> void:  # 0 = StatusEffectSystem.StatusEffect.QUARANTINE
	condition_id = "not_status"
	status_type = p_status_type

func check(context: ActionContext) -> bool:
	if context.status_effects == null or context.actor == null:
		return true
	return not context.status_effects.has_effect(context.actor.id, status_type)
