namespace ProjectFR.Infiltration;

public static class SecurityBehaviorFactory
{
    public static SecurityBehaviorNode? Create(string behaviorKey)
    {
        return behaviorKey switch
        {
            SecurityBehaviorKeys.CursorCrossedMonitoredNode => BuildCursorCrossedBehavior(),
            SecurityBehaviorKeys.FolderNavigation => BuildFolderNavigationBehavior(),
            SecurityBehaviorKeys.SearchSweep => BuildSearchSweepBehavior(),
            SecurityBehaviorKeys.CursorCrossedGuardScanner => BuildCursorCrossedBehavior(),
            SecurityBehaviorKeys.CursorCrossedIndexerScout => BuildIndexerScoutCursorBehavior(),
            SecurityBehaviorKeys.CursorCrossedAiMonitor => BuildAiMonitorCursorBehavior(),
            SecurityBehaviorKeys.CursorCrossedFirewallSentinel => BuildFirewallCursorBehavior(),
            SecurityBehaviorKeys.FolderNavigationGuardScanner => BuildFolderNavigationBehavior(),
            SecurityBehaviorKeys.FolderNavigationIndexerScout => BuildIndexerFolderBehavior(),
            SecurityBehaviorKeys.FolderNavigationAiMonitor => BuildAiMonitorFolderBehavior(),
            SecurityBehaviorKeys.FolderNavigationFirewallSentinel => BuildFirewallFolderBehavior(),
            SecurityBehaviorKeys.SearchSweepIndexerScout => BuildIndexerSearchBehavior(),
            SecurityBehaviorKeys.SearchSweepAiMonitor => BuildAiMonitorSearchBehavior(),
            _ => null
        };
    }

