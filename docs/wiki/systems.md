# Systems — 침투 런타임, 창 시스템, 파일 작업, 보안 반응

## 1. InfiltrationManager

침투 런의 모든 상태를 소유하고 조율하는 핵심 클래스.
`res/infiltration/InfiltrationManager.cs`

```
InfiltrationManager
  ├── InfiltrationState State        — 모든 런 상태 데이터
  ├── List<SecurityAgent> SecurityAgents
  ├── MissionData Mission
  └── SecurityBehaviorExecutor       — 보안 반응 실행기
```

### 주요 API

| 메서드 | 역할 |
|---|---|
| `Initialize(startPath, knownNodes)` | 런 초기화, 메인 창 생성 |
| `AdvanceTurn()` | 턴 진행 (작업 틱 → 멀티창 Trace → 타이머 틱 → 보안 에이전트 이동) |
| `QueueCommand(entry)` | 명령 큐에 추가 |
| `ExecuteQueuedCommands()` | 큐 전체 실행 → FileOperation 시작 |
| `StartOperation(operation)` | 단일 작업 즉시 시작 |
| `AddTrace(amount, reason)` | Trace 증가 및 AlertStage 재계산 |
| `ReduceTrace(amount, reason)` | Trace 감소 |
| `TryCopyToClipboard(path, kind, size)` | 클립보드에 파일 추가 |
| `TryMoveClipboardToPouch(path, size)` | 클립보드 → 볼주머니 이동 |
| `TryRestoreFromPouch(path)` | 볼주머니 → 클립보드 복원 |
| `MoveCursor(path)` | 커서 이동 + 감시 반응 트리거 |
| `HandleFolderNavigation(path, directJump)` | 폴더 이동 + 감시 반응 |
| `TriggerSearchSweep(path)` | Search 실행 시 IndexerScout/AiMonitor 반응 |
| `GrantPermissionOverride(path, reason, trace, turns)` | 임시 접근 권한 부여 |
| `UnlockExit(reason)` | 탈출 허가 (목표 완료 시 호출) |
| `TryEscape()` | 탈출 시도 |
| `OpenWindow(type, title, path, traceModifier)` | 보조창 열기 |
| `CloseWindow(windowId)` | 창 닫기 (Main 창 제외) |
| `OpenLogViewerWindow()` | Log Viewer 창 열기 |
| `TryClearDetection(reason)` | `IsDetected` 해제 시도 — Trace ≤ `DetectionClearTraceThreshold(40)` 이면 false 로 리셋 |
| `OnOperationCompleted(operation)` | 작업 완료 내부 콜백 — Copy/Cut/Paste/RewriteLog 각 case별 상태 변경 + CompletionNotes 기록 |

---

## 2. InfiltrationState

런 상태를 담는 데이터 컨테이너.
`res/infiltration/InfiltrationState.cs`

| 필드 | 타입 | 역할 |
|---|---|---|
| `CurrentFolderPath` | string | 현재 표시 폴더 |
| `TurnCount` | int | 경과 턴 수 |
| `Trace` | int | 현재 흔적 수치 (0~100) |
| `AlertStage` | SecurityAwarenessStage | 현재 보안 경계 단계 |
| `ExitUnlocked` | bool | 탈출 가능 여부 |
| `RunStatus` | RunStatus | Active / ObjectiveCompleted / Escaped / Failed |
| `ObjectiveState` | ObjectiveState | Hidden / Revealed / Completed |
| `CursorAgent` | CursorAgent | 플레이어 에이전트 상태 |
| `Windows` | List\<ExplorerWindowState\> | 열린 창 목록 |
| `Clipboard` | List\<ClipboardEntry\> | 클립보드 항목 |
| `PouchCache` | List\<ClipboardEntry\> | 볼주머니 캐시 항목 |
| `ExposedPouchPaths` | HashSet\<string\> | 노출된 볼주머니 경로 |
| `PermissionOverrideTurns` | Dict\<string, int\> | 임시 접근 권한 남은 턴 |
| `TrackedPathTurns` | Dict\<string, int\> | 추적 표식 남은 턴 |
| `ForcedLockTurns` | Dict\<string, int\> | 강제 잠금 남은 턴 |
| `ScanPressureTurns` | Dict\<string, int\> | 스캔 압박 남은 턴 |
| `ActiveOperations` | List\<FileOperation\> | 진행 중 작업 목록 |
| `CommandQueue` | List\<CommandQueueEntry\> | 대기 중 명령 큐 |
| `EventLog` | List\<string\> | 이벤트 로그 (최대 100줄) |

