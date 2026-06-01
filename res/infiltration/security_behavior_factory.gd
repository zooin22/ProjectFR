class_name SecurityBehaviorFactory

static func create(behavior_key: String) -> SecurityBehaviorNode:
	match behavior_key:
		SecurityBehaviorKeys.CURSOR_CROSSED_MONITORED_NODE:
			return _build_cursor_crossed_behavior()
		SecurityBehaviorKeys.FOLDER_NAVIGATION:
			return _build_folder_navigation_behavior()
		SecurityBehaviorKeys.SEARCH_SWEEP:
			return _build_search_sweep_behavior()
		SecurityBehaviorKeys.CURSOR_CROSSED_GUARD_SCANNER:
			return _build_cursor_crossed_behavior()
		SecurityBehaviorKeys.CURSOR_CROSSED_INDEXER_SCOUT:
			return _build_indexer_scout_cursor_behavior()
		SecurityBehaviorKeys.CURSOR_CROSSED_AI_MONITOR:
			return _build_ai_monitor_cursor_behavior()
		SecurityBehaviorKeys.CURSOR_CROSSED_FIREWALL_SENTINEL:
			return _build_firewall_cursor_behavior()
		SecurityBehaviorKeys.FOLDER_NAVIGATION_GUARD_SCANNER:
			return _build_folder_navigation_behavior()
		SecurityBehaviorKeys.FOLDER_NAVIGATION_INDEXER_SCOUT:
			return _build_indexer_folder_behavior()
		SecurityBehaviorKeys.FOLDER_NAVIGATION_AI_MONITOR:
			return _build_ai_monitor_folder_behavior()
		SecurityBehaviorKeys.FOLDER_NAVIGATION_FIREWALL_SENTINEL:
			return _build_firewall_folder_behavior()
		SecurityBehaviorKeys.SEARCH_SWEEP_INDEXER_SCOUT:
			return _build_indexer_search_behavior()
		SecurityBehaviorKeys.SEARCH_SWEEP_AI_MONITOR:
			return _build_ai_monitor_search_behavior()
		SecurityBehaviorKeys.CURSOR_CROSSED_ANTIVIRUS_HEAVY:
			return _build_antivirus_cursor_behavior()
		SecurityBehaviorKeys.CURSOR_CROSSED_BACKUP_REPAIRER:
			return _build_backup_repairer_cursor_behavior()
		SecurityBehaviorKeys.FOLDER_NAVIGATION_ANTIVIRUS_HEAVY:
			return _build_antivirus_folder_behavior()
		SecurityBehaviorKeys.FOLDER_NAVIGATION_BACKUP_REPAIRER:
			return _build_backup_repairer_folder_behavior()
		SecurityBehaviorKeys.SEARCH_SWEEP_ANTIVIRUS_HEAVY:
			return _build_antivirus_search_behavior()
		SecurityBehaviorKeys.SEARCH_SWEEP_BACKUP_REPAIRER:
			return _build_backup_repairer_search_behavior()
		_:
			return null


static func _build_cursor_crossed_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agents.size() > 0),
		SecurityBehaviorNode.ActionNode.new(_alert_agents),
		SecurityBehaviorNode.ActionNode.new(_apply_trace_and_log),
	])


static func _build_folder_navigation_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agents.size() > 0),
		SecurityBehaviorNode.ActionNode.new(_alert_agents),
		SecurityBehaviorNode.ActionNode.new(_apply_trace_and_log),
	])


static func _build_search_sweep_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agents.size() > 0),
		SecurityBehaviorNode.ActionNode.new(_alert_agents),
	])


