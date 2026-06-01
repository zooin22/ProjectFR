# Changelog — TODO 완료 기반 구현 이력

> `docs/TODO.md`의 완료 항목을 기반으로 작성.
> 최종 업데이트: 2026-06-01

---

## Architecture · Bug · Campaign · Content · UX (2026-06-01 — TODO 3차)

### [Architecture] InfiltrationManager.OnOperationCompleted에 순수 상태 변경 이관
- `FileOperation`에 `NodeKind`, `NodeSize`, `CompletionNotes` 속성 추가
- `InfiltrationManager.OnOperationCompleted()`에 Copy / Cut / Paste / RewriteLog 핸들러 추가:
  - **Copy**: `TryCopyToClipboard(path, kind, size)` → 성공/실패를 `CompletionNotes`에 기록
  - **Cut**: `TryCopyToClipboard(path, kind, size)` → InfiltrationState 클립보드 동기화
  - **Paste**: `State.Clipboard.Remove(pasted)` → 클립보드 항목 제거
  - **RewriteLog**: `ReduceTrace()` + `ClearTrackedPath()` (x2) + `ClearScanPressure()` (x2) + `TryClearDetection()` → 각 효과를 CompletionNotes에 기록
- `BattleScene.ExecuteQueuedCommandEntry()`에서 FileOperation 생성 시 `NodeKind`, `NodeSize` 설정
- `BattleScene.ProcessCompletedOperations()`에서 상태 변경 코드 제거, `CompletionNotes`로 UI 응답 구동
- 규칙: InfiltrationManager는 상태 변경, Scene은 UI + MissionProgress 갱신만 담당

### [Architecture] BattleManager BattleLog → InfiltrationState EventLog 전환
- `AddOperationLog(string msg)` 헬퍼 추가: `_battleManager.AddLog()` + `_infiltrationManager.State.AddLog()` 동시 호출
- `UpdateOperationLog()`의 소스를 `_battleManager.BattleLog` → `_infiltrationManager.State.EventLog`로 교체
- 미션 시작/테마/목표/액션 결과/미션 완료/피해 메시지를 `AddOperationLog`로 통일
- BattleManager는 HP 추적 · IsBattleEnd · IsPlayerAlive · StatusEffects(액션 조건용) 전용으로 격리

### [Bug] ExtractPathFromLogEntry 경로 추출 개선
- `"res://"` 외에 `"root/"` 접두사도 탐색 — 경로 포맷 변경에 대응
- 종료 구분자에 `(` · `:` 추가로 `(3T) ::` 형태 로그에서 경로 과추출 방지
- 추출 후 TrimEnd에 `/` 추가해 루트 경로 말미 슬래시 제거

### [Campaign] 미션 종료 오버레이에 세력 평판 변화 행 추가
- `UpdateBattleEndOverlay()`에서 세력 이름 · 의뢰인 · 평판 델타 · 현재 평판을 한 줄로 표시
  예: `세력: Corporate Espionage (Morrow Proxy) | 평판 +1 → 2`
- `CampaignState.GetFactionReputation(result.Mission.Client.FactionId)` 호출로 최신 평판 반영

### [Content] AiMonitor를 SeedSecurityAgents()에 추가
- `AiMonitor` 에이전트가 `CacheTempPath` → `BossZipPath` 경로를 순찰
- 행동(CursorCrossedAiMonitor, FolderNavigationAiMonitor, SearchSweepAiMonitor), 아이콘(ai_monitor.svg), 뱃지([AI])가 모두 연결됨

### [UX] Temp Window ItemList 전환 — 노드 선택 상호작용 추가
- `BattleScene.tscn`에서 `TempWindowItemsLabel` (RichTextLabel) → `TempWindowItemList` (ItemList)로 교체
- `_tempWindowItemList.ItemSelected += OnTempWindowItemSelected` 핸들러 연결
- 항목 클릭 시 `_selectedNodePath` 갱신 + `AppendConsoleFeed` + `UpdateUi()` 호출
- 항목 메타데이터에 노드 경로 저장, 현재 선택 경로와 일치하는 항목 자동 선택 표시

