class_name MissionResult

var mission: MissionData
var success: bool
var summary: String
var credits_delta: int
var reputation_delta: int
var heat_delta: int
var objective_completed: bool
var player_survived: bool
var dungeon_cleared: bool
var turns_used: int

func _init(
	p_mission: MissionData,
	p_success: bool,
	p_summary: String,
	p_credits_delta: int,
	p_reputation_delta: int,
	p_heat_delta: int,
	p_objective_completed: bool,
	p_player_survived: bool,
	p_dungeon_cleared: bool,
	p_turns_used: int
) -> void:
	mission = p_mission
	success = p_success
	summary = p_summary
	credits_delta = p_credits_delta
	reputation_delta = p_reputation_delta
	heat_delta = p_heat_delta
	objective_completed = p_objective_completed
	player_survived = p_player_survived
	dungeon_cleared = p_dungeon_cleared
	turns_used = p_turns_used
