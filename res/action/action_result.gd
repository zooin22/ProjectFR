class_name ActionResult

var success: bool
var message: String
var damage_dealt: int
var data: Dictionary

func _init(p_success: bool, p_message: String = "", p_damage: int = 0) -> void:
	success = p_success
	message = p_message
	damage_dealt = p_damage
	data = {}
