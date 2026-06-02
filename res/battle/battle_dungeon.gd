class_name BattleDungeon

var root: FolderNode
var current_container: ContainerNode
var total_node_count: int

var _folder_metadata: Dictionary = {}   # String(lower) -> DungeonFolderMetadata
var _node_index: Dictionary = {}        # String(lower) -> NodeData
var _parent_index: Dictionary = {}      # String(lower) -> ContainerNode or null
var _cleared_paths: Dictionary = {}     # String(lower) -> true

func _init(p_root: FolderNode, p_folder_metadata: Dictionary) -> void:
	root = p_root
	current_container = p_root
	for k: String in p_folder_metadata:
		_folder_metadata[k.to_lower()] = p_folder_metadata[k]
	_index_node(p_root, null)
	total_node_count = _count_nodes(p_root)

var cleared_node_count: int:
	get: return _cleared_paths.size()

func get_current_encounter_nodes() -> Array[NodeData]:
	var result: Array[NodeData] = []
	for child: NodeData in current_container.children:
		if not _cleared_paths.has(child.path.to_lower()):
			result.append(child)
	return result

func get_all_folders() -> Array:
	var result := []
	for node in _node_index.values():
		if node is FolderNode:
			result.append(node)
	return result

func get_node_at(path: String) -> NodeData:
	return _node_index.get(path.to_lower(), null)

func get_parent_container(path: String) -> ContainerNode:
	return _parent_index.get(path.to_lower(), null)

func enter_container(path: String) -> bool:
	var node: NodeData = _node_index.get(path.to_lower(), null)
	if node == null or not node is ContainerNode:
		return false
	current_container = node as ContainerNode
	return true

func enter_parent_of_current() -> bool:
	var parent: ContainerNode = _parent_index.get(current_container.path.to_lower(), null)
	if parent == null:
		return false
	current_container = parent
	return true

func is_cleared(path: String) -> bool:
	return _cleared_paths.has(path.to_lower())

func mark_cleared(node: NodeData) -> void:
	_mark_cleared_recursive(node)
	var parent: ContainerNode = get_parent_container(node.path)
	if parent != null:
		parent.remove_child_node(node)
	if current_container == node and parent != null:
		current_container = parent

func restore_node(path: String) -> bool:
	var key := path.to_lower()
	if not _cleared_paths.has(key):
		return false
	var node: NodeData = _node_index.get(key, null)
	if node == null:
		return false
	_cleared_paths.erase(key)
	var parent: ContainerNode = get_parent_container(path)
	if parent != null:
		parent.add_child_node(node)
	return true

func move_node(node_path: String, target_container_path: String) -> bool:
	var node: NodeData = _node_index.get(node_path.to_lower(), null)
	if node == null:
		return false
	var target = _node_index.get(target_container_path.to_lower(), null)
	if target == null or not target is ContainerNode:
		return false
	var current_parent: ContainerNode = _parent_index.get(node_path.to_lower(), null)
	if current_parent == null:
		return false
	if current_parent.path.to_lower() == (target as ContainerNode).path.to_lower():
		return false
	current_parent.remove_child_node(node)
	(target as ContainerNode).add_child_node(node)
	_parent_index[node_path.to_lower()] = target
	return true

func has_remaining_nodes() -> bool:
	for node: NodeData in _enumerate_all_nodes(root):
		if not _cleared_paths.has(node.path.to_lower()):
			return true
	return false

func get_progress_label() -> String:
	return "Cleared %d/%d nodes" % [cleared_node_count, total_node_count]

func get_current_metadata() -> DungeonFolderMetadata:
	return get_metadata_for_path(current_container.path)

func get_metadata_for_path(path: String) -> DungeonFolderMetadata:
	var cursor := path
	while true:
		var meta: DungeonFolderMetadata = _folder_metadata.get(cursor.to_lower(), null)
		if meta != null:
			return meta
		var parent: ContainerNode = get_parent_container(cursor)
		if parent == null:
			break
		cursor = parent.path
	return DungeonFolderMetadata.new("Unclassified", "No event data", "No reward data", 0)

func _index_node(node: NodeData, parent: ContainerNode) -> void:
	_node_index[node.path.to_lower()] = node
	_parent_index[node.path.to_lower()] = parent
	if not node is ContainerNode:
		return
	for child: NodeData in (node as ContainerNode).children:
		_index_node(child, node as ContainerNode)

func _mark_cleared_recursive(node: NodeData) -> void:
	_cleared_paths[node.path.to_lower()] = true
	if not node is ContainerNode:
		return
	for child: NodeData in (node as ContainerNode).children.duplicate():
		_mark_cleared_recursive(child)

func _count_nodes(container: ContainerNode) -> int:
	var total := 0
	for child: NodeData in container.children:
		total += 1
		if child is ContainerNode:
			total += _count_nodes(child as ContainerNode)
	return total

func _enumerate_all_nodes(container: ContainerNode) -> Array[NodeData]:
	var result: Array[NodeData] = []
	for child: NodeData in container.children:
		result.append(child)
		if child is ContainerNode:
			result.append_array(_enumerate_all_nodes(child as ContainerNode))
	return result
