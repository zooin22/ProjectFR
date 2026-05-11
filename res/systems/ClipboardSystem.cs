using ProjectFR.Data.Nodes;

namespace ProjectFR.Systems;

public enum ClipboardMode
{
    Copy,
    Cut
}

public class ClipboardSystem
{
    private NodeData? _content;
    private ClipboardMode _mode;

    public NodeData? Content => _content;
    public ClipboardMode Mode => _mode;
    public bool HasContent => _content != null;

    public void Copy(NodeData node)
    {
        _content = node;
        _mode = ClipboardMode.Copy;
    }

    public void Cut(NodeData node)
    {
        _content = node;
        _mode = ClipboardMode.Cut;
    }

    public NodeData? Paste()
    {
        if (_content == null)
            return null;

        var result = _content;
        if (_mode == ClipboardMode.Cut)
        {
            _content = null;
        }
        return result;
    }

    public void Clear()
    {
        _content = null;
    }
}
