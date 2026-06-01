class_name LogForgeAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.LOG_FORGE
	display_name = "Rewrite Log"
	ap_cost = ActionConstants.LOG_FORGE_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.LOG_FORGE_ACTION_AP_COST),
	]

func execute(context: ActionContext) -> ActionResult:
	if context.target_node == null:
		return ActionResult.new(false, "No target selected")
	context.consume_ap(ap_cost)
	return ActionResult.new(true, "Rewrote traces around %s" % context.target_node.name)
