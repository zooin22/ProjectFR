extends Node

enum StatusEffect {
	QUARANTINE,
	COMPRESSED,
	CORRUPTED,
}

class StatusEffectInstance:
	var type: int
	var duration: int
	var magnitude: int

	func _init(p_type: int, p_duration: int, p_magnitude: int = 0) -> void:
		type = p_type
		duration = p_duration
		magnitude = p_magnitude

	func decrement_duration() -> void:
		duration = max(0, duration - 1)

	var is_expired: bool:
		get: return duration <= 0

var _actor_effects: Dictionary = {}  # String -> Array[StatusEffectInstance]

func add_effect(actor_id: String, type: int, duration: int, magnitude: int = 0) -> void:
	if not _actor_effects.has(actor_id):
		_actor_effects[actor_id] = []
	_actor_effects[actor_id].append(StatusEffectInstance.new(type, duration, magnitude))

func remove_effect(actor_id: String, type: int) -> void:
	if _actor_effects.has(actor_id):
		_actor_effects[actor_id] = _actor_effects[actor_id].filter(func(e): return e.type != type)

func has_effect(actor_id: String, type: int) -> bool:
	if not _actor_effects.has(actor_id):
		return false
	return _actor_effects[actor_id].any(func(e): return e.type == type and not e.is_expired)

func get_effects(actor_id: String) -> Array:
	if not _actor_effects.has(actor_id):
		return []
	return _actor_effects[actor_id].filter(func(e): return not e.is_expired)

func update_durations(actor_id: String) -> void:
	if not _actor_effects.has(actor_id):
		return
	for effect in _actor_effects[actor_id]:
		effect.decrement_duration()
	_actor_effects[actor_id] = _actor_effects[actor_id].filter(func(e): return not e.is_expired)

func clear_effects(actor_id: String) -> void:
	if _actor_effects.has(actor_id):
		_actor_effects[actor_id].clear()

func get_attack_modifier(actor_id: String) -> int:
	if not has_effect(actor_id, StatusEffect.COMPRESSED):
		return 0
	for e in get_effects(actor_id):
		if e.type == StatusEffect.COMPRESSED:
			return e.magnitude
	return 0
