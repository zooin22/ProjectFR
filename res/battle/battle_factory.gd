class_name BattleFactory

static func create_default_player() -> ActorState:
	return ActorState.new(
		BattleConstants.DEFAULT_PLAYER_MAX_HP,
		BattleConstants.DEFAULT_PLAYER_MAX_AP,
		BattleConstants.DEFAULT_PLAYER_ATTACK_POWER,
		BattleConstants.PLAYER_DISPLAY_NAME
	)

static func create_default_dungeon(mission: MissionData = null) -> BattleDungeon:
	if mission != null:
		match mission.objective_type:
			MissionObjectiveType.DELETE, MissionObjectiveType.MODIFY:
				return _create_compact_dungeon(mission)
			MissionObjectiveType.SCAN, MissionObjectiveType.ESCAPE:
				return _create_deep_dungeon(mission)
	return _create_standard_dungeon(mission)

static func _create_standard_dungeon(mission: MissionData) -> BattleDungeon:
	var root := FolderNode.new(BattleConstants.DUNGEON_ROOT_NAME, BattleConstants.DUNGEON_ROOT_PATH, _folder_profile("Root Directory", NodeData.NodeThreatLevel.LOW, 8, 2, 1))
	var build_cache := FolderNode.new(BattleConstants.ROOT_BUILD_CACHE_NAME, BattleConstants.ROOT_BUILD_CACHE_PATH, _folder_profile("Guard Folder", NodeData.NodeThreatLevel.MEDIUM, 9, 2, 2))
	var assets := FolderNode.new(BattleConstants.CACHE_ASSETS_NAME, BattleConstants.CACHE_ASSETS_PATH, _folder_profile("Vault Folder", NodeData.NodeThreatLevel.HIGH, 11, 3, 3))
	var boss_archive := ArchiveNode.new(
		BattleConstants.BOSS_ZIP_NAME,
		BattleConstants.BOSS_ZIP_PATH,
		BattleConstants.BOSS_ZIP_SIZE,
		NodeData.NodeCombatProfile.new("Archive Boss", "Critical", NodeData.NodeThreatLevel.CRITICAL, 12, 3, 4, true, true, "Archive broken open; hostile payloads flooded the explorer.")
	)

	root.add_child_node(FileNode.new(BattleConstants.ROOT_README_NAME, BattleConstants.ROOT_README_PATH, BattleConstants.ROOT_README_SIZE, _file_profile("Text File", NodeData.NodeThreatLevel.LOW, 7, 2, 1)))
	root.add_child_node(build_cache)

	build_cache.add_child_node(FileNode.new(BattleConstants.CACHE_TEMP_NAME, BattleConstants.CACHE_TEMP_PATH, BattleConstants.CACHE_TEMP_SIZE, _file_profile("Temp File", NodeData.NodeThreatLevel.MEDIUM, 8, 2, 2)))
	build_cache.add_child_node(FileNode.new(BattleConstants.SYSTEM_LOG_NAME, BattleConstants.SYSTEM_LOG_PATH, BattleConstants.SYSTEM_LOG_SIZE, _file_profile("Log File", NodeData.NodeThreatLevel.MEDIUM, 7, 2, 2)))
	build_cache.add_child_node(assets)

	assets.add_child_node(boss_archive)
	boss_archive.add_child_node(FileNode.new("payload.exe", BattleConstants.BOSS_ZIP_PATH + "/payload.exe", 9, _file_profile("Executable", NodeData.NodeThreatLevel.CRITICAL, 9, 3, 4, true)))
	boss_archive.add_child_node(FileNode.new("hook.dll", BattleConstants.BOSS_ZIP_PATH + "/hook.dll", 6, _file_profile("Library", NodeData.NodeThreatLevel.HIGH, 8, 2, 3)))
	boss_archive.add_child_node(FileNode.new("trace.log", BattleConstants.BOSS_ZIP_PATH + "/trace.log", 4, _file_profile("Log File", NodeData.NodeThreatLevel.MEDIUM, 6, 2, 2)))

	var metadata := {
		BattleConstants.DUNGEON_ROOT_PATH: DungeonFolderMetadata.new("Root Directory", "A noisy root folder with mixed junk and one suspicious cache directory.", "Clipboard setup opportunity", 0),
		BattleConstants.ROOT_BUILD_CACHE_PATH: DungeonFolderMetadata.new("Build Cache", "Stale cache data leaks AP if left alive for too long.", "Safe AP reset before deeper dive", 1),
		BattleConstants.CACHE_ASSETS_PATH: DungeonFolderMetadata.new("Packed Assets", "A volatile vault hiding a configurable payload node.", "Clear the hostile object and collapse the route", 2),
	}

	var dungeon := BattleDungeon.new(root, metadata)
	if mission != null:
		_apply_mission_variants(dungeon, mission, build_cache)
	return dungeon

