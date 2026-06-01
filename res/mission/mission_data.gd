class_name MissionData

var id: String
var title: String
var client: MissionClientProfile
var briefing: String
var objective_type: int
var target_path: String
var turn_limit: int
var reward_credits: int
var reward_reputation: int
var failure_penalty_credits: int
var failure_heat: int
var prerequisite_mission_id: String = ""
var conflict_group: String = ""
var required_faction_reputation: int = -1

func _init(
	p_id: String,
	p_title: String,
	p_client: MissionClientProfile,
	p_briefing: String,
	p_objective_type: int,
	p_target_path: String,
	p_turn_limit: int,
	p_reward_credits: int,
	p_reward_reputation: int,
	p_failure_penalty_credits: int,
	p_failure_heat: int,
	p_prerequisite_mission_id: String = "",
	p_conflict_group: String = "",
	p_required_faction_reputation: int = -1
) -> void:
	id = p_id
	title = p_title
	client = p_client
	briefing = p_briefing
	objective_type = p_objective_type
	target_path = p_target_path
	turn_limit = p_turn_limit
	reward_credits = p_reward_credits
	reward_reputation = p_reward_reputation
	failure_penalty_credits = p_failure_penalty_credits
	failure_heat = p_failure_heat
	prerequisite_mission_id = p_prerequisite_mission_id
	conflict_group = p_conflict_group
	required_faction_reputation = p_required_faction_reputation
