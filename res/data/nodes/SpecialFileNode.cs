namespace ProjectFR.Data.Nodes;

/// <summary>
/// Represents a special file node - a more dangerous file type.
/// Moved from action implementation to the data model to reduce coupling.
/// </summary>
public class SpecialFileNode : FileNode
{
    /// <summary>
    /// Initializes a new instance of the SpecialFileNode class
    /// </summary>
    /// <param name="name">The name of the file</param>
    /// <param name="path">The path to the file</param>
    /// <param name="size">The size of the file in bytes</param>
    public SpecialFileNode(string name, string path, long size = 0)
        : base(name, path, size)
    {
    }
}
