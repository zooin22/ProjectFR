class_name SkillExecutionContext

var definition: SkillDefinition
var action_context: ActionContext
var target_node: NodeData

var perform_search_response: Callable   # func(node: NodeData)
var reveal_pouch_mask: Callable         # func(node: NodeData, msg: String, trace: int, action_id: String)
var is_permission_locked: Callable      # func(path: String) -> bool
var grant_permission_override: Callable # func(path: String, msg: String, trace: int, duration: int)
var append_console_feed: Callable       # func(text: String)
