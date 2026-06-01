# ProjectFR - Design Document Index

> Status: active — 전체 기획 문서의 진입점(허브)
> Subtitle Candidate: **No Tail Signal**
> One-liner: 꼬리 없는 해커 햄스터가 파일 탐색기를 해킹해, 삭제된 존재들의 기록을 되찾는 로그라이크.

---

## 0. 이 문서의 목적

ProjectFR의 기획 문서는 "비전 → 시스템 → 데이터/구현"의 3계층으로 나뉜다.
이 인덱스는 각 문서가 무엇을 담당하는지, 어떤 순서로 읽어야 하는지, 그리고 문서 간 용어가
서로 어긋나지 않도록 하는 **단일 진실 출처(single source of truth)의 지도**다.

새 기능을 설계할 때는 항상 다음 순서를 따른다.

```text
GAME_CONCEPT (왜) → 시스템 기획서 (무엇을/어떻게) → DATA_MODEL (코드 형태) → REDESIGN_STEPS (구현 진행)
```

### 0.1 폴더 구조

```text
ProjectFR_docs/
├── 00_DESIGN_INDEX.md          ← 이 문서 (허브)
├── 01_vision/                  비전 계층 (왜/무엇)
│   ├── GAME_CONCEPT.md
│   ├── STORY_WORLD.md
│   └── MISSION_FACTIONS.md
├── 02_systems/                 시스템 계층 (어떻게)
│   ├── COMBAT_SYSTEM_DESIGN.md
│   ├── SECURITY_AI_DESIGN.md
│   ├── EXPLORER_INTERACTION_DESIGN.md
│   ├── LOBBY_MISSION_DESIGN.md
│   ├── PROGRESSION_ECONOMY_DESIGN.md
│   ├── LEVEL_GENERATION_DESIGN.md
│   ├── RANDOMNESS_DESIGN.md
│   ├── UI_UX_DESIGN.md
│   └── MULTI_WINDOW_EXPANSION.md
└── 03_data_impl/               데이터·구현 계층
    ├── MISSION_DATA_MODEL.md
    └── PROJECT_REDESIGN_STEPS.md
```

> 문서 간 상호 참조는 폴더 경로 없이 **파일명만** 표기한다(이식성). 위 트리에서 위치를 찾는다.

---

## 1. 문서 맵

### 1.1 비전 계층 (Vision) — `01_vision/`

| 문서 | 역할 | 핵심 질문 |
|---|---|---|
| `GAME_CONCEPT.md` | 코어 비전 캐논 | 이 게임은 무엇인가? |
| `STORY_WORLD.md` | 스토리·세계관 캐논 | 왜 햄스터가 해킹하는가? |
| `MISSION_FACTIONS.md` | 의뢰인 세력·톤 | 누가 의뢰하는가? |

### 1.2 시스템 계층 (System Design) — `02_systems/`

| 문서 | 담당 시스템 | 다루는 범위 |
|---|---|---|
| `COMBAT_SYSTEM_DESIGN.md` | 침투 전투 코어 | 파일 작업 진행률, 명령 큐, AP, Trace 상호작용, 턴 흐름 |
| `SECURITY_AI_DESIGN.md` | 보안 모듈 AI | 8종 모듈, 5단계 반응, 행동 트리, 순찰·감시·추적 |
| `EXPLORER_INTERACTION_DESIGN.md` | 탐색기 조작 | 포인터 전용 조작, 우클릭, 드래그, 검색(클릭), 속성창 |
| `LOBBY_MISSION_DESIGN.md` | 로비·미션 보드 | 의뢰 선택, 브리핑, 출격 준비, 미션 구조, 목표 타입 |
| `PROGRESSION_ECONOMY_DESIGN.md` | 재화·메타·빌드 | Credits/Reputation/Heat, 로그라이크 루프, 빌드 아키타입 |
| `LEVEL_GENERATION_DESIGN.md` | 레벨 생성 | 폴더 구조 절차 생성, 노드 역할 배치, 함정·자산 |
| `RANDOMNESS_DESIGN.md` | 운·무작위성 | RNG 4계층, 행동 판정, 보상 드래프트, 변동성 완화 |
| `UI_UX_DESIGN.md` | UI·UX | 화면 레이아웃, 데스크톱 오버레이, 멀티윈도우 표현 |
| `MULTI_WINDOW_EXPANSION.md` | 멀티윈도우 (기존) | 서브창 단계 확장 — UI_UX와 함께 읽음 |

### 1.3 데이터·구현 계층 (Data & Implementation) — `03_data_impl/`

| 문서 | 역할 |
|---|---|
| `MISSION_DATA_MODEL.md` | 미션 시스템 코드 형태(클래스/필드) 정렬 기준 |
| `PROJECT_REDESIGN_STEPS.md` | 구현 진행 로그 (Step 1~7) |

---

## 2. 코어 루프 (Core Loops)

ProjectFR은 세 개의 중첩된 루프로 돌아간다. 모든 시스템 기획서는 이 루프 중
하나 이상에 기여하도록 설계된다.

### 2.1 작전 루프 (Operation Loop) — 초 단위

한 번의 명령 실행 사이클. `COMBAT_SYSTEM_DESIGN.md`의 핵심.

```text
선택 → 명령 큐 적재 → 실행 → 파일 작업 진행 → 보안 반응 → Trace 정산 → 다음 턴
```

### 2.2 침투 루프 (Run Loop) — 분 단위

미션 1회. `LEVEL_GENERATION` + `COMBAT` + `SECURITY_AI`가 맞물린다.

