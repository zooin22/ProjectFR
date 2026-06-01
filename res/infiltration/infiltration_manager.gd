class_name InfiltrationManager

var state: InfiltrationState = InfiltrationState.new()
var security_agents: Array = []
var mission: MissionData
var _security_behavior_executor: SecurityBehaviorExecutor = SecurityBehaviorExecutor.new()

func _init(p_mission: MissionData) -> void:
	mission = p_mission


func initialize(start_folder_path: String, known_nodes: Array) -> void:
	state.current_folder_path = start_folder_path
	state.cursor_agent.current_node_path = start_folder_path
	state.known_node_paths.clear()
	state.windows.clear()
	state.clipboard.clear()
	state.pouch_cache.clear()
	state.exposed_pouch_paths.clear()
	state.permission_override_turns.clear()
	state.tracked_path_turns.clear()
	state.forced_lock_turns.clear()
	state.scan_pressure_turns.clear()
	state.active_operations.clear()
	state.command_queue.clear()
	state.run_status = RunState.RunStatus.ACTIVE
	state.objective_state = RunState.ObjectiveState.REVEALED
	state.operator_hp = state.operator_max_hp
	state.last_turn_contact_damage = 0
	for node in known_nodes:
		state.known_node_paths[node.path.to_lower()] = true

	var main_window: ExplorerWindowState = ExplorerWindowState.new()
	main_window.window_type = ExplorerWindowState.ExplorerWindowType.MAIN
	main_window.title = "Main Infiltration Window"
	main_window.bound_path = start_folder_path
	main_window.is_open = true
	main_window.is_focused = true
	main_window.slot_index = 0
	state.windows.append(main_window)

	state.add_log("Infiltration started: %s" % mission.title)


func add_security_agent(agent: SecurityAgent) -> void:
	security_agents.append(agent)


func open_window(window_type: int, title: String, bound_path: String, trace_modifier: int = 0) -> ExplorerWindowState:
	for w in state.windows:
		if w.window_type == window_type and w.bound_path.to_lower() == bound_path.to_lower():
			w.is_open = true
			focus_window(w.window_id)
			return w

	for w in state.windows:
		w.is_focused = false

	var window: ExplorerWindowState = ExplorerWindowState.new()
	window.window_type = window_type
	window.title = title
	window.bound_path = bound_path
	window.is_open = true
	window.is_focused = true
	window.slot_index = state.windows.size()
	window.trace_modifier = trace_modifier
	state.windows.append(window)

	if trace_modifier > 0:
		add_trace(trace_modifier, "Opened %s" % title)
	state.add_log("Window opened: %s @ %s" % [title, bound_path])
	return window


func focus_window(window_id: String) -> void:
	for w in state.windows:
		w.is_focused = (w.window_id == window_id)


func close_window(window_id: String) -> void:
	var window: ExplorerWindowState = null
	for w in state.windows:
		if w.window_id == window_id:
			window = w
			break
	if window == null or window.window_type == ExplorerWindowState.ExplorerWindowType.MAIN:
		return
	window.is_open = false
	window.is_focused = false
	for w in state.windows:
		if w.window_type == ExplorerWindowState.ExplorerWindowType.MAIN:
			w.is_focused = true
			break
	state.add_log("Window closed: %s" % window.title)


func open_log_viewer_window() -> ExplorerWindowState:
	return open_window(ExplorerWindowState.ExplorerWindowType.LOG_VIEWER, "Event Log", "system://event-log", 0)


func close_log_viewer_window() -> bool:
	for w in state.windows:
		if w.window_type == ExplorerWindowState.ExplorerWindowType.LOG_VIEWER and w.is_open:
			close_window(w.window_id)
			return true
	return false


func advance_turn() -> void:
	state.turn_count += 1
	state.cursor_agent.restore_action_points()
	_tick_operations()
	_apply_multi_window_parallel_operation_trace()
	_tick_permission_overrides()
	_tick_turn_dictionary(state.tracked_path_turns, "Tracked path expired")
	_tick_turn_dictionary(state.forced_lock_turns, "Forced lock expired")
	_tick_turn_dictionary(state.scan_pressure_turns, "Scan pressure expired")
	var was_detected: bool = state.cursor_agent.is_detected
	_advance_security_agents()
	if not was_detected and state.cursor_agent.is_detected:
		_interrupt_monitored_operations_on_detection()
	_apply_detection_contact_damage()
	state.add_log("Turn advanced to %d" % state.turn_count)