---

## 3. 파일 작업 시스템 (FileOperation)

`res/infiltration/FileOperation.cs`

```
FileOperation
  ├── OperationType Type      — MoveCursor / Copy / Compress / RewriteLog / Delete / Stun ...
  ├── string TargetNodePath
  ├── int Progress            — 0 ~ RequiredTicks
  ├── OperationStatus Status  — Pending / Running / Completed / Failed
  ├── bool CompletionHandled
  ├── ExplorerNodeKind NodeKind — 작업 대상 노드 종류 (File / Folder / Archive / …)
  ├── long NodeSize           — 대상 노드 크기 (클립보드 용량 판단에 사용)
  └── List<string> CompletionNotes — 완료 시 상태 문자열 목록 (UI 피드백용)
```

**CompletionNotes 패턴**: `OnOperationCompleted`에서 상태 변경과 함께 notes를 채운다.
BattleScene은 notes를 읽어 콘솔 피드에 표시하며, 상태 변경 자체는 Manager가 담당한다.

BattleScene의 `ExecuteQueuedCommandEntry()`에서 FileOperation 생성 시 `NodeKind`와 `NodeSize`를 설정한다:
```csharp
var operation = new FileOperation(entry.OperationType, selectedNode.Path, requiredTicks, entry.SecondaryTargetPath)
{
    NodeKind = ResolveExplorerNodeKind(selectedNode),
    NodeSize = selectedNode.Size
};
```

### 작업 상태 흐름

```
Pending → Running (Start() 호출) → Completed (Tick()이 RequiredTicks에 도달)
                                  → Failed (강제 중단)
```

### 발각 시 작업 중단

`InterruptMonitoredOperationsOnDetection()` — `CursorAgent.IsDetected`가 true로 바뀌는 순간,
현재 Running 중인 작업 중 감시 노드 대상 작업을 전부 `Failed`로 처리한다.

---

## 4. 명령 큐 (CommandQueue)

플레이어는 바로 실행하지 않고 명령을 예약한다.

```
CommandQueueEntry
  ├── OperationType OperationType
  ├── string PrimaryTargetPath
  ├── string? SecondaryTargetPath
  ├── int Order
  └── string Summary
```

흐름:
1. 액션 버튼 클릭 → `InfiltrationManager.QueueCommand(entry)`
2. "Execute Queue" 버튼 → `ExecuteQueuedCommands()` → 각 entry를 `FileOperation`으로 변환 후 시작
3. `AdvanceTurn()` 시 각 작업 `Tick()` → 완료 처리

**AP 게이트:** `ExecuteQueuedCommandEntry()` 진입 시 `CursorAgent.ActionPoints`를 액션 비용과 비교한다.
AP가 부족하면 해당 entry를 건너뛰고 로그에 기록한다. 즉시 실행 액션과 지연 작업 모두 동일한 AP 소비 규칙이 적용된다.
`CursorAgent.ActionPoints`가 AP의 단일 권위 소스이며, `BattleManager.Player.CurrentAp`는 액션 조건 평가 전에 항상 동기화된다(`SyncCursorApToPlayer()`).

---

## 5. 보안 반응 파이프라인

`SecurityBehaviorKeys` → `SecurityBehaviorFactory` → `SecurityBehaviorExecutor`

### 반응 트리거 3종

| 트리거 | 호출 위치 | 관련 에이전트 |
|---|---|---|
| `CursorCrossedMonitoredNode` | `MoveCursor()` | 감시 중인 에이전트 전체 |
| `FolderNavigation` | `HandleFolderNavigation()` | 해당 폴더의 가시 에이전트 |
| `SearchSweep` | `TriggerSearchSweep()` | IndexerScout, AiMonitor |

### 에이전트 타입별 행동 키 분해

