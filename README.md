# ProjectFR

A **tactical infiltration roguelike** built in **Godot 4.6 + C# (.NET 8)**. You control a cursor agent navigating a file system, using OS-style operations — Copy, Cut, Paste, Delete, Compress, Rewrite Log — as tactical actions against security agents.

## Status

The codebase is mid-pivot. A turn-based battle prototype (`res/battle/`) still runs but is being replaced by the infiltration runtime (`res/infiltration/`). Both layers are wired in parallel through `BattleScene`. New work goes into the infiltration layer.

## How to Build & Run

| Task | How |
|------|-----|
| Open project | Launch Godot 4.6 editor → open `project.godot` |
| Build C# | Auto-compiles in editor, or `Alt+B`, or `dotnet build ProjectFR.csproj` |
| Run game | `F5` in editor (entry point: `res/scenes/main.tscn`) |
| Run one scene | Open `.tscn`, press `F6` |

There are no unit tests.

## Key Folders

```
res/
  action/          IAction system — 13 actions, ActionRegistry, ActionIds
  battle/          Legacy battle prototype (BattleManager, turn flow)
  infiltration/    Active runtime (InfiltrationManager, SecurityAgent, FileOperation)
  mission/         MissionData, MissionObjectiveType
  scenes/          BattleScene (UI + drives both layers), MainMenu
  skills/          Extensibility scaffold for multi-step tactical skills
  data/            ActorState, NodeData, TargetType, ContextActionData
  systems/         ClipboardSystem, StatusEffectSystem (battle layer)
  core/            GameManager autoload singleton
docs/              Design docs, implementation logs, world-building
```

## Where to Read More

See [`docs/README.md`](docs/README.md) for the full docs index and reading order.
