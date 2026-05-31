namespace ProjectFR.Action.Implementations;

public static class ActionConstants
{
    public const int OpenActionApCost = 1;
    public const int OpenFileDamage = 3;
    public const int OpenFolderDamage = 1;

    public const int InspectActionApCost = 0;

    public const int CopyActionApCost = 1;

    public const int CutActionApCost = 2;
    public const int CutDamage = 3;

    public const int DeleteActionApCost = 2;
    public const int DeleteDamage = 7;

    public const int CleanActionApCost = 3;
    public const int CleanDamage = 3;

    public const int PasteActionApCost = 2;
    public const int PasteFileDamage = 5;
    public const int PasteSpecialFileBaseDamage = 8;
    public const double PasteSpecialFileMultiplier = 1.5;

    public const int QuarantineActionApCost = 2;
    public const int QuarantineEffectDuration = 2;

    public const int CompressActionApCost = 2;
    public const int CompressEffectDuration = 3;
    public const int CompressAttackModifier = -1;

    public const int LogForgeActionApCost = 1;
    public const int SearchActionApCost = 1;
    public const int ShowHiddenActionApCost = 1;
    public const int PermissionOverrideActionApCost = 2;
}
