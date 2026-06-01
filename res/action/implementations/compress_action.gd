class_name CompressAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.COMPRESS
	display_name = "Compress (M)"
	ap_cost = ActionConstants.COMPRESS_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.COMPRESS_ACTION_AP_COST),
		TargetAliveCondition.new(),
	]

func execute(context: ActionContext) -> ActionResult:
	if not can_execute(context):
		return ActionResult.new(false, "Cannot compress")
	if context.target == null:
		return ActionResult.new(false, "No target")
	if context.status_effects == null:
		return ActionResult.new(false, "Status effects system not available")

	context.consume_ap(ap_cost)
	context.status_effects.add_effect(
		context.target.id,
		StatusEffectSystem.StatusEffect.COMPRESSED,
		ActionConstants.COMPRESS_EFFECT_DURATION,
		ActionConstants.COMPRESS_ATTACK_MODIFIER
	)
	var node_name := context.target_node.name if context.target_node != null else context.target.id
	return ActionResult.new(
		true,
		"Compressed %s reducing attack by %d for %d turns" % [node_name, abs(ActionConstants.COMPRESS_ATTACK_MODIFIER), ActionConstants.COMPRESS_EFFECT_DURATION]
	)