### [UX] 키보드 단축키 추가
- `BattleScene._Input()`에 `InputEventKey` 핸들러 추가:
  - `Enter` / `KpEnter` → Execute Queue (큐가 비어있지 않을 때)
  - `Delete` → 선택 노드에 Delete 액션 큐 적재
  - `Backspace` → Navigate Up (상위 폴더로)
  - `F5` → 현재 컨테이너 새로 고침
- `ShowConsoleHelp()`에 단축키 목록 출력 행 추가

---

## Architecture · Campaign · Security · Content (2026-06-01 — TODO 2차)

### [Architecture] CursorAgent.ActionPoints AP 게이트 구현
- `ExecuteQueuedCommandEntry()` 진입 시 `CursorAgent.ActionPoints` 부족이면 해당 entry 스킵
- 즉시 실행 액션: `SyncCursorApToPlayer()` 후 `action.Execute()` → 성공 시 Player.CurrentAp → CursorAgent.ActionPoints 역동기화
- 지연 작업(Copy / Compress / RewriteLog): action.Execute() 없이 `cursor.ActionPoints` 직접 차감
- `BattleManager.PlayerAction()` 호출 제거 — 직접 `action.Execute(context)` + AP 동기화 패턴으로 교체
- `SyncCursorApToPlayer()` 헬퍼 추가; `UpdateActionButtons()`, `BuildContextActionIds()`, `CanExecuteImmediateOperation()`, `QueueSelectedCommand()` 등 ActionContext 생성 전 호출

### [Campaign] ConflictGroup 뮤텍스 적용
- `CampaignState.IsMissionAvailable()`에 ConflictGroup 체크 추가
- 같은 `ConflictGroup` 내 다른 미션이 `_completedMissionIds`에 있으면 false 반환
- 예: `"readme_conflict"` 그룹의 "Loose End Cleanup" 완료 시 "Mirror Snatch" 잠금

### [Campaign] 완료 미션 시각적 구분
- `CampaignState.IsMissionCompleted(missionId)` 공개 메서드 추가
- `MainMenu.RefreshMenu()`에서 완료 미션 제목 앞에 `(완료)` 접두어 표시

### [Campaign] 빈 가용 미션 목록 방어
- `GetSelectedMission()` / `SelectNextMission()` / `SelectPreviousMission()` 에서
  필터 후 가용 미션이 0개면 `_missionBoard` 전체로 폴백
- `EnsureInitialized()`도 동일 폴백 적용

### [Security] IsDetected 해제 수단 추가
- `InfiltrationManager.TryClearDetection(reason)` 추가
- 조건: `CursorAgent.IsDetected == true` && `Trace <= InfiltrationTuning.DetectionClearTraceThreshold(40)`
- 트리거: `RewriteLog` 완료 (`ProcessCompletedOperations`) 또는 `CleanAction` 성공 (`ExecuteImmediateAction`)
- `InfiltrationTuning.DetectionClearTraceThreshold = 40` 상수 추가

### [Security] Quarantine/Purge 단계 커서 수렴 이동
- `AdvanceSecurityAgents()`에 `AlertStage >= Quarantine` 수렴 패턴 추가
- 순찰 경로에 커서 경로 포함 시: 경로 인덱스를 한 칸 커서 방향으로 전진
- 순찰 경로 밖이면: 커서 위치로 직접 이동
- 에이전트 `AwarenessStage`를 현재 `AlertStage`로 갱신

### [Content] SeedSecurityAgents 미션 목표 경로 연동
- `EnsureMissionTargetPatrolled(agents, targetPath)` 헬퍼 추가
- `MissionData.TargetPath`가 어떤 에이전트 순찰 경로에도 없으면,
  가장 긴 공통 조상 경로를 순찰하는 에이전트 끝에 `TargetPath` 추가
- 미션마다 목표 경로 경비 밀도 자동 차별화

---

## 버그 수정 · 보안 에이전트 · 캠페인 · UI (2026-06-01)

### [Bug] BattleFactory에 system.log 노드 추가
- `BattleFactory.CreateDefaultDungeon()`의 `BuildCache` 폴더에 `system.log` FileNode 추가
- `mission_modify_syslog` 미션의 `MissionObjectiveType.Modify` 목표가 이제 실제로 달성 가능
- `BattleConstants.SystemLogPath` (`res://BuildCache/system.log`), 크기 6, 위협도 Medium