func queue_command(entry: CommandQueueEntry) -> void:
	var error: String = _validate_queue_entry(entry)
	if error != "":
		state.add_log("Queue rejected: %s" % error)
		return
	entry.order = state.command_queue.size() + 1
	state.command_queue.append(entry)
	state.add_log("Queued: %s" % entry.summary)


func clear_queue() -> void:
	state.command_queue.clear()
	state.add_log("Command queue cleared")


func execute_queued_commands() -> void:
	var sorted: Array = state.command_queue.duplicate()
	sorted.sort_custom(func(a: CommandQueueEntry, b: CommandQueueEntry) -> bool: return a.order < b.order)
	var started_count: int = 0
	var skipped_count: int = 0
	for entry in sorted:
		var error: String = _validate_queue_entry(entry)
		if error != "":
			skipped_count += 1
			state.add_log("Queue skipped: %s" % error)
			continue
		var operation: FileOperation = _create_operation_from_queue_entry(entry)
		start_operation(operation)
		started_count += 1
	state.command_queue.clear()
	state.add_log("Command queue executed (%d started, %d skipped)" % [started_count, skipped_count])


func start_operation(operation: FileOperation) -> void:
	operation.start()
	state.active_operations.append(operation)
	state.add_log("Operation started: %d @ %s" % [operation.type, operation.target_node_path])
	_apply_tracked_path_action_trace(operation.target_node_path, operation.type)
	if get_monitoring_agents(operation.target_node_path).size() > 0:
		add_trace(InfiltrationTuning.MONITORED_OPERATION_TRACE_INCREASE, "Monitored operation: %d @ %s" % [operation.type, operation.target_node_path])


func add_trace(amount: int, reason: String) -> void:
	state.trace = min(state.max_trace, state.trace + amount)
	state.add_log("Trace +%d: %s" % [amount, reason])
	_update_alert_stage()


func reduce_trace(amount: int, reason: String) -> void:
	state.trace = max(0, state.trace - amount)
	state.add_log("Trace -%d: %s" % [amount, reason])
	_update_alert_stage()


func try_copy_to_clipboard(node_path: String, node_kind: int, size: int = 0) -> bool:
	if state.clipboard.size() >= state.cursor_agent.clipboard_capacity:
		state.add_log("Clipboard full")
		return false
	var entry: ClipboardEntry = ClipboardEntry.new()
	entry.node_path = node_path
	entry.node_kind = node_kind
	entry.size = size
	state.clipboard.append(entry)
	state.add_log("Clipboard add: %s" % node_path)
	return true


func try_move_clipboard_to_pouch(node_path: String, size: int) -> bool:
	if state.pouch_cache.size() >= state.cursor_agent.pouch_capacity:
		state.add_log("Pouch cache full")
		return false
	if size > state.cursor_agent.pouch_max_file_size:
		state.add_log("Pouch rejected oversized file: %s (%d)" % [node_path, size])
		return false
	var clipboard_entry: ClipboardEntry = null
	for e in state.clipboard:
		if e.node_path.to_lower() == node_path.to_lower():
			clipboard_entry = e
			break
	if clipboard_entry == null:
		state.add_log("Clipboard entry missing: %s" % node_path)
		return false
	clipboard_entry.size = size
	clipboard_entry.is_ghosted = true
	state.exposed_pouch_paths.erase(node_path.to_lower())
	state.clipboard.erase(clipboard_entry)
	state.pouch_cache.append(clipboard_entry)
	reduce_trace(InfiltrationTuning.POUCH_HIDE_TRACE_REDUCTION, "Cheek pouch hid small file: %s" % node_path)
	state.add_log("Pouch cache add: %s" % node_path)
	return true


