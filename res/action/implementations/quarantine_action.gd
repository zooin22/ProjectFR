class_name QuarantineAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.QUARANTINE
	display_name = "Quarantine (Q)"
	ap_cost = ActionConstants.QUARANTINE_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.QUARANTINE_ACTION_AP_COST),
		TargetAliveCondition.new(),
	]

func execute(context: ActionContext) -> ActionResult:
	if not can_execute(context):
		return ActionResult.new(false, "Cannot quarantine")
	if context.target == null or context.status_effects == null:
		return ActionResult.new(false, "Invalid target or status effects system")

	context.consume_ap(ap_cost)
	context.status_effects.add_effect(
		context.target.id,
		StatusEffectSystem.StatusEffect.QUARANTINE,
		ActionConstants.QUARANTINE_EFFECT_DURATION
	)
	var node_name := context.target_node.name if context.target_node != null else "target"
	return ActionResult.new(true, "Quarantined %s for %d turns" % [node_name, ActionConstants.QUARANTINE_EFFECT_DURATION])
