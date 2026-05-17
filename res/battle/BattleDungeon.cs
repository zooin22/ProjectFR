using ProjectFR.Data.Nodes;

namespace ProjectFR.Battle;

public class BattleDungeon
{
    private readonly Queue<FolderNode> _pendingFolders;

    public FolderNode Root { get; }
    public FolderNode CurrentFolder { get; private set; }
    public int TotalNodeCount { get; }
    public int ClearedNodeCount { get; private set; }

    public BattleDungeon(FolderNode root)
    {
        Root = root;
        CurrentFolder = root;
        TotalNodeCount = CountNodes(root);
        _pendingFolders = new Queue<FolderNode>(EnumerateFoldersDepthFirst(root).Skip(1));
    }

    public IReadOnlyList<NodeData> GetCurrentEncounterNodes()
    {
        return CurrentFolder.Children;
    }

    public string GetCurrentFolderLabel()
    {
        return $"Folder: {CurrentFolder.Path}";
    }

    public bool AdvanceAfterCurrentEncounter()
    {
        ClearedNodeCount = Math.Min(TotalNodeCount, ClearedNodeCount + CurrentFolder.Children.Count);

        while (_pendingFolders.Count > 0)
        {
            var nextFolder = _pendingFolders.Dequeue();
            CurrentFolder = nextFolder;

            if (CurrentFolder.Children.Count > 0)
                return true;
        }

        return false;
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
