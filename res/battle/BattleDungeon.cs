using ProjectFR.Data.Nodes;

namespace ProjectFR.Battle;

public class BattleDungeon
{
    private readonly List<FolderNode> _folderOrder;
    private readonly Dictionary<string, DungeonFolderMetadata> _folderMetadata;
    private int _currentFolderIndex;

    public FolderNode Root { get; }
    public FolderNode CurrentFolder => _folderOrder[_currentFolderIndex];
    public int TotalNodeCount { get; }
    public int ClearedNodeCount { get; private set; }
    public int EncounterIndex => _currentFolderIndex + 1;
    public int EncounterCount => _folderOrder.Count;

    public BattleDungeon(FolderNode root, Dictionary<string, DungeonFolderMetadata> folderMetadata)
    {
        Root = root;
        _folderMetadata = folderMetadata;
        _folderOrder = EnumerateFoldersDepthFirst(root)
            .Where(folder => folder.Children.Count > 0)
            .ToList();
        _currentFolderIndex = 0;
        TotalNodeCount = CountNodes(root);
    }

    public IReadOnlyList<NodeData> GetCurrentEncounterNodes()
    {
        return CurrentFolder.Children;
    }

    public DungeonFolderMetadata GetCurrentMetadata()
    {
        return GetMetadata(CurrentFolder);
    }

    public FolderNode? PeekNextFolder()
    {
        var nextIndex = _currentFolderIndex + 1;
        return nextIndex < _folderOrder.Count ? _folderOrder[nextIndex] : null;
    }

    public DungeonFolderMetadata? PeekNextMetadata()
    {
        var nextFolder = PeekNextFolder();
        return nextFolder != null ? GetMetadata(nextFolder) : null;
    }

    public bool AdvanceAfterCurrentEncounter()
    {
        ClearedNodeCount = Math.Min(TotalNodeCount, ClearedNodeCount + CurrentFolder.Children.Count);

        if (_currentFolderIndex + 1 >= _folderOrder.Count)
            return false;

        _currentFolderIndex++;
        return true;
    }

    public string GetProgressLabel()
    {
        return $"Encounter {EncounterIndex}/{EncounterCount} · Cleared {ClearedNodeCount}/{TotalNodeCount} nodes";
    }

    private DungeonFolderMetadata GetMetadata(FolderNode folder)
    {
        return _folderMetadata.GetValueOrDefault(folder.Path)
            ?? new DungeonFolderMetadata("Unclassified", "No event data", "No reward data", 0);
    }

    private static int CountNodes(FolderNode folder)
    {
        int total = 0;
        foreach (var child in folder.Children)
        {
            total++;
            if (child is FolderNode childFolder)
            {
                total += CountNodes(childFolder);
            }
        }

        return total;
    }

    private static IEnumerable<FolderNode> EnumerateFoldersDepthFirst(FolderNode folder)
    {
        yield return folder;

        foreach (var childFolder in folder.Children.OfType<FolderNode>())
        {
            foreach (var nestedFolder in EnumerateFoldersDepthFirst(childFolder))
            {
                yield return nestedFolder;
            }
        }
    }
}
