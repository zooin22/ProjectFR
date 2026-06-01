class_name InspectAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.INSPECT
	display_name = "Inspect"
	ap_cost = ActionConstants.INSPECT_ACTION_AP_COST
	scope = TargetType.SINGLE

func can_execute(context: ActionContext) -> bool:
	return super.can_execute(context) and (context.target_node != null or context.target != null)

func execute(context: ActionContext) -> ActionResult:
	if context.target_node == null and context.target == null:
		return ActionResult.new(false, "No target to inspect")

	var node_name := ""
	if context.target_node != null:
		node_name = context.target_node.name
	elif context.target != null:
		node_name = context.target.display_name

	var label := node_name if node_name != "" else "target"
	var result := ActionResult.new(true, "Inspected %s" % label)
	if node_name != "":
		result.data["target_name"] = node_name
	if context.target != null:
		result.data["target_hp"] = context.target.current_hp
		result.data["target_max_hp"] = context.target.max_hp
	return result
