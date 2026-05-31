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
}
