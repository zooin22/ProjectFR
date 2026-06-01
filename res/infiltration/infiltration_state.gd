class_name InfiltrationState

var current_folder_path: String = "res://"
var turn_count: int = 0
var trace: int = 0
var max_trace: int = 100
var operator_max_hp: int = InfiltrationTuning.OPERATOR_MAX_HP
var operator_hp: int = InfiltrationTuning.OPERATOR_MAX_HP
var last_turn_contact_damage: int = 0
var alert_stage: int = SecurityAgent.SecurityAwarenessStage.PASSIVE
var exit_unlocked: bool = false
var run_status: int = RunState.RunStatus.ACTIVE
var objective_state: int = RunState.ObjectiveState.HIDDEN

var cursor_agent: CursorAgent = CursorAgent.new()
var known_node_paths: Dictionary = {}
var windows: Array = []
var clipboard: Array = []
var pouch_cache: Array = []
var exposed_pouch_paths: Dictionary = {}
var permission_override_turns: Dictionary = {}
var tracked_path_turns: Dictionary = {}
var forced_lock_turns: Dictionary = {}
var scan_pressure_turns: Dictionary = {}
var active_operations: Array = []
var command_queue: Array = []
var event_log: Array[String] = []

var is_operator_alive: bool:
	get:
		return operator_hp > 0

func take_operator_damage(amount: int) -> void:
	operator_hp = max(0, operator_hp - amount)
	add_log("Operator took %d contact damage (HP: %d/%d)" % [amount, operator_hp, operator_max_hp])

func add_log(message: String) -> void:
	event_log.append(message)
	if event_log.size() > 100:
		event_log.remove_at(0)