func try_restore_from_pouch(node_path: String) -> bool:
	if state.clipboard.size() >= state.cursor_agent.clipboard_capacity:
		state.add_log("Clipboard full")
		return false
	var pouch_entry: ClipboardEntry = null
	for e in state.pouch_cache:
		if e.node_path.to_lower() == node_path.to_lower():
			pouch_entry = e
			break
	if pouch_entry == null:
		state.add_log("Pouch cache entry missing: %s" % node_path)
		return false
	pouch_entry.is_ghosted = false
	state.exposed_pouch_paths.erase(node_path.to_lower())
	state.pouch_cache.erase(pouch_entry)
	state.clipboard.append(pouch_entry)
	state.add_log("Pouch cache restore: %s" % node_path)
	return true


func move_cursor(node_path: String) -> void:
	state.cursor_agent.current_node_path = node_path
	state.add_log("Cursor moved: %s" % node_path)
	_execute_security_behavior(
		SecurityBehaviorKeys.CURSOR_CROSSED_MONITORED_NODE,
		node_path,
		get_monitoring_agents(node_path),
		SecurityAgent.SecurityAwarenessStage.SUSPICIOUS,
		InfiltrationTuning.CURSOR_MONITORED_TRACE_INCREASE,
		"Cursor crossed monitored node: %s" % node_path)


func set_current_folder(folder_path: String) -> void:
	state.current_folder_path = folder_path


func handle_folder_navigation(folder_path: String, direct_jump: bool = false) -> void:
	state.current_folder_path = folder_path
	state.cursor_agent.current_node_path = folder_path
	state.add_log("Folder navigation: %s" % folder_path)
	var visible_agents: Array = get_visible_security_agents(folder_path)
	if visible_agents.size() == 0:
		return
	var trace_gain: int = InfiltrationTuning.DIRECT_FOLDER_JUMP_TRACE_INCREASE if direct_jump else InfiltrationTuning.FOLDER_NAVIGATION_TRACE_INCREASE
	var awareness: int = SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN if direct_jump else SecurityAgent.SecurityAwarenessStage.SUSPICIOUS
	_execute_security_behavior(
		SecurityBehaviorKeys.FOLDER_NAVIGATION,
		folder_path,
		visible_agents,
		awareness,
		trace_gain,
		"Navigated into monitored folder: %s" % folder_path,
		direct_jump)


func trigger_search_sweep(node_path: String) -> void:
	var agents: Array = []
	for agent in security_agents:
		if agent.agent_type == SecurityAgent.SecurityAgentType.INDEXER_SCOUT or agent.agent_type == SecurityAgent.SecurityAgentType.AI_MONITOR:
			agents.append(agent)
	_execute_security_behavior(
		SecurityBehaviorKeys.SEARCH_SWEEP,
		node_path,
		agents,
		SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN,
		0,
		"Search sweep escalated at %s" % node_path)


func get_visible_security_agents(current_folder_path: String) -> Array:
	var result: Array = []
	for agent in security_agents:
		if agent.current_node_path.to_lower() == current_folder_path.to_lower():
			result.append(agent)
			continue
		for path in agent.patrol_route:
			if path.to_lower() == current_folder_path.to_lower():
				result.append(agent)
				break
	return result


func is_node_hidden_in_pouch(node_path: String) -> bool:
	for e in state.pouch_cache:
		if e.node_path.to_lower() == node_path.to_lower():
			return true
	return false


func is_pouch_masking_broken(node_path: String) -> bool:
	return state.exposed_pouch_paths.has(node_path.to_lower())


func has_permission_override(node_path: String) -> bool:
	return get_permission_override_turns(node_path) > 0


func get_permission_override_turns(node_path: String) -> int:
	return state.permission_override_turns.get(node_path.to_lower(), 0)


func is_permission_locked(node_path: String) -> bool:
	if has_permission_override(node_path):
		return false
	if get_forced_lock_turns(node_path) > 0:
		return true
	for agent in security_agents:
		if agent.agent_type == SecurityAgent.SecurityAgentType.FIREWALL_SENTINEL and agent.current_node_path.to_lower() == node_path.to_lower():
			return true
	return false


func is_path_tracked(node_path: String) -> bool:
	return get_tracked_path_turns(node_path) > 0


