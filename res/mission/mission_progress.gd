class_name MissionProgress

var mission: MissionData
var objective_completed: bool = false

func _init(p_mission: MissionData) -> void:
	mission = p_mission

func register_action(action_id: String, target_path: String) -> String:
	if objective_completed or target_path.is_empty():
		return ""
	if not target_path.to_lower() == mission.target_path.to_lower():
		return ""

	var matched: bool = false
	match mission.objective_type:
		MissionObjectiveType.EXTRACT:
			matched = action_id == ActionIds.COPY
		MissionObjectiveType.DELETE:
			matched = action_id == ActionIds.DELETE
		MissionObjectiveType.SCAN:
			matched = action_id == ActionIds.INSPECT
		MissionObjectiveType.MODIFY:
			matched = action_id == ActionIds.LOG_FORGE
		MissionObjectiveType.ESCAPE:
			matched = action_id == ActionIds.EXTRACT

	if not matched:
		return ""

	objective_completed = true
	return "Mission objective complete: %d @ %s" % [mission.objective_type, mission.target_path]

func register_escape(current_path: String) -> String:
	if objective_completed or mission.objective_type != MissionObjectiveType.ESCAPE:
		return ""
	objective_completed = true
	var resolved_path: String = current_path if not current_path.is_empty() else "extraction point"
	return "Mission objective complete: %d @ %s" % [mission.objective_type, resolved_path]

func has_exceeded_turn_limit(turn_count: int, turn_limit: int) -> bool:
	return turn_count >= turn_limit

func resolve(player_survived: bool, extracted: bool, turns_used: int, turn_limit: int, trace: int = 0) -> MissionResult:
	var success: bool = player_survived and extracted and objective_completed and not has_exceeded_turn_limit(turns_used, turn_limit)
	var summary: String = (
		"%s complete. Client package delivered cleanly." % mission.title
		if success else
		_build_failure_summary(player_survived, extracted, turns_used, turn_limit)
	)
	var trace_heat: int = trace / InfiltrationTuning.TRACE_PER_HEAT_POINT
	var heat_delta: int = trace_heat if success else mission.failure_heat + trace_heat
	return MissionResult.new(
		mission,
		success,
		summary,
		mission.reward_credits if success else -mission.failure_penalty_credits,
		mission.reward_reputation if success else -1,
		heat_delta,
		objective_completed,
		player_survived,
		extracted,
		turns_used
	)

func _build_failure_summary(player_survived: bool, extracted: bool, turns_used: int, turn_limit: int) -> String:
	if not player_survived:
		return "%s failed. Operator trace collapsed before extraction." % mission.title
	if has_exceeded_turn_limit(turns_used, turn_limit):
		return "%s failed. Trace level spiked past turn limit." % mission.title
	if not objective_completed:
		return "%s failed. Objective at %s was not secured." % [mission.title, mission.target_path]
	if not extracted:
		return "%s failed. Objective was secured, but extraction never completed." % mission.title
	return "%s failed." % mission.title