    private static SecurityBehaviorNode BuildCursorCrossedBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agents.Count > 0),
            new SecurityActionNode(AlertAgents),
            new SecurityActionNode(ApplyTraceAndLog));
    }

    private static SecurityBehaviorNode BuildFolderNavigationBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agents.Count > 0),
            new SecurityActionNode(AlertAgents),
            new SecurityActionNode(ApplyTraceAndLog));
    }

    private static SecurityBehaviorNode BuildSearchSweepBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agents.Count > 0),
            new SecurityActionNode(AlertAgents));
    }

    private static SecurityBehaviorNode BuildIndexerScoutCursorBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agent != null),
            new SecurityActionNode(context => AlertSingleAgent(
                context,
                context.IsObjectiveRoute ? SecurityAwarenessStage.ActiveScan : SecurityAwarenessStage.Suspicious,
                context.IsObjectiveRoute
                    ? "Indexer Scout matched cursor drift against an objective route."
                    : "Indexer Scout flagged cursor drift.")),
            new SecurityActionNode(context => context.IsObjectiveRoute
                ? ApplyModifiedTraceAndLog(context, SecurityBehaviorTuning.ObjectiveRouteTraceBonus)
                : ApplyTraceAndLog(context)),
            new SecurityActionNode(context =>
            {
                if (context.IsObjectiveRoute || context.IsObjectivePath)
                {
                    context.MarkTrackedPath(context.PrimaryPath, SecurityBehaviorTuning.TraceMarkerDurationTurns, "Indexer Scout marked the route");
                }

                return SecurityBehaviorStatus.Success;
            }));
    }

    private static SecurityBehaviorNode BuildAiMonitorCursorBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agent != null),
            new SecurityActionNode(context => AlertSingleAgent(
                context,
                context.IsObjectiveRoute ? SecurityAwarenessStage.Quarantine : SecurityAwarenessStage.ActiveScan,
                context.IsObjectiveRoute
                    ? "AI Monitor linked the cursor anomaly to the mission objective route."
                    : "AI Monitor escalated cursor anomaly.")),
            new SecurityActionNode(context => ApplyModifiedTraceAndLog(
                context,
                SecurityBehaviorTuning.AiMonitorCursorTraceBonus + (context.IsObjectiveRoute ? SecurityBehaviorTuning.ObjectiveRouteTraceBonus : 0))),
            new SecurityActionNode(context =>
            {
                if (context.IsObjectiveRoute || context.IsObjectivePath)
                {
                    context.ApplyScanPressure(context.CurrentFolderPath, SecurityBehaviorTuning.ScanPressureDurationTurns, "AI Monitor pressured the current folder");
                }

                return SecurityBehaviorStatus.Success;
            }));
    }

    private static SecurityBehaviorNode BuildFirewallCursorBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agent != null),
            new SecurityActionNode(context => AlertSingleAgent(
                context,
                context.IsObjectiveRoute ? SecurityAwarenessStage.Quarantine : SecurityAwarenessStage.ActiveScan,
                context.IsObjectiveRoute
                    ? "Firewall Sentinel contested the objective route directly."
                    : "Firewall Sentinel marked the route as contested.")),
            new SecurityActionNode(context => context.IsObjectiveRoute
                ? ApplyModifiedTraceAndLog(context, SecurityBehaviorTuning.ObjectiveRouteTraceBonus)
                : ApplyTraceAndLog(context)),
            new SecurityActionNode(context =>
            {
                if (context.IsObjectiveRoute || context.IsObjectivePath)
                {
                    context.ApplyForcedLock(context.PrimaryPath, SecurityBehaviorTuning.ForcedLockDurationTurns, "Firewall Sentinel contested the node");
                }

                return SecurityBehaviorStatus.Success;
            }));
    }

    private static SecurityBehaviorNode BuildIndexerFolderBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agent != null),
            new SecurityActionNode(context => AlertSingleAgent(
                context,
                context.IsObjectiveRoute ? SecurityAwarenessStage.ActiveScan : context.AwarenessStage,
                context.IsObjectiveRoute
                    ? "Indexer Scout recorded traversal along an objective route."
                    : "Indexer Scout recorded folder traversal.")),
            new SecurityActionNode(context => context.IsObjectiveRoute
                ? ApplyModifiedTraceAndLog(context, SecurityBehaviorTuning.ObjectiveRouteTraceBonus)
                : ApplyTraceAndLog(context)),
            new SecurityActionNode(context =>
            {
                if (context.IsObjectiveRoute)
                {
                    context.MarkTrackedPath(context.PrimaryPath, SecurityBehaviorTuning.TraceMarkerDurationTurns, "Indexer Scout marked the folder route");
                }

                return SecurityBehaviorStatus.Success;
            }));
    }

    private static SecurityBehaviorNode BuildAiMonitorFolderBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agent != null),
            new SecurityActionNode(context => AlertSingleAgent(
                context,
                context.IsObjectiveRoute ? SecurityAwarenessStage.Quarantine : SecurityAwarenessStage.ActiveScan,
                context.IsObjectiveRoute
                    ? "AI Monitor reclassified the objective-route jump as hostile."
                    : "AI Monitor reclassified the folder jump as hostile.")),
            new SecurityActionNode(context => context.IsObjectiveRoute
                ? ApplyModifiedTraceAndLog(context, SecurityBehaviorTuning.ObjectiveRouteTraceBonus)
                : ApplyTraceAndLog(context)),
            new SecurityActionNode(context =>
            {
                if (context.IsObjectiveRoute)
                {
                    context.ApplyScanPressure(context.PrimaryPath, SecurityBehaviorTuning.ScanPressureDurationTurns, "AI Monitor saturated the folder with scans");
                }

                return SecurityBehaviorStatus.Success;
            }));
    }

    private static SecurityBehaviorNode BuildFirewallFolderBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agent != null),
            new SecurityActionNode(context => AlertSingleAgent(
                context,
                context.IsObjectiveRoute ? SecurityAwarenessStage.Quarantine : SecurityAwarenessStage.ActiveScan,
                context.IsObjectiveRoute
                    ? "Firewall Sentinel hardened an objective-facing route."
                    : "Firewall Sentinel hardened the traversed route.")),
            new SecurityActionNode(context => ApplyModifiedTraceAndLog(
                context,
                SecurityBehaviorTuning.FirewallFolderNavigationTraceBonus + (context.IsObjectiveRoute ? SecurityBehaviorTuning.ObjectiveRouteTraceBonus : 0))),
            new SecurityActionNode(context =>
            {
                if (context.IsObjectiveRoute)
                {
                    context.ApplyForcedLock(context.PrimaryPath, SecurityBehaviorTuning.ForcedLockDurationTurns, "Firewall Sentinel hardened the folder");
                }

                return SecurityBehaviorStatus.Success;
            }));
    }

    private static SecurityBehaviorNode BuildIndexerSearchBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agent != null),
            new SecurityActionNode(context => AlertSingleAgent(
                context,
                context.IsObjectivePath || context.IsObjectiveRoute ? SecurityAwarenessStage.Quarantine : SecurityAwarenessStage.ActiveScan,
                context.IsObjectivePath || context.IsObjectiveRoute
                    ? "Indexer Scout found residue aligned with the objective signature."
                    : "Indexer Scout started a residue search pass.")),
            new SecurityActionNode(context => context.IsObjectivePath || context.IsObjectiveRoute
                ? ApplyModifiedTraceAndLog(context, SecurityBehaviorTuning.ObjectiveSearchTraceBonus)
                : SecurityBehaviorStatus.Success),
            new SecurityActionNode(context =>
            {
                if (context.IsObjectivePath || context.IsObjectiveRoute)
                {
                    context.MarkTrackedPath(context.PrimaryPath, SecurityBehaviorTuning.TraceMarkerDurationTurns, "Indexer Scout traced the search residue");
                }

                return SecurityBehaviorStatus.Success;
            }));
    }

    private static SecurityBehaviorNode BuildAiMonitorSearchBehavior()
    {
        return new SecuritySequenceNode(
            new SecurityConditionNode(context => context.Agent != null),
            new SecurityActionNode(context => AlertSingleAgent(
                context,
                context.IsObjectivePath || context.IsObjectiveRoute ? SecurityAwarenessStage.Purge : SecurityAwarenessStage.Quarantine,
                context.IsObjectivePath || context.IsObjectiveRoute
                    ? "AI Monitor tied the search directly to the objective signature and escalated to purge review."
                    : "AI Monitor escalated the query to quarantine review.")),
            new SecurityActionNode(context => ApplyModifiedTraceAndLog(
                context,
                SecurityBehaviorTuning.AiMonitorSearchTraceBonus + ((context.IsObjectivePath || context.IsObjectiveRoute) ? SecurityBehaviorTuning.ObjectiveSearchTraceBonus : 0))),
            new SecurityActionNode(context =>
            {
                context.ApplyScanPressure(context.CurrentFolderPath, SecurityBehaviorTuning.ScanPressureDurationTurns, "AI Monitor widened active scans");
                if (context.IsObjectivePath || context.IsObjectiveRoute)
                {
                    context.MarkTrackedPath(context.PrimaryPath, SecurityBehaviorTuning.TraceMarkerDurationTurns, "AI Monitor pinned the objective-aligned search route");
                }

                return SecurityBehaviorStatus.Success;
            }));
    }

    private static SecurityBehaviorStatus AlertAgents(SecurityBehaviorContext context)
    {
        foreach (var agent in context.Agents)
        {
            context.AlertAgent(agent, context.AwarenessStage);
        }

        return SecurityBehaviorStatus.Success;
    }

    private static SecurityBehaviorStatus AlertSingleAgent(SecurityBehaviorContext context, SecurityAwarenessStage awarenessStage, string logMessage)
    {
        if (context.Agent == null)
            return SecurityBehaviorStatus.Failure;

        context.AlertAgent(context.Agent, awarenessStage);
        context.AddLog(logMessage);
        return SecurityBehaviorStatus.Success;
    }

    private static SecurityBehaviorStatus ApplyTraceAndLog(SecurityBehaviorContext context)
    {
        if (context.TraceAmount > 0)
        {
            context.AddTrace(context.TraceAmount, context.TraceReason);
        }

        context.AddLog($"Security behavior executed: {context.PrimaryPath} [{context.AwarenessStage}]");
        return SecurityBehaviorStatus.Success;
    }

    private static SecurityBehaviorStatus ApplyModifiedTraceAndLog(SecurityBehaviorContext context, int traceBonus)
    {
        if (context.TraceAmount + traceBonus > 0)
        {
            context.AddTrace(context.TraceAmount + traceBonus, context.TraceReason);
        }

        context.AddLog($"Security behavior executed: {context.PrimaryPath} [{context.AwarenessStage}] +bonus {traceBonus}");
        return SecurityBehaviorStatus.Success;
    }
}
