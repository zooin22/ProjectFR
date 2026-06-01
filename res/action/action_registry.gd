class_name ActionRegistry

var _actions: Dictionary = {}  # String -> ActionBase

func _init() -> void:
	_register_default_actions()

func _register_default_actions() -> void:
	register(OpenAction.new())
	register(DeleteAction.new())
	register(InspectAction.new())
	register(CopyAction.new())
	register(CutAction.new())
	register(PasteAction.new())
	register(CleanAction.new())
	register(QuarantineAction.new())
	register(CompressAction.new())
	register(LogForgeAction.new())
	register(SearchAction.new())
	register(ShowHiddenAction.new())
	register(PermissionOverrideAction.new())
	register(StunAction.new())

func register(action: ActionBase) -> void:
	_actions[action.action_id.to_lower()] = action

func get_action(action_id: String) -> ActionBase:
	return _actions.get(action_id.to_lower(), null)

func get_all_actions() -> Array:
	return _actions.values()

func get_random_action() -> ActionBase:
	var actions := get_all_actions()
	if actions.size() == 0:
		return null
	return actions[randi() % actions.size()]

func get_executable_actions(context: ActionContext) -> Array:
	return get_all_actions().filter(func(a): return a.can_execute(context))