### [Security] AntivirusHeavy · BackupRepairer 행동 파이프라인 구현
- `SecurityBehaviorKeys`에 6개 신규 키 추가:
  `CursorCrossedAntivirusHeavy`, `CursorCrossedBackupRepairer`,
  `FolderNavigationAntivirusHeavy`, `FolderNavigationBackupRepairer`,
  `SearchSweepAntivirusHeavy`, `SearchSweepBackupRepairer`
- `SecurityBehaviorFactory`에 6개 빌더 구현:
  - **AntivirusHeavy** — 커서 감지 시 Quarantine/Purge 급 에스컬레이션, 목표 경로 ForcedLock 적용
  - **BackupRepairer** — 커서 감지 시 Suspicious/ActiveScan, 항상 ScanPressure 적용 (3턴)
- `InfiltrationManager.ResolveSecurityBehaviorKey()`에 두 타입 분기 추가
- `BattleScene.SeedSecurityAgents()`에 BackupRepairer 추가 (`system.log` 경로 순찰)
- `SecurityBehaviorTuning`에 7개 튜닝 상수 추가

### [Campaign] CampaignState.Reset() + New Game 버튼
- `CampaignState.Reset()` 정적 메서드 추가: Credits/Reputation/Heat/completedMissions/factionReputation 전체 초기화 후 `EnsureInitialized()` 재호출
- `main.tscn`에 "새 게임" 버튼 추가 (`PrimaryButtonRow` 내)
- `MainMenu.cs`에 `OnNewGamePressed()` 핸들러 연결 → `CampaignState.Reset()` 후 메뉴 갱신

### [Campaign] 액션 버튼 enablement 분리
- `BattleScene.UpdateActionButtons()`의 게이팅 조건을 `_battleManager.CurrentState == BattleState.PlayerTurn`에서 `_infiltrationManager.State.RunStatus == RunStatus.Active`로 교체
- 턴 레이블 상태 표시도 `BattleManager.CurrentState` → `InfiltrationState.RunStatus`로 교체

### [UI] 턴 제한 근접 시각적 경고
- `UpdateUi()`에서 `TurnCount >= EffectiveTurnLimit - 2`이면 `_turnStateLabel` · `_traceBar` 색상을 빨간색 (`#FF5959`)으로 변경
- 정상 상태에서는 기본 흰색으로 복원

### [UI] 드래그 호버 대상 하이라이트
- `RebuildExplorerField()`에서 `_dragHoverTargetPath`와 일치하는 항목에 `SetItemCustomBgColor(파란색 반투명)`으로 배경 강조
- 마우스 릴리즈 전에도 드롭 대상이 시각적으로 구분됨

### [Architecture] BattleManager 의존성 분석 및 부분 분리
- `BattleScene.cs`에 `// TODO(battle-removal):` 블록으로 남은 의존성 목록 문서화
  (Player HP/AP, StatusEffects, Enemies, BattleLog, PlayerAction, FinishBattle, LoadEncounter)
- 이미 분리된 항목: 액션 버튼 게이팅(RunStatus), 턴 레이블 상태(RunStatus)

### [Content] BattleFactory 미션별 던전 변형
- `BattleFactory.CreateDefaultDungeon(MissionData? mission = null)` 파라미터 추가
- `ApplyMissionVariants()` 메서드로 미션 ID별 노드 프로파일 조정:
  - `mission_delete/extract_readme`: Readme.txt → High 위협, HP+3, AP+1
  - `mission_modify_syslog`: system.log → High 위협, HP+2, AP+1 + `audit_snapshot.dat` 추가
  - `mission_extract/delete_boss`: Boss.zip → Critical 위협, HP+4, AP+1
  - `mission_scan_cache`: BuildCache에 `index.db` · `scan_queue.tmp` 추가
  - `mission_escape_only`: 전체 노드 HP -2 경량화
- `BattleScene.InitializeBattle()`에서 `CreateDefaultDungeon(_currentMission)` 전달

---

## Multi-Window Expansion (Step 7)

