namespace ProjectFR.Data.Nodes;

public abstract class NodeData
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsFolder { get; set; }
    public long Size { get; set; }

    protected NodeData(string name, string path, bool isFolder, long size = 0)
    {
        Name = name;
        Path = path;
        IsFolder = isFolder;
        Size = size;
    }

    public override string ToString() => $"{Name} ({(IsFolder ? "Folder" : "File")})";
}

public class FileNode : NodeData
{
    public FileNode(string name, string path, long size = 0)
        : base(name, path, false, size)
    {
    }
}

public class FolderNode : NodeData
{
    public List<NodeData> Children { get; } = new();

    public FolderNode(string name, string path)
        : base(name, path, true, 0)
    {
    }

    public void AddChild(NodeData node)
    {
        if (Children.FirstOrDefault(c => c.Path == node.Path) == null)
        {
            Children.Add(node);
        }
    }

    public void RemoveChild(NodeData node)
    {
        Children.Remove(node);
    }
}
