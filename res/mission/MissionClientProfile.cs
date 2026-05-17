namespace ProjectFR.Mission;

public class MissionClientProfile
{
    public string Name { get; }
    public string Faction { get; }
    public string Agenda { get; }
    public string RiskNote { get; }

    public MissionClientProfile(string name, string faction, string agenda, string riskNote)
    {
        Name = name;
        Faction = faction;
        Agenda = agenda;
        RiskNote = riskNote;
    }
}
