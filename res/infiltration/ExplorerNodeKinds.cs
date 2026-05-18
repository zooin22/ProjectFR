namespace ProjectFR.Infiltration;

public enum ExplorerNodeKind
{
    Folder,
    File,
    Log,
    Temp,
    Archive,
    Encrypted,
    Executable,
    Shortcut,
    Exit,
    Decoy,
    System
}

public enum ExplorerNodeRole
{
    Objective,
    Resource,
    Hazard,
    Evidence,
    Utility,
    Decoy,
    Exit,
    SecurityAnchor
}
