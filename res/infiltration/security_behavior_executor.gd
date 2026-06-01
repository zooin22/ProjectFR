class_name SecurityBehaviorExecutor

func try_execute(behavior_key: String, context: SecurityBehaviorContext) -> bool:
	var behavior: SecurityBehaviorNode = SecurityBehaviorFactory.create(behavior_key)
	if behavior == null:
		return false
	return behavior.tick(context) == SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS
