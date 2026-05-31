# Docs Policy

> Status: active — operational policy, update when rules change

Practical rules for keeping this repo's documentation accurate and navigable without busy-work.

## Canonical Docs and When to Update Them

| File | Owns | Update when |
|------|------|-------------|
| `GAME_CONCEPT.md` | Core vision, systems direction, play feel | Core design changes (not every iteration) |
| `STORY_WORLD.md` | World, themes, narrative structure | Story or faction premise changes |
| `MISSION_FACTIONS.md` | Client factions, mission tone, relationship axes | Faction roster or mission structure changes |
| `PROJECT_REDESIGN_STEPS.md` | Step-by-step pivot log (battle → infiltration) | After completing or abandoning a step |
| `MULTI_WINDOW_EXPANSION.md` | Multi-window system design | When window types, state, or scope change |
| `MISSION_DATA_MODEL.md` | Mission code/design mapping, gap analysis, evolution plan | When mission data model or faction wiring changes |

`docs/README.md` is the index. Add a new doc there immediately after creating it.

## Adding a New Doc

1. Scope it to a single feature or decision area — no duplicate plan docs.
2. Add a status line at the top: `> Status: <draft | active | stale | archived>`.
3. Register it in `docs/README.md` before closing your PR.

## Marking a Doc Stale

When a doc's content is substantially superseded but the history is worth keeping:

1. Change its status line to `> Status: stale — superseded by <OtherFile.md>`.
2. Add a one-line note at the top body explaining what replaced it.
3. Move it to the **Reference / Legacy Docs** section in `docs/README.md`.

Do **not** delete stale docs — git history is not enough; the file should be discoverable by someone browsing the folder.

## Archiving vs. Deleting

- **Archive** (preferred): status → `archived`, move to `docs/archive/` if the folder gets crowded.
- **Delete**: only if the file was created by mistake and has never been referenced. Note the deletion in the relevant commit message.

## What Does Not Need a Doc

- Code-level decisions already captured in CLAUDE.md or inline comments.
- Temporary experiments (use a branch or a scratch file outside `docs/`).
- Content that belongs in git commit messages or PR descriptions.