func get_tracked_path_turns(node_path: String) -> int:
	return state.tracked_path_turns.get(node_path.to_lower(), 0)


func get_forced_lock_turns(node_path: String) -> int:
	return state.forced_lock_turns.get(node_path.to_lower(), 0)


func has_scan_pressure(node_path: String) -> bool:
	return get_scan_pressure_turns(node_path) > 0


func get_scan_pressure_turns(node_path: String) -> int:
	return state.scan_pressure_turns.get(node_path.to_lower(), 0)


func mark_tracked_path(node_path: String, duration_turns: int, reason: String) -> void:
	var turns: int = max(1, duration_turns)
	state.tracked_path_turns[node_path.to_lower()] = turns
	state.add_log("Tracked path marked: %s (%dT) :: %s" % [node_path, turns, reason])


func apply_forced_lock(node_path: String, duration_turns: int, reason: String) -> void:
	var turns: int = max(1, duration_turns)
	state.forced_lock_turns[node_path.to_lower()] = turns
	state.add_log("Forced lock applied: %s (%dT) :: %s" % [node_path, turns, reason])


func apply_scan_pressure(node_path: String, duration_turns: int, reason: String) -> void:
	var turns: int = max(1, duration_turns)
	state.scan_pressure_turns[node_path.to_lower()] = turns
	state.add_log("Scan pressure applied: %s (%dT) :: %s" % [node_path, turns, reason])


func clear_tracked_path(node_path: String, reason: String) -> bool:
	var key: String = node_path.to_lower()
	if not state.tracked_path_turns.has(key):
		return false
	state.tracked_path_turns.erase(key)
	state.add_log("Tracked path cleared: %s :: %s" % [node_path, reason])
	return true


func clear_scan_pressure(node_path: String, reason: String) -> bool:
	var key: String = node_path.to_lower()
	if not state.scan_pressure_turns.has(key):
		return false
	state.scan_pressure_turns.erase(key)
	state.add_log("Scan pressure cleared: %s :: %s" % [node_path, reason])
	return true


func grant_permission_override(node_path: String, reason: String, trace_increase: int, duration_turns: int = InfiltrationTuning.PERMISSION_OVERRIDE_DURATION_TURNS) -> void:
	var turns: int = max(InfiltrationTuning.PERMISSION_OVERRIDE_MINIMUM_DURATION_TURNS, duration_turns)
	state.permission_override_turns[node_path.to_lower()] = turns
	add_trace(trace_increase, reason)
	state.add_log("Permission override granted: %s (%dT)" % [node_path, turns])


func expose_pouch_hidden_node(node_path: String, reason: String, trace_increase: int) -> void:
	if not is_node_hidden_in_pouch(node_path):
		return
	state.exposed_pouch_paths[node_path.to_lower()] = true
	add_trace(trace_increase, reason)
	state.add_log("Pouch exposed: %s" % node_path)


func get_monitoring_agents(node_path: String) -> Array:
	var pouch_hidden: bool = is_node_hidden_in_pouch(node_path) and not is_pouch_masking_broken(node_path)
	var scan_pressure: bool = has_scan_pressure(node_path) or has_scan_pressure(state.current_folder_path)
	var pouch_entry_size: int = 0
	if pouch_hidden:
		for e in state.pouch_cache:
			if e.node_path.to_lower() == node_path.to_lower():
				pouch_entry_size = e.size
				break

	var result: Array = []
	for agent in security_agents:
		var at_node: bool = agent.current_node_path.to_lower() == node_path.to_lower()
		var in_sight: bool = _is_node_in_sight(agent, node_path)
		if not (at_node or in_sight):
			continue
		if scan_pressure or not pouch_hidden:
			result.append(agent)
			continue
		if agent.agent_type == SecurityAgent.SecurityAgentType.INDEXER_SCOUT:
			continue
		if agent.agent_type == SecurityAgent.SecurityAgentType.AI_MONITOR:
			if pouch_entry_size < InfiltrationTuning.POUCH_SIZE_AI_MONITOR_DETECTION_THRESHOLD:
				continue
		result.append(agent)
	return result


