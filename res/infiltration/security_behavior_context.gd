class_name SecurityBehaviorContext

var primary_path: String = ""
var agent: SecurityAgent = null
var agents: Array = []
var current_folder_path: String = ""
var cursor_path: String = ""
var objective_path: String = ""
var is_objective_path: bool = false
var is_objective_route: bool = false
var agent_on_objective_route: bool = false
var direct_jump: bool = false
var trace_amount: int = 0
var trace_reason: String = ""
var awareness_stage: int = SecurityAgent.SecurityAwarenessStage.SUSPICIOUS

# Callables: func(amount: int, reason: String), func(msg: String),
# func(agent: SecurityAgent, stage: int), func(path: String, turns: int, reason: String)
var add_trace: Callable
var add_log: Callable
var alert_agent: Callable
var mark_tracked_path: Callable
var apply_forced_lock: Callable
var apply_scan_pressure: Callable
