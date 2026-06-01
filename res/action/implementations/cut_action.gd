class_name CutAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.CUT
	display_name = "Cut (Ctrl+X)"
	ap_cost = ActionConstants.CUT_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.CUT_ACTION_AP_COST),
		TargetAliveCondition.new(),
	]

func execute(context: ActionContext) -> ActionResult:
	if not can_execute(context):
		return ActionResult.new(false, "Cannot cut")
	if context.target_node == null:
		return ActionResult.new(false, "No target node")
	if context.clipboard == null:
		return ActionResult.new(false, "No clipboard available")
	if context.target == null:
		return ActionResult.new(false, "No target available")

	context.consume_ap(ap_cost)
	var damage := ActionConstants.CUT_DAMAGE
	context.target.take_damage(damage)
	context.clipboard.cut(context.target_node)
	return ActionResult.new(true, "Cut %s dealing %d damage" % [context.target_node.name, damage], damage)
