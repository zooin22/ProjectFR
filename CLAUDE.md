# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ProjectFR is a **tactical infiltration roguelike** built in **Godot 4.6 with C# (.NET 8)**. The player controls a cursor agent navigating a file system, using OS-style operations (Copy, Cut, Paste, Delete, Compress, Rewrite Log, etc.) as tactical actions against security agents. The original turn-based battle prototype is still present in `res/battle/` but is being superseded by the infiltration runtime in `res/infiltration/`.

## Build & Run Commands

There are no standalone build scripts. Development is done through Godot's editor:

- **Open project**: Launch Godot 4.6 editor and open `project.godot`
- **Build C#**: Godot compiles automatically, or press `Alt+B` in the editor
- **Run game**: Press `F5` in Godot editor (runs `res/scenes/main.tscn`)
- **Run scene directly**: Open a `.tscn` file and press `F6`
- **CLI build** (no editor): `dotnet build ProjectFR.csproj`

There are no unit tests in this project.

## Architecture

### Pivot: Battle Prototype → Infiltration Runtime

The codebase has two layers running in parallel:
- **Battle layer** (`res/battle/`): the original prototype (BattleManager, ActorState, turn flow). Still present; not yet removed.
- **Infiltration layer** (`res/infiltration/`): the active runtime being built out. `InfiltrationManager` runs alongside `BattleManager` and will gradually replace it.

### Runtime Overview

```
GameManager (Autoload Singleton)
  └── owns: ClipboardSystem, StatusEffectSystem, CurrentBattle (BattleManager)

BattleManager (legacy, still active)
  └── owns: Player ActorState, Enemy ActorState[]
  └── uses: ActionRegistry to resolve and execute actions
  └── turn flow: PlayerTurn → EnemyTurn → EndTurn → BattleEnd (enum state machine)

InfiltrationManager (active runtime)
  └── owns: InfiltrationState, List<SecurityAgent>, MissionData
  └── InfiltrationState holds: CursorAgent, Windows, Clipboard, PouchCache,
        ActiveOperations, CommandQueue, Trace/AlertStage, EventLog,
        path-effect dictionaries (TrackedPathTurns, ForcedLockTurns, ScanPressureTurns)
  └── turn flow: CommandQueue → FileOperation completion → SecurityAgent reactions

BattleScene (UI Node — drives both layers)
  └── renders: ExplorerField (icon grid), ClipboardWindow, action bar, queue panel, log
  └── drives: BattleManager.ExecutePlayerAction() and InfiltrationManager turn/queue APIs
```

### Action System (`res/action/`)

- **`IAction`** — interface: `ActionId`, `DisplayName`, `ApCost`, `Scope` (TargetType), `CanExecute()`, `Execute()`
- **`ActionBase`** — abstract convenience base implementing common boilerplate
- **`IActionCondition`** — predicate on `ActionContext`; actions declare required conditions
- **`ActionRegistry`** — registers all actions at init; `GetExecutableActions()` filters by conditions
- **`ActionContext`** — carries caster, primary target, target list, ClipboardSystem, StatusEffectSystem
- **`ActionResult`** — returned by `Execute()`, contains log message and success flag
- **`ActionIds`** — string constants for every registered action identifier

Current actions (13) and AP costs: `OpenAction` (1), `CopyAction` (1), `InspectAction` (0), `DeleteAction` (2), `CutAction` (2), `PasteAction` (2), `QuarantineAction` (2), `CompressAction` (2), `SearchAction` (1), `LogForgeAction` (1), `ShowHiddenAction` (1), `PermissionOverrideAction` (2), `CleanAction` (3 AoE).

Conditions in `res/action/conditions/`: `MinApCondition`, `TargetAliveCondition`, `ClipboardNotEmptyCondition`, `NotStatusCondition`.

### Infiltration Layer (`res/infiltration/`)

