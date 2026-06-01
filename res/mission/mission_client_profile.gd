class_name MissionClientProfile

enum FactionId {
	CORPORATE_ESPIONAGE,
	LEGAL_FIXERS,
	SECURITY_CONTRACTORS,
	LEAK_BROKERS,
	CIVIC_LEAKERS,
}

# matches FactionId enum order (0..4)
const FACTION_NAMES: Dictionary = {
	0: "Corporate Espionage",
	1: "Legal Fixers",
	2: "Security Contractors",
	3: "Leak Brokers",
	4: "Civic Leakers",
}

var faction_id: int
var faction: String
var name: String
var agenda: String
var risk_note: String

func _init(p_name: String, p_faction_id: int, p_agenda: String, p_risk_note: String) -> void:
	name = p_name
	faction_id = p_faction_id
	faction = FACTION_NAMES.get(p_faction_id, "Unknown")
	agenda = p_agenda
	risk_note = p_risk_note
