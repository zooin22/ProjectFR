# ProjectFR TODO Planning Workflow Plan

> Created: 2026-05-31 20:10:48 KST
> Target project: `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR`
> Plan mode note: this document is the only project file intentionally written in this turn. `docs/TODO.md` and the session TodoTool should be updated during the execution turn, not during plan mode.

## Goal

Prepare a concrete execution plan for the ProjectFR `/plan` workflow:

1. Read all Markdown/text documentation in `docs/`.
2. Use Claude codebase analysis only if documentation is insufficient to determine current implementation state.
3. Write or update `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/TODO.md` as the future `/todo` source of truth.
4. Mirror pending Markdown tasks into the session TodoTool.
5. Return a Telegram-friendly summary with total/done/pending counts and the next 3 unchecked tasks.

## Current Context / Assumptions

### Documentation inspected

All current Markdown/text files in `docs/` were read or checked:

- `docs/README.md`
- `docs/DOCS_POLICY.md`
- `docs/GAME_CONCEPT.md`
- `docs/STORY_WORLD.md`
- `docs/MISSION_FACTIONS.md`
- `docs/MISSION_DATA_MODEL.md`
- `docs/MULTI_WINDOW_EXPANSION.md`
- `docs/PROJECT_REDESIGN_STEPS.md`

No `.txt` files were found in `docs/`.

### Existing project direction

ProjectFR is a Windows Explorer-inspired tactical roguelike about a tailless hacker hamster restoring deleted memories and resisting Tail Signal / CleanSweep / OPTIMA systems. The primary gameplay direction is:

- Explorer UI as the tactical battlefield.
- File operations as combat/actions.
- File operation progress, command queues, Trace/Heat, and security reactions as the core tactical loop.
- Mission completion based on objective completion plus escape, not enemy elimination.
- Multi-window operations as a tactical expansion, with Clipboard Window already implemented and Temp/Log/Backup/Archive windows planned.

### Current implementation state from docs

The docs indicate the following are already at least partially implemented:

- Domain split for infiltration concepts.
- `ExplorerRunManager` / infiltration manager scaffolding.
- Clipboard and pouch cache state.
- Large-icon explorer field prototype.
- Command queue with delayed file operations.
- Context menu and drag/drop prototypes.
- Search, Show Hidden, Permission Override actions.
- Mission escape loop based on objective + extraction.
- Mission board, factions, prerequisites, per-faction reputation storage.
- Multi-window scaffolding and Clipboard Window UI.
- Security behavior executor/scaffolding and state effects like tracked paths, forced locks, scan pressure.
- Badge/status visibility for watch/lock/track/pressure/pouch states.

### Likely source inspection need

Docs contain detailed progress notes, but before finalizing `docs/TODO.md`, execution should ask Claude to inspect the codebase for drift because several docs say “partial”, “scaffolding”, or “planned”. Use `claude -p` from the ProjectFR root to confirm current code state before writing TODO tasks.

## Proposed Approach

During the execution turn, generate `docs/TODO.md` from design gaps and implementation gaps, not from broad feature wishes. Prioritize tasks that are:

- Concrete and implementable in one `/todo` run.
- Grounded in canonical docs.
- Ordered so foundational systems come before content expansion.
- Suitable for Claude AI to implement and commit independently.
- Verifiable by tests/build/smoke checks.

Recommended ordering:

1. Complete mission/objective examples that are already supported by enums and progress code but unused in default board.
2. Surface existing conflict/faction systems in UI and data before adding deeper campaign mechanics.
3. Implement the next multi-window feature after Clipboard Window: Temp Window, then Log Viewer Window.
4. Expand security pressure/reaction loops only where UI and player counterplay already exist.
5. Keep narrative/content work tied to runnable missions and visible UI.

## Step-by-Step Execution Plan

### 1. Re-read docs immediately before editing TODO

Use read-only file access to read every `.md` and `.txt` file under:

`/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/`

Minimum expected set:

- `README.md`
- `DOCS_POLICY.md`
- `GAME_CONCEPT.md`
- `STORY_WORLD.md`
- `MISSION_FACTIONS.md`
- `MISSION_DATA_MODEL.md`
- `MULTI_WINDOW_EXPANSION.md`
- `PROJECT_REDESIGN_STEPS.md`

If `docs/TODO.md` already exists by then, read it too and preserve completed `- [x]` items unless they are clearly obsolete.

### 2. Ask Claude to inspect implementation state

Run from project root:

```bash
cd /mnt/e/Agent/workspaces/Godot/projects/ProjectFR
claude -p "Inspect this Godot/C# project and compare current implementation against docs/PROJECT_REDESIGN_STEPS.md, docs/MISSION_DATA_MODEL.md, and docs/MULTI_WINDOW_EXPANSION.md. Return a concise list of implemented, partially implemented, and missing features. Do not edit files."
```

Use this only for analysis. Do not let this command mutate files during the planning phase. In the actual execution phase, mutation is allowed if the user invoked the ProjectFR `/plan` workflow rather than plan mode.

### 3. Build a concrete TODO candidate list

Use these high-priority tasks as initial candidates, then adjust based on Claude's inspection:

- [ ] Add default `Modify` and `Escape` mission examples to the mission board and ensure their objective progress paths are playable.
- [ ] Add conflict-group metadata to opposing missions that share a target path and surface the conflict warning in `MainMenu`.
- [ ] Make per-faction reputation affect at least one visible runtime decision, such as mission availability, reward modifier, risk note, or heat penalty.
- [ ] Implement the first Temp Window slice: open/focus/close Temp Window, bind it to a temp path, and show current temp/hidden items.
- [ ] Implement Log Viewer Window as a focused UI for tracked paths, scan pressure, and `Rewrite Log` cleanup targets.
- [ ] Add player-facing feedback when `Rewrite Log` removes `Tracked` and `Scan Pressure` state effects.
- [ ] Add tests or smoke coverage for mission prerequisites, faction reputation updates, and conflict metadata rendering.
- [ ] Add tests or smoke coverage for Clipboard/Pouch edge cases: oversized rejection, ghost state, show-hidden exposure, and restore to clipboard.
- [ ] Create or wire the Cache Zone 17 first-mission scenario around deletion report recovery or family photo restoration.
- [ ] Update docs after each completed system slice, especially `PROJECT_REDESIGN_STEPS.md`, `MISSION_DATA_MODEL.md`, and `MULTI_WINDOW_EXPANSION.md`.

### 4. Write or update `docs/TODO.md`

Target path:

`/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/TODO.md`

Required format:

```markdown
# ProjectFR TODO

> Updated: 2026-05-31

- [ ] Add default `Modify` and `Escape` mission examples to the mission board and ensure their objective progress paths are playable.
- [ ] Add conflict-group metadata to opposing missions that share a target path and surface the conflict warning in `MainMenu`.
- [ ] Make per-faction reputation affect at least one visible runtime decision, such as mission availability, reward modifier, risk note, or heat penalty.
```

Guidelines:

- Keep tasks one implementation slice each.
- Avoid vague tasks like “improve gameplay”.
- Prefer tasks that mention the system, expected behavior, and verification target.
- Preserve completed tasks if an existing TODO file has them.
- If previous pending tasks remain valid, keep them before newly inferred tasks unless a new prerequisite should come first.

### 5. Mirror pending tasks into TodoTool

After writing `docs/TODO.md`, parse pending tasks matching:

```text
- [ ] <task>
```

Replace the session TodoTool list with those pending items in file order. Suggested IDs:

- `projectfr-001`
- `projectfr-002`
- `projectfr-003`
- etc.

Each TodoTool item should be:

- `content`: the Markdown task text without `- [ ]`
- `status`: `pending`

Completed Markdown tasks should not be mirrored into active TodoTool pending items.

### 6. Produce Telegram-friendly summary

Return a concise summary with:

- TODO path.
- Total task count.
- Done count.
- Pending count.
- Next 3 unchecked tasks.

Suggested format:

```text
ProjectFR TODO 갱신 완료

파일: /mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/TODO.md
전체: N개
완료: X개
미완료: Y개

다음 작업 3개:
1. ...
2. ...
3. ...
```

## Files Likely to Change During Execution

Primary:

- `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/TODO.md`

Possible docs updates after future `/todo` implementation tasks:

- `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/PROJECT_REDESIGN_STEPS.md`
- `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/MISSION_DATA_MODEL.md`
- `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/MULTI_WINDOW_EXPANSION.md`
- `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/README.md` if new docs are added

Likely code paths for future `/todo` tasks:

- `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/res/mission/`
- `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/res/infiltration/`
- `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/res/skills/`
- `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/res/action/`
- Battle/main menu scene/controller files, especially files responsible for `MainMenu`, `BattleScene`, explorer field UI, window UI, and action queue rendering.

## Tests / Validation

For the TODO-generation execution itself:

- Confirm `docs/TODO.md` exists.
- Confirm first line is exactly `# ProjectFR TODO`.
- Confirm it includes an `> Updated:` line.
- Confirm task lines use only GitHub checkbox syntax:
  - `- [ ] ...`
  - `- [x] ...`
- Confirm TodoTool pending items match the unchecked Markdown tasks.
- Confirm Telegram summary counts match parsed Markdown counts.

For future code tasks selected by `/todo`:

- Run the project’s existing test/build command if available.
- If no formal tests exist, run the Godot/.NET build or compile check available in this repo.
- For C# changes, compile the project and inspect for errors.
- For UI/system changes, perform the closest available smoke test and document what was actually run.

## Risks / Tradeoffs

- The docs are detailed but may be ahead of or behind code. Claude source inspection should be used before writing final TODO priorities.
- Some tasks, such as Temp Window or Log Viewer Window, may be too large if written broadly. They should be scoped as first slices.
- `docs/TODO.md` must remain a source of truth for automation, so avoid narrative paragraphs inside the task list.
- Existing completed tasks should not be deleted unless clearly stale.
- If the active skill is plan mode, do not update `docs/TODO.md` or TodoTool in that same turn; only write this plan file.

## Open Questions

- What is the canonical project build/test command for ProjectFR in the current environment?
- Should `docs/TODO.md` keep only pending tasks, or retain historical completed `- [x]` tasks as a short changelog?
- Should `/todo` always pick the first unchecked task, or support hints/selectors from the user?
- Should planning tasks prioritize playable mission loop completion over multi-window expansion, or continue the most recent multi-window/security behavior direction?