func is_node_monitored(node_path: String) -> bool:
	return get_monitoring_agents(node_path).size() > 0


func unlock_exit(reason: String) -> void:
	state.exit_unlocked = true
	state.run_status = RunState.RunStatus.OBJECTIVE_COMPLETED
	state.objective_state = RunState.ObjectiveState.COMPLETED
	state.add_log("Exit unlocked: %s" % reason)


func try_escape() -> bool:
	if not state.exit_unlocked:
		state.add_log("Escape blocked: exit locked")
		return false
	state.run_status = RunState.RunStatus.ESCAPED
	state.add_log("Escape successful")
	return true


func set_run_failed(reason: String) -> void:
	if state.run_status != RunState.RunStatus.ACTIVE:
		return
	state.run_status = RunState.RunStatus.FAILED
	state.add_log("Run failed: %s" % reason)


func set_run_timed_out() -> void:
	if state.run_status != RunState.RunStatus.ACTIVE:
		return
	state.run_status = RunState.RunStatus.TIMED_OUT
	state.add_log("Run timed out: turn limit reached")


func try_clear_detection(reason: String) -> bool:
	if not state.cursor_agent.is_detected:
		return false
	if state.trace > InfiltrationTuning.DETECTION_CLEAR_TRACE_THRESHOLD:
		return false
	state.cursor_agent.is_detected = false
	state.add_log("Detection cleared: %s" % reason)
	return true


func _tick_permission_overrides() -> void:
	var keys: Array = state.permission_override_turns.keys()
	for path in keys:
		var remaining: int = state.permission_override_turns[path] - 1
		if remaining <= 0:
			state.permission_override_turns.erase(path)
			state.add_log("Permission override expired: %s" % path)
		else:
			state.permission_override_turns[path] = remaining


func _tick_turn_dictionary(turns_by_path: Dictionary, expire_label: String) -> void:
	var keys: Array = turns_by_path.keys()
	for path in keys:
		var remaining: int = turns_by_path[path] - 1
		if remaining <= 0:
			turns_by_path.erase(path)
			state.add_log("%s: %s" % [expire_label, path])
		else:
			turns_by_path[path] = remaining


func _apply_tracked_path_action_trace(node_path: String, operation_type: int) -> void:
	var target_tracked: int = get_tracked_path_turns(node_path)
	var folder_tracked: int = get_tracked_path_turns(state.current_folder_path)
	if target_tracked <= 0 and folder_tracked <= 0:
		return
	var tracked_turns: int = max(target_tracked, folder_tracked)
	add_trace(InfiltrationTuning.TRACKED_ACTION_TRACE_BONUS, "Tracked path pressure: %d @ %s (%dT)" % [operation_type, node_path, tracked_turns])


func _advance_security_agents() -> void:
	var active_targets: Dictionary = {}
	for op in state.active_operations:
		if op.status == OperationStatus.RUNNING:
			active_targets[op.target_node_path.to_lower()] = op.target_node_path

	var cursor_path: String = state.cursor_agent.current_node_path
	var convergence_active: bool = state.alert_stage >= SecurityAgent.SecurityAwarenessStage.QUARANTINE and cursor_path != ""

	for agent in security_agents:
		if agent.disabled_turns > 0:
			agent.disabled_turns -= 1
			continue

		if convergence_active:
			var cursor_index: int = -1
			for i in agent.patrol_route.size():
				if agent.patrol_route[i].to_lower() == cursor_path.to_lower():
					cursor_index = i
					break
			if cursor_index >= 0:
				if agent.patrol_index < cursor_index:
					agent.patrol_index += 1
				elif agent.patrol_index > cursor_index:
					agent.patrol_index -= 1
				agent.current_node_path = agent.patrol_route[agent.patrol_index]
			else:
				agent.current_node_path = cursor_path
			agent.awareness_stage = state.alert_stage
			continue

		if agent.is_alerted and active_targets.size() > 0:
			agent.current_node_path = active_targets.values()[0]
			if state.alert_stage >= SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN:
				agent.awareness_stage = SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN
			else:
				agent.awareness_stage = SecurityAgent.SecurityAwarenessStage.SUSPICIOUS
			continue

		if agent.patrol_route.size() > 0:
			agent.patrol_index = (agent.patrol_index + 1) % agent.patrol_route.size()
			agent.current_node_path = agent.patrol_route[agent.patrol_index]
			agent.awareness_stage = state.alert_stage

	if not state.cursor_agent.is_detected:
		for agent in security_agents:
			if agent.is_alerted and agent.current_node_path.to_lower() == state.cursor_agent.current_node_path.to_lower():
				state.cursor_agent.is_detected = true
				state.add_log("Cursor agent detected at %s" % state.cursor_agent.current_node_path)
				break


