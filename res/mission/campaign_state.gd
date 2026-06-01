extends Node

const SAVE_FILE_PATH: String = "user://campaign.json"

var credits: int = 100
var reputation: int = 0
var heat: int = 0
var current_mission: MissionData = null
var last_mission_result: MissionResult = null

var _mission_board: Array = []
var _faction_reputation: Dictionary = {}
var _completed_mission_ids: Array = []
var _selected_mission_index: int = 0

func get_faction_reputation(faction_id: int) -> int:
	return _faction_reputation.get(faction_id, 0)

func is_mission_available(mission: MissionData) -> bool:
	if not mission.prerequisite_mission_id.is_empty() and not _completed_mission_ids.has(mission.prerequisite_mission_id):
		return false
	if mission.required_faction_reputation >= 0 and get_faction_reputation(mission.client.faction_id) < mission.required_faction_reputation:
		return false
	if not mission.conflict_group.is_empty():
		for m in _mission_board:
			if m.id != mission.id and m.conflict_group == mission.conflict_group and _completed_mission_ids.has(m.id):
				return false
	return true

func is_mission_completed(mission_id: String) -> bool:
	return _completed_mission_ids.has(mission_id)

func ensure_initialized() -> void:
	if _mission_board.size() > 0:
		return
	_mission_board.append_array(MissionBoardFactory.create_default_board())
	_try_load_from_file()
	_selected_mission_index = 0
	var initial := _get_available_missions()
	current_mission = initial[0] if initial.size() > 0 else _mission_board[0]

func reset() -> void:
	credits = 100
	reputation = 0
	heat = 0
	_completed_mission_ids.clear()
	_faction_reputation.clear()
	_mission_board.clear()
	_selected_mission_index = 0
	current_mission = null
	last_mission_result = null
	if FileAccess.file_exists(SAVE_FILE_PATH):
		var dir := DirAccess.open("user://")
		if dir:
			dir.remove("campaign.json")
	ensure_initialized()

func get_selected_mission() -> MissionData:
	ensure_initialized()
	var available := _get_available_missions()
	if available.size() == 0:
		available = _mission_board
	_selected_mission_index = clampi(_selected_mission_index, 0, available.size() - 1)
	return available[_selected_mission_index]

func select_next_mission() -> MissionData:
	ensure_initialized()
	var available := _get_available_missions()
	if available.size() == 0:
		available = _mission_board
	_selected_mission_index = (_selected_mission_index + 1) % available.size()
	current_mission = available[_selected_mission_index]
	return current_mission

func select_previous_mission() -> MissionData:
	ensure_initialized()
	var available := _get_available_missions()
	if available.size() == 0:
		available = _mission_board
	_selected_mission_index = (_selected_mission_index - 1 + available.size()) % available.size()
	current_mission = available[_selected_mission_index]
	return current_mission

func begin_selected_mission() -> void:
	current_mission = get_selected_mission()

func get_modifiers() -> CampaignModifiers:
	if heat >= 6:
		return CampaignModifiers.new(2, 2, 1, 3, "TRACE CRITICAL · security actively hardens the node")
	elif heat >= 3:
		return CampaignModifiers.new(1, 1, 0, 1, "TRACE ELEVATED · patrols are alert")
	else:
		return CampaignModifiers.new(0, 0, 0, 0, "TRACE LOW · standard intrusion posture")

func apply_mission_result(result: MissionResult) -> void:
	credits = maxi(0, credits + result.credits_delta)
	reputation = maxi(-10, reputation + result.reputation_delta)
	heat = maxi(0, heat + result.heat_delta)
	if result.success:
		heat = maxi(0, heat - InfiltrationTuning.SUCCESS_HEAT_REDUCTION)
		if not _completed_mission_ids.has(result.mission.id):
			_completed_mission_ids.append(result.mission.id)
	var faction_id: int = result.mission.client.faction_id
	_faction_reputation[faction_id] = maxi(-10, get_faction_reputation(faction_id) + result.reputation_delta)
	last_mission_result = result
	_save_to_file()

func _get_available_missions() -> Array:
	var result: Array = []
	for m in _mission_board:
		if is_mission_available(m):
			result.append(m)
	return result

func _save_to_file() -> void:
	var data := {
		"credits": credits,
		"reputation": reputation,
		"heat": heat,
		"faction_reputation": _faction_reputation.duplicate(),
		"completed_mission_ids": _completed_mission_ids.duplicate(),
	}
	var file := FileAccess.open(SAVE_FILE_PATH, FileAccess.WRITE)
	if file:
		file.store_string(JSON.stringify(data))
	else:
		push_warning("CampaignState: failed to save campaign.json — %d" % FileAccess.get_open_error())

func _try_load_from_file() -> void:
	if not FileAccess.file_exists(SAVE_FILE_PATH):
		return
	var file := FileAccess.open(SAVE_FILE_PATH, FileAccess.READ)
	if not file:
		return
	var text := file.get_as_text()
	var parsed = JSON.parse_string(text)
	if parsed == null:
		var bak_file := FileAccess.open(SAVE_FILE_PATH + ".bak", FileAccess.WRITE)
		if bak_file:
			bak_file.store_string(text)
		push_warning("CampaignState: failed to parse campaign.json — backed up to campaign.json.bak; starting fresh.")
		return
	credits = int(parsed.get("credits", 100))
	reputation = int(parsed.get("reputation", 0))
	heat = int(parsed.get("heat", 0))
	var fr: Dictionary = parsed.get("faction_reputation", {})
	for k in fr:
		_faction_reputation[int(k)] = int(fr[k])
	var completed: Array = parsed.get("completed_mission_ids", [])
	for id in completed:
		if not _completed_mission_ids.has(id):
			_completed_mission_ids.append(id)
