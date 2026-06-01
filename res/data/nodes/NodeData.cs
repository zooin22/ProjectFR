using ProjectFR.Infiltration;

namespace ProjectFR.Data.Nodes;

public enum NodeThreatLevel
{
    Low,
    Medium,
    High,
    Critical
}

public sealed class NodeCombatProfile
{
    public string TypeName { get; }
    public string ThreatLabel { get; }
    public NodeThreatLevel ThreatLevel { get; }
    public int BaseMaxHp { get; }
    public int BaseMaxAp { get; }
    public int BaseAttackPower { get; }
    public bool IsBoss { get; }
    public bool RevealsChildrenOnOpen { get; }
    public string? RevealSummary { get; }

    public NodeCombatProfile(
        string typeName,
        string threatLabel,
        NodeThreatLevel threatLevel,
        int baseMaxHp,
        int baseMaxAp,
        int baseAttackPower,
        bool isBoss = false,
        bool revealsChildrenOnOpen = false,
        string? revealSummary = null)
    {
        TypeName = typeName;
        ThreatLabel = threatLabel;
        ThreatLevel = threatLevel;
        BaseMaxHp = baseMaxHp;
        BaseMaxAp = baseMaxAp;
        BaseAttackPower = baseAttackPower;
        IsBoss = isBoss;
        RevealsChildrenOnOpen = revealsChildrenOnOpen;
        RevealSummary = revealSummary;
    }
}

public abstract class NodeData
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsFolder { get; protected set; }
    public long Size { get; set; }
    public NodeCombatProfile CombatProfile { get; set; }
    public ExplorerNodeRole? Role { get; set; }

    protected NodeData(string name, string path, bool isFolder, long size = 0, NodeCombatProfile? combatProfile = null)
    {
        Name = name;
        Path = path;
        IsFolder = isFolder;
        Size = size;
        CombatProfile = combatProfile ?? CreateFallbackProfile(isFolder);
    }

    public virtual bool IsContainer => false;
    public virtual string UiTypeName => CombatProfile.TypeName;

    public override string ToString() => $"{Name} ({UiTypeName})";

    private static NodeCombatProfile CreateFallbackProfile(bool isFolder)
    {
        return isFolder
            ? new NodeCombatProfile("Folder", "Medium", NodeThreatLevel.Medium, 8, 2, 2, revealsChildrenOnOpen: true)
            : new NodeCombatProfile("File", "Low", NodeThreatLevel.Low, 7, 2, 2);
    }
}

public abstract class ContainerNode : NodeData
{
    public List<NodeData> Children { get; } = new();

    protected ContainerNode(string name, string path, bool isFolder, long size = 0, NodeCombatProfile? combatProfile = null)
        : base(name, path, isFolder, size, combatProfile)
    {
    }

    public override bool IsContainer => true;

    public void AddChild(NodeData node)
    {
        if (Children.All(child => !string.Equals(child.Path, node.Path, StringComparison.OrdinalIgnoreCase)))
        {
            Children.Add(node);
        }
    }

    public void RemoveChild(NodeData node)
    {
        Children.Remove(node);
    }
}

public class FileNode : NodeData
{
    public FileNode(string name, string path, long size = 0, NodeCombatProfile? combatProfile = null)
        : base(name, path, false, size, combatProfile)
    {
    }
}

public class FolderNode : ContainerNode
{
    public FolderNode(string name, string path, NodeCombatProfile? combatProfile = null)
        : base(name, path, true, 0, combatProfile)
    {
    }
}

public class ArchiveNode : ContainerNode
{
    public ArchiveNode(string name, string path, long size = 0, NodeCombatProfile? combatProfile = null)
        : base(name, path, false, size, combatProfile ?? new NodeCombatProfile("Archive", "High", NodeThreatLevel.High, 10, 3, 3, revealsChildrenOnOpen: true, revealSummary: "Archive cracked open; payload spilled into view."))
    {
    }

    public override string UiTypeName => CombatProfile.TypeName;
}