func _apply_detection_contact_damage() -> void:
	state.last_turn_contact_damage = 0
	if not state.cursor_agent.is_detected:
		return
	var cursor_path: String = state.cursor_agent.current_node_path
	var threatening_count: int = 0
	for agent in security_agents:
		if (agent.agent_type == SecurityAgent.SecurityAgentType.GUARD_SCANNER or agent.agent_type == SecurityAgent.SecurityAgentType.ANTIVIRUS_HEAVY) \
				and agent.current_node_path.to_lower() == cursor_path.to_lower():
			threatening_count += 1
	if threatening_count == 0:
		return
	var damage: int = InfiltrationTuning.DETECTION_CONTACT_DAMAGE * threatening_count
	state.take_operator_damage(damage)
	state.last_turn_contact_damage = damage


func _interrupt_monitored_operations_on_detection() -> void:
	for op in state.active_operations:
		if op.status == OperationStatus.RUNNING and is_node_monitored(op.target_node_path):
			op.fail()
			state.add_log("Operation interrupted by detection: %d @ %s" % [op.type, op.target_node_path])


func _apply_multi_window_parallel_operation_trace() -> void:
	var running_paths: Dictionary = {}
	for op in state.active_operations:
		if op.status == OperationStatus.RUNNING:
			running_paths[op.target_node_path.to_lower()] = true
	if running_paths.size() == 0:
		return
	var windows_with_op: int = 0
	for w in state.windows:
		if not w.is_open:
			continue
		for path in running_paths.keys():
			if path.begins_with(w.bound_path.rstrip("/").to_lower()):
				windows_with_op += 1
				break
	var extra_windows: int = windows_with_op - 1
	if extra_windows <= 0:
		return
	add_trace(extra_windows * InfiltrationTuning.MULTI_WINDOW_PARALLEL_OPERATION_TRACE_COST_PER_WINDOW, "Parallel operations across %d windows" % windows_with_op)


func _tick_operations() -> void:
	for op in state.active_operations.duplicate():
		if op.status != OperationStatus.RUNNING:
			continue
		op.tick()
		if op.status == OperationStatus.COMPLETED:
			_on_operation_completed(op)
			state.add_log("Operation completed: %d @ %s" % [op.type, op.target_node_path])


func _on_operation_completed(operation: FileOperation) -> void:
	match operation.type:
		OperationType.MOVE_CURSOR:
			move_cursor(operation.target_node_path)
		OperationType.COPY:
			var ok: bool = try_copy_to_clipboard(operation.target_node_path, operation.node_kind, operation.node_size)
			operation.completion_notes.append("copy complete" if ok else "copy blocked :: clipboard full")
		OperationType.CUT:
			var ok: bool = try_copy_to_clipboard(operation.target_node_path, operation.node_kind, operation.node_size)
			operation.completion_notes.append("cut clipboard synced" if ok else "cut blocked :: clipboard full")
		OperationType.PASTE:
			var pasted: ClipboardEntry = null
			for e in state.clipboard:
				if e.node_path.to_lower() == operation.target_node_path.to_lower():
					pasted = e
					break
			if pasted != null:
				state.clipboard.erase(pasted)
				operation.completion_notes.append("paste complete :: clipboard cleared")
		OperationType.REWRITE_LOG:
			reduce_trace(InfiltrationTuning.REWRITE_LOG_TRACE_REDUCTION, "Log rewritten at %s" % operation.target_node_path)
			var cleared: Array[String] = []
			if clear_tracked_path(operation.target_node_path, "Rewrite Log scrubbed node route"):
				cleared.append("tracked")
			if clear_tracked_path(state.current_folder_path, "Rewrite Log scrubbed current folder route"):
				cleared.append("folder-tracked")
			if clear_scan_pressure(operation.target_node_path, "Rewrite Log diffused node scan pressure"):
				cleared.append("pressure")
			if clear_scan_pressure(state.current_folder_path, "Rewrite Log diffused folder scan pressure"):
				cleared.append("folder-pressure")
			if cleared.size() > 0:
				operation.completion_notes.append("log scrub :: cleared %s" % ", ".join(cleared))
			if try_clear_detection("Rewrite log completed at %s" % operation.target_node_path):
				operation.completion_notes.append("detection cleared :: log rewritten")


