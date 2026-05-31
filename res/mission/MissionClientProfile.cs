namespace ProjectFR.Mission;

public enum FactionId
{
    CorporateEspionage,
    LegalFixers,
    SecurityContractors,
    LeakBrokers,
    CivicLeakers,
}

public class MissionClientProfile
{
    public FactionId FactionId { get; }
    public string Faction { get; }
    public string Name { get; }
    public string Agenda { get; }
    public string RiskNote { get; }

    public MissionClientProfile(string name, FactionId factionId, string agenda, string riskNote)
    {
        Name = name;
        FactionId = factionId;
        Faction = factionId switch
        {
            FactionId.CorporateEspionage  => "Corporate Espionage",
            FactionId.LegalFixers         => "Legal Fixers",
            FactionId.SecurityContractors => "Security Contractors",
            FactionId.LeakBrokers         => "Leak Brokers",
            FactionId.CivicLeakers        => "Civic Leakers",
            _                             => factionId.ToString()
        };
        Agenda = agenda;
        RiskNote = riskNote;
    }
}
