namespace ProjectFR.Data.Nodes;

/// <summary>
/// Legacy shorthand for a dangerous payload file.
/// New content should prefer FileNode or ArchiveNode with an explicit NodeCombatProfile.
/// </summary>
public class SpecialFileNode : FileNode
{
    public SpecialFileNode(string name, string path, long size = 0)
        : base(name, path, size, new NodeCombatProfile("Special File", "High", NodeThreatLevel.High, 10, 3, 3, isBoss: true))
    {
    }
}