같은 트리거라도 에이전트 타입에 따라 다른 행동 키로 분해된다.

예:
- `CursorCrossedMonitoredNode` + GuardScanner → `CursorCrossedGuardScanner`
- `CursorCrossedMonitoredNode` + IndexerScout → `CursorCrossedIndexerScout`
- `CursorCrossedMonitoredNode` + AntivirusHeavy → `CursorCrossedAntivirusHeavy`
- `CursorCrossedMonitoredNode` + BackupRepairer → `CursorCrossedBackupRepairer`
- `CursorCrossedMonitoredNode` + AiMonitor → `CursorCrossedAiMonitor`
- `FolderNavigation` + FirewallSentinel → `FolderNavigationFirewallSentinel`
- `FolderNavigation` + AntivirusHeavy → `FolderNavigationAntivirusHeavy`
- `FolderNavigation` + AiMonitor → `FolderNavigationAiMonitor`
- `SearchSweep` + AntivirusHeavy → `SearchSweepAntivirusHeavy`
- `SearchSweep` + BackupRepairer → `SearchSweepBackupRepairer`
- `SearchSweep` + AiMonitor → `SearchSweepAiMonitor`

### AntivirusHeavy 반응 요약

| 트리거 | 인식 단계 | 부가 효과 |
|---|---|---|
| Cursor Crossed | 목표 경로 시 Purge, 기타 Quarantine | ForcedLock (목표 경로) |
| Folder Navigation | 목표 경로 시 Purge, 기타 Quarantine | TrackedPath + ForcedLock (목표 경로) |
| Search Sweep | 목표 경로 시 Purge, 기타 Quarantine | ForcedLock + TrackedPath (목표 경로) |

Trace 보너스: Cursor+2, Folder+3, Search+3 (목표 경로 시 +추가).

### BackupRepairer 반응 요약

| 트리거 | 인식 단계 | 부가 효과 |
|---|---|---|
| Cursor Crossed | 목표 경로 시 ActiveScan, 기타 Suspicious | ScanPressure(3턴) 항상 적용 |
| Folder Navigation | 목표 경로 시 ActiveScan, 기타 현재 단계 | TrackedPath (목표 경로) |
| Search Sweep | 목표 경로 시 ActiveScan, 기타 Suspicious | ScanPressure(3턴) 현재 폴더에 적용 |

Trace 보너스: 각 +1 (소폭). 조용하지만 장기적 압박을 가한다.

**`SecurityBehaviorTuning` 신규 상수:**

| 상수 | 값 |
|---|---|
| `AntivirusCursorTraceBonus` | 2 |
| `AntivirusFolderNavigationTraceBonus` | 3 |
| `AntivirusSearchTraceBonus` | 3 |
| `BackupRepairerCursorTraceBonus` | 1 |
| `BackupRepairerFolderNavigationTraceBonus` | 1 |
| `BackupRepairerSearchTraceBonus` | 1 |
| `BackupRepairerScanPressureDurationTurns` | 3 |

### 반응 결과 (지속 효과)

| 효과 | 딕셔너리 | 설명 |
|---|---|---|
| TrackedPath | `TrackedPathTurns` | 추적된 경로 — 해당 경로 작업 시 Trace +2 추가 |
| ForcedLock | `ForcedLockTurns` | 강제 잠금 — FirewallSentinel 반응 시 임시 차단 |
| ScanPressure | `ScanPressureTurns` | 스캔 압박 — 해당 경로 볼주머니 은닉 효과 무력화 |

`RewriteLog` 완료 시 → 해당 노드/폴더의 `TrackedPath` + `ScanPressure` 제거

### 목표 경로 근접도 판단

보안 반응은 `IsObjectiveRoute(path, objectivePath)`로 목표 경로 근처 여부를 판단해
더 공격적인 반응(`ActiveScan / Quarantine / Purge`)을 유발한다.

---

## 6. 멀티 윈도우 시스템

`res/infiltration/ExplorerWindowState.cs`

창은 전술 자원이다. 창이 많아질수록 선택지가 늘지만 Trace 리스크도 커진다.

### 창 타입

