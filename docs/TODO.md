# ProjectFR TODO

> Updated: 2026-05-31

## Multi-Window Expansion (Step 7)

- [x] Implement Temp Window: add `ExplorerWindowType.TempFolder` open/close to `InfiltrationManager`, add a "Temp" button to `BattleScene`'s window toolbar, render the temp folder's node list in the sub-panel with Trace +1 on open (mirrors Clipboard Window pattern).
- [ ] Implement Log Viewer Window: add `ExplorerWindowType.LogViewer` open/close to `InfiltrationManager`, add a "Log" button in `BattleScene`, render `InfiltrationState.EventLog` entries in the panel, and allow `LogForge` to be queued on selected log entries from this window.
- [ ] Enforce multi-window parallel-operation Trace cost: in `InfiltrationManager.ProcessTurn()`, add +1 Trace per turn for each window beyond the first that has a concurrent `ActiveOperation` (implements the Phase B risk rule from `MULTI_WINDOW_EXPANSION.md`).

## File Operation & Security (Steps 4–5)

- [ ] Implement operation interruption on detection: in `InfiltrationManager`'s security processing, when `CursorAgent.IsDetected` flips true, set any `ActiveOperation` whose `TargetPath` is on a monitored node to `OperationStatus.Failed` and append a log entry explaining the interruption.
- [ ] Render SecurityAgent positions on ExplorerField icon cards: in `BattleScene.RefreshExplorerField()`, check each displayed node against each `SecurityAgent.CurrentNodePath` and append a compact badge (e.g. `[SCOUT]`, `[AI]`) to the card label when an agent occupies that node.

## Mission & Escape Loop (Step 6)

- [ ] Scale `HeatDelta` by `Trace` at escape: in `MissionProgress.Resolve()`, add `InfiltrationState.Trace / InfiltrationTuning.TracePerHeatPoint` (define the constant in `InfiltrationTuning`) to the base `FailureHeat` or reward heat so high-Trace extractions are more costly.
- [ ] Add a `Modify`-type mission and an `Escape`-type mission to `MissionBoardFactory.CreateDefaultBoard()`: one targeting a log file (`MissionObjectiveType.Modify`) and one pure-escape run (`MissionObjectiveType.Escape`, no file target), exercising the `logforge` and extraction-path code paths already wired in `MissionProgress`.

## Mission Data Model

- [ ] Add `string? ConflictGroup` to `MissionData` and set `"readme_conflict"` on both `mission_extract_readme` and `mission_delete_readme` in `MissionBoardFactory`; surface active conflicts in `MainMenu.RefreshMenu()` with a short conflict notice line.
- [ ] Wire faction reputation thresholds: add a nullable `int RequiredFactionReputation` field to `MissionData`, set a non-zero value on at least one mission in `MissionBoardFactory`, and update `CampaignState.IsMissionAvailable()` to reject missions where `GetFactionReputation(client.FactionId) < RequiredFactionReputation`.

## UI Layout (Step 3)

- [ ] Add a right-panel "Security" section to `BattleScene` that lists each active `SecurityAgent` (type, awareness state, current path) and each active `FileOperation` (target, progress %, status), replacing the placeholder `FieldSecurityLabel` summary text.