### Temp Window 구현
- `ExplorerWindowType.TempFolder` 추가
- `InfiltrationManager`에 `TempFolder` 열기/닫기 API 추가
- `BattleScene` 창 툴바에 "Temp" 버튼 추가
- temp 폴더 노드 목록을 서브패널에 렌더링
- 창 열기 시 Trace +1 (다른 창과 동일한 트레이스 규칙 적용)

### Log Viewer Window 구현
- `ExplorerWindowType.LogViewer` 추가
- `InfiltrationManager`에 `OpenLogViewerWindow()` / `CloseLogViewerWindow()` API 추가
- `BattleScene`에 "Log" 버튼 추가
- `InfiltrationState.EventLog` 항목을 패널에 렌더링
- Log Viewer 창에서 선택한 로그 항목에 `LogForge` 큐 적재 가능

### 멀티 윈도우 병렬 작업 Trace 비용 구현
- `InfiltrationManager.ProcessTurn()` (AdvanceTurn 내) 에서 `ApplyMultiWindowParallelOperationTrace()` 호출
- Running 중인 작업이 2개 이상의 창 BoundPath에 걸쳐 있으면, 초과 창 수당 Trace +1/턴
- 해당 상수: `InfiltrationTuning.MultiWindowParallelOperationTraceCostPerWindow = 1`

---

## File Operation & Security (Steps 4–5)

### 발각 시 작업 중단 구현
- `InfiltrationManager.InterruptMonitoredOperationsOnDetection()` 추가
- `AdvanceTurn()` 내 보안 처리 후 `CursorAgent.IsDetected`가 true로 전환되는 시점에 호출
- Running 중이면서 감시 대상 노드의 작업을 `OperationStatus.Failed`로 처리
- 중단 이유를 EventLog에 기록

### 보안 에이전트 위치 뱃지 렌더링
- `BattleScene.RefreshExplorerField()`에서 각 표시 노드를 SecurityAgent 목록과 대조
- 에이전트 점유 노드에 `[SCOUT]`, `[AI]`, `[GUARD]`, `[FIREWALL]` 등 컴팩트 뱃지 표시
- 탐색기 카드 레이블에 뱃지 텍스트 추가

---

## Mission & Escape Loop (Step 6)

### HeatDelta에 Trace 비율 반영
- `MissionProgress.Resolve()` 수정: `trace / InfiltrationTuning.TracePerHeatPoint` 계산
- 성공 시: `traceHeat` 추가, 실패 시: `FailureHeat + traceHeat` 추가
- `InfiltrationTuning.TracePerHeatPoint = 10` 상수 추가

### Modify/Escape 타입 미션 추가
- `MissionBoardFactory.CreateDefaultBoard()`에 두 미션 추가:
  - `mission_modify_log`: Glass Key Collective 의뢰, `MissionObjectiveType.Modify`, 로그 파일 대상 (logforge 경로 테스트)
  - `mission_escape_run`: Ember Circuit 의뢰, `MissionObjectiveType.Escape`, 파일 타깃 없음 (순수 탈출 런)

---

## Mission Data Model

### ConflictGroup 필드 추가
- `MissionData`에 `string? ConflictGroup` 필드 추가
- `mission_extract_readme`와 `mission_delete_readme` 양쪽에 `"readme_conflict"` 설정
- `MainMenu.RefreshMenu()`에서 같은 ConflictGroup 내 활성 미션이 있으면 충돌 안내 표시

### 세력 평판 요건 연결
- `MissionData`에 `int? RequiredFactionReputation` 필드 추가
- `CampaignState.IsMissionAvailable()`에 평판 체크 추가:
  `GetFactionReputation(mission.Client.FactionId) < mission.RequiredFactionReputation`이면 false 반환
- `MissionBoardFactory`의 특정 미션에 비-0 값 설정으로 동작 검증

---

## UI Layout (Step 3)

### 우측 패널 Security 섹션 추가
- `BattleScene` 우측 패널에 "Security" 전용 섹션 구현
- 각 활성 `SecurityAgent` 표시: 타입, 인식 상태(AwarenessStage), 현재 경로
- 각 활성 `FileOperation` 표시: 타깃 경로, 진행률(%), 상태
- 기존 `FieldSecurityLabel` 요약 텍스트를 이 섹션으로 대체
