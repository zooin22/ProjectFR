namespace ProjectFR.Battle;

public class DungeonFolderMetadata
{
    public string ThemeName { get; }
    public string EventSummary { get; }
    public string RewardPreview { get; }
    public int Depth { get; }

    public DungeonFolderMetadata(string themeName, string eventSummary, string rewardPreview, int depth)
    {
        ThemeName = themeName;
        EventSummary = eventSummary;
        RewardPreview = rewardPreview;
        Depth = depth;
    }
}