| ExplorerWindowType | 역할 |
|---|---|
| Main | 기본 침투 창 (항상 존재, 닫기 불가) |
| Clipboard | 클립보드/볼주머니 관리 |
| TempFolder | 임시 은닉/경유 |
| LogViewer | 이벤트 로그 표시, LogForge 큐 적재 |
| Backup | (미구현) 복원 서비스 대응 |
| Archive | (미구현) 압축 내부 관리 |

### ExplorerWindowState 필드

| 필드 | 설명 |
|---|---|
| `WindowId` | 고유 ID (Guid 기반) |
| `WindowType` | 창 타입 |
| `Title` | 표시 제목 |
| `BoundPath` | 창에 바인딩된 경로 |
| `IsOpen` | 열림 여부 |
| `IsFocused` | 포커스 여부 (한 번에 1개) |
| `SlotIndex` | 창 순서 |
| `TraceModifier` | 열릴 때 발생하는 Trace |

### 멀티 창 병렬 작업 Trace 규칙

`ApplyMultiWindowParallelOperationTrace()` — 매 턴 호출됨.
동시에 Running 중인 작업이 2개 이상의 창 경로에 걸쳐 있으면,
초과 창 수 × `InfiltrationTuning.MultiWindowParallelOperationTraceCostPerWindow(=1)` 만큼 Trace 추가.

---

## 7. InfiltrationTuning — 밸런스 상수

모든 수치는 `res/infiltration/InfiltrationTuning.cs`에 집중.

| 상수 | 값 | 의미 |
|---|---|---|
| `MonitoredOperationTraceIncrease` | 4 | 감시 중 작업 시작 시 Trace 증가 |
| `TrackedActionTraceBonus` | 2 | 추적 경로 작업 시 추가 Trace |
| `PouchHideTraceReduction` | 2 | 볼주머니 은닉 시 Trace 감소 |
| `CursorMonitoredTraceIncrease` | 6 | 커서가 감시 노드 통과 시 |
| `FolderNavigationTraceIncrease` | 3 | 일반 폴더 이동 |
| `DirectFolderJumpTraceIncrease` | 5 | 폴더 직접 점프 |
| `PermissionOverrideTraceIncrease` | 4 | Permission Override 사용 |
| `PermissionOverrideDurationTurns` | 3 | 임시 접근 권한 지속 턴 |
| `ShowHiddenPouchExposeTraceIncrease` | 2 | ShowHidden으로 볼주머니 노출 시 |
| `PermissionOverridePouchExposeTraceIncrease` | 5 | PermissionOverride로 볼주머니 노출 시 |
| `SearchTraceIncrease` | 7 | Search 실행 시 |
| `RewriteLogTraceReduction` | 8 | Rewrite Log 완료 시 |
| `MoveTraceIncrease` | 4 | 파일 이동 시 |
| `ArchiveTraceReduction` | 2 | 압축 시 |
| `MultiWindowParallelOperationTraceCostPerWindow` | 1 | 멀티창 병렬 작업당 추가 Trace |
| `TracePerHeatPoint` | 10 | Trace 10점 = Heat 1점 |
| `DetectionClearTraceThreshold` | 40 | `TryClearDetection()` 성공 조건 — Trace 이 값 이하여야 탐지 해제 |

밸런스 조정은 이 파일만 수정하면 된다. 매직 넘버를 Manager/Scene 코드에 직접 쓰지 않는다.

---

## 8. 액션 시스템

`res/action/`

### 인터페이스 계층

```
IAction
  └── ActionBase (abstract)
        └── 각 구체 Action 구현체
```

| 인터페이스/클래스 | 역할 |
|---|---|
| `IAction` | ActionId, DisplayName, ApCost, Scope, CanExecute(), Execute() |
| `ActionBase` | 공통 보일러플레이트 구현 |
| `IActionCondition` | 실행 조건 단일 predicate |
| `ActionRegistry` | 모든 액션 등록 + GetExecutableActions() 필터 |
| `ActionContext` | 캐스터, 타깃, ClipboardSystem, StatusEffectSystem |
| `ActionResult` | 로그 메시지 + 성공 여부 |
| `ActionIds` | 모든 액션 ID 문자열 상수 |

### 구현된 액션 13종