static func _create_compact_dungeon(mission: MissionData) -> BattleDungeon:
	var root := FolderNode.new(BattleConstants.DUNGEON_ROOT_NAME, BattleConstants.DUNGEON_ROOT_PATH, _folder_profile("Root Directory", NodeData.NodeThreatLevel.LOW, 8, 2, 1))
	var build_cache := FolderNode.new(BattleConstants.ROOT_BUILD_CACHE_NAME, BattleConstants.ROOT_BUILD_CACHE_PATH, _folder_profile("Guard Folder", NodeData.NodeThreatLevel.HIGH, 10, 2, 2))

	root.add_child_node(FileNode.new(BattleConstants.ROOT_README_NAME, BattleConstants.ROOT_README_PATH, BattleConstants.ROOT_README_SIZE, _file_profile("Text File", NodeData.NodeThreatLevel.LOW, 7, 2, 1)))
	root.add_child_node(build_cache)

	build_cache.add_child_node(FileNode.new(BattleConstants.CACHE_TEMP_NAME, BattleConstants.CACHE_TEMP_PATH, BattleConstants.CACHE_TEMP_SIZE, _file_profile("Temp File", NodeData.NodeThreatLevel.MEDIUM, 8, 2, 2)))
	build_cache.add_child_node(FileNode.new(BattleConstants.SYSTEM_LOG_NAME, BattleConstants.SYSTEM_LOG_PATH, BattleConstants.SYSTEM_LOG_SIZE, _file_profile("Log File", NodeData.NodeThreatLevel.MEDIUM, 7, 2, 2)))

	var sentinel_path := BattleConstants.ROOT_BUILD_CACHE_PATH + "/firewall_sentinel.sys"
	build_cache.add_child_node(FileNode.new("firewall_sentinel.sys", sentinel_path, 2,
		NodeData.NodeCombatProfile.new("Firewall Sentinel", "CRITICAL", NodeData.NodeThreatLevel.CRITICAL, 13, 3, 4, false)))

	var metadata := {
		BattleConstants.DUNGEON_ROOT_PATH: DungeonFolderMetadata.new("Root Directory", "Compact layout — the objective is close, but a sentinel guards the cache.", "Neutralize the sentinel to clear the path", 0),
		BattleConstants.ROOT_BUILD_CACHE_PATH: DungeonFolderMetadata.new("Build Cache (Locked)", "Firewall Sentinel deployed at objective depth. Eliminate before proceeding.", "Sentinel clear opens the objective node", 1),
	}

	var dungeon := BattleDungeon.new(root, metadata)
	_apply_mission_variants(dungeon, mission, build_cache)
	return dungeon

