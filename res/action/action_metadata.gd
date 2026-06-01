class_name ActionMetadata

static func get_tooltip_text(action_id: String) -> String:
	match action_id:
		ActionIds.OPEN: return "Basic attack"
		ActionIds.INSPECT: return "Free information check"
		ActionIds.DELETE: return "High damage single target"
		ActionIds.COPY: return "Copy target to clipboard"
		ActionIds.CUT: return "Damage and cut target to clipboard"
		ActionIds.PASTE: return "Paste from clipboard with bonus effect"
		ActionIds.CLEAN: return "AoE damage + clear own status effects"
		ActionIds.QUARANTINE: return "Prevent enemy action for %d turns" % ActionConstants.QUARANTINE_EFFECT_DURATION
		ActionIds.COMPRESS: return "Reduce enemy attack by %d for %d turns" % [abs(ActionConstants.COMPRESS_ATTACK_MODIFIER), ActionConstants.COMPRESS_EFFECT_DURATION]
		ActionIds.LOG_FORGE: return "Rewrite records to reduce trace after a delay"
		ActionIds.SEARCH: return "Scan for signatures and reveal clues, but raises trace"
		ActionIds.SORT: return "Reorder listing to surface targets and reduce scanning friction"
		ActionIds.SHOW_HIDDEN: return "Show hidden layers and partially break pouch masking"
		ActionIds.MOVE: return "Relocate target to another container path"
		ActionIds.EXTRACT: return "Extract packaged data at a valid extraction point"
		ActionIds.INJECT: return "Inject payload into the target for tactical disruption"
		ActionIds.STUN: return "Stagger target actions for a short window"
		ActionIds.DECOY: return "Deploy a decoy to redirect security attention"
		ActionIds.PERMISSION_OVERRIDE: return "Break a Firewall Sentinel lock to force access. Side effect: exposes any pouch-masked files and raises Trace."
		_: return ""

static func get_ready_color(action_id: String) -> Color:
	match action_id:
		ActionIds.OPEN: return Color(0.47, 0.78, 1.0)
		ActionIds.INSPECT: return Color(0.76, 0.67, 1.0)
		ActionIds.DELETE: return Color(1.0, 0.49, 0.45)
		ActionIds.COPY: return Color(0.47, 0.9, 0.72)
		ActionIds.CUT: return Color(1.0, 0.64, 0.4)
		ActionIds.PASTE: return Color(0.98, 0.79, 0.37)
		ActionIds.CLEAN: return Color(0.42, 0.86, 0.86)
		ActionIds.QUARANTINE: return Color(0.83, 0.59, 1.0)
		ActionIds.COMPRESS: return Color(0.59, 0.92, 0.6)
		ActionIds.LOG_FORGE: return Color(0.86, 0.82, 1.0)
		ActionIds.SEARCH: return Color(0.98, 0.88, 0.49)
		ActionIds.SORT: return Color(0.78, 0.86, 0.98)
		ActionIds.SHOW_HIDDEN: return Color(0.72, 0.89, 1.0)
		ActionIds.MOVE: return Color(0.64, 0.9, 0.78)
		ActionIds.EXTRACT: return Color(0.55, 0.95, 0.9)
		ActionIds.INJECT: return Color(0.99, 0.63, 0.63)
		ActionIds.STUN: return Color(0.87, 0.77, 1.0)
		ActionIds.DECOY: return Color(0.9, 0.82, 0.62)
		ActionIds.PERMISSION_OVERRIDE: return Color(1.0, 0.72, 0.52)
		_: return Color.WHITE
