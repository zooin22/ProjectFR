class_name MissionBoardFactory

static func create_default_board() -> Array:
	var morrow_proxy := MissionClientProfile.new(
		"Morrow Proxy",
		MissionClientProfile.FactionId.CORPORATE_ESPIONAGE,
		"Steal sealed research packages before the original owner notices.",
		"Pays well, but leaves ugly traces when jobs go loud."
	)
	var northline_legal := MissionClientProfile.new(
		"Northline Legal",
		MissionClientProfile.FactionId.LEGAL_FIXERS,
		"Erase or bury documents that can become liabilities.",
		"Low noise preferred. Failures spread quickly through compliance networks."
	)
	var helix_ops := MissionClientProfile.new(
		"Helix Ops",
		MissionClientProfile.FactionId.SECURITY_CONTRACTORS,
		"Map hostile systems and identify weak points before a full breach.",
		"Professional and cautious, but they remember sloppy field work."
	)
	var ember_circuit := MissionClientProfile.new(
		"Ember Circuit",
		MissionClientProfile.FactionId.LEAK_BROKERS,
		"Move sensitive payloads through deniable operators.",
		"High payout, high suspicion. Heat rises fast if you stumble."
	)
	var glass_key := MissionClientProfile.new(
		"Glass Key Collective",
		MissionClientProfile.FactionId.CIVIC_LEAKERS,
		"Pull hidden evidence into public reach.",
		"They value clean retrieval over collateral damage."
	)

	return [
		MissionData.new(
			"mission_extract_boss",
			"Archive Lift",
			morrow_proxy,
			"Dive into the asset vault, copy the Boss.zip package, then clear the route before the trace closes.",
			MissionObjectiveType.EXTRACT,
			"res://BuildCache/Assets/Boss.zip",
			10, 90, 2, 25, 2
		),
		MissionData.new(
			"mission_delete_readme",
			"Loose End Cleanup",
			northline_legal,
			"Erase the root Readme before it gets mirrored into a compliance snapshot.",
			MissionObjectiveType.DELETE,
			"res://Readme.txt",
			10, 65, 1, 20, 1,
			"", "readme_conflict"
		),
		MissionData.new(
			"mission_scan_cache",
			"Cache Recon",
			helix_ops,
			"Inspect the BuildCache folder, map the live defenses, and exfiltrate before security tightens.",
			MissionObjectiveType.SCAN,
			"res://BuildCache",
			10, 55, 1, 15, 1
		),
		MissionData.new(
			"mission_extract_logs",
			"Operator Manifest",
			helix_ops,
			"Follow-up to Cache Recon — pull the system.log before the next audit cycle overwrites the entries we flagged.",
			MissionObjectiveType.EXTRACT,
			"res://BuildCache/system.log",
			9, 70, 2, 20, 2,
			"mission_scan_cache", "", 1
		),
		MissionData.new(
			"mission_extract_readme",
			"Mirror Snatch",
			glass_key,
			"Copy the root Readme intact. The client wants the document, not a cleanup operation.",
			MissionObjectiveType.EXTRACT,
			"res://Readme.txt",
			9, 60, 2, 15, 1,
			"", "readme_conflict"
		),
		MissionData.new(
			"mission_delete_boss",
			"Burn Notice",
			ember_circuit,
			"Push through the archive and wipe Boss.zip before a broker handoff completes.",
			MissionObjectiveType.DELETE,
			"res://BuildCache/Assets/Boss.zip",
			9, 95, 1, 30, 3,
			"mission_extract_boss", "", 2
		),
		MissionData.new(
			"mission_modify_syslog",
			"Audit Wash",
			northline_legal,
			"Forge the system.log before an automated compliance sweep captures the incriminating entries.",
			MissionObjectiveType.MODIFY,
			"res://BuildCache/system.log",
			8, 75, 2, 20, 2
		),
		MissionData.new(
			"mission_escape_only",
			"Clean Exfil",
			helix_ops,
			"No package retrieval — just map the system and extract without triggering an alert cascade.",
			MissionObjectiveType.ESCAPE,
			"",
			7, 50, 1, 10, 1
		),
	]
