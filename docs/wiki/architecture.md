# Architecture — 코드 구조

## 1. 디렉토리 구조

```
res/
  action/           — 액션 시스템 (IAction, ActionRegistry, 구현체 13종)
  battle/           — 레거시 배틀 프로토타입 (병행 유지, 점진적 대체)
  core/             — GameManager autoload singleton
  data/             — 공유 데이터 모델 (ActorState, NodeData, TargetType)
  infiltration/     — 침투 런타임 (InfiltrationManager, SecurityAgent 등)
  mission/          — 미션 시스템 (MissionData, CampaignState, MissionProgress)
  scenes/           — UI 씬 (BattleScene, MainMenu, TitleScene)
  skills/           — 확장성용 스킬 스캐폴딩
  systems/          — 공유 게임 시스템 (ClipboardSystem, StatusEffectSystem)
docs/
  wiki/             — 이 위키
  *.md              — 설계 문서
```

---

## 2. 네임스페이스 맵

| 네임스페이스 | 디렉토리 |
|---|---|
| `ProjectFR.Action` | `res/action/` |
| `ProjectFR.Battle` | `res/battle/` |
| `ProjectFR.Core` | `res/core/` |
| `ProjectFR.Data` | `res/data/` |
| `ProjectFR.Data.Nodes` | `res/data/nodes/` |
| `ProjectFR.Infiltration` | `res/infiltration/` |
| `ProjectFR.Mission` | `res/mission/` |
| `ProjectFR.Scenes` | `res/scenes/` |
| `ProjectFR.Skills` | `res/skills/` |
| `ProjectFR.Systems` | `res/systems/` |

---

## 3. 주요 클래스 관계도

```
GameManager (Autoload Singleton)
  └── ClipboardSystem
  └── StatusEffectSystem
  └── CurrentBattle : BattleManager (레거시)

BattleScene (UI — 두 레이어를 동시에 구동)
  ├── BattleManager (레거시 턴제 배틀)
  │     └── ActorState (Player/Enemy)
  │     └── ActionRegistry
  └── InfiltrationManager (활성 런타임)
        ├── InfiltrationState
        │     ├── CursorAgent
        │     ├── List<ExplorerWindowState>
        │     ├── List<ClipboardEntry>  (Clipboard + PouchCache)
        │     ├── List<FileOperation>   (ActiveOperations)
        │     ├── List<CommandQueueEntry>
        │     └── EventLog
        ├── List<SecurityAgent>
        │     └── SecurityBehaviorExecutor
        └── MissionData
              └── MissionProgress (런 중 추적)
```

---

## 4. 씬 구조

### TitleScene
게임 시작 화면. 메인 메뉴로 전환.

### MainMenu (`res/scenes/MainMenu.cs`)
미션 보드 UI. `CampaignState`에서 가용 미션 목록을 읽어 표시.
- 미션 전환: `SelectNextMission()` / `SelectPreviousMission()`
- 시작: `BeginSelectedMission()` → BattleScene 전환
- 표시: 제목, 의뢰인, 세력, 브리핑, 목표, 보상/패널티, Operator 상태
- 충돌 미션(`ConflictGroup`) 표시 포함

### BattleScene (`res/scenes/BattleScene.cs`)
실제 침투 런을 실행하는 메인 UI 씬.

레이아웃:
- **중앙 (ExplorerField)**: 큰 아이콘 보기 형태의 전술 공간. 파일/폴더를 카드로 표시.
  보안 에이전트 뱃지(`[SCOUT]`, `[AI]` 등) 오버레이.
- **우측 패널**: 선택 노드 인스펙터, Trace/AlertStage, 보안 에이전트 목록, 진행 중 작업 목록
- **하단 패널**: 액션 바, 명령 큐 표시, Execute/Clear Queue 버튼, 창 전환 버튼
- **보조 창**: Clipboard Window, Temp Window (`ItemList` 컨트롤 — 항목 클릭 시 `_selectedNodePath` 갱신), Log Viewer Window (독립 내부 창)
- **키보드 단축키**: `Enter` (Execute Queue), `Delete` (Delete 큐 적재), `Backspace` (Navigate Up), `F5` (새로 고침)

---

## 5. 레거시 vs 활성 레이어

현재 두 레이어가 병행 실행된다.

| 레이어 | 위치 | 상태 |
|---|---|---|
| **Battle Layer** (레거시) | `res/battle/` | 유지 중, 점진적 대체 예정 |
| **Infiltration Layer** (활성) | `res/infiltration/` | 새 기능은 여기에 |

