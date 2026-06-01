class_name FolderNode
extends ContainerNode

func _init(p_name: String, p_path: String, p_combat_profile: NodeData.NodeCombatProfile = null) -> void:
	super(p_name, p_path, true, 0, p_combat_profile)
