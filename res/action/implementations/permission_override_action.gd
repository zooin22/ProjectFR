class_name PermissionOverrideAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.PERMISSION_OVERRIDE
	display_name = "Permission Override"
	ap_cost = ActionConstants.PERMISSION_OVERRIDE_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.PERMISSION_OVERRIDE_ACTION_AP_COST),
		TargetAliveCondition.new(),
	]

func execute(context: ActionContext) -> ActionResult:
	if not can_execute(context):
		return ActionResult.new(false, "Cannot override permissions")
	context.consume_ap(ap_cost)
	var node_name := context.target_node.name if context.target_node != null else "target"
	return ActionResult.new(true, "Permission override challenged %s" % node_name)
