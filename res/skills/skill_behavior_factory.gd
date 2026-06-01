class_name SkillBehaviorFactory

static func create(behavior_key: String) -> SkillBehaviorNode:
	match behavior_key:
		SkillBehaviorKeys.SEARCH:
			return _build_search_behavior()
		SkillBehaviorKeys.SHOW_HIDDEN:
			return _build_show_hidden_behavior()
		SkillBehaviorKeys.PERMISSION_OVERRIDE:
			return _build_permission_override_behavior()
		_:
			return null

static func _build_search_behavior() -> SkillBehaviorNode:
	return SkillBehaviorNode.SkillActionNode.new(func(ctx: SkillExecutionContext) -> int:
		ctx.perform_search_response.call(ctx.target_node)
		return SkillBehaviorNode.SkillNodeStatus.SUCCESS
	)

static func _build_show_hidden_behavior() -> SkillBehaviorNode:
	return SkillBehaviorNode.SkillActionNode.new(func(ctx: SkillExecutionContext) -> int:
		ctx.reveal_pouch_mask.call(
			ctx.target_node,
			"Show Hidden pierced cheek pouch masking",
			InfiltrationTuning.SHOW_HIDDEN_POUCH_EXPOSE_TRACE_INCREASE,
			ctx.definition.action_id
		)
		return SkillBehaviorNode.SkillNodeStatus.SUCCESS
	)

static func _build_permission_override_behavior() -> SkillBehaviorNode:
	return SkillBehaviorNode.SkillSequenceNode.new([
		SkillBehaviorNode.SkillSelectorNode.new([
			SkillBehaviorNode.SkillSequenceNode.new([
				SkillBehaviorNode.SkillConditionNode.new(func(ctx: SkillExecutionContext) -> bool:
					return ctx.is_permission_locked.call(ctx.target_node.path)
				),
				SkillBehaviorNode.SkillActionNode.new(func(ctx: SkillExecutionContext) -> int:
					ctx.grant_permission_override.call(
						ctx.target_node.path,
						"Permission Override forced access at %s" % ctx.target_node.path,
						InfiltrationTuning.PERMISSION_OVERRIDE_TRACE_INCREASE,
						InfiltrationTuning.PERMISSION_OVERRIDE_DURATION_TURNS
					)
					ctx.append_console_feed.call("permission override :: access granted :: %s :: %dT" % [ctx.target_node.name, InfiltrationTuning.PERMISSION_OVERRIDE_DURATION_TURNS])
					return SkillBehaviorNode.SkillNodeStatus.SUCCESS
				),
			]),
			SkillBehaviorNode.SkillActionNode.new(func(ctx: SkillExecutionContext) -> int:
				ctx.append_console_feed.call("permission override :: no lock present :: %s" % ctx.target_node.name)
				return SkillBehaviorNode.SkillNodeStatus.SUCCESS
			),
		]),
		SkillBehaviorNode.SkillActionNode.new(func(ctx: SkillExecutionContext) -> int:
			ctx.reveal_pouch_mask.call(
				ctx.target_node,
				"Permission Override exposed cheek pouch cache",
				InfiltrationTuning.PERMISSION_OVERRIDE_POUCH_EXPOSE_TRACE_INCREASE,
				ctx.definition.action_id
			)
			return SkillBehaviorNode.SkillNodeStatus.SUCCESS
		),
	])