static func _build_indexer_scout_cursor_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var stage: int = SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN if ctx.is_objective_route else SecurityAgent.SecurityAwarenessStage.SUSPICIOUS
			var msg: String = "Indexer Scout matched cursor drift against an objective route." if ctx.is_objective_route else "Indexer Scout flagged cursor drift."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route:
				return _apply_modified_trace_and_log(ctx, SecurityBehaviorTuning.OBJECTIVE_ROUTE_TRACE_BONUS)
			return _apply_trace_and_log(ctx)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route or ctx.is_objective_path:
				ctx.mark_tracked_path.call(ctx.primary_path, SecurityBehaviorTuning.TRACE_MARKER_DURATION_TURNS, "Indexer Scout marked the route")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_ai_monitor_cursor_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var stage: int = SecurityAgent.SecurityAwarenessStage.QUARANTINE if ctx.is_objective_route else SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN
			var msg: String = "AI Monitor linked the cursor anomaly to the mission objective route." if ctx.is_objective_route else "AI Monitor escalated cursor anomaly."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var bonus: int = SecurityBehaviorTuning.AI_MONITOR_CURSOR_TRACE_BONUS
			if ctx.is_objective_route:
				bonus += SecurityBehaviorTuning.OBJECTIVE_ROUTE_TRACE_BONUS
			return _apply_modified_trace_and_log(ctx, bonus)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route or ctx.is_objective_path:
				ctx.apply_scan_pressure.call(ctx.current_folder_path, SecurityBehaviorTuning.SCAN_PRESSURE_DURATION_TURNS, "AI Monitor pressured the current folder")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_firewall_cursor_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var stage: int = SecurityAgent.SecurityAwarenessStage.QUARANTINE if ctx.is_objective_route else SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN
			var msg: String = "Firewall Sentinel contested the objective route directly." if ctx.is_objective_route else "Firewall Sentinel marked the route as contested."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route:
				return _apply_modified_trace_and_log(ctx, SecurityBehaviorTuning.OBJECTIVE_ROUTE_TRACE_BONUS)
			return _apply_trace_and_log(ctx)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route or ctx.is_objective_path:
				ctx.apply_forced_lock.call(ctx.primary_path, SecurityBehaviorTuning.FORCED_LOCK_DURATION_TURNS, "Firewall Sentinel contested the node")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_indexer_folder_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var stage: int = SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN if ctx.is_objective_route else ctx.awareness_stage
			var msg: String = "Indexer Scout recorded traversal along an objective route." if ctx.is_objective_route else "Indexer Scout recorded folder traversal."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route:
				return _apply_modified_trace_and_log(ctx, SecurityBehaviorTuning.OBJECTIVE_ROUTE_TRACE_BONUS)
			return _apply_trace_and_log(ctx)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route:
				ctx.mark_tracked_path.call(ctx.primary_path, SecurityBehaviorTuning.TRACE_MARKER_DURATION_TURNS, "Indexer Scout marked the folder route")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_ai_monitor_folder_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var stage: int = SecurityAgent.SecurityAwarenessStage.QUARANTINE if ctx.is_objective_route else SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN
			var msg: String = "AI Monitor reclassified the objective-route jump as hostile." if ctx.is_objective_route else "AI Monitor reclassified the folder jump as hostile."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route:
				return _apply_modified_trace_and_log(ctx, SecurityBehaviorTuning.OBJECTIVE_ROUTE_TRACE_BONUS)
			return _apply_trace_and_log(ctx)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route:
				ctx.apply_scan_pressure.call(ctx.primary_path, SecurityBehaviorTuning.SCAN_PRESSURE_DURATION_TURNS, "AI Monitor saturated the folder with scans")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_firewall_folder_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var stage: int = SecurityAgent.SecurityAwarenessStage.QUARANTINE if ctx.is_objective_route else SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN
			var msg: String = "Firewall Sentinel hardened an objective-facing route." if ctx.is_objective_route else "Firewall Sentinel hardened the traversed route."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var bonus: int = SecurityBehaviorTuning.FIREWALL_FOLDER_NAVIGATION_TRACE_BONUS
			if ctx.is_objective_route:
				bonus += SecurityBehaviorTuning.OBJECTIVE_ROUTE_TRACE_BONUS
			return _apply_modified_trace_and_log(ctx, bonus)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route:
				ctx.apply_forced_lock.call(ctx.primary_path, SecurityBehaviorTuning.FORCED_LOCK_DURATION_TURNS, "Firewall Sentinel hardened the folder")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_indexer_search_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var on_obj: bool = ctx.is_objective_path or ctx.is_objective_route
			var stage: int = SecurityAgent.SecurityAwarenessStage.QUARANTINE if on_obj else SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN
			var msg: String = "Indexer Scout found residue aligned with the objective signature." if on_obj else "Indexer Scout started a residue search pass."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_path or ctx.is_objective_route:
				return _apply_modified_trace_and_log(ctx, SecurityBehaviorTuning.OBJECTIVE_SEARCH_TRACE_BONUS)
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_path or ctx.is_objective_route:
				ctx.mark_tracked_path.call(ctx.primary_path, SecurityBehaviorTuning.TRACE_MARKER_DURATION_TURNS, "Indexer Scout traced the search residue")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_antivirus_cursor_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var stage: int = SecurityAgent.SecurityAwarenessStage.PURGE if ctx.is_objective_route else SecurityAgent.SecurityAwarenessStage.QUARANTINE
			var msg: String = "Antivirus Heavy intercepted the cursor on an objective-adjacent route." if ctx.is_objective_route else "Antivirus Heavy flagged anomalous cursor activity."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var bonus: int = SecurityBehaviorTuning.ANTIVIRUS_CURSOR_TRACE_BONUS
			if ctx.is_objective_route:
				bonus += SecurityBehaviorTuning.OBJECTIVE_ROUTE_TRACE_BONUS
			return _apply_modified_trace_and_log(ctx, bonus)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route or ctx.is_objective_path:
				ctx.apply_forced_lock.call(ctx.primary_path, SecurityBehaviorTuning.FORCED_LOCK_DURATION_TURNS, "Antivirus Heavy locked the contested node")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_backup_repairer_cursor_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var stage: int = SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN if ctx.is_objective_route else SecurityAgent.SecurityAwarenessStage.SUSPICIOUS
			var msg: String = "Backup Repairer detected cursor motion near a protected zone." if ctx.is_objective_route else "Backup Repairer logged cursor drift."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			return _apply_modified_trace_and_log(ctx, SecurityBehaviorTuning.BACKUP_REPAIRER_CURSOR_TRACE_BONUS)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			ctx.apply_scan_pressure.call(ctx.primary_path, SecurityBehaviorTuning.BACKUP_REPAIRER_SCAN_PRESSURE_DURATION_TURNS, "Backup Repairer initiated integrity scan")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_antivirus_folder_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var stage: int = SecurityAgent.SecurityAwarenessStage.PURGE if ctx.is_objective_route else SecurityAgent.SecurityAwarenessStage.QUARANTINE
			var msg: String = "Antivirus Heavy quarantined a traversal path toward the objective." if ctx.is_objective_route else "Antivirus Heavy quarantined the folder traversal."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var bonus: int = SecurityBehaviorTuning.ANTIVIRUS_FOLDER_NAVIGATION_TRACE_BONUS
			if ctx.is_objective_route:
				bonus += SecurityBehaviorTuning.OBJECTIVE_ROUTE_TRACE_BONUS
			return _apply_modified_trace_and_log(ctx, bonus)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			ctx.mark_tracked_path.call(ctx.primary_path, SecurityBehaviorTuning.TRACE_MARKER_DURATION_TURNS, "Antivirus Heavy marked the traversal route")
			if ctx.is_objective_route:
				ctx.apply_forced_lock.call(ctx.primary_path, SecurityBehaviorTuning.FORCED_LOCK_DURATION_TURNS, "Antivirus Heavy hardened the objective-facing folder")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_backup_repairer_folder_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var stage: int = SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN if ctx.is_objective_route else ctx.awareness_stage
			var msg: String = "Backup Repairer scheduled a sweep of the traversed objective path." if ctx.is_objective_route else "Backup Repairer logged folder traversal for review."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			return _apply_modified_trace_and_log(ctx, SecurityBehaviorTuning.BACKUP_REPAIRER_FOLDER_NAVIGATION_TRACE_BONUS)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_route:
				ctx.mark_tracked_path.call(ctx.primary_path, SecurityBehaviorTuning.TRACE_MARKER_DURATION_TURNS, "Backup Repairer queued a restoration sweep")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_antivirus_search_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var on_obj: bool = ctx.is_objective_path or ctx.is_objective_route
			var stage: int = SecurityAgent.SecurityAwarenessStage.PURGE if on_obj else SecurityAgent.SecurityAwarenessStage.QUARANTINE
			var msg: String = "Antivirus Heavy matched the search signature against a known threat vector." if on_obj else "Antivirus Heavy classified the search query as a potential intrusion."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var bonus: int = SecurityBehaviorTuning.ANTIVIRUS_SEARCH_TRACE_BONUS
			if ctx.is_objective_path or ctx.is_objective_route:
				bonus += SecurityBehaviorTuning.OBJECTIVE_SEARCH_TRACE_BONUS
			return _apply_modified_trace_and_log(ctx, bonus)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			if ctx.is_objective_path or ctx.is_objective_route:
				ctx.apply_forced_lock.call(ctx.primary_path, SecurityBehaviorTuning.FORCED_LOCK_DURATION_TURNS, "Antivirus Heavy locked the matched objective node")
				ctx.mark_tracked_path.call(ctx.primary_path, SecurityBehaviorTuning.TRACE_MARKER_DURATION_TURNS, "Antivirus Heavy pinned the threat signature")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_backup_repairer_search_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var on_obj: bool = ctx.is_objective_path or ctx.is_objective_route
			var stage: int = SecurityAgent.SecurityAwarenessStage.ACTIVE_SCAN if on_obj else SecurityAgent.SecurityAwarenessStage.SUSPICIOUS
			var msg: String = "Backup Repairer detected a search query near a protected archive." if on_obj else "Backup Repairer logged an unusual search pattern."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			return _apply_modified_trace_and_log(ctx, SecurityBehaviorTuning.BACKUP_REPAIRER_SEARCH_TRACE_BONUS)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			ctx.apply_scan_pressure.call(ctx.current_folder_path, SecurityBehaviorTuning.BACKUP_REPAIRER_SCAN_PRESSURE_DURATION_TURNS, "Backup Repairer initiated integrity scan on current folder")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _build_ai_monitor_search_behavior() -> SecurityBehaviorNode:
	return SecurityBehaviorNode.SequenceNode.new([
		SecurityBehaviorNode.ConditionNode.new(func(ctx: SecurityBehaviorContext) -> bool:
			return ctx.agent != null),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var on_obj: bool = ctx.is_objective_path or ctx.is_objective_route
			var stage: int = SecurityAgent.SecurityAwarenessStage.PURGE if on_obj else SecurityAgent.SecurityAwarenessStage.QUARANTINE
			var msg: String = "AI Monitor tied the search directly to the objective signature and escalated to purge review." if on_obj else "AI Monitor escalated the query to quarantine review."
			return _alert_single_agent(ctx, stage, msg)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			var bonus: int = SecurityBehaviorTuning.AI_MONITOR_SEARCH_TRACE_BONUS
			if ctx.is_objective_path or ctx.is_objective_route:
				bonus += SecurityBehaviorTuning.OBJECTIVE_SEARCH_TRACE_BONUS
			return _apply_modified_trace_and_log(ctx, bonus)),
		SecurityBehaviorNode.ActionNode.new(func(ctx: SecurityBehaviorContext) -> int:
			ctx.apply_scan_pressure.call(ctx.current_folder_path, SecurityBehaviorTuning.SCAN_PRESSURE_DURATION_TURNS, "AI Monitor widened active scans")
			if ctx.is_objective_path or ctx.is_objective_route:
				ctx.mark_tracked_path.call(ctx.primary_path, SecurityBehaviorTuning.TRACE_MARKER_DURATION_TURNS, "AI Monitor pinned the objective-aligned search route")
			return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS),
	])


