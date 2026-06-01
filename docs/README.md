# ProjectFR Docs Index

> Status: active — living index, update when adding, moving, or archiving any doc

ProjectFR 문서의 최상위 인덱스다. 전체 설계의 단일 허브는 `00_DESIGN_INDEX.md`이고, 이 README는 사람이 폴더를 열었을 때 빠르게 목적지로 이동하기 위한 탐색용 문서다.

## Quick Start

1. [`00_DESIGN_INDEX.md`](00_DESIGN_INDEX.md)
   - 전체 설계 문서 허브, 문서 맵, 핵심 루프, 용어집.
2. [`01_vision/GAME_CONCEPT.md`](01_vision/GAME_CONCEPT.md)
   - 게임의 최상위 컨셉, 핵심 판타지, 시스템 방향.
3. [`01_vision/STORY_WORLD.md`](01_vision/STORY_WORLD.md)
   - 세계관, 주제, 주요 서사 구조.
4. [`01_vision/MISSION_FACTIONS.md`](01_vision/MISSION_FACTIONS.md)
   - 의뢰 세력, 관계축, 초반 서사 구조.
5. 관심 시스템 문서
   - 전투/침투: [`02_systems/COMBAT_SYSTEM_DESIGN.md`](02_systems/COMBAT_SYSTEM_DESIGN.md)
   - 보안 AI: [`02_systems/SECURITY_AI_DESIGN.md`](02_systems/SECURITY_AI_DESIGN.md)
   - 조작/탐색기: [`02_systems/EXPLORER_INTERACTION_DESIGN.md`](02_systems/EXPLORER_INTERACTION_DESIGN.md)
   - 로비/미션: [`02_systems/LOBBY_MISSION_DESIGN.md`](02_systems/LOBBY_MISSION_DESIGN.md)
6. 구현 기준
   - 데이터 모델: [`03_data_impl/MISSION_DATA_MODEL.md`](03_data_impl/MISSION_DATA_MODEL.md)
   - 진행 로그: [`03_data_impl/PROJECT_REDESIGN_STEPS.md`](03_data_impl/PROJECT_REDESIGN_STEPS.md)

## Folder Structure

```text
docs/
├── README.md
├── DOCS_POLICY.md
├── 00_DESIGN_INDEX.md
├── CHANGELOG.md
├── TODO.md
├── 01_vision/
│   ├── GAME_CONCEPT.md
│   ├── STORY_WORLD.md
│   └── MISSION_FACTIONS.md
├── 02_systems/
│   ├── COMBAT_SYSTEM_DESIGN.md
│   ├── SECURITY_AI_DESIGN.md
│   ├── EXPLORER_INTERACTION_DESIGN.md
│   ├── LOBBY_MISSION_DESIGN.md
│   ├── PROGRESSION_ECONOMY_DESIGN.md
│   ├── LEVEL_GENERATION_DESIGN.md
│   ├── RANDOMNESS_DESIGN.md
│   ├── UI_UX_DESIGN.md
│   └── MULTI_WINDOW_EXPANSION.md
├── 03_data_impl/
│   ├── MISSION_DATA_MODEL.md
│   └── PROJECT_REDESIGN_STEPS.md
└── wiki/
    ├── README.md
    ├── gameplay.md
    ├── systems.md
    ├── architecture.md
    ├── content-pipeline.md
    ├── dev-guide.md
    └── changelog-from-todo.md
```

## Canonical Design Docs

### 0) Design Hub

- [`00_DESIGN_INDEX.md`](00_DESIGN_INDEX.md)
- 역할: 전체 기획 문서의 진입점, 문서 맵, 코어 루프, 용어집, 읽기 순서.
- 상태: **전체 설계 허브**

### 1) Vision Layer — `01_vision/`

- [`01_vision/GAME_CONCEPT.md`](01_vision/GAME_CONCEPT.md)
  - 역할: 게임 전체 정체성, 핵심 시스템, 플레이 감각 정의.
  - 상태: **코어 비전 기준 문서**
- [`01_vision/STORY_WORLD.md`](01_vision/STORY_WORLD.md)
  - 역할: 세계관, 주제, 주요 서사 구조 정의.
  - 상태: **스토리/세계관 기준 문서**
- [`01_vision/MISSION_FACTIONS.md`](01_vision/MISSION_FACTIONS.md)
  - 역할: 의뢰인 세력, 미션 톤, 관계축 정리.
  - 상태: **세력/의뢰 기준 문서**

### 2) System Design Layer — `02_systems/`

- [`02_systems/COMBAT_SYSTEM_DESIGN.md`](02_systems/COMBAT_SYSTEM_DESIGN.md)
  - 역할: 침투 전투 코어, 파일 작업 진행률, 명령 큐, AP, Trace, 턴 흐름.
- [`02_systems/SECURITY_AI_DESIGN.md`](02_systems/SECURITY_AI_DESIGN.md)
  - 역할: 보안 모듈 AI, 8종 모듈, 반응 단계, 행동 트리, 순찰/감시/추적.
- [`02_systems/EXPLORER_INTERACTION_DESIGN.md`](02_systems/EXPLORER_INTERACTION_DESIGN.md)
  - 역할: 포인터 전용 조작, 우클릭, 드래그, 검색/속성창의 전술화.
