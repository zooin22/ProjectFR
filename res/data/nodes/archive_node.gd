class_name ArchiveNode
extends ContainerNode

func _init(p_name: String, p_path: String, p_size: int = 0, p_combat_profile: NodeData.NodeCombatProfile = null) -> void:
	if p_combat_profile == null:
		p_combat_profile = NodeData.NodeCombatProfile.new(
			"Archive", "High", NodeData.NodeThreatLevel.HIGH, 10, 3, 3, false, true,
			"Archive cracked open; payload spilled into view."
		)
	super(p_name, p_path, false, p_size, p_combat_profile)
