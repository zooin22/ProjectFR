class_name SkillExecutor

var _catalog: SkillCatalog

func _init(p_catalog: SkillCatalog) -> void:
	_catalog = p_catalog

func get_definition(action_id: String) -> SkillDefinition:
	return _catalog.get_definition(action_id)

func try_execute_post_action_behavior(action_id: String, context: SkillExecutionContext) -> bool:
	var definition := _catalog.get_definition(action_id)
	if definition == null or definition.behavior_key == "":
		return false

	var behavior := SkillBehaviorFactory.create(definition.behavior_key)
	if behavior == null:
		return false

	var status := behavior.tick(context)
	return status == SkillBehaviorNode.SkillNodeStatus.SUCCESS
