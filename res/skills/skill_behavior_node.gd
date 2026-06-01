class_name SkillBehaviorNode

enum SkillNodeStatus {
	SUCCESS,
	FAILURE,
	RUNNING,
}

func tick(_context: SkillExecutionContext) -> int:
	push_error("SkillBehaviorNode.tick() must be overridden")
	return SkillNodeStatus.FAILURE


class SkillSequenceNode extends SkillBehaviorNode:
	var _children: Array

	func _init(p_children: Array) -> void:
		_children = p_children

	func tick(context: SkillExecutionContext) -> int:
		for child in _children:
			var status: int = child.tick(context)
			if status != SkillNodeStatus.SUCCESS:
				return status
		return SkillNodeStatus.SUCCESS


class SkillSelectorNode extends SkillBehaviorNode:
	var _children: Array

	func _init(p_children: Array) -> void:
		_children = p_children

	func tick(context: SkillExecutionContext) -> int:
		for child in _children:
			var status: int = child.tick(context)
			if status != SkillNodeStatus.FAILURE:
				return status
		return SkillNodeStatus.FAILURE


class SkillConditionNode extends SkillBehaviorNode:
	var _predicate: Callable

	func _init(p_predicate: Callable) -> void:
		_predicate = p_predicate

	func tick(context: SkillExecutionContext) -> int:
		return SkillNodeStatus.SUCCESS if _predicate.call(context) else SkillNodeStatus.FAILURE


class SkillActionNode extends SkillBehaviorNode:
	var _action: Callable

	func _init(p_action: Callable) -> void:
		_action = p_action

	func tick(context: SkillExecutionContext) -> int:
		return _action.call(context)