- [`02_systems/LOBBY_MISSION_DESIGN.md`](02_systems/LOBBY_MISSION_DESIGN.md)
  - 역할: 로비, 미션 보드, 브리핑, 출격 준비, 목표 타입.
- [`02_systems/PROGRESSION_ECONOMY_DESIGN.md`](02_systems/PROGRESSION_ECONOMY_DESIGN.md)
  - 역할: Credits, Reputation, Heat, 로그라이크 진행, 빌드 아키타입.
- [`02_systems/LEVEL_GENERATION_DESIGN.md`](02_systems/LEVEL_GENERATION_DESIGN.md)
  - 역할: 폴더 구조 절차 생성, 노드 역할 배치, 목표/함정/자산 배치.
- [`02_systems/RANDOMNESS_DESIGN.md`](02_systems/RANDOMNESS_DESIGN.md)
  - 역할: RNG 4계층, 판정 공개, 보상 드래프트, 불운 보호.
- [`02_systems/UI_UX_DESIGN.md`](02_systems/UI_UX_DESIGN.md)
  - 역할: 데스크톱 오버레이, 화면 레이아웃, 상태 가시화, 멀티윈도우 표현.
- [`02_systems/MULTI_WINDOW_EXPANSION.md`](02_systems/MULTI_WINDOW_EXPANSION.md)
  - 역할: 멀티윈도우 확장 설계. `UI_UX_DESIGN.md`, `EXPLORER_INTERACTION_DESIGN.md`와 함께 읽는다.

### 3) Data & Implementation Layer — `03_data_impl/`

- [`03_data_impl/MISSION_DATA_MODEL.md`](03_data_impl/MISSION_DATA_MODEL.md)
  - 역할: `res/mission/` 코드 구조와 세력/미션 설계 간 매핑, 갭 분석, 데이터 진화 계획.
  - 상태: **코드/설계 정합 참고 문서**
- [`03_data_impl/PROJECT_REDESIGN_STEPS.md`](03_data_impl/PROJECT_REDESIGN_STEPS.md)
  - 역할: 배틀 프로토타입 → 침투 런타임 재설계의 단계별 구현 진행 로그.
  - 상태: **구현 진행 기준 문서**

## Operations Docs

- [`DOCS_POLICY.md`](DOCS_POLICY.md)
  - 역할: 문서 분류, 업데이트 규칙, stale/archive 처리 기준.
- [`TODO.md`](TODO.md)
  - 역할: `/op`와 `/todo`가 사용하는 실행 작업 source of truth.
- [`CHANGELOG.md`](CHANGELOG.md)
  - 역할: 문서 구조 정리와 큰 문서 변경 이력.

## Generated Wiki

- [`wiki/README.md`](wiki/README.md)
  - 역할: 소스와 설계 문서를 읽어 갱신하는 레퍼런스 위키의 인덱스.
- [`wiki/gameplay.md`](wiki/gameplay.md) — 게임 루프, Trace/Heat, 미션 구조, 보안 에이전트.
- [`wiki/systems.md`](wiki/systems.md) — InfiltrationManager API, FileOperation, CommandQueue, 튜닝 상수, 액션 목록.
- [`wiki/architecture.md`](wiki/architecture.md) — 코드 구조, 네임스페이스, 클래스 관계.
- [`wiki/content-pipeline.md`](wiki/content-pipeline.md) — 미션/세력/에이전트/액션/창 추가 방법.
- [`wiki/dev-guide.md`](wiki/dev-guide.md) — 빌드/실행/컨벤션/작업 흐름.
- [`wiki/changelog-from-todo.md`](wiki/changelog-from-todo.md) — `TODO.md` 완료 항목 기반 구현 이력.

## Suggested Rule Going Forward

새 문서를 만들기 전에 아래 분류 중 하나인지 먼저 확인한다.

- 코어 비전 변경 → `01_vision/GAME_CONCEPT.md`
- 세계관/서사 변경 → `01_vision/STORY_WORLD.md`
- 세력/의뢰 구조 변경 → `01_vision/MISSION_FACTIONS.md`
- 전투/침투 루프 변경 → `02_systems/COMBAT_SYSTEM_DESIGN.md`
- 보안 AI 변경 → `02_systems/SECURITY_AI_DESIGN.md`
- 조작/탐색기 UX 변경 → `02_systems/EXPLORER_INTERACTION_DESIGN.md` 또는 `02_systems/UI_UX_DESIGN.md`
- 로비/미션 구조 변경 → `02_systems/LOBBY_MISSION_DESIGN.md`
- 경제/진행 변경 → `02_systems/PROGRESSION_ECONOMY_DESIGN.md`
- 레벨 생성 변경 → `02_systems/LEVEL_GENERATION_DESIGN.md`
- 무작위성/RNG 변경 → `02_systems/RANDOMNESS_DESIGN.md`
- 미션 데이터/코드 매핑 변경 → `03_data_impl/MISSION_DATA_MODEL.md`
- 구현 진행/다음 단계 변경 → `03_data_impl/PROJECT_REDESIGN_STEPS.md` 또는 `TODO.md`

새 문서를 추가해야 할 때는:

- 기능 단위 문서만 만든다.
- 비슷한 목적의 계획 문서를 중복 생성하지 않는다.
- 설계 문서는 `00_DESIGN_INDEX.md`에 등록한다.
- 사람이 바로 찾아야 하는 문서는 이 `README.md`에도 등록한다.