func _validate_queue_entry(entry: CommandQueueEntry) -> String:
	if entry.primary_target_path == "":
		return "%d has no primary target path" % entry.operation_type
	return ""


func _create_operation_from_queue_entry(entry: CommandQueueEntry) -> FileOperation:
	var required_ticks: int = 1
	match entry.operation_type:
		OperationType.MOVE_CURSOR:
			required_ticks = 1
		OperationType.COPY:
			required_ticks = 2
		OperationType.COMPRESS:
			required_ticks = 2
		OperationType.REWRITE_LOG:
			required_ticks = 2
		OperationType.DELETE:
			required_ticks = 1
		OperationType.STUN:
			required_ticks = 1
	return FileOperation.new(entry.operation_type, entry.primary_target_path, required_ticks, entry.secondary_target_path)


func _update_alert_stage() -> void:
	if state.trace >= 85:
		state.alert_stage = SecurityAgent.SecurityAwarenessStage.PURGE
	elif state.trace >= 60:
		state.alert_stage = SecurityAgent.SecurityAwarenessStage.QUARANTINE
	elif state.trace >= 35:
		state.alert_stage = SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN
	elif state.trace >= 15:
		state.alert_stage = SecurityAgent.SecurityAwarenessStage.SUSPICIOUS
	else:
		state.alert_stage = SecurityAgent.SecurityAwarenessStage.PASSIVE


func _execute_security_behavior(
		behavior_key: String,
		primary_path: String,
		agents: Array,
		awareness_stage: int,
		trace_amount: int,
		trace_reason: String,
		direct_jump: bool = false) -> bool:
	if agents.size() == 0:
		return false
	var executed: bool = false
	for agent in agents:
		var resolved_key: String = _resolve_security_behavior_key(behavior_key, agent.agent_type)
		var is_obj_path: bool = primary_path.to_lower() == mission.target_path.to_lower()
		var is_obj_route: bool = _is_objective_route(primary_path, mission.target_path)
		var agent_on_obj_route: bool = _is_objective_route(agent.current_node_path, mission.target_path)
		var ctx: SecurityBehaviorContext = SecurityBehaviorContext.new()
		ctx.primary_path = primary_path
		ctx.agent = agent
		ctx.agents = [agent]
		ctx.current_folder_path = state.current_folder_path
		ctx.cursor_path = state.cursor_agent.current_node_path
		ctx.objective_path = mission.target_path
		ctx.is_objective_path = is_obj_path
		ctx.is_objective_route = is_obj_route
		ctx.agent_on_objective_route = agent_on_obj_route
		ctx.direct_jump = direct_jump
		ctx.trace_amount = trace_amount
		ctx.trace_reason = trace_reason
		ctx.awareness_stage = awareness_stage
		ctx.add_trace = add_trace
		ctx.add_log = state.add_log
		ctx.alert_agent = _alert_agent
		ctx.mark_tracked_path = mark_tracked_path
		ctx.apply_forced_lock = apply_forced_lock
		ctx.apply_scan_pressure = apply_scan_pressure
		if _security_behavior_executor.try_execute(resolved_key, ctx):
			executed = true
	return executed


func _alert_agent(agent: SecurityAgent, awareness_stage: int) -> void:
	agent.is_alerted = true
	agent.awareness_stage = awareness_stage


