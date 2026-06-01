# Docs Policy

> Status: active — operational policy, update when documentation structure or maintenance rules change

ProjectFR 문서를 정확하고 탐색 가능하게 유지하기 위한 운영 규칙이다. 현재 `docs/`는 **설계 문서**, **작업/변경 로그**, **소스 기반 wiki**를 분리해서 관리한다.

## Canonical Docs and When to Update Them

| File / Folder | Owns | Update when |
|---|---|---|
| `00_DESIGN_INDEX.md` | 전체 설계 문서 허브, 읽기 순서, 문서 맵, 용어집 | 설계 문서가 추가/이동/분류 변경될 때 |
| `01_vision/GAME_CONCEPT.md` | 코어 비전, 핵심 판타지, 플레이 감각 | 게임 정체성이나 핵심 루프가 바뀔 때 |
| `01_vision/STORY_WORLD.md` | 세계관, 주제, 서사 구조 | 스토리 premise, 세계관, 주제축이 바뀔 때 |
| `01_vision/MISSION_FACTIONS.md` | 의뢰 세력, 미션 톤, 관계축 | 세력 로스터, 의뢰 구조, 세력 평판 방향이 바뀔 때 |
| `02_systems/COMBAT_SYSTEM_DESIGN.md` | 침투/전투 코어, 턴 흐름, AP, Trace, FileOperation | 작전 루프나 전투 규칙이 바뀔 때 |
| `02_systems/SECURITY_AI_DESIGN.md` | 보안 모듈 AI, 반응 단계, 행동 트리 | 보안 에이전트 타입/행동/튜닝 방향이 바뀔 때 |
| `02_systems/EXPLORER_INTERACTION_DESIGN.md` | 탐색기 조작, 우클릭, 드래그, 포인터 전용 조작 | 플레이어 입력 모델이나 탐색기 상호작용이 바뀔 때 |
| `02_systems/LOBBY_MISSION_DESIGN.md` | 로비, 미션 보드, 브리핑, 출격 준비 | 미션 선택/해금/브리핑 흐름이 바뀔 때 |
| `02_systems/PROGRESSION_ECONOMY_DESIGN.md` | Credits, Reputation, Heat, 메타 진행, 빌드 | 캠페인 경제나 장기 진행 구조가 바뀔 때 |
| `02_systems/LEVEL_GENERATION_DESIGN.md` | 폴더 구조 생성, 노드 역할, 목표/함정 배치 | 레벨 생성 규칙이나 던전 레이아웃 정책이 바뀔 때 |
| `02_systems/RANDOMNESS_DESIGN.md` | RNG 계층, 판정 공개, 불운 보호 | 무작위성 철학/판정/보상 드래프트가 바뀔 때 |
| `02_systems/UI_UX_DESIGN.md` | 화면 레이아웃, 데스크톱 오버레이, 상태 가시화 | UI 레이아웃, HUD, 피드백 방식이 바뀔 때 |
| `02_systems/MULTI_WINDOW_EXPANSION.md` | 멀티윈도우 확장 설계 | 창 타입, 상태, Clipboard/Log/Temp Window 범위가 바뀔 때 |
| `03_data_impl/MISSION_DATA_MODEL.md` | 미션 데이터 모델과 코드 구조 정합성 | `res/mission/` 모델이나 세력/미션 데이터 연결이 바뀔 때 |
| `03_data_impl/PROJECT_REDESIGN_STEPS.md` | 구현 진행 로그, battle → infiltration pivot 이력 | 주요 구현 단계가 완료/폐기/재정의될 때 |
| `CHANGELOG.md` | 문서 구조 정리 및 큰 문서 변경 이력 | 문서 세트를 대규모로 재분류/정리했을 때 |
| `TODO.md` | 실행 작업의 source of truth | `/op` 또는 수동 계획으로 작업 항목을 추가/완료할 때 |
| `wiki/` | 현재 소스 기반 레퍼런스 | 코드/설계가 크게 바뀌었거나 `/wiki` 갱신을 수행했을 때 |

`docs/README.md`는 사람이 보는 최상위 인덱스다. 새 문서를 만들거나 이동하면 `README.md`와 `00_DESIGN_INDEX.md` 중 해당되는 인덱스를 즉시 갱신한다.

## Folder Roles

- `01_vision/` — **왜/무엇**: 게임 정체성, 세계관, 세력, 플레이 판타지.
- `02_systems/` — **어떻게**: 전투, 보안 AI, 조작, 로비, 경제, 레벨 생성, RNG, UI, 멀티윈도우.
- `03_data_impl/` — **코드로 어떻게 맞출지**: 데이터 모델, 구현 진행 로그.
- `wiki/` — 코드와 TODO를 읽어 생성/갱신하는 소스 기반 레퍼런스.
- 루트 문서 — `README.md`, `DOCS_POLICY.md`, `00_DESIGN_INDEX.md`, `TODO.md`, `CHANGELOG.md`처럼 탐색·운영·작업 관리에 필요한 문서.

## Adding or Moving a Doc

1. 먼저 문서가 `01_vision`, `02_systems`, `03_data_impl`, `wiki`, 루트 운영 문서 중 어디에 속하는지 정한다.
2. 설계 문서라면 상단에 다음 메타 정보를 둔다:
   - `> Status: <draft | design | active | stale | archived>`
   - 필요하면 `> Read with:` / `> Owns:` / `> Related:`를 추가한다.
3. 새 설계 문서는 `00_DESIGN_INDEX.md`의 폴더 트리와 문서 맵에 등록한다.
4. 사람이 바로 찾아야 하는 문서는 `docs/README.md`에도 등록한다.
5. 문서를 이동했다면 옛 경로를 참조하는 링크를 검색해 갱신한다.

## Status Labels

- `draft` — 아직 합의되지 않은 초안.
- `design` — 구현 전/구현 중인 시스템 명세.
- `active` — 현재 기준 문서.
- `stale` — 일부 내용이 오래되었고 다른 문서가 우선함.
- `archived` — 역사 보존용. 새 작업의 기준이 아님.

## Marking a Doc Stale or Archived

문서 내용이 대체되었지만 이력이 필요하면 삭제하지 말고 다음을 수행한다.

1. 상태 라인을 `> Status: stale — superseded by <OtherFile.md>` 또는 `> Status: archived — <reason>`로 바꾼다.
2. 본문 상단에 무엇이 대체했는지 한 줄로 적는다.
3. `docs/README.md`의 Reference / Legacy 섹션으로 옮긴다.
4. 설계 문서라면 `00_DESIGN_INDEX.md`에서도 상태를 맞춘다.

## What Does Not Need a Doc

- 코드 수준 세부 사항이 `CLAUDE.md`, inline comment, 타입/메서드명으로 충분히 드러나는 경우.
- 일회성 실험이나 임시 메모.
- git commit message나 PR description에만 있으면 되는 변경 기록.
- `TODO.md`에만 있어도 충분한 작은 작업 항목.

## Link and Naming Rules

- 설계 문서 본문에서 같은 문서 세트 안의 상호 참조는 가능하면 파일명 중심으로 쓴다. 실제 위치는 `00_DESIGN_INDEX.md`에서 찾는다.
- `README.md`처럼 탐색 목적의 문서는 클릭 가능한 상대 경로를 사용한다.
- 파일명은 기존 convention을 따른다: `UPPER_SNAKE_CASE.md` for design docs, kebab-case for generated wiki docs.
- 한국어 본문 + 영문 기술 용어를 유지한다. 예: Trace, FileOperation, CommandQueue.
