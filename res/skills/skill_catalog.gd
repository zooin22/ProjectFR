class_name SkillCatalog

var _definitions: Dictionary = {}  # String -> SkillDefinition

func register(definition: SkillDefinition) -> void:
	_definitions[definition.action_id.to_lower()] = definition

func get_definition(action_id: String) -> SkillDefinition:
	return _definitions.get(action_id.to_lower(), null)

func get_all() -> Array:
	return _definitions.values()

static func create_default(registry: ActionRegistry) -> SkillCatalog:
	var catalog := SkillCatalog.new()
	for action in registry.get_all_actions():
		catalog.register(SkillDefinition.from_action(
			action,
			_map_action_id_to_operation_type(action.action_id),
			_get_behavior_key(action.action_id),
			_get_behavior_resource_path(action.action_id)
		))
	return catalog

static func _get_behavior_key(action_id: String) -> String:
	match action_id:
		ActionIds.SEARCH: return SkillBehaviorKeys.SEARCH
		ActionIds.SHOW_HIDDEN: return SkillBehaviorKeys.SHOW_HIDDEN
		ActionIds.PERMISSION_OVERRIDE: return SkillBehaviorKeys.PERMISSION_OVERRIDE
		_: return ""

static func _get_behavior_resource_path(action_id: String) -> String:
	match action_id:
		ActionIds.SEARCH: return "res://res/skills/behaviors/search.btres"
		ActionIds.SHOW_HIDDEN: return "res://res/skills/behaviors/show_hidden.btres"
		ActionIds.PERMISSION_OVERRIDE: return "res://res/skills/behaviors/permission_override.btres"
		_: return ""

static func _map_action_id_to_operation_type(action_id: String) -> int:
	match action_id:
		ActionIds.OPEN: return OperationType.ACCESS
		ActionIds.COPY: return OperationType.COPY
		ActionIds.CUT: return OperationType.CUT
		ActionIds.PASTE: return OperationType.PASTE
		ActionIds.MOVE: return OperationType.MOVE
		ActionIds.DELETE: return OperationType.DELETE
		ActionIds.COMPRESS: return OperationType.COMPRESS
		ActionIds.EXTRACT: return OperationType.EXTRACT_ARCHIVE
		ActionIds.INSPECT: return OperationType.PROPERTIES
		ActionIds.SEARCH: return OperationType.SEARCH
		ActionIds.SORT: return OperationType.SORT
		ActionIds.SHOW_HIDDEN: return OperationType.SHOW_HIDDEN
		ActionIds.LOG_FORGE: return OperationType.REWRITE_LOG
		ActionIds.QUARANTINE: return OperationType.QUARANTINE
		ActionIds.CLEAN: return OperationType.CLEAN
		ActionIds.INJECT: return OperationType.INJECT
		ActionIds.STUN: return OperationType.STUN
		ActionIds.DECOY: return OperationType.DECOY
		ActionIds.PERMISSION_OVERRIDE: return OperationType.PERMISSION_OVERRIDE
		_: return OperationType.ACCESS