- **`InfiltrationManager`** — run orchestrator: initializes state, processes turns, routes security reactions, manages operations and queue execution
- **`InfiltrationState`** — all mutable runtime state: `CurrentFolderPath`, `Trace`, `AlertStage`, `CursorAgent`, `Windows`, `Clipboard`, `PouchCache`, `ActiveOperations`, `CommandQueue`, `EventLog`, turn counters, path-scoped effect dictionaries
- **`CursorAgent`** — player entity: `CurrentNodePath`, `ActionPoints`, `ClipboardCapacity`, `PouchCapacity`, `PouchMaxFileSize`, `IsDetected`
- **`SecurityAgent`** — guard/antivirus/indexer/firewall AI: patrol route, sight range, awareness state; each `SecurityAgentType` (IndexerScout, AiMonitor, FirewallSentinel, GuardScanner) has its own behavior key mapping
- **`FileOperation`** — in-progress operation with `OperationType`, `TargetPath`, `Progress`, `Status`; resolves over turns, not instantly
- **`CommandQueueEntry`** — queued player command
- **`ExplorerWindowState`** — open sub-window (Main, Clipboard, Temp, LogViewer, etc.) with focus/open state and Trace modifier
- **`InfiltrationTuning`** / **`SecurityBehaviorTuning`** — all balance numbers (Trace costs, durations, pouch limits) in one place

Security behavior pipeline: `SecurityBehaviorKeys` → `SecurityBehaviorFactory` → `SecurityBehaviorExecutor` driven by `SecurityBehaviorContext`. Reactions vary by agent type and proximity to objective paths.

### Skills Layer (`res/skills/`)

Extensibility scaffold for complex tactical skills composed from multiple steps:
- **`SkillDefinition`** / **`SkillCatalog`** — data-driven skill registration
- **`SkillExecutor`** — executes skills via behavior tree-style dispatch
- **`SkillBehaviorNode`** hierarchy — composable steps; Search, ShowHidden, PermissionOverride behaviors are mapped here

### Data Models (`res/data/`)

- **`ActorState`** — HP/AP with max values; tracks alive status (battle layer)
- **`NodeData`** (abstract) / `SpecialFileNode` — file system objects used as tactical targets (`res/data/nodes/`)
- **`TargetType`** enum — Single, AoE, Self, Multiple, All, Adjacent, Line
- **`ContextActionData`** — serializable action metadata for data-driven configuration

### Systems (`res/systems/`)

- **`ClipboardSystem`** — holds a `NodeData` in Copy or Cut mode; Cut clears after Paste (battle layer)
- **`StatusEffectSystem`** — tracks per-actor effects (Quarantine, Compressed, Corrupted) with duration/magnitude; auto-expires on turn end (battle layer)

### Battle Layer (`res/battle/`)

Legacy system kept in parallel during the pivot. Contains `BattleManager`, `BattleDungeon`, `BattleFactory`, `BattleConstants`, `DungeonFolderMetadata`.

### Mission (`res/mission/`)

- **`MissionData`** — defines the infiltration objective and target layout
- **`MissionObjectiveType`** — enum of objective kinds (Extract, Delete, Modify, Scan, Escape)

### Namespaces

| Namespace | Directory |
|-----------|-----------|
| `ProjectFR.Action` | `res/action/` |
| `ProjectFR.Battle` | `res/battle/` |
| `ProjectFR.Core` | `res/core/` |
| `ProjectFR.Data` / `ProjectFR.Data.Nodes` | `res/data/` |
| `ProjectFR.Infiltration` | `res/infiltration/` |
| `ProjectFR.Mission` | `res/mission/` |
| `ProjectFR.Scenes` | `res/scenes/` |
| `ProjectFR.Skills` | `res/skills/` |
| `ProjectFR.Systems` | `res/systems/` |

## Key Conventions

- **Autoload**: `GameManager` is the only Godot autoload singleton. Access it via `GameManager.Instance`.
- **No direct system construction in actions**: Systems are always received through `ActionContext`, never instantiated inside action classes.
- **Action identifiers**: Use constants from `ActionIds` — never raw strings.
- **Infiltration tuning**: All balance numbers (Trace costs, operation durations, pouch limits) go in `InfiltrationTuning` or `SecurityBehaviorTuning`. Do not scatter magic numbers in manager or scene code.
- **Infiltration log**: `InfiltrationState.AddLog(string)` (capped at 100). Battle log: `BattleManager.AddLog(string)` (capped at 50).
- **Enemy AI**: SecurityAgents use the behavior pipeline (`SecurityBehaviorExecutor`). BattleManager enemies still use simple random selection from `GetExecutableActions()`.
- **Nullable enabled**: `<Nullable>enable</Nullable>` in the csproj — handle nullability explicitly.