```text
브리핑 → 침투 → 목표 탐색 → 작업 수행/흔적 관리 → Exit 해금 → 탈출 → 결과 정산
```

### 2.3 캠페인 루프 (Campaign Loop) — 시간 단위

여러 미션에 걸친 메타 진행. `LOBBY` + `PROGRESSION_ECONOMY`가 담당.

```text
로비 → 의뢰 선택 → 빌드 준비 → 침투 루프 → 보상/평판/Heat 정산 → 빌드 갱신 → 다음 의뢰
```

---

## 3. 핵심 자원 한눈에 보기

| 자원 | 범위 | 담당 문서 | 한 줄 정의 |
|---|---|---|---|
| **AP / 명령 슬롯** | 턴 | COMBAT | 한 턴에 예약 가능한 명령 수 |
| **Trace** | 런 | COMBAT / SECURITY_AI | 이번 작전에서 시스템이 나를 포착한 정도 |
| **Credits** | 캠페인 | PROGRESSION_ECONOMY | 의뢰 보상 화폐 |
| **Reputation** | 캠페인 | PROGRESSION_ECONOMY | 전역 + 세력별 신뢰도 |
| **Heat** | 캠페인 | PROGRESSION_ECONOMY | 당국이 햄스터 공동체를 위험 집단으로 보는 정도 |
| **Clipboard** | 런 | EXPLORER_INTERACTION | 복사한 파일의 임시 운반 공간 |
| **Cheek Pouch Cache** | 런 | EXPLORER_INTERACTION | 작은 파일을 낮은 Trace로 숨기는 볼주머니 슬롯 |

---

## 4. 용어집 (Glossary)

문서 전체에서 동일한 의미로 쓰는 용어. 새 용어는 여기 먼저 등록한다.

| 용어 | 정의 |
|---|---|
| **Tail Signal** | 마우스의 꼬리가 내는 구식 유선 커서 신호. 시민권·인증·접근 권한의 기준. |
| **Wireless Cursor** | 햄스터가 쓰는 무선 커서 신호. 짧은 펄스, 추적 회피, 비표준 입력. |
| **CursorAgent** | 플레이어 본체. 탐색기 위의 커서 + 해커 햄스터로 시각화. |
| **SecurityAgent** | 보안 모듈. 시스템 프로세스가 캐릭터처럼 의인화된 적 유닛. |
| **ExplorerNode** | 파일/폴더 노드. 역할(Role)에 따라 목표·자산·함정·환경이 됨. |
| **FileOperation** | 진행률을 가진 파일 작업(복사/삭제/압축/로그 위조 등). |
| **CommandQueue** | 여러 명령을 예약 후 일괄 실행하는 계획형 큐. |
| **Trace** | 런 단위 발각도. 5단계 보안 반응을 견인. |
| **Heat** | 캠페인 단위 장기 추적도. 난이도 티어를 견인. |
| **Ghost** | 볼주머니에 숨겨져 일부 스캔/검색에서 제외된 파일 상태. |
| **Tracked** | 보안 반응으로 표식된 노드/폴더. 작업 시 추가 Trace. |
| **CleanSweep** | 오래된·미등록 파일을 삭제 대기열로 보내는 자동화 정책(보안 반응 최종 단계). |
| **OPTIMA** | 인터넷 세계 관리 AI. 최종 보스. "삭제가 아니라 정리"라고 믿음. |
| **Deleted Being** | 오래된 사진·메모·세이브 등 감정 가치를 가진 기억 파일 생명체. |

---

## 5. 톤 & 표기 규칙

- 본문은 한국어 산문, 기술 용어는 영문 그대로(예: Trace, FileOperation).
- 게임 내 표시 텍스트 예시는 코드 블록으로 감싼다.
- 밸런싱 수치는 **플레이스홀더**이며 `[balance]` 의도가 보이도록 표기한다. 최종 수치는 튜닝 단계에서 확정.
- 각 시스템 문서는 상단에 `Status` / `Read with` / `Owns(이 문서가 정의하는 것)`를 명시한다.

### 5.1 핵심 설계 결정 (전 문서 공통)

다음 두 결정은 모든 시스템 문서가 따른다.

| 결정 | 내용 | 영향 문서 |
|---|---|---|
| **포인터 전용 (No-Keyboard)** | 전투가 계획형 턴제이므로 타이핑 없이 마우스만으로 모든 조작을 완결한다. 검색어·경로·파일명 직접 입력은 클릭 가능한 선택지(스윕/필터 칩/수집 키워드/브레드크럼/바로가기)로 환원한다. | EXPLORER_INTERACTION §1.1, COMBAT §2, UI_UX |
| **운은 양념, 실력이 뼈대** | 로그라이크 무작위성을 도입하되 확정 작업이 뼈대이고 운은 부가 선택지에만. 모든 판정 확률은 커밋 전 공개하며 불운 보호 장치를 둔다. | RANDOMNESS 전반, COMBAT §6.4, PROGRESSION |

---

## 6. 권장 읽기 순서

1. 처음 합류한 사람: `GAME_CONCEPT` → `STORY_WORLD` → 이 인덱스 → 관심 시스템 문서
2. 시스템 설계자: 이 인덱스 §2 코어 루프 → 담당 시스템 문서 → 인접 문서의 "경계(Interfaces)" 절
3. 구현 담당: 시스템 문서 → `MISSION_DATA_MODEL` → `PROJECT_REDESIGN_STEPS`