static func _create_deep_dungeon(mission: MissionData) -> BattleDungeon:
	var root := FolderNode.new(BattleConstants.DUNGEON_ROOT_NAME, BattleConstants.DUNGEON_ROOT_PATH, _folder_profile("Root Directory", NodeData.NodeThreatLevel.LOW, 8, 2, 1))
	var build_cache := FolderNode.new(BattleConstants.ROOT_BUILD_CACHE_NAME, BattleConstants.ROOT_BUILD_CACHE_PATH, _folder_profile("Guard Folder", NodeData.NodeThreatLevel.MEDIUM, 9, 2, 2))
	var assets := FolderNode.new(BattleConstants.CACHE_ASSETS_NAME, BattleConstants.CACHE_ASSETS_PATH, _folder_profile("Vault Folder", NodeData.NodeThreatLevel.HIGH, 11, 3, 3))
	var boss_archive := ArchiveNode.new(
		BattleConstants.BOSS_ZIP_NAME,
		BattleConstants.BOSS_ZIP_PATH,
		BattleConstants.BOSS_ZIP_SIZE,
		NodeData.NodeCombatProfile.new("Archive Boss", "Critical", NodeData.NodeThreatLevel.CRITICAL, 12, 3, 4, true, true, "Archive broken open; hostile payloads flooded the explorer.")
	)

	var deep_store_path := BattleConstants.ROOT_BUILD_CACHE_PATH + "/DeepStore"
	var deep_store := FolderNode.new("DeepStore", deep_store_path, _folder_profile("Deep Store", NodeData.NodeThreatLevel.HIGH, 10, 2, 3))
	deep_store.add_child_node(FileNode.new("snapshot.bin", deep_store_path + "/snapshot.bin", 7, _file_profile("Binary Snapshot", NodeData.NodeThreatLevel.HIGH, 9, 2, 3)))
	deep_store.add_child_node(FileNode.new("manifest.xml", deep_store_path + "/manifest.xml", 3, _file_profile("Manifest", NodeData.NodeThreatLevel.MEDIUM, 7, 2, 2)))

	var staging_path := BattleConstants.ROOT_BUILD_CACHE_PATH + "/Staging"
	var staging := FolderNode.new("Staging", staging_path, _folder_profile("Staging Area", NodeData.NodeThreatLevel.MEDIUM, 9, 2, 2))
	staging.add_child_node(FileNode.new("exit_vector.cfg", staging_path + "/exit_vector.cfg", 4, _file_profile("Config File", NodeData.NodeThreatLevel.MEDIUM, 7, 2, 2)))
	staging.add_child_node(FileNode.new("decoy_stub.dll", staging_path + "/decoy_stub.dll", 5, _file_profile("Library", NodeData.NodeThreatLevel.MEDIUM, 8, 2, 2)))

	root.add_child_node(FileNode.new(BattleConstants.ROOT_README_NAME, BattleConstants.ROOT_README_PATH, BattleConstants.ROOT_README_SIZE, _file_profile("Text File", NodeData.NodeThreatLevel.LOW, 7, 2, 1)))
	root.add_child_node(build_cache)

	build_cache.add_child_node(FileNode.new(BattleConstants.CACHE_TEMP_NAME, BattleConstants.CACHE_TEMP_PATH, BattleConstants.CACHE_TEMP_SIZE, _file_profile("Temp File", NodeData.NodeThreatLevel.MEDIUM, 8, 2, 2)))
	build_cache.add_child_node(FileNode.new(BattleConstants.SYSTEM_LOG_NAME, BattleConstants.SYSTEM_LOG_PATH, BattleConstants.SYSTEM_LOG_SIZE, _file_profile("Log File", NodeData.NodeThreatLevel.MEDIUM, 7, 2, 2)))
	build_cache.add_child_node(assets)
	build_cache.add_child_node(deep_store)
	build_cache.add_child_node(staging)

	assets.add_child_node(boss_archive)
	boss_archive.add_child_node(FileNode.new("payload.exe", BattleConstants.BOSS_ZIP_PATH + "/payload.exe", 9, _file_profile("Executable", NodeData.NodeThreatLevel.CRITICAL, 9, 3, 4, true)))
	boss_archive.add_child_node(FileNode.new("hook.dll", BattleConstants.BOSS_ZIP_PATH + "/hook.dll", 6, _file_profile("Library", NodeData.NodeThreatLevel.HIGH, 8, 2, 3)))
	boss_archive.add_child_node(FileNode.new("trace.log", BattleConstants.BOSS_ZIP_PATH + "/trace.log", 4, _file_profile("Log File", NodeData.NodeThreatLevel.MEDIUM, 6, 2, 2)))

	var metadata := {
		BattleConstants.DUNGEON_ROOT_PATH: DungeonFolderMetadata.new("Root Directory", "Extended layout — two extra folders deepen the traversal surface.", "Clipboard setup opportunity", 0),
		BattleConstants.ROOT_BUILD_CACHE_PATH: DungeonFolderMetadata.new("Build Cache", "Stale cache data with additional deep storage and staging branches.", "Safe AP reset before deeper dive", 1),
		BattleConstants.CACHE_ASSETS_PATH: DungeonFolderMetadata.new("Packed Assets", "A volatile vault hiding a configurable payload node.", "Clear the hostile object and collapse the route", 2),
		deep_store_path: DungeonFolderMetadata.new("Deep Store", "Archival node cache — rich scan targets but heavily monitored.", "Scan yields high-value intelligence", 2),
		staging_path: DungeonFolderMetadata.new("Staging Area", "Temporary staging zone used for payload handoffs and escape vectors.", "Escape route unlock on clear", 2),
	}

	var dungeon := BattleDungeon.new(root, metadata)
	_apply_mission_variants(dungeon, mission, build_cache)
	return dungeon

