extends Node

const MAX_RECENT_ENTRIES = 250

var _recent_entries: Array[String] = []
var _log_path: String = ""
var _initialized: bool = false

signal entry_added(entry: String)

var enabled: bool:
	get: return OS.is_debug_build() or "--projectfr-debug-log" in OS.get_cmdline_user_args()

func _ready() -> void:
	if enabled:
		_initialize()

func _initialize() -> void:
	if _initialized:
		return
	var debug_dir := ProjectSettings.globalize_path("user://debug")
	DirAccess.make_dir_recursive_absolute(debug_dir)
	_log_path = debug_dir + "/projectfr-debug-latest.log"
	var f := FileAccess.open(_log_path, FileAccess.WRITE)
	if f:
		f.store_string("=== ProjectFR debug session started %s ===\n" % _timestamp())
	_initialized = true
	info("DebugLog", "initialized :: " + _log_path)

func trace(category: String, message: String) -> void:
	_write("TRACE", category, message)

func info(category: String, message: String) -> void:
	_write("INFO", category, message)

func warn(category: String, message: String) -> void:
	_write("WARNING", category, message)

func error(category: String, message: String) -> void:
	_write("ERROR", category, message)

var recent_entries: Array[String]:
	get: return _recent_entries

var current_log_path: String:
	get: return _log_path

func _write(level: String, category: String, message: String) -> void:
	if not enabled:
		return
	if not _initialized:
		_initialize()
	var entry := "[%s] [%s] [%s] %s" % [_timestamp(), level, category, message]
	_recent_entries.append(entry)
	if _recent_entries.size() > MAX_RECENT_ENTRIES:
		_recent_entries.pop_front()
	if _log_path != "":
		var f := FileAccess.open(_log_path, FileAccess.READ_WRITE)
		if f:
			f.seek_end()
			f.store_string(entry + "\n")
	match level:
		"WARNING":
			push_warning(entry)
		"ERROR":
			push_error(entry)
		_:
			print(entry)
	entry_added.emit(entry)

func _timestamp() -> String:
	return Time.get_datetime_string_from_system()
