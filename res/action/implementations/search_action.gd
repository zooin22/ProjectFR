class_name SearchAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.SEARCH
	display_name = "Search"
	ap_cost = ActionConstants.SEARCH_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.SEARCH_ACTION_AP_COST),
		TargetAliveCondition.new(),
	]

func execute(context: ActionContext) -> ActionResult:
	if not can_execute(context):
		return ActionResult.new(false, "Cannot search")
	context.consume_ap(ap_cost)
	var node_name := context.target_node.name if context.target_node != null else "target"
	return ActionResult.new(true, "Search indexed around %s" % node_name)
