class_name TargetAliveCondition
extends ActionCondition

func _init() -> void:
	condition_id = "target_alive"

func check(context: ActionContext) -> bool:
	return context.target != null and context.target.is_alive