static func _alert_agents(ctx: SecurityBehaviorContext) -> int:
	for agent in ctx.agents:
		ctx.alert_agent.call(agent, ctx.awareness_stage)
	return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS


static func _alert_single_agent(ctx: SecurityBehaviorContext, awareness_stage: int, log_message: String) -> int:
	if ctx.agent == null:
		return SecurityBehaviorNode.SecurityBehaviorStatus.FAILURE
	ctx.alert_agent.call(ctx.agent, awareness_stage)
	ctx.add_log.call(log_message)
	return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS


static func _apply_trace_and_log(ctx: SecurityBehaviorContext) -> int:
	if ctx.trace_amount > 0:
		ctx.add_trace.call(ctx.trace_amount, ctx.trace_reason)
	ctx.add_log.call("Security behavior executed: %s [%d]" % [ctx.primary_path, ctx.awareness_stage])
	return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS


static func _apply_modified_trace_and_log(ctx: SecurityBehaviorContext, trace_bonus: int) -> int:
	var total: int = ctx.trace_amount + trace_bonus
	if total > 0:
		ctx.add_trace.call(total, ctx.trace_reason)
	ctx.add_log.call("Security behavior executed: %s [%d] +bonus %d" % [ctx.primary_path, ctx.awareness_stage, trace_bonus])
	return SecurityBehaviorNode.SecurityBehaviorStatus.SUCCESS
