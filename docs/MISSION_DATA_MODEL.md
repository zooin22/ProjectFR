# ProjectFR - Mission Data Model

> Status: active — code/design alignment reference; update as mission system evolves
> Read with: `MISSION_FACTIONS.md`, `STORY_WORLD.md`
> Related: `res/mission/`

## Current Code Shape

All classes live in `res/mission/` under namespace `ProjectFR.Mission`.

### Classes

**`MissionClientProfile`** — embedded in each `MissionData`; fields: `FactionId`, `Faction` (derived display string), `Name`, `Agenda`, `RiskNote`. The typed identifier now supports code-side branching while preserving current UI labels.

**`MissionData`** — immutable value object constructed once. Fields:
- `Id`, `Title`, `Briefing` — identity and display text
- `Client : MissionClientProfile` — embedded (not a reference)
- `ObjectiveType : MissionObjectiveType` — `Extract | Delete | Scan | Modify | Escape`
- `TargetPath : string` — single path, matched by string equality
- `TurnLimit`, `RewardCredits`, `RewardReputation`, `FailurePenaltyCredits`, `FailureHeat`
- `PrerequisiteMissionId : string?` — nullable; mission is locked until the named mission ID appears in `CampaignState.CompletedMissionIds`. `null` means always available.

**`MissionObjectiveType`** — enum with five values: `Extract`, `Delete`, `Scan`, `Modify`, `Escape`.

**`MissionProgress`** — per-run tracker for a single mission objective. `RegisterAction(actionId, targetPath)` completes the objective when the action matches: Extract → `copy`, Delete → `delete`, Scan → `inspect`, Modify → `logforge`, Escape → `extract`. `RegisterEscape(currentPath)` exists for the extraction flow. `Resolve()` returns a `MissionResult`; success requires player alive, objective completed, extracted, and within turn limit.

**`MissionResult`** — outcome snapshot: `Success`, `Summary`, `CreditsDelta`, `ReputationDelta`, `HeatDelta`, `ObjectiveCompleted`, `PlayerSurvived`, `DungeonCleared`, `TurnsUsed`.

**`CampaignState`** — static runtime state. Fields: `Credits` (starts 100), `Reputation` (global), `Heat`. Per-faction standing tracked in `_factionReputation : Dictionary<FactionId, int>` (read via `GetFactionReputation(FactionId)`). Completed mission IDs stored in `_completedMissionIds : HashSet<string>`. `IsMissionAvailable(MissionData)` returns true when the mission has no prerequisite or its prerequisite ID is in `_completedMissionIds`. Selection and navigation (`GetSelectedMission`, `SelectNextMission`, `SelectPreviousMission`) operate over `GetAvailableMissions()` — the filtered subset — so locked missions are never reachable. `GetModifiers()` returns heat-tiered `CampaignModifiers`. `ApplyMissionResult()` adjusts Credits/global Reputation/Heat, increments the client faction's standing, and on success records the mission ID in `_completedMissionIds` (which may unlock previously locked missions on the next navigation call).

**`CampaignModifiers`** — three heat tiers (LOW < 3, ELEVATED 3–5, CRITICAL ≥ 6) that scale `HeatTurnPenalty`, `EnemyAttackBonus`, `EnemyApBonus`, `EnemyHpBonus`. Read by both `BattleScene` and `MainMenu`.

### Mission Board (`MissionBoardFactory.CreateDefaultBoard`)

| Id | Title | Client | Faction | Type | Reward | Prerequisite |
|---|---|---|---|---|---|---|
| mission_extract_boss | Archive Lift | Morrow Proxy | Corporate Espionage | Extract | 90cr rep+2 | — |
| mission_delete_readme | Loose End Cleanup | Northline Legal | Legal Fixers | Delete | 65cr rep+1 | — |
| mission_scan_cache | Cache Recon | Helix Ops | Security Contractors | Scan | 55cr rep+1 | — |
| mission_extract_readme | Mirror Snatch | Glass Key Collective | Civic Leakers | Extract | 60cr rep+2 | — |
| mission_delete_boss | Burn Notice | Ember Circuit | Leak Brokers | Delete | 95cr rep+1 | mission_extract_boss |

