# ProjectFR TODO

> Updated: 2026-06-01

## Architecture

- [x] `BattleScene.cs` 상단 `TODO(battle-removal)` 주석에 나열된 의존성을 침투 레이어로 이관하고 `BattleManager` / `BattleScene` 내 legacy 경로를 제거한다: `ActionContext`의 플레이어·클립보드·적 목록을 `InfiltrationState` 기반으로 교체하고, `BattleManager.PlayerAction()` 호출을 `InfiltrationManager.StartOperation()` / `ExecuteImmediateAction()` 흐름으로 일원화한다.
- [x] `CursorAgent.ActionPoints`를 실제 액션 게이트로 사용한다: 현재 `UpdateActionButtons()`와 `ExecuteImmediateAction()`은 `_battleManager.Player.CurrentAp`를 소비하지만 `CursorAgent.ActionPoints`는 표시만 되고 소비되지 않는다 — `ExecuteQueuedCommandEntry()` 진입 시 `CursorAgent.ActionPoints` 차감과 부족 시 스킵 처리를 추가한다.
- [x] `ProcessCompletedOperations()`의 `else` 폴스루를 제거한다: 현재 처리되지 않은 `OperationType`(Stun 등)은 `ExecuteImmediateAction(operation.Type, node)`로 떨어져 `MapOperationTypeToActionId()`가 `null`을 반환하며 콘솔 오류를 낸다 — 명시적 case 분기를 추가하거나 `default: operation.MarkCompletionHandled()` 가드를 넣어 폴스루를 차단한다.
- [x] `res/action/MyAction.cs` 플레이스홀더 파일을 삭제한다: `// Placeholder file kept intentionally empty.` 주석만 남아 있는 빈 파일로 혼란을 준다 — 파일을 제거하고 관련 참조가 없는지 확인한다.
- [x] `InfiltrationManager.OnOperationCompleted`에 Copy/RewriteLog/Cut/Paste 완료 효과를 이관한다: 현재 `BattleScene.ProcessCompletedOperations()`가 Clipboard 동기화·Trace 감소·LogForge 미션 업데이트를 모두 처리하여 Scene 코드가 비대해진다 — 순수 인필트레이션 상태 변경(Clipboard 추가, Trace 감소, Tracked/ScanPressure 정리)을 `InfiltrationManager.OnOperationCompleted`로 이동하고 Scene 레이어는 UI 응답(AppendConsoleFeed, MissionProgress 업데이트)만 남긴다.
- [x] 잔여 `BattleManager` 표면을 `InfiltrationState` 기반으로 교체해 BattleManager를 HP 추적·게임 종료 전용으로 격리한다: `_battleManager.StatusEffects`(인스펙터/상태 표시용), `_battleManager.BattleLog`(오퍼레이션 로그 UI 소스), `_battleManager.Clipboard`(ClipboardSystem ↔ InfiltrationState.Clipboard 이중 유지)를 정리하고, `ActionContext`에 BattleManager 의존을 최소화해 추후 BattleManager 완전 제거 경로를 연다.
- [x] `InfiltrationState`에 오퍼레이터 HP 필드를 추가하고 `BattleManager.Player` HP 의존을 제거한다: `ApplyDetectionDamage()`·`ApplyMissionFailureChecks()`·`_playerStatusLabel` 갱신이 모두 `_battleManager.Player.CurrentHp/MaxHp/TakeDamage()`를 호출한다 — `InfiltrationState`에 `OperatorHp`·`OperatorMaxHp` 속성과 `TakeOperatorDamage(int)` 메서드를 추가하고, `InfiltrationManager.AdvanceTurn()` 흐름 안에서 피해를 적용해 `BattleManager.Player` 의존을 `_playerStatusLabel` 표시용 1곳으로 줄인다.
- [x] `_battleManager.FinishBattle(msg)` / `IsBattleEnd` 를 `InfiltrationState.RunStatus` 기반으로 교체한다: `TryExtractMission`, `ApplyMissionFailureChecks`, `OnBattleEnd`, 각종 Button.Disabled 가드가 모두 `_battleManager.IsBattleEnd`를 폴링한다 — `InfiltrationManager`에 `SetRunFailed(string reason)`·`SetRunTimedOut()` 메서드를 추가해 `RunStatus`를 갱신하고, Scene은 `_infiltrationManager.State.RunStatus != RunStatus.Active` 하나만 확인하도록 변경해 `BattleManager.FinishBattle`을 마지막에 `OnBattleEnd()` 안에서만 호출하도록 좁힌다.
- [x] `ActionContext` 생성 시 `BattleManager.Player/Clipboard/StatusEffects/Enemies` 의존을 `InfiltrationState` 기반으로 교체한다: `QueueSelectedCommand`·`CanExecuteImmediateOperation`·`ExecuteImmediateAction`·`UpdateActionButtons` 등 7개 이상 위치에서 `new ActionContext(_battleManager.Player) { Clipboard = _battleManager.Clipboard, ... }` 패턴이 반복된다 — `ActionContext`에 AP 정수·클립보드 유무 플래그를 직접 받는 생성자 오버로드를 추가하고, `InfiltrationState`·`CursorAgent`를 소스로 사용하도록 교체해 `BattleManager` 의존을 `SyncCursorApToPlayer` 단 하나로 국한시킨다.

