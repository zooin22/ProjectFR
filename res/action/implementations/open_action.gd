class_name OpenAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.OPEN
	display_name = "Open (Enter)"
	ap_cost = ActionConstants.OPEN_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.OPEN_ACTION_AP_COST),
		TargetAliveCondition.new(),
	]

func execute(context: ActionContext) -> ActionResult:
	if not can_execute(context):
		return ActionResult.new(false, "Cannot execute Open action")

	context.consume_ap(ap_cost)

	if context.target_node is ContainerNode:
		return ActionResult.new(true, "Opened %s" % context.target_node.name)

	var damage := ActionConstants.OPEN_FILE_DAMAGE
	if context.target != null:
		context.target.take_damage(damage)
		var node_name := context.target_node.name if context.target_node != null else "target"
		return ActionResult.new(true, "Opened %s dealing %d damage" % [node_name, damage], damage)

	return ActionResult.new(false, "No valid target")
