class_name SkillDefinition

var action_id: String = ""
var display_name: String = ""
var description: String = ""
var operation_type: int = OperationType.ACCESS
var queueable: bool = true
var show_in_command_deck: bool = true
var show_in_context_menu: bool = true
var behavior_key: String = ""
var behavior_resource_path: String = ""

static func from_action(action: ActionBase, p_operation_type: int, p_behavior_key: String = "", p_behavior_resource_path: String = "") -> SkillDefinition:
	var def := SkillDefinition.new()
	def.action_id = action.action_id
	def.display_name = action.display_name
	def.description = ActionMetadata.get_tooltip_text(action.action_id)
	def.operation_type = p_operation_type
	def.behavior_key = p_behavior_key
	def.behavior_resource_path = p_behavior_resource_path
	return def
