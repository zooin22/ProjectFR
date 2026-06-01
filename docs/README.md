# ProjectFR Docs Index

> Status: active — living index, update when adding or archiving any doc

이 폴더의 기준 문서와 보조 문서를 빠르게 찾기 위한 인덱스.

## Recommended Reading Order

1. `기획서/GAME_CONCEPT.md`
   - 게임의 최상위 컨셉, 핵심 판타지, 시스템 방향
2. `기획서/STORY_WORLD.md`
   - 세계관, 스토리 구조, 주제
3. `기획서/MISSION_FACTIONS.md`
   - 의뢰 세력, 관계축, 초반 서사 구조
4. `기획서/PROJECT_REDESIGN_STEPS.md`
   - 침투 런 방향으로의 재설계 진행 로그 (Steps 1-7 부분 구현 완료)
5. `기획서/MULTI_WINDOW_EXPANSION.md`
   - 멀티윈도우 확장 설계 (Clipboard Window 1차 구현 완료)
6. `기획서/class_diagram.puml`
   - 런타임 구조를 보는 UML 소스 (침투 레이어 포함 갱신)

---

## Canonical Docs

### 1) Core Vision
- `기획서/GAME_CONCEPT.md`
- 역할: 게임 전체 정체성, 핵심 시스템, 플레이 감각 정의
- 상태: **기준 문서**

### 2) Story / World
- `기획서/STORY_WORLD.md`
- 역할: 세계관, 주제, 주요 서사 구조 정의
- 상태: **기준 문서**

### 3) Mission / Factions
- `기획서/MISSION_FACTIONS.md`
- 역할: 의뢰인 세력, 미션 톤, 관계축 정리
- 상태: **기준 문서**

### 4) Implementation Progress Log
- `기획서/PROJECT_REDESIGN_STEPS.md`
- 역할: 배틀 프로토타입 → 침투 런타임 재설계의 단계별 진행 로그. Steps 1-7 모두 부분 구현 완료 상태.
- 상태: **현재 작업 기준 문서**

### 5) Feature Expansion
- `기획서/MULTI_WINDOW_EXPANSION.md`
- 역할: 멀티윈도우 시스템 설계. Clipboard Window와 창 타입/상태 스캐폴딩은 구현 완료.
- 상태: **세부 설계 문서 (Phase A 구현 완료)**

### 6) Structure Reference
- `기획서/class_diagram.puml`
- 역할: 클래스/런타임 구조 다이어그램. 침투 레이어 포함, 양 레이어 병렬 구조 반영.
- 상태: **구조 참고 문서 (Mission 패키지·서브네임스페이스 갱신)**

### 7) Mission Data Model
- `기획서/MISSION_DATA_MODEL.md`
- 역할: `res/mission/` 코드 구조와 세력 설계(`기획서/MISSION_FACTIONS.md`) 간 매핑; 갭 분석 및 다음 데이터 진화 계획.
- 상태: **코드/설계 정합 참고 문서**

### 8) Docs Maintenance Policy
- `DOCS_POLICY.md`
- 역할: 캐노니컬 문서 정의, 업데이트 규칙, stale/archive 처리 기준
- 상태: **운영 정책 문서**

---

### 9) Wiki (Generated)
- `wiki/README.md` — 전체 위키 인덱스
- 역할: 게임플레이 규칙, 시스템 레퍼런스, 아키텍처, 콘텐츠 추가 가이드, 개발 규칙, 구현 이력을 한 곳에서 탐색 가능한 생성 위키.
- 상태: **자동 생성 레퍼런스 (소스 기반)**

---

## Reference / Legacy Docs

### `ProjectFR_ReworkPlan.md`
- 역할: 예전 리워크 초안을 가리키는 포인터 문서
- 상태: **아카이브 포인터**
- 비고: 핵심 내용은 `기획서/PROJECT_REDESIGN_STEPS.md`에 흡수됨

---

## Suggested Rule Going Forward

새 문서를 만들기 전에 아래 분류 중 하나인지 먼저 확인한다.

- 코어 비전 변경 -> `기획서/GAME_CONCEPT.md` 업데이트
- 세계관/서사 변경 -> `기획서/STORY_WORLD.md` 업데이트
- 세력/의뢰 구조 변경 -> `기획서/MISSION_FACTIONS.md` 업데이트
- 구현 진행/다음 단계 변경 -> `기획서/PROJECT_REDESIGN_STEPS.md` 업데이트
- 특정 기능의 깊은 설계 -> `기획서/` 하위에 별도 문서 추가

새 문서를 추가해야 할 때는:
- 기능 단위 문서만 만든다
- 비슷한 목적의 계획 문서를 중복 생성하지 않는다
- 추가 후 이 `README.md` 인덱스에 바로 등록한다
