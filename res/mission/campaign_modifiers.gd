class_name CampaignModifiers

var heat_turn_penalty: int
var enemy_attack_bonus: int
var enemy_ap_bonus: int
var enemy_hp_bonus: int
var summary: String

func _init(
	p_heat_turn_penalty: int,
	p_enemy_attack_bonus: int,
	p_enemy_ap_bonus: int,
	p_enemy_hp_bonus: int,
	p_summary: String
) -> void:
	heat_turn_penalty = p_heat_turn_penalty
	enemy_attack_bonus = p_enemy_attack_bonus
	enemy_ap_bonus = p_enemy_ap_bonus
	enemy_hp_bonus = p_enemy_hp_bonus
	summary = p_summary