## Bug

- [x] `ExtractPathFromLogEntry`가 `res://` 접두사를 찾지만 게임 경로는 `root/...` 형식이다: `BattleScene.cs:2215`의 `logEntry.IndexOf("res://", ...)` 조건이 EventLog의 실제 경로 문자열(`root/build_cache/...` 등)과 일치하지 않아 Log Window의 Logforge 버튼이 항상 비활성화된다 — 경로 추출 패턴을 `root/` 접두사 탐색으로 교체하고, 경로 끝 구분 문자 트림 로직도 검증한다.
- [x] Copy 완료 처리에서 `ClipboardSystem`과 `InfiltrationState.Clipboard`의 이중 기록을 단일화한다: `InfiltrationManager.OnOperationCompleted`가 이미 `TryCopyToClipboard`로 `InfiltrationState.Clipboard`에 항목을 추가하지만, `BattleScene.ProcessCompletedOperations()` Copy 분기(line 848)에서 `_battleManager.Clipboard.Copy(node)`도 별도로 호출해 두 클립보드가 따로 유지된다 — `_battleManager.Clipboard.Copy(node)` 호출을 제거하고, `ClipboardNotEmptyCondition`과 `PasteAction.CanExecute`가 `InfiltrationState.Clipboard`를 기준으로 동작하도록 `ActionContext`에 `ClipboardItemCount` 속성을 추가한다.
- [x] `SecurityBehaviorContext.RestoreNode` 델리게이트 미설정으로 `BuildBackupRepairerRestoreBehavior`가 항상 실패한다: `SecurityBehaviorFactory.BuildBackupRepairerRestoreBehavior`는 `context.RestoreNode != null` 조건을 확인하지만 `InfiltrationManager.ExecuteSecurityBehavior`에서 생성하는 `SecurityBehaviorContext`에 `RestoreNode`가 할당되지 않아 항상 Failure를 반환한다 — `ApplyBackupRepairerRestores()`의 복구 로직을 `InfiltrationManager`에 이관하고 `SecurityBehaviorContext.RestoreNode`를 적절히 설정하거나, 반대로 `SecurityBehaviorKeys.RestoreNode` 경로를 제거하고 씬 레이어 구현(`ApplyBackupRepairerRestores`)으로 단일화해 데드 코드를 정리한다.

## Campaign / Run Loop

- [x] `CampaignState.IsMissionAvailable()`에 conflict-group 뮤텍스를 적용한다: `"readme_conflict"` 그룹의 두 미션("Loose End Cleanup", "Mirror Snatch") 중 하나가 `_completedMissionIds`에 있으면 같은 `ConflictGroup`의 나머지 미션을 잠금 처리한다.
- [x] 미션 보드에서 이미 완료한 미션을 시각적으로 구분한다: `MainMenu.RefreshMenu()`에서 `_completedMissionIds`를 참조해 미션 제목 옆에 "(완료)" 표시를 추가하거나 선택 비활성화해 재플레이 여부를 명확히 한다.
- [x] `CampaignState.GetAvailableMissions()`가 빈 리스트를 반환할 때 발생하는 `IndexOutOfRangeException`을 방어한다: 선행 조건·세력 평판 필터 후 가용 미션이 0개면 전체 `_missionBoard`로 폴백해 최소 하나를 반환하도록 `EnsureInitialized()`와 `GetSelectedMission()`에 가드를 추가한다.
- [x] 미션 종료 오버레이에 세력 평판 변화를 표시한다: `UpdateBattleEndOverlay`에서 `_battleEndStatsLabel`이 Credits/Rep/Heat 델타만 보여주고 어느 세력의 평판이 변했는지 알 수 없다 — `result.Mission.Client.Name`, `result.Mission.Client.Faction`, `CampaignState.GetFactionReputation(result.Mission.Client.FactionId)` 를 포함한 세력 평판 요약 행(`세력: X → Rep Y`)을 추가한다.
- [x] `CampaignState`를 JSON 파일로 직렬화해 런 간 진행 상황을 유지한다: 현재 Credits·Heat·Reputation·FactionReputation·CompletedMissionIds가 모두 인메모리이며 게임 재시작 시 초기화된다 — `ApplyMissionResult()` 호출 후 `OS.GetUserDataDir() + "/campaign.json"`에 저장하고 `EnsureInitialized()`에서 파일이 있으면 복원하도록 구현한다; `Reset()`은 파일을 삭제해 뉴게임을 지원한다.

