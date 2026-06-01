class_name CopyAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.COPY
	display_name = "Copy (Ctrl+C)"
	ap_cost = ActionConstants.COPY_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.COPY_ACTION_AP_COST),
	]

func execute(context: ActionContext) -> ActionResult:
	if context.target_node == null:
		return ActionResult.new(false, "No target to copy")
	if context.clipboard == null:
		return ActionResult.new(false, "Clipboard unavailable")

	context.consume_ap(ap_cost)
	context.clipboard.copy(context.target_node)
	return ActionResult.new(true, "Copied %s to clipboard" % context.target_node.name)
