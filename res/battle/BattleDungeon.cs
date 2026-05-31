using ProjectFR.Data.Nodes;

namespace ProjectFR.Battle;

public class BattleDungeon
{
    private readonly Dictionary<string, DungeonFolderMetadata> _folderMetadata;
    private readonly Dictionary<string, NodeData> _nodeIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ContainerNode?> _parentIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _clearedPaths = new(StringComparer.OrdinalIgnoreCase);

    public FolderNode Root { get; }
    public ContainerNode CurrentContainer { get; private set; }
    public int TotalNodeCount { get; }
    public int ClearedNodeCount => _clearedPaths.Count;

    public BattleDungeon(FolderNode root, Dictionary<string, DungeonFolderMetadata> folderMetadata)
    {
        Root = root;
        CurrentContainer = root;
        _folderMetadata = new Dictionary<string, DungeonFolderMetadata>(folderMetadata, StringComparer.OrdinalIgnoreCase);

        IndexNode(root, null);
        TotalNodeCount = CountNodes(root);
    }

    public IReadOnlyList<NodeData> GetCurrentEncounterNodes()
    {
        return CurrentContainer.Children
            .Where(child => !_clearedPaths.Contains(child.Path))
            .ToList();
    }

    public IEnumerable<FolderNode> GetAllFolders()
    {
        return _nodeIndex.Values.OfType<FolderNode>();
    }

    public NodeData? GetNode(string path)
    {
        return _nodeIndex.GetValueOrDefault(path);
    }

    public ContainerNode? GetParentContainer(string path)
    {
        return _parentIndex.GetValueOrDefault(path);
    }

    public bool EnterContainer(string path)
    {
        if (_nodeIndex.GetValueOrDefault(path) is not ContainerNode container)
            return false;

        CurrentContainer = container;
        return true;
    }

    public bool EnterParentOfCurrent()
    {
        if (!_parentIndex.TryGetValue(CurrentContainer.Path, out var parent) || parent == null)
            return false;

        CurrentContainer = parent;
        return true;
    }

    public bool IsCleared(string path)
    {
        return _clearedPaths.Contains(path);
    }

    public void MarkCleared(NodeData node)
    {
        MarkClearedRecursive(node);

        var parent = GetParentContainer(node.Path);
        parent?.RemoveChild(node);

        if (ReferenceEquals(CurrentContainer, node) && parent != null)
        {
            CurrentContainer = parent;
        }
    }

    public bool MoveNode(string nodePath, string targetContainerPath)
    {
        if (_nodeIndex.GetValueOrDefault(nodePath) is not NodeData node)
            return false;

        if (_nodeIndex.GetValueOrDefault(targetContainerPath) is not ContainerNode targetContainer)
            return false;

        if (_parentIndex.GetValueOrDefault(nodePath) is not ContainerNode currentParent)
            return false;

        if (string.Equals(currentParent.Path, targetContainer.Path, StringComparison.OrdinalIgnoreCase))
            return false;

        currentParent.RemoveChild(node);
        targetContainer.AddChild(node);
        _parentIndex[node.Path] = targetContainer;
        return true;
    }

    public bool HasRemainingNodes()
    {
        return EnumerateAllNodes(Root).Any(node => !_clearedPaths.Contains(node.Path));
    }

    public string GetProgressLabel()
    {
        return $"Cleared {ClearedNodeCount}/{TotalNodeCount} nodes";
    }

    public DungeonFolderMetadata GetCurrentMetadata()
    {
        return GetMetadataForPath(CurrentContainer.Path);
    }

    public DungeonFolderMetadata GetMetadataForPath(string path)
    {
        var cursor = path;
        while (true)
        {
            if (_folderMetadata.TryGetValue(cursor, out var metadata))
                return metadata;

            var parent = GetParentContainer(cursor);
            if (parent == null)
                break;

            cursor = parent.Path;
        }

        return new DungeonFolderMetadata("Unclassified", "No event data", "No reward data", 0);
    }

    private void IndexNode(NodeData node, ContainerNode? parent)
    {
        _nodeIndex[node.Path] = node;
        _parentIndex[node.Path] = parent;

        if (node is not ContainerNode container)
            return;

        foreach (var child in container.Children)
        {
            IndexNode(child, container);
        }
    }

    private void MarkClearedRecursive(NodeData node)
    {
        _clearedPaths.Add(node.Path);

        if (node is not ContainerNode container)
            return;

        foreach (var child in container.Children.ToList())
        {
            MarkClearedRecursive(child);
        }
    }

    private static int CountNodes(ContainerNode container)
    {
        int total = 0;
        foreach (var child in container.Children)
        {
            total++;
            if (child is ContainerNode nested)
            {
                total += CountNodes(nested);
            }
        }

        return total;
    }

    private static IEnumerable<NodeData> EnumerateAllNodes(ContainerNode container)
    {
        foreach (var child in container.Children)
        {
            yield return child;
            if (child is ContainerNode nested)
            {
                foreach (var nestedChild in EnumerateAllNodes(nested))
                {
                    yield return nestedChild;
                }
            }
        }
    }
}