## Security / Detection

- [x] `CursorAgent.IsDetected` 해제 수단을 추가한다: 현재 탐지 플래그는 런 내 단방향으로 세팅만 되고 지울 수 없다 — `RewriteLog` 완료 또는 `CleanAction` 성공 시 `InfiltrationManager`에서 `CursorAgent.IsDetected = false`로 리셋하고, 가능 조건을 `InfiltrationTuning`에 상수로 정의한다.
- [x] Quarantine / Purge 경보 단계에서 보안 에이전트가 커서 방향으로 수렴하도록 `AdvanceSecurityAgents()`를 개선한다: 현재 경보 단계는 추적도 임계값 표시에만 쓰이고 에이전트 이동 로직은 `IsAlerted + 활성 오퍼레이션` 조합에만 반응한다 — `AlertStage >= Quarantine`일 때 에이전트가 `CursorAgent.CurrentNodePath` 방향으로 한 칸씩 이동하는 수렴 패턴을 추가한다.
- [x] 오퍼레이터 HP 감소를 침투 실패 경로로 연결한다: `_battleManager.Player.CurrentHp`가 UI에 표시되지만 어떤 보안 에이전트도 침투 중 HP 피해를 주지 않아 HP 소진 실패 경로가 없다 — GuardScanner 또는 AntivirusHeavy가 탐지(`IsDetected == true`) 상태에서 커서 노드와 같은 위치에 있을 때 `AdvanceSecurityAgents()` 안에서 `_battleManager.Player.TakeDamage()` 호출을 추가하고, `ApplyMissionFailureChecks()`에서 `_battleManager.Player.IsAlive == false` 조건으로 미션을 실패 처리한다.

## Content

- [x] `SeedSecurityAgents()`를 `MissionData.TargetPath`에 연동한다: 현재 모든 미션이 동일한 하드코딩 순찰 경로를 사용한다 — 미션 목표 경로(예: `SystemLogPath`, `RootReadmePath`)를 포함하는 순찰 구간을 적어도 하나의 에이전트에 할당해 목표 경로마다 경비 밀도가 다르게 느껴지도록 변경한다.
- [x] `BackupRepairer`에 노드 복원 메커니즘을 추가한다: 현재 ScanPressure만 적용하며 이름과 역할이 어긋난다 — `OperationType.Restore`를 추가하고, `BackupRepairer`가 순찰 경로 안에서 클리어된 노드를 감지하면 `_dungeon`의 해당 노드를 복구하는 `SecurityBehaviorKeys.RestoreNode` 행동을 `SecurityBehaviorFactory`에 구현한다; 복구 시 해당 경로에 `AddTrace(InfiltrationTuning.BackupRepairTraceIncrease)`를 추가해 플레이어에게 신호를 준다.
- [x] `AiMonitor`를 `SeedSecurityAgents()`에 추가한다: `SecurityAgentType.AiMonitor`는 행동(`CursorCrossedAiMonitor`, `FolderNavigationAiMonitor`, `SearchSweepAiMonitor`), 아이콘(`ai_monitor.svg`), 배지(`[AI]`)가 모두 구현됐지만 `BattleScene.SeedSecurityAgents()`에서 한 번도 인스턴스화되지 않는다 — `BossZipPath` 또는 `CacheTempPath` 기준 순찰 루트로 에이전트를 추가한다.
- [ ] `BattleFactory`에 미션 유형별 던전 레이아웃을 추가한다: 현재 7개 미션이 모두 동일한 `CreateDefaultDungeon` 구조를 사용한다 — Delete/Modify 미션용으로 BuildCache 뎁스를 줄이고 목표 경로 인접에 Firewall Sentinel을 배치한 `CreateCompactDungeon`을, Scan/Escape 미션용으로 하위 폴더 2개를 추가한 `CreateDeepDungeon`을 구현하고, `CreateDefaultDungeon(MissionData mission)` 내부 `switch(mission.ObjectiveType)`로 라우팅한다.
- [ ] Helix Ops 세력 연계 미션 `mission_extract_logs`를 `MissionBoardFactory`에 추가한다: "mission_scan_cache" 완료 시 해금되는 Extract 의뢰(`TargetPath: BattleConstants.SystemLogPath`, `PrerequisiteMissionId: "mission_scan_cache"`, `TurnLimit: 9`, `RequiredFactionReputation: 1`)를 추가해 세력 평판 진행이 보드에 반영되게 한다.