| ActionId | AP | Scope | 효과 |
|---|---|---|---|
| `open` | 1 | Single | 폴더/파일 열기 |
| `copy` | 1 | Single | 클립보드에 복사 |
| `inspect` | 0 | Single | 파일 정보 확인 |
| `delete` | 2 | Single | 파일 삭제 |
| `cut` | 2 | Single | 잘라내기 |
| `paste` | 2 | Self | 붙여넣기 |
| `quarantine` | 2 | Single | 격리 |
| `compress` | 2 | Single | 압축 (Trace 감소) |
| `search` | 1 | Single | 검색 (Trace +7, IndexerScout 반응) |
| `logforge` | 1 | Single | 로그 위조 (Trace -8) |
| `showhidden` | 1 | Single | 숨김 파일 표시 |
| `permoverride` | 2 | Single | 권한 우회 (임시 접근) |
| `clean` | 3 | AoE | 광역 정리 |

### 조건

`res/action/conditions/`

| 조건 | 설명 |
|---|---|
| `MinApCondition` | 최소 AP 체크 |
| `TargetAliveCondition` | 타깃 alive 여부 |
| `ClipboardNotEmptyCondition` | 클립보드 비어있지 않은지 |
| `NotStatusCondition` | 특정 상태 효과 없음 |

---

## 9. 던전 생성 — BattleFactory

`res/battle/BattleFactory.cs`

```
BattleFactory.CreateDefaultDungeon(MissionData? mission)
  ↓ 기본 던전 생성 (root, buildCache, assets, bossArchive)
  ↓ mission != null 이면 ApplyMissionVariants() 호출
      - BoostNodeProfile(targetPath, newThreat, hpBonus, apBonus) — 목표 노드 강화
      - ReduceAllNodeProfiles(hpReduction)         — 전체 노드 약화
      - buildCache.AddChild(...)                   — 미션 전용 추가 노드
```

### 기본 던전 구조

```
res://                     (FolderNode · Low)
  Readme.txt               (FileNode · Low · HP 7)
  BuildCache/              (FolderNode · Medium · HP 9)
    Temp.tmp               (FileNode · Medium · HP 8)
    system.log             (FileNode · Medium · HP 7)   ← mission_modify_syslog 대상
    Assets/                (FolderNode · High · HP 11)
      Boss.zip             (ArchiveNode · Critical · HP 12 · boss)
        payload.exe        (FileNode · Critical · HP 9 · boss)
        hook.dll           (FileNode · High · HP 8)
        trace.log          (FileNode · Medium · HP 6)
```

### 미션별 변형

| 미션 | 변형 |
|---|---|
| `mission_delete/extract_readme` | Readme.txt → High · HP+3 · AP+1 |
| `mission_modify_syslog` | system.log → High · HP+2 · AP+1 + `audit_snapshot.dat` 추가 |
| `mission_extract/delete_boss` | Boss.zip → Critical · HP+4 · AP+1 |
| `mission_scan_cache` | `index.db` · `scan_queue.tmp` BuildCache 추가 |
| `mission_escape_only` | 전체 노드 HP-2 (경량 런) |

### 보안 에이전트 시드 (SeedSecurityAgents)

| 타입 | 초기 위치 | 순찰 경로 |
|---|---|---|
| IndexerScout | Readme.txt | Readme.txt ↔ BuildCache/ |
| GuardScanner | BuildCache/ | BuildCache/ ↔ Temp.tmp |
| FirewallSentinel | Assets/ | Assets/ |
| AntivirusHeavy | Boss.zip | Boss.zip |
| BackupRepairer | system.log | system.log ↔ BuildCache/ |
| AiMonitor | Boss.zip | Temp.tmp ↔ Boss.zip |

**미션 목표 경로 자동 연동 (`EnsureMissionTargetPatrolled`):**
에이전트 시드 후 `MissionData.TargetPath`가 어떤 에이전트의 순찰 경로에도 없으면,
해당 경로와 가장 긴 공통 조상 경로를 가진 에이전트의 순찰 경로 끝에 `TargetPath`를 추가한다.
미션마다 목표 경로 경비 밀도가 자동으로 달라진다.
