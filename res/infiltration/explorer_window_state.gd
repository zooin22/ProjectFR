class_name ExplorerWindowState

enum ExplorerWindowType {
	MAIN,
	CLIPBOARD,
	TEMP,
	LOG_VIEWER,
	BACKUP,
	ARCHIVE,
}

var window_id: String
var window_type: int = ExplorerWindowType.MAIN
var title: String = "Window"
var bound_path: String = ""
var is_open: bool = true
var is_focused: bool = false
var slot_index: int = 0
var trace_modifier: int = 0

func _init() -> void:
	window_id = _gen_id()

static func _gen_id() -> String:
	return "%08x%08x" % [Time.get_ticks_msec(), randi()]
