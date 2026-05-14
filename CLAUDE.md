# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ProjectFR is a turn-based battle system built in **Godot 4.6 with C# (.NET 8)**. The game's theme uses OS file system operations (Copy, Cut, Paste, Delete, Compress, Quarantine, etc.) as combat actions.

## Build & Run Commands

There are no standalone build scripts. Development is done through Godot's editor:

- **Open project**: Launch Godot 4.6 editor and open `project.godot`
- **Build C#**: Godot compiles automatically, or press `Alt+B` in the editor
- **Run game**: Press `F5` in Godot editor (runs `res/scenes/main.tscn`)
- **Run scene directly**: Open a `.tscn` file and press `F6`
- **CLI build** (no editor): `dotnet build ProjectFR.csproj`

There are no unit tests in this project.

## Architecture

### Core Pattern: Command + Registry

Actions are implemented as stateless command objects. `ActionRegistry` owns all 9 action instances and provides filtered lookups. `ActionContext` is passed into every `Execute()` call carrying the caster, targets, and system references — actions never store mutable state themselves.

```
GameManager (Autoload Singleton)
  └── owns: ClipboardSystem, StatusEffectSystem, CurrentBattle (BattleManager)

BattleManager
  └── owns: Player ActorState, Enemy ActorState[]
  └── uses: ActionRegistry to resolve and execute actions
  └── turn flow: PlayerTurn → EnemyTurn → EndTurn → BattleEnd (enum state machine)

BattleScene (UI Node)
  └── reads: BattleManager state to build UI
  └── drives: BattleManager.ExecutePlayerAction() on button press / keyboard shortcut
```

### Action System (`res/action/`)

- **`IAction`** — interface with `ActionId`, `DisplayName`, `ApCost`, `Scope` (TargetType), `CanExecute()`, `Execute()`
- **`IActionCondition`** — predicate on `ActionContext`; actions declare required conditions
- **`ActionRegistry`** — registers all 9 actions at init; `GetExecutableActions()` filters by conditions
- **`ActionContext`** — carries caster, primary target, target list, ClipboardSystem, StatusEffectSystem
- **`ActionResult`** — returned by `Execute()`, contains log message and success flag

The 9 actions and their AP costs: `OpenAction` (1), `CopyAction` (1), `DeleteAction` (2), `CutAction` (2), `PasteAction` (2), `QuarantineAction` (2), `CompressAction` (2), `InspectAction` (0), `CleanAction` (3 AoE).

Conditions in `res/action/conditions/`: `MinApCondition`, `TargetAliveCondition`, `ClipboardNotEmptyCondition`, `NotStatusCondition`.

### Data Models (`res/data/`)

- **`ActorState`** — HP/AP with max values; tracks alive status; handles damage/healing
- **`NodeData`** (abstract) / `FileNode` / `FolderNode` / `SpecialFileNode` — file system objects used as combat targets; `FolderNode` reduces damage, `SpecialFileNode` amplifies it
- **`TargetType`** enum — Single, AoE, Self, Multiple, All, Adjacent, Line
- **`ContextActionData`** — serializable action metadata for data-driven configuration

### Systems (`res/systems/`)

- **`ClipboardSystem`** — holds a `NodeData` in Copy or Cut mode; Cut clears clipboard after Paste
- **`StatusEffectSystem`** — tracks per-actor effects (Quarantine, Compressed, Corrupted) with duration and magnitude; provides attack modifier lookup; auto-expires effects on turn end

### Namespaces

| Namespace | Directory |
|-----------|-----------|
| `ProjectFR.Action` | `res/action/` |
| `ProjectFR.Battle` | `res/battle/` |
| `ProjectFR.Core` | `res/core/` |
| `ProjectFR.Data` | `res/data/` |
| `ProjectFR.Systems` | `res/systems/` |
| `ProjectFR.Scenes` | `res/scenes/` |

## Key Conventions

- **Autoload**: `GameManager` is the only Godot autoload singleton. Access it via `GameManager.Instance`.
- **No direct system construction in actions**: Systems are always received through `ActionContext`, never instantiated inside action classes.
- **Enemy AI**: Simple random selection from `GetExecutableActions()` — no scoring or difficulty tiers yet.
- **Battle log**: Capped at 50 messages; use `BattleManager.AddLog(string)` to append.
- **Nullable enabled**: `<Nullable>enable</Nullable>` in the csproj — handle nullability explicitly.
