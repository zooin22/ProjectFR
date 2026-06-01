class_name SecurityBehaviorNode

enum SecurityBehaviorStatus {
	SUCCESS,
	FAILURE,
	RUNNING,
}

func tick(_context: SecurityBehaviorContext) -> int:
	return SecurityBehaviorStatus.FAILURE


class SequenceNode extends SecurityBehaviorNode:
	var _children: Array = []

	func _init(children: Array) -> void:
		_children = children

	func tick(context: SecurityBehaviorContext) -> int:
		for child in _children:
			var status: int = child.tick(context)
			if status != SecurityBehaviorStatus.SUCCESS:
				return status
		return SecurityBehaviorStatus.SUCCESS


class SelectorNode extends SecurityBehaviorNode:
	var _children: Array = []

	func _init(children: Array) -> void:
		_children = children

	func tick(context: SecurityBehaviorContext) -> int:
		for child in _children:
			var status: int = child.tick(context)
			if status != SecurityBehaviorStatus.FAILURE:
				return status
		return SecurityBehaviorStatus.FAILURE


class ConditionNode extends SecurityBehaviorNode:
	var _predicate: Callable

	func _init(predicate: Callable) -> void:
		_predicate = predicate

	func tick(context: SecurityBehaviorContext) -> int:
		return SecurityBehaviorStatus.SUCCESS if _predicate.call(context) else SecurityBehaviorStatus.FAILURE


class ActionNode extends SecurityBehaviorNode:
	var _action: Callable

	func _init(action: Callable) -> void:
		_action = action

	func tick(context: SecurityBehaviorContext) -> int:
		return _action.call(context)
