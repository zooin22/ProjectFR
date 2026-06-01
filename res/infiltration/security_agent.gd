class_name SecurityAgent

enum SecurityAgentType {
	GUARD_SCANNER,
	ANTIVIRUS_HEAVY,
	INDEXER_SCOUT,
	BACKUP_REPAIRER,
	FIREWALL_SENTINEL,
	AI_MONITOR,
}

enum SecurityAwarenessStage {
	PASSIVE,
	SUSPICIOUS,
	ACTIVE_SCAN,
	QUARANTINE,
	PURGE,
}

var id: String
var agent_type: int
var display_name: String
var current_node_path: String
var patrol_route: Array[String] = []
var awareness_stage: int = SecurityAwarenessStage.PASSIVE
var disabled_turns: int = 0
var is_alerted: bool = false
var patrol_index: int = 0
var sight_range: int = 1

func _init(
	p_agent_type: int,
	p_display_name: String,
	p_current_node_path: String,
	p_patrol_route: Array = [],
	p_id: String = ""
) -> void:
	id = p_id if p_id != "" else _gen_id()
	agent_type = p_agent_type
	display_name = p_display_name
	current_node_path = p_current_node_path
	awareness_stage = SecurityAwarenessStage.PASSIVE
	for route in p_patrol_route:
		patrol_route.append(str(route))

static func _gen_id() -> String:
	return "%08x%08x" % [Time.get_ticks_msec(), randi()]
