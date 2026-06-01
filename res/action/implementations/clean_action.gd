class_name CleanAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.CLEAN
	display_name = "Clean (L)"
	ap_cost = ActionConstants.CLEAN_ACTION_AP_COST
	scope = TargetType.AOE
	conditions = [
		MinApCondition.new(ActionConstants.CLEAN_ACTION_AP_COST),
	]

func execute(context: ActionContext) -> ActionResult:
	if not can_execute(context):
		return ActionResult.new(false, "Cannot execute Clean action")

	context.consume_ap(ap_cost)
	var damage := ActionConstants.CLEAN_DAMAGE

	if context.all_actors.size() > 0:
		var total_damage := 0
		for actor in context.all_actors:
			actor.take_damage(damage)
			total_damage += damage
		if context.status_effects != null and context.actor_id != "":
			context.status_effects.clear_effects(context.actor_id)
		return ActionResult.new(true, "Cleaned area dealing %d damage and removing own status effects" % total_damage, total_damage)

	return ActionResult.new(false, "Cannot execute AoE action")