## Actions

- [x] Cut/Paste 액션을 컨텍스트 메뉴와 큐 흐름에 연결한다: `CutAction`·`PasteAction`이 `ActionRegistry`에 등록되어 있지만 `BuildContextActionIds()`에 `ActionIds.Cut`과 `ActionIds.Paste`가 빠져 있어 플레이어가 접근할 수 없다 — 컨텍스트 메뉴 후보에 추가하고, `ProcessCompletedOperations()`에 `OperationType.Cut`·`OperationType.Paste` case를 추가해 `InfiltrationState.Clipboard`(레거시 `ClipboardSystem` 아님)와 동기화한다.
- [x] `StunAction`을 구현하고 등록한다: `OperationType.Stun`이 `CreateOperationFromQueueEntry()`에 매핑되어 있고 `SecurityAgent.DisabledTurns`가 `AdvanceSecurityAgents()`에서 소비되지만, `StunAction` 클래스가 존재하지 않아 큐에 넣을 수 없다 — `res/action/implementations/StunAction.cs`를 작성(AP 비용 2, 대상 노드의 보안 에이전트에 `DisabledTurns = InfiltrationTuning.StunDurationTurns` 설정)하고 `ActionRegistry.RegisterDefaultActions()`에 등록한다.
- [ ] `ActionIds.Stun`을 컨텍스트 메뉴 후보에 추가한다: `StunAction`은 `ActionRegistry`에 등록되어 있고 덱 버튼으로는 접근 가능하지만 `BuildContextActionIds()`에 포함되지 않아 우클릭 메뉴에는 나타나지 않는다 — `candidates` 목록에 `ActionIds.Stun`을 추가하고, 컨텍스트 제목을 `GetContextActionTitle`에서 `"Stun Agent"`로 매핑한다.

## UX

- [x] Temp Window에서 노드를 선택해 액션 덱에 연결한다: 현재 Temp Window는 바인드 경로의 노드를 `ItemList`로 보여주기만 하고(`UpdateTempWindowUi`), 항목 클릭 시 `_selectedNodePath`를 갱신하거나 컨텍스트 메뉴를 여는 상호작용이 없다 — `_tempWindowItemsLabel` 대신 `ItemList` 컨트롤을 사용하도록 바꾸거나 현재 `ItemList`에 `ItemSelected` 핸들러를 연결해 `_selectedNodePath`를 업데이트하고 `UpdateUi()`를 호출해 Temp Window를 읽기 전용 뷰에서 탐색 가능한 보조창으로 승격시킨다.
- [x] 키보드 단축키를 추가한다: `Enter` → Execute Queue, `Delete` → Delete 큐 적재, `Backspace` → Navigate Up, `F5` → 현재 컨테이너 새로 고침; `BattleScene._Input()`에서 `_battleManager.IsBattleEnd`와 `GetSelectedNode()`를 체크한 뒤 해당 핸들러를 호출하고, `ShowConsoleHelp()`에 단축키 목록을 포함시킨다.
- [ ] 남은 턴 3 이하 시 ConsoleFeed에 경보 메시지를 출력한다: `ExecuteQueuedCommands()`의 각 턴 틱 이후 `_effectiveTurnLimit - State.TurnCount <= 3`이면 `AppendConsoleFeed($"⚠ {remaining}턴 남음 :: 추적도 임계 접근")`을 호출한다. 현재 `_turnStateLabel` 색상 변경만으로는 마감이 박두했음을 인지하기 어렵다.

## QA / Smoke Test

- [x] `RunSmokeTestIfRequested()`를 전체 미션 골든 패스로 확장한다: 현재 Inspect + Open 두 번만 수행하며 Copy·Extract 흐름은 검증되지 않는다 — Extract 타입 미션(`mission_extract_boss`)에 대해 대상 노드 Copy 큐 → 실행 → 루트 이동 → Extract 순서를 자동화해 `_missionResult.Success == true`를 assertions 없이 로그로 확인할 수 있게 한다.