static func _apply_mission_variants(dungeon: BattleDungeon, mission: MissionData, build_cache: FolderNode) -> void:
	match mission.id:
		"mission_delete_readme", "mission_extract_readme":
			_boost_node_profile(dungeon.root, BattleConstants.ROOT_README_PATH, NodeData.NodeThreatLevel.HIGH, 3, 1)
		"mission_modify_syslog":
			_boost_node_profile(dungeon.root, BattleConstants.SYSTEM_LOG_PATH, NodeData.NodeThreatLevel.HIGH, 2, 1)
			build_cache.add_child_node(FileNode.new("audit_snapshot.dat", BattleConstants.ROOT_BUILD_CACHE_PATH + "/audit_snapshot.dat", 5,
				_file_profile("Audit File", NodeData.NodeThreatLevel.MEDIUM, 6, 2, 2)))
		"mission_extract_boss", "mission_delete_boss":
			_boost_node_profile(dungeon.root, BattleConstants.BOSS_ZIP_PATH, NodeData.NodeThreatLevel.CRITICAL, 4, 1)
		"mission_scan_cache":
			build_cache.add_child_node(FileNode.new("index.db", BattleConstants.ROOT_BUILD_CACHE_PATH + "/index.db", 8,
				_file_profile("Index File", NodeData.NodeThreatLevel.MEDIUM, 7, 2, 2)))
			build_cache.add_child_node(FileNode.new("scan_queue.tmp", BattleConstants.ROOT_BUILD_CACHE_PATH + "/scan_queue.tmp", 3,
				_file_profile("Queue File", NodeData.NodeThreatLevel.LOW, 5, 2, 1)))
		"mission_escape_only":
			_reduce_all_node_profiles(dungeon.root, 2)

	_mark_objective_node(dungeon.root, mission.target_path)

static func _mark_objective_node(container: ContainerNode, target_path: String) -> void:
	for node: NodeData in _enumerate_all(container):
		if node.path.to_lower() == target_path.to_lower():
			node.role = ExplorerNodeRole.OBJECTIVE
			return

static func _boost_node_profile(container: ContainerNode, target_path: String, new_threat: int, hp_bonus: int, ap_bonus: int) -> void:
	for node: NodeData in _enumerate_all(container):
		if node.path.to_lower() != target_path.to_lower():
			continue
		var p: NodeData.NodeCombatProfile = node.combat_profile
		node.combat_profile = NodeData.NodeCombatProfile.new(
			p.type_name, NodeData.NodeThreatLevel.keys()[new_threat].to_upper(), new_threat,
			p.base_max_hp + hp_bonus, p.base_max_ap + ap_bonus, p.base_attack_power + 1,
			p.is_boss, p.reveals_children_on_open, p.reveal_summary
		)
		return

static func _reduce_all_node_profiles(container: ContainerNode, hp_reduction: int) -> void:
	for node: NodeData in _enumerate_all(container):
		var p: NodeData.NodeCombatProfile = node.combat_profile
		node.combat_profile = NodeData.NodeCombatProfile.new(
			p.type_name, p.threat_label, p.threat_level,
			max(1, p.base_max_hp - hp_reduction), p.base_max_ap, p.base_attack_power,
			p.is_boss, p.reveals_children_on_open, p.reveal_summary
		)

static func create_encounter(container: ContainerNode, modifiers: CampaignModifiers, include: Callable = Callable()) -> Array:
	var result := []
	for node: NodeData in container.children:
		if include.is_valid() and not include.call(node):
			continue
		result.append([create_actor_for_node(node, modifiers), node])
	return result

static func create_actor_for_node(node: NodeData, modifiers: CampaignModifiers) -> ActorState:
	var profile: NodeData.NodeCombatProfile = node.combat_profile
	return ActorState.new(
		profile.base_max_hp + modifiers.enemy_hp_bonus,
		profile.base_max_ap + modifiers.enemy_ap_bonus,
		profile.base_attack_power + modifiers.enemy_attack_bonus,
		node.name
	)

static func _file_profile(type_name: String, threat_level: int, hp: int, ap: int, attack_power: int, is_boss: bool = false) -> NodeData.NodeCombatProfile:
	return NodeData.NodeCombatProfile.new(type_name, NodeData.NodeThreatLevel.keys()[threat_level].to_upper(), threat_level, hp, ap, attack_power, is_boss)

static func _folder_profile(type_name: String, threat_level: int, hp: int, ap: int, attack_power: int) -> NodeData.NodeCombatProfile:
	return NodeData.NodeCombatProfile.new(type_name, NodeData.NodeThreatLevel.keys()[threat_level].to_upper(), threat_level, hp, ap, attack_power, false, true, "Container opened; hidden children spilled into view.")

static func _enumerate_all(container: ContainerNode) -> Array[NodeData]:
	var result: Array[NodeData] = []
	for child: NodeData in container.children:
		result.append(child)
		if child is ContainerNode:
			result.append_array(_enumerate_all(child as ContainerNode))
	return result
