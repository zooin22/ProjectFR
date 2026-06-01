class_name DeleteAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.DELETE
	display_name = "Delete"
	ap_cost = ActionConstants.DELETE_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.DELETE_ACTION_AP_COST),
		TargetAliveCondition.new(),
	]

func execute(context: ActionContext) -> ActionResult:
	if context.target == null:
		return ActionResult.new(false, "No valid target")
	if not can_execute(context):
		return ActionResult.new(false, "Delete prerequisites not met")

	var damage := ActionConstants.DELETE_DAMAGE
	context.consume_ap(ap_cost)
	context.target.take_damage(damage)
	var node_name := context.target_node.name if context.target_node != null else "unknown"
	return ActionResult.new(true, "Deleted %s dealing %d damage" % [node_name, damage], damage)
