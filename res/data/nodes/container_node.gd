class_name ContainerNode
extends NodeData

var children: Array[NodeData] = []

func _init(p_name: String, p_path: String, p_is_folder: bool, p_size: int = 0, p_combat_profile: NodeData.NodeCombatProfile = null) -> void:
	super(p_name, p_path, p_is_folder, p_size, p_combat_profile)

var is_container: bool:
	get: return true

func add_child_node(node: NodeData) -> void:
	for child in children:
		if child.path.to_lower() == node.path.to_lower():
			return
	children.append(node)

func remove_child_node(node: NodeData) -> void:
	children.erase(node)
