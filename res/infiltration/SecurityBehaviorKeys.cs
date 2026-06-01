namespace ProjectFR.Infiltration;

public static class SecurityBehaviorKeys
{
    public const string CursorCrossedMonitoredNode = "security.cursor_crossed_monitored_node";
    public const string FolderNavigation = "security.folder_navigation";
    public const string SearchSweep = "security.search_sweep";

    public const string CursorCrossedGuardScanner = "security.cursor_crossed.guard_scanner";
    public const string CursorCrossedIndexerScout = "security.cursor_crossed.indexer_scout";
    public const string CursorCrossedAiMonitor = "security.cursor_crossed.ai_monitor";
    public const string CursorCrossedFirewallSentinel = "security.cursor_crossed.firewall_sentinel";

    public const string FolderNavigationGuardScanner = "security.folder_navigation.guard_scanner";
    public const string FolderNavigationIndexerScout = "security.folder_navigation.indexer_scout";
    public const string FolderNavigationAiMonitor = "security.folder_navigation.ai_monitor";
    public const string FolderNavigationFirewallSentinel = "security.folder_navigation.firewall_sentinel";

    public const string SearchSweepIndexerScout = "security.search_sweep.indexer_scout";
    public const string SearchSweepAiMonitor = "security.search_sweep.ai_monitor";

    public const string CursorCrossedAntivirusHeavy = "security.cursor_crossed.antivirus_heavy";
    public const string CursorCrossedBackupRepairer = "security.cursor_crossed.backup_repairer";
    public const string FolderNavigationAntivirusHeavy = "security.folder_navigation.antivirus_heavy";
    public const string FolderNavigationBackupRepairer = "security.folder_navigation.backup_repairer";
    public const string SearchSweepAntivirusHeavy = "security.search_sweep.antivirus_heavy";
    public const string SearchSweepBackupRepairer = "security.search_sweep.backup_repairer";

    public const string RestoreNode = "security.restore_node";
}
