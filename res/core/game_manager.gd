extends Node

# current_battle is untyped during migration; will be typed once BattleManager is ported to GDScript
var current_battle = null

func _ready() -> void:
	DebugLog.info("GameManager", "singleton ready")

func start_new_battle() -> void:
	if current_battle != null:
		DebugLog.warn("GameManager", "start_new_battle called while a battle is already active; ignoring")
		return
	DebugLog.info("GameManager", "starting new battle")
	# BattleManager instantiation deferred to C# layer during migration

func end_current_battle() -> void:
	DebugLog.info("GameManager", "ending current battle")
	current_battle = null
