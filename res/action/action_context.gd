class_name ActionContext

var actor: ActorState
var target: ActorState
var target_node: NodeData
var all_actors: Array = []
var clipboard = null  # ClipboardSystem autoload or null
var status_effects = null  # StatusEffectSystem autoload or null
var consume_ap_callback: Callable = Callable()

var _direct_ap: int = 0
var _direct_clipboard_item_count: int = 0

var actor_id: String:
	get: return actor.id if actor != null else ""

var current_ap: int:
	get: return actor.current_ap if actor != null else _direct_ap

var clipboard_item_count: int:
	get:
		if clipboard != null:
			return 1 if clipboard.has_content else 0
		return _direct_clipboard_item_count

var clipboard_has_content: bool:
	get: return clipboard_item_count > 0

static func from_actor(p_actor: ActorState) -> ActionContext:
	var ctx := ActionContext.new()
	ctx.actor = p_actor
	return ctx

static func from_direct(p_current_ap: int, p_clipboard_item_count: int) -> ActionContext:
	var ctx := ActionContext.new()
	ctx._direct_ap = p_current_ap
	ctx._direct_clipboard_item_count = p_clipboard_item_count
	return ctx

func consume_ap(amount: int) -> void:
	if consume_ap_callback.is_valid():
		consume_ap_callback.call(amount)
	elif actor != null:
		actor.consume_ap(amount)

func set_target(p_target: ActorState, p_node: NodeData = null) -> void:
	target = p_target
	target_node = p_node
