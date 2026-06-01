class_name NodeData

enum NodeThreatLevel {
	LOW,
	MEDIUM,
	HIGH,
	CRITICAL,
}

class NodeCombatProfile:
	var type_name: String
	var threat_label: String
	var threat_level: int
	var base_max_hp: int
	var base_max_ap: int
	var base_attack_power: int
	var is_boss: bool
	var reveals_children_on_open: bool
	var reveal_summary: String

	func _init(
		p_type_name: String,
		p_threat_label: String,
		p_threat_level: int,
		p_base_max_hp: int,
		p_base_max_ap: int,
		p_base_attack_power: int,
		p_is_boss: bool = false,
		p_reveals_children_on_open: bool = false,
		p_reveal_summary: String = ""
	) -> void:
		type_name = p_type_name
		threat_label = p_threat_label
		threat_level = p_threat_level
		base_max_hp = p_base_max_hp
		base_max_ap = p_base_max_ap
		base_attack_power = p_base_attack_power
		is_boss = p_is_boss
		reveals_children_on_open = p_reveals_children_on_open
		reveal_summary = p_reveal_summary

var name: String
var path: String
var is_folder: bool
var size: int
var combat_profile: NodeCombatProfile
var role: int = -1

func _init(p_name: String, p_path: String, p_is_folder: bool, p_size: int = 0, p_combat_profile: NodeCombatProfile = null) -> void:
	name = p_name
	path = p_path
	is_folder = p_is_folder
	size = p_size
	combat_profile = p_combat_profile if p_combat_profile != null else _create_fallback_profile(p_is_folder)

var is_container: bool:
	get: return false

var ui_type_name: String:
	get: return combat_profile.type_name

func _to_string() -> String:
	return "%s (%s)" % [name, ui_type_name]

func _create_fallback_profile(p_is_folder: bool) -> NodeCombatProfile:
	if p_is_folder:
		return NodeCombatProfile.new("Folder", "Medium", NodeThreatLevel.MEDIUM, 8, 2, 2, false, true)
	return NodeCombatProfile.new("File", "Low", NodeThreatLevel.LOW, 7, 2, 2)