func _resolve_security_behavior_key(behavior_key: String, agent_type: int) -> String:
	match behavior_key:
		SecurityBehaviorKeys.CURSOR_CROSSED_MONITORED_NODE:
			match agent_type:
				SecurityAgent.SecurityAgentType.GUARD_SCANNER:
					return SecurityBehaviorKeys.CURSOR_CROSSED_GUARD_SCANNER
				SecurityAgent.SecurityAgentType.INDEXER_SCOUT:
					return SecurityBehaviorKeys.CURSOR_CROSSED_INDEXER_SCOUT
				SecurityAgent.SecurityAgentType.AI_MONITOR:
					return SecurityBehaviorKeys.CURSOR_CROSSED_AI_MONITOR
				SecurityAgent.SecurityAgentType.FIREWALL_SENTINEL:
					return SecurityBehaviorKeys.CURSOR_CROSSED_FIREWALL_SENTINEL
				SecurityAgent.SecurityAgentType.ANTIVIRUS_HEAVY:
					return SecurityBehaviorKeys.CURSOR_CROSSED_ANTIVIRUS_HEAVY
				SecurityAgent.SecurityAgentType.BACKUP_REPAIRER:
					return SecurityBehaviorKeys.CURSOR_CROSSED_BACKUP_REPAIRER
		SecurityBehaviorKeys.FOLDER_NAVIGATION:
			match agent_type:
				SecurityAgent.SecurityAgentType.GUARD_SCANNER:
					return SecurityBehaviorKeys.FOLDER_NAVIGATION_GUARD_SCANNER
				SecurityAgent.SecurityAgentType.INDEXER_SCOUT:
					return SecurityBehaviorKeys.FOLDER_NAVIGATION_INDEXER_SCOUT
				SecurityAgent.SecurityAgentType.AI_MONITOR:
					return SecurityBehaviorKeys.FOLDER_NAVIGATION_AI_MONITOR
				SecurityAgent.SecurityAgentType.FIREWALL_SENTINEL:
					return SecurityBehaviorKeys.FOLDER_NAVIGATION_FIREWALL_SENTINEL
				SecurityAgent.SecurityAgentType.ANTIVIRUS_HEAVY:
					return SecurityBehaviorKeys.FOLDER_NAVIGATION_ANTIVIRUS_HEAVY
				SecurityAgent.SecurityAgentType.BACKUP_REPAIRER:
					return SecurityBehaviorKeys.FOLDER_NAVIGATION_BACKUP_REPAIRER
		SecurityBehaviorKeys.SEARCH_SWEEP:
			match agent_type:
				SecurityAgent.SecurityAgentType.INDEXER_SCOUT:
					return SecurityBehaviorKeys.SEARCH_SWEEP_INDEXER_SCOUT
				SecurityAgent.SecurityAgentType.AI_MONITOR:
					return SecurityBehaviorKeys.SEARCH_SWEEP_AI_MONITOR
				SecurityAgent.SecurityAgentType.ANTIVIRUS_HEAVY:
					return SecurityBehaviorKeys.SEARCH_SWEEP_ANTIVIRUS_HEAVY
				SecurityAgent.SecurityAgentType.BACKUP_REPAIRER:
					return SecurityBehaviorKeys.SEARCH_SWEEP_BACKUP_REPAIRER
	return behavior_key


static func _is_node_in_sight(agent: SecurityAgent, node_path: String) -> bool:
	if agent.patrol_route.size() == 0:
		return false
	var index: int = -1
	for i in agent.patrol_route.size():
		if agent.patrol_route[i].to_lower() == node_path.to_lower():
			index = i
			break
	if index < 0:
		return false
	return abs(index - agent.patrol_index) <= agent.sight_range


static func _is_objective_route(path: String, objective_path: String) -> bool:
	if path == "" or objective_path == "":
		return false
	if path.to_lower() == objective_path.to_lower():
		return true
	var obj_lower: String = objective_path.rstrip("/").to_lower()
	var path_lower: String = path.rstrip("/").to_lower()
	return obj_lower.begins_with(path_lower + "/") or path_lower.begins_with(obj_lower + "/")
