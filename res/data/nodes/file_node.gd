class_name FileNode
extends NodeData

func _init(p_name: String, p_path: String, p_size: int = 0, p_combat_profile: NodeData.NodeCombatProfile = null) -> void:
	super(p_name, p_path, false, p_size, p_combat_profile)
