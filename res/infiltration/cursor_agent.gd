class_name CursorAgent

var current_node_path: String = "res://"
var action_points: int = 3
var max_action_points: int = 3
var clipboard_capacity: int = 1
var pouch_capacity: int = 1
var pouch_max_file_size: int = 4
var is_detected: bool = false

func restore_action_points() -> void:
	action_points = max_action_points
