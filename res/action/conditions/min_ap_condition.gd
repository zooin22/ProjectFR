class_name MinApCondition
extends ActionCondition

var required_ap: int = 1

func _init(p_required_ap: int = 1) -> void:
	condition_id = "min_ap"
	required_ap = p_required_ap

func check(context: ActionContext) -> bool:
	return context.current_ap >= required_ap
