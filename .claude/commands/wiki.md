---
description: "ProjectFR wiki 생성/갱신. 현재 소스 기준으로 docs/wiki/ 전체를 업데이트한다."
---

ProjectFR 프로젝트(`/mnt/e/Agent/workspaces/Godot/projects/ProjectFR`)의 위키를 현재 소스 기준으로 갱신한다.

## 절차

### Step 1 — 소스 읽기

다음 파일들을 읽어 현재 상태를 파악한다:

- `docs/기획서/*.md` (게임 컨셉, 세계관, 미션, 멀티창, 재설계 단계 등)
- `docs/TODO.md` (완료된 항목들 → changelog 반영)
- `res/infiltration/*.cs`
- `res/mission/*.cs`
- `res/action/*.cs`, `res/action/implementations/*.cs`
- `res/scenes/BattleScene.cs`, `res/scenes/MainMenu.cs`
- `res/infiltration/InfiltrationTuning.cs`, `SecurityBehaviorTuning.cs`

### Step 2 — 위키 파일 업데이트

`docs/wiki/` 디렉토리가 없으면 생성한다.

각 파일을 현재 코드와 설계 문서에서 확인한 실제 값으로 정확하게 작성한다:

- `README.md` — 전체 개요, 설계 문서 링크
- `gameplay.md` — 게임 루프, Trace/Heat 수치, 미션 구조, 보안 에이전트
- `systems.md` — InfiltrationManager API, FileOperation, CommandQueue, 멀티창, 튜닝 상수, 액션 목록
- `architecture.md` — 코드 구조, 네임스페이스, 클래스 관계도
- `content-pipeline.md` — 미션/세력/에이전트/액션/창 추가 방법
- `dev-guide.md` — 빌드/실행/컨벤션/작업 흐름
- `changelog-from-todo.md` — `docs/TODO.md` 완료 항목 기반 구현 이력

### Step 3 — 완료 보고

어떤 파일이 업데이트됐는지 한 줄로 보고한다.