BattleScene이 두 레이어를 동시에 구동하며, 새 기능은 모두 Infiltration Layer에 추가한다.
레거시 Battle Layer는 아직 제거하지 않은 상태.

**AP 권위 소스 이관 완료:**
`CursorAgent.ActionPoints`가 실제 AP 게이트로 작동한다.
액션 조건(`MinApCondition`)은 `BattleManager.Player.CurrentAp`를 읽으므로,
BattleScene은 `ActionContext` 생성 전 `SyncCursorApToPlayer()`로 값을 동기화한다.
액션 실행 후에는 Player.CurrentAp(액션이 `ConsumeAp()` 적용 완료) → CursorAgent.ActionPoints 로 역동기화된다.
지연 작업(Copy / Compress / RewriteLog)은 action.Execute() 없이 직접 `cursor.ActionPoints`를 차감한다.

---

## 6. 보안 반응 파이프라인 구조

`res/infiltration/Security*.cs`

```
SecurityBehaviorKeys   — 행동 키 상수 문자열
SecurityBehaviorFactory — 키에 대응하는 SecurityBehaviorNode 생성
SecurityBehaviorExecutor — TryExecute(key, context) 호출
SecurityBehaviorNode    — 개별 반응 로직 단위
SecurityBehaviorContext — 경로, 에이전트, 커서, 목표 경로, 콜백 등 컨텍스트
SecurityBehaviorTuning  — 보안 행동 관련 밸런스 상수
```

---

## 7. 스킬 스캐폴딩 (Skills Layer)

`res/skills/` — 복잡한 다단계 전술 스킬을 데이터+행동 조합으로 구현하기 위한 확장 기반.

| 클래스 | 역할 |
|---|---|
| `SkillDefinition` | 스킬 메타데이터 |
| `SkillCatalog` | 스킬 등록소 |
| `SkillExecutor` | 스킬 실행 엔진 |
| `SkillBehaviorNode` | BT 스타일 행동 노드 계층 |
| `SkillBehaviorFactory` | 키로 노드 생성 |
| `SkillBehaviorKeys` | Search / ShowHidden / PermissionOverride 행동 매핑 |
| `SkillExecutionContext` | 실행 컨텍스트 |

현재 Search / ShowHidden / PermissionOverride 행동이 매핑되어 있으며, 복잡한 스킬 구현의 기반으로 사용된다.

---

## 8. 미션 시스템 구조

`res/mission/`

```
MissionBoardFactory.CreateDefaultBoard()
  → List<MissionData>

CampaignState (static)
  ├── _missionBoard
  ├── _factionReputation : Dictionary<FactionId, int>
  ├── _completedMissionIds : HashSet<string>
  ├── Credits / Reputation / Heat
  ├── GetAvailableMissions() — PrerequisiteMissionId + RequiredFactionReputation + ConflictGroup 필터
  ├── IsMissionAvailable(mission) — ConflictGroup 뮤텍스 포함: 같은 그룹 내 완료 미션 있으면 false
  ├── IsMissionCompleted(missionId) — 완료 여부 공개 조회
  ├── ApplyMissionResult(result) — Credits/Reputation/Heat/FactionRep 업데이트
  └── GetModifiers() — Heat 티어별 CampaignModifiers

빈 가용 미션 가드: GetSelectedMission() / SelectNextMission() / SelectPreviousMission() 에서
필터 후 가용 미션이 0개면 전체 _missionBoard로 폴백해 최소 하나를 반환한다.

MissionProgress (런 중 추적)
  ├── RegisterAction(actionId, targetPath) — 목표 완료 감지
  ├── RegisterEscape(currentPath) — Escape 타입 완료
  └── Resolve(survived, extracted, turns, turnLimit, trace) → MissionResult
```

---

## 9. 코딩 컨벤션

- **Nullable 활성화**: `<Nullable>enable</Nullable>` — null 처리를 명시적으로.
- **Autoload 접근**: `GameManager.Instance` 만 사용. 씬 코드에서 직접 싱글톤 생성 금지.
- **액션 ID**: `ActionIds` 상수 사용. 문자열 리터럴 사용 금지.
- **밸런스 숫자**: `InfiltrationTuning` / `SecurityBehaviorTuning`에만. 매직 넘버 분산 금지.
- **로그**: BattleScene의 `AddOperationLog(msg)` 헬퍼 사용 → `BattleManager.AddLog` + `InfiltrationState.AddLog` 동시 기록. `UpdateOperationLog()`는 `InfiltrationState.EventLog`를 단일 소스로 사용.
- **시스템 주입**: `ActionContext`를 통해 ClipboardSystem / StatusEffectSystem 수신. 액션 클래스 내부에서 직접 생성 금지.
