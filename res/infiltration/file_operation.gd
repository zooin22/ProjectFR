class_name FileOperation

var id: String
var type: int
var target_node_path: String
var secondary_target_path: String = ""
var progress: float = 0.0
var required_ticks: int
var elapsed_ticks: int = 0
var status: int = OperationStatus.QUEUED
var completion_handled: bool = false
var node_kind: int = ExplorerNodeKind.FILE
var node_size: int = 0
var completion_notes: Array[String] = []

func _init(p_type: int, p_target_node_path: String, p_required_ticks: int = 1, p_secondary_target_path: String = "") -> void:
	id = _gen_id()
	type = p_type
	target_node_path = p_target_node_path
	secondary_target_path = p_secondary_target_path
	required_ticks = max(1, p_required_ticks)

static func _gen_id() -> String:
	return "%08x%08x" % [Time.get_ticks_msec(), randi()]

func start() -> void:
	if status == OperationStatus.QUEUED:
		status = OperationStatus.RUNNING

func tick() -> void:
	if status != OperationStatus.RUNNING:
		return
	elapsed_ticks = min(required_ticks, elapsed_ticks + 1)
	progress = float(elapsed_ticks) / float(required_ticks)
	if elapsed_ticks >= required_ticks:
		status = OperationStatus.COMPLETED
		progress = 1.0

func interrupt() -> void:
	if status == OperationStatus.RUNNING:
		status = OperationStatus.INTERRUPTED

func fail() -> void:
	status = OperationStatus.FAILED

func mark_completion_handled() -> void:
	completion_handled = true
