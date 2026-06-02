class_name DungeonFolderMetadata

var theme_name: String
var event_summary: String
var reward_preview: String
var depth: int

func _init(
	p_theme_name: String,
	p_event_summary: String,
	p_reward_preview: String,
	p_depth: int
) -> void:
	theme_name = p_theme_name
	event_summary = p_event_summary
	reward_preview = p_reward_preview
	depth = p_depth