"Burn Notice" is locked until "Archive Lift" is completed. Both target `BossZipPath`; the narrative tension is that Morrow Proxy pays to retrieve Boss.zip while Ember Circuit later pays to destroy the same file — but only once you've proven you can reach it.

### MainMenu Rendering

`MainMenu.RefreshMenu()` displays: mission title + `Client.Name`, `Client.Faction`, `Briefing`, `ObjectiveType + TargetPath`, heat-adjusted turn limit, `Client.Agenda`, `Client.RiskNote`, rewards/penalties, and operator status (Credits/Reputation/Heat + enemy modifiers). All five faction profiles render correctly in the lobby.

---

## Gaps vs Faction Design

### 1. Per-faction reputation is additive but not yet gating
`CampaignState.GetFactionReputation(FactionId)` now tracks standing per faction, updated by `ApplyMissionResult`. The global `Reputation` is preserved. Standing thresholds (unlock tiers, faction-specific risk changes) are not yet read at runtime — the data structure exists but nothing branches on the values yet.

### 2. Mission prerequisites wired; deeper chaining not yet supported
`MissionData.PrerequisiteMissionId` accepts one nullable prerequisite. `CampaignState` filters by it, so locked missions are invisible in navigation. A full prerequisite graph (multiple requirements, reputation thresholds) would require a richer condition model.

### 3. Faction fields still have no programmatic effect beyond identity
`FactionId` now exists, but `Agenda` and `RiskNote` remain display-only. Reward magnitude, heat generation, and difficulty scaling are still encoded per-mission manually, not derived from faction identity.

### 4. Conflict axis is data-coincident only
`mission_extract_readme` (Glass Key, Extract `RootReadmePath`) and `mission_delete_readme` (Northline Legal, Delete `RootReadmePath`) share the same `TargetPath` — the design's core tension. Nothing in code detects or surfaces this conflict.

---

## Practical Next Data Evolution

Ordered by scope; each step is additive and does not break existing code.

### Step 1 — Typed faction identity (done)
`FactionId` now exists on `MissionClientProfile`, while `Faction` remains a derived display string. Code can branch safely on faction identity without breaking the current lobby text.

### Step 2 — Per-faction reputation (done)
`CampaignState._factionReputation : Dictionary<FactionId, int>` tracks per-faction standing. `GetFactionReputation(FactionId)` reads it (returns 0 for untouched factions). `ApplyMissionResult` updates the client faction alongside global `Reputation`. Global `Reputation` preserved for existing UI callers.

### Step 3 — Objective enum expansion (partially done)
`Modify` and `Escape` now exist in the enum. `MissionProgress` supports `Modify -> logforge` and has extraction-path support for `Escape`, but there are still no default missions that use those objective types.

### Step 4 — Mission prerequisites (done)
`MissionData.PrerequisiteMissionId : string?` (nullable = always available). `CampaignState._completedMissionIds : HashSet<string>` records successful mission IDs via `ApplyMissionResult`. `IsMissionAvailable(MissionData)` gates `GetAvailableMissions()`, which drives selection and navigation. "Burn Notice" requires "Archive Lift" as the first live example.

### Step 5 — Mission board factory/data split (done for factory)
Default mission construction already moved out of `CampaignState` into `MissionBoardFactory`. Next step would be moving from factory code to JSON/resource-backed data if authoring needs grow.

### Step 6 — Conflict group field (future / design-dependent)
Once prerequisites or faction standing exist, add `string? ConflictGroup` to `MissionData` to group missions targeting the same objective from opposing factions. `MainMenu` can highlight active conflicts. No logic needed initially — just the data shape for later UI use.
