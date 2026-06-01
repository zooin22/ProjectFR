class_name PasteAction
extends ActionBase

func _init() -> void:
	action_id = ActionIds.PASTE
	display_name = "Paste (Ctrl+V)"
	ap_cost = ActionConstants.PASTE_ACTION_AP_COST
	scope = TargetType.SINGLE
	conditions = [
		MinApCondition.new(ActionConstants.PASTE_ACTION_AP_COST),
		ClipboardNotEmptyCondition.new(),
	]

func execute(context: ActionContext) -> ActionResult:
	if not can_execute(context):
		return ActionResult.new(false, "Cannot paste")
	if context.clipboard == null:
		return ActionResult.new(false, "Clipboard unavailable")
	if context.target == null:
		return ActionResult.new(false, "No target")

	var pasted_node: NodeData = context.clipboard.paste()
	if pasted_node == null:
		return ActionResult.new(false, "Nothing to paste")

	context.consume_ap(ap_cost)

	if pasted_node is SpecialFileNode:
		return _apply_special_file_effect(context)
	elif pasted_node is FolderNode:
		return _apply_folder_effect(context)
	elif pasted_node is FileNode:
		return _apply_file_effect(context)
	return ActionResult.new(false, "Unknown clipboard content")

func _apply_special_file_effect(context: ActionContext) -> ActionResult:
	var damage := int(ActionConstants.PASTE_SPECIAL_FILE_BASE_DAMAGE * ActionConstants.PASTE_SPECIAL_FILE_MULTIPLIER)
	context.target.take_damage(damage)
	return ActionResult.new(true, "Pasted Special File dealing %d damage" % damage, damage)

func _apply_file_effect(context: ActionContext) -> ActionResult:
	context.target.take_damage(ActionConstants.PASTE_FILE_DAMAGE)
	return ActionResult.new(true, "Pasted File dealing %d damage" % ActionConstants.PASTE_FILE_DAMAGE, ActionConstants.PASTE_FILE_DAMAGE)

func _apply_folder_effect(context: ActionContext) -> ActionResult:
	if context.status_effects == null:
		return ActionResult.new(false, "Cannot apply status effect")
	context.status_effects.add_effect(
		context.target.id,
		StatusEffectSystem.StatusEffect.QUARANTINE,
		ActionConstants.QUARANTINE_EFFECT_DURATION
	)
	return ActionResult.new(true, "Pasted Folder and applied Quarantine (%d turns)" % ActionConstants.QUARANTINE_EFFECT_DURATION)
