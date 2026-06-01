class_name SpecialFileNode
extends FileNode

func _init(p_name: String, p_path: String, p_size: int = 0) -> void:
	var profile := NodeData.NodeCombatProfile.new(
		"Special File", "High", NodeData.NodeThreatLevel.HIGH, 10, 3, 3, true
	)
	super(p_name, p_path, p_size, profile)
