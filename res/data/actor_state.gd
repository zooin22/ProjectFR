class_name ActorState

var id: String
var display_name: String
var max_hp: int
var current_hp: int
var max_ap: int
var current_ap: int
var attack_power: int

func _init(p_max_hp: int = 20, p_max_ap: int = 5, p_attack_power: int = 3, p_display_name: String = "", p_id: String = "") -> void:
	id = p_id if p_id != "" else _gen_id()
	display_name = p_display_name if p_display_name != "" else "Actor-" + id.substr(0, 6)
	max_hp = p_max_hp
	current_hp = p_max_hp
	max_ap = p_max_ap
	current_ap = p_max_ap
	attack_power = p_attack_power

static func _gen_id() -> String:
	return "%08x%08x" % [Time.get_ticks_msec(), randi()]

var is_alive: bool:
	get: return current_hp > 0

var has_ap: bool:
	get: return current_ap > 0

func take_damage(damage: int) -> void:
	current_hp = max(0, current_hp - damage)

func heal(amount: int) -> void:
	current_hp = min(max_hp, current_hp + amount)

func consume_ap(amount: int) -> void:
	current_ap = max(0, current_ap - amount)

func restore_ap(amount: int) -> void:
	current_ap = min(max_ap, current_ap + amount)

func restore_all_ap() -> void:
	current_ap = max_ap

func clone() -> ActorState:
	var c := ActorState.new(max_hp, max_ap, attack_power, display_name)
	c.current_hp = current_hp
	c.current_ap = current_ap
	return c

func _to_string() -> String:
	return display_name
