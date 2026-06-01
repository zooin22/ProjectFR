class_name ShowHiddenAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.SHOW_HIDDEN
	display_name = "Show Hidden"
	ap_cost = ActionConstants.SHOW_HIDDEN_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.SHOW_HIDDEN_ACTION_AP_COST),
		TargetAliveCondition.new(),
	]

func execute(context: ActionContext) -> ActionResult:
	if not can_execute(context):
		return ActionResult.new(false, "Cannot show hidden")
	context.consume_ap(ap_cost)
	var node_name := context.target_node.name if context.target_node != null else "target"
	return ActionResult.new(true, "Show Hidden probed %s" % node_name)
