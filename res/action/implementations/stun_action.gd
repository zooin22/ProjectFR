class_name StunAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.STUN
	display_name = "Stun"
	ap_cost = ActionConstants.STUN_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.STUN_ACTION_AP_COST),
		TargetAliveCondition.new(),
	]

func execute(context: ActionContext) -> ActionResult:
	if not can_execute(context):
		return ActionResult.new(false, "Cannot stun")
	if context.target == null:
		return ActionResult.new(false, "No target")

	context.consume_ap(ap_cost)
	context.target.take_damage(ActionConstants.STUN_DAMAGE)
	# SecurityAgent DisabledTurns is applied by the scene layer when the Stun FileOperation completes
	var node_name := context.target_node.name if context.target_node != null else "target"
	return ActionResult.new(true, "Stunned %s for %d damage" % [node_name, ActionConstants.STUN_DAMAGE])
