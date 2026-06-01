# ProjectFR Redesign Steps

> Status: active — implementation progress log; Steps 1-7 partially implemented
> Related: `README.md`, `GAME_CONCEPT.md`, `MULTI_WINDOW_EXPANSION.md`
> System specs: 상세 시스템 명세는 `02_systems/`의 기획서가 캐논 (이 문서는 구현 진행 로그)


## Goal
현재 배틀 프로토타입을 `docs/GAME_CONCEPT.md` 기준의 **Explorer-Based Tactical Roguelike**로 재설계한다.

핵심 목표:
- 탐색기 조작 자체를 전투 시스템으로 사용
- 파일 작업 진행률을 전술 중심축으로 전환
- 보안 모듈을 시스템 프로세스 캐릭터로 분리
- 커서형 플레이어 + 명령 큐 기반 계획형 턴제로 전환

---

## Current -> Target

## Keep / Replace

### Keep
- 타이틀 / 로비 / 던전 3단 씬 구조
- 미션 보드, Credits / Reputation / Heat 루프
- 파일 탐색기형 화면 방향
- 액션을 파일 조작 명령으로 해석하는 축

### Replace / Reduce
- `파일 = 몬스터` 전투 해석
- `NodeCombatProfile` 중심의 직접 전투 설계
- `BattleManager.Enemies` 중심 턴 흐름
- 파일을 공격해서 없애는 단순 배틀 루프


### Current
- BattleManager 중심 턴제 전투
- 파일 노드가 사실상 적 역할도 겸함
- Tree 리스트 기반 파일 표시
- 액션은 즉시 실행 중심
- 미션 성공 판정도 전투 종료 감각이 남아 있음

### Target
- ExplorerRunManager 중심 침투 런 관리
- 파일 노드 = 목표물 / 자산 / 함정 / 환경
- SecurityAgent = Guard / Antivirus / Indexer / Firewall 등
- CursorAgent = 플레이어 본체
- FileOperation = 복사/삭제/압축/로그 위조 등 진행률 보유 작업
- CommandQueue = 여러 명령 예약 후 실행
- 승리 조건 = 목표 달성 + 탈출

## BattleScene Re-Role

기존 `BattleScene` 이름은 유지할 수 있지만 실제 역할은 아래처럼 본다.

### Left
- 폴더 트리
- 경로 구조
- 봉쇄 / 격리 상태 표시

### Center
- 현재 폴더 내부 파일 / 폴더 표시
- 각 파일 주변의 보안 캐릭터 표현
- 커서 에이전트 위치 표시

### Right
- 선택한 파일 / 보안 캐릭터 상세 정보
- Trace / Alert / 이벤트 로그

### Bottom
- 액션 바
- Access / Copy / Move / Delete / Compress / Stun / Rewrite Log / Override 등

---

## Step-by-step Plan

### Step 1. Domain Split [완료]
목표: 기존 "노드=적" 구조 분리

작업:
- ExplorerNodeKind / ExplorerNodeRole 정의
- CursorAgent 정의
- FileOperation / OperationType / OperationStatus 정의
- CommandQueueEntry 정의
- Run 종료 조건을 침투 관점으로 다시 정리

산출물:
- 새 도메인 타입 파일들
- 기존 Battle 구조와 병행 가능한 스캐폴딩

현재 반영:
- `CursorAgent.cs` — 단독 파일
- `ExplorerNodeKind.cs` — 단독 파일 (기존 `ExplorerNodeKinds.cs`에서 분리)
- `ExplorerNodeRole.cs` — 단독 파일 (기존 `ExplorerNodeKinds.cs`에서 분리)
- `FileOperation.cs` — 클래스만 포함 (enum 분리 완료)
- `OperationType.cs` — 단독 파일 (기존 `FileOperation.cs`에서 분리)
- `OperationStatus.cs` — 단독 파일 (기존 `FileOperation.cs`에서 분리)
- `CommandQueueEntry.cs` — 단독 파일
- `RunState.cs` — RunStatus / ObjectiveState (침투 관점 종료 조건)

---

### Step 2. Run Manager Introduction [진행중]
목표: BattleManager 옆에 새 침투 런 관리자 도입

작업:
- ExplorerRunManager 추가
- CurrentFolderPath / Trace / AlertStage / Clipboard / ActiveOperations / Queue 관리
- SecurityAgent와 노드 가시성 연결

산출물:
- 침투 런 상태 관리자
- 로그/Trace/큐 관리 API

현재 반영:
- Clipboard 상태
- `PouchCache` 상태
- RunStatus / ObjectiveState
- Queue 실행 API
- 기본 Copy 완료 -> Clipboard 반영
- `TryMoveClipboardToPouch(...)`, `TryRestoreFromPouch(...)` 스캐폴딩 추가
- Exit unlock / escape 스캐폴딩

---

### Step 3. UI Layout Pivot [착수]
목표: 중앙을 디테일 리스트에서 큰 아이콘 보기 전술 공간으로 전환

작업:
- BattleScene 중앙 파일 Tree 의존 축소
- 노드 그리드(아이콘 보기) 뷰 추가
- 파일/폴더/보안 캐릭터/커서 표시 슬롯 정의
- 우측 패널에 목표/보안/작업 진행률 표시 강화

산출물:
- ExplorerField 전용 UI
- 보안 캐릭터와 파일이 함께 보이는 장면

현재 반영:
- 중앙 패널에 `ExplorerFieldList` 추가
- 기존 `FileListTree`는 숨기고, 큰 아이콘 보기 프로토타입 진입
- 파일/폴더를 아이콘 카드처럼 표시하는 1차 렌더링 추가
- 선택/더블클릭으로 기존 노드 선택 및 폴더 열기 연결
- `CursorStatusLabel`, `FieldSecurityLabel` 추가
- 중앙 전술 공간 상단에 커서 상태 / 보안 배치 요약 표시
- 선택 노드와 커서 위치가 카드 텍스트/색상에 반영되도록 개선

---

### Step 4. File Operation Progress [착수]
목표: 즉시 실행 액션을 진행률 기반 작업으로 전환

작업:
- Copy / Delete / Compress / Rewrite Log 등의 작업 시간 규칙 정의
- 작업 시작/중단/완료 처리
- 감시 중 방해와 Trace 상승 반영

산출물:
- 진행률 기반 작업 시스템
- 작업 큐 UI

현재 반영:
- Action Bar 클릭 시 즉시 실행 대신 Command Queue에 적재
- `Execute Queue` / `Clear Queue` 버튼 추가
- Queue 내용이 하단 패널에 실시간 표시
- Queue 실행 시 `InfiltrationManager`의 `FileOperation` 시작 + 기존 액션 브리지 실행
- 최근 Activity 패널에 진행 중 작업의 퍼센트/상태 표시
- `Copy`, `Compress`, `Rewrite Log`를 지연 완료 작업으로 분리
- 작업 완료 시점에만 실제 효과가 발동되도록 `CompletionHandled` 기반 처리 추가
- `Rewrite Log` 액션(`logforge`) 자체도 새로 등록

---

### Step 5. Interaction Model Upgrade [착수]
목표: 선택 후 명령 / 우클릭 / 드래그 기반 조작 도입

작업:
- Context menu 모델 정의
- Quick bar / queue / execute 버튼 흐름 정리
- 드래그 앤 드롭 규칙 스캐폴딩

산출물:
- 탐색기다운 조작 모델

현재 반영:
- `Move Cursor` 버튼 추가
- 선택 노드 기준으로 `OperationType.MoveCursor`를 큐에 적재 가능
- 턴 진행 시 보안 에이전트가 patrol route를 따라 이동하도록 스캐폴딩 추가
- 경계/작업 중 상태에서는 보안 에이전트가 활성 작업 대상으로 수렴하도록 1차 반응 추가
- 커서가 가드 노드에 들어가면 Trace 상승 + agent alert 반응 추가
- 중앙 전술 공간 우클릭 시 `ExplorerContextMenu` 팝업 추가
- 컨텍스트 메뉴에서 `Move Cursor`, `Open`, `Properties`, `Copy`, `Compress`, `Rewrite Log`, `Delete` 등을 선택 노드 기준으로 큐 적재 가능
- 중앙 전술 공간에서 드래그 시작/드롭 대상으로 폴더 또는 아카이브를 잡으면 `Move`/`Compress` 큐 적재 가능
- 드롭 완료 시 `BattleDungeon.MoveNode(...)`로 컨테이너 재배치 프로토타입 처리
- `SecurityAgent.SightRange`, `GetMonitoringAgents(...)`, `IsNodeMonitored(...)` 추가
- 감시 중 노드는 리스트/전술 카드/툴팁/인스펙터에서 `WATCH` 또는 `Monitored` 상태로 노출
- 감시 중 노드에서 작업 시작 시 Trace 추가 상승
- `HandleFolderNavigation(...)` 추가로 폴더 진입/트리 점프/상위 이동도 감시 반응과 Trace 증가를 유발
- `SearchAction` 추가 및 등록
- Search 실행 시 Trace 상승 + Indexer/AI Monitor를 Active Scan으로 전환
- Search 결과가 현재 컨테이너/선택 폴더/정확한 목표와 어떻게 맞물리는지에 따라 `exact hit / nearby / deeper hint / no hit` 단서 제공

---

### Step 6. Mission & Escape Loop [착수]
목표: 적 전멸 대신 작전 완료/탈출 판정으로 변경

작업:
- Extract/Delete/Modify/Scan/Escape 흐름 정리
- Exit node / extraction zone 개념 추가
- 탈출 시 Trace/Heat 정산 반영

산출물:
- 새 미션 완료 루프

현재 반영:
- 목표 완료 시 `InfiltrationManager.UnlockExit(...)` 호출
- 하단 패널에 `Extract` 버튼 추가
- 루트 컨테이너에서만 추출 가능하도록 1차 규칙 적용
- 미션 성공 판정을 `enemy clear`가 아니라 `objective completed + escaped` 기준으로 변경

---

### Step 7. Multi-Window Expansion [설계 착수]
목표: 메인 침투 창 외에 기능성 서브창을 전술 자원으로 확장

작업:
- 창 타입/상태 모델 정의
- 메인 창 외 Clipboard / Temp / Log Viewer 같은 보조창 설계
- 포커스/열기/닫기/Trace 비용 규칙 정의
- 이후 Clipboard Window부터 실제 UI 구현

산출물:
- 멀티윈도우 기획 문서
- 확장 스캐폴딩 타입

현재 반영:
- `docs/MULTI_WINDOW_EXPANSION.md` 작성
- `ExplorerWindowType`, `ExplorerWindowState` 추가
- `InfiltrationState.Windows` 추가
- `InfiltrationManager.OpenWindow(...)`, `FocusWindow(...)`, `CloseWindow(...)` 스캐폴딩 추가
- 메인 침투 창 상태를 런 초기화 시 자동 생성
- `Clipboard Window` 1차 UI 추가
- 하단 패널의 `Clipboard` 버튼으로 열기 가능, 열려 있을 때는 다시 눌러 포커스 복귀
- 별도 `Close` 버튼으로 닫기 가능
- Clipboard Window에서 현재 clipboard 항목, ghost 상태, 포커스/바인드 경로 표시
- Clipboard Window에서 `Cheek Pouch Cache` 상태도 함께 표시
- 선택 노드가 clipboard에 있으면 `Store in Pouch` 버튼 활성화
- 선택 노드가 pouch에 있으면 `Restore to Clipboard` 버튼 활성화
- Clipboard Window에서 clipboard <-> pouch 이동을 직접 조작 가능
- pouch에는 `PouchMaxFileSize` 이하의 작은 파일만 저장 가능하도록 제한
- oversized 파일은 pouch 저장이 차단되고 콘솔에 차단 이유 표시
- pouch에 숨긴 파일은 `ghost` 상태가 되고 Trace를 소폭 감소시킴
- pouch에 숨긴 파일은 `IndexerScout` / `AiMonitor`의 일부 감시와 Search 정확도에서 제외됨
- `Show Hidden` / `Permission Override` 액션 추가
- 두 액션은 pouch-hidden 파일을 `Pouch Exposed` 상태로 바꿔 Search/Indexer 은닉을 일부 해제
- `Permission Override`는 `Firewall Sentinel`이 잠근 컨테이너를 `Access: Overridden` 상태로 바꿔 실제 진입 가능하게 함
- override 접근권은 영구가 아니라 기본 3턴짜리 임시 창으로 동작하며, 만료 후 다시 잠김
- 침투 런타임 밸런스 숫자는 `res/infiltration/InfiltrationTuning.cs`로 모아 후속 밸런싱/리팩토링 비용을 낮춤
- `BattleScene` UI 부트스트랩은 참조 캐시 / 이벤트 바인딩 / 초기 상태 설정으로 분리하고, 액션 문자열은 `res/action/ActionIds.cs`로 모아 후속 분해 작업 기반을 마련함
- 확장성용 스킬 스캐폴딩 추가: `res/skills/SkillDefinition.cs`, `SkillCatalog.cs`, `SkillExecutor.cs`, BT 스타일 `SkillBehaviorNode` 계층 및 `Search`/`Show Hidden`/`Permission Override` 행동 매핑을 도입해 복잡한 전술 스킬을 데이터+행동 조합으로 옮길 기반을 마련함
- 보안 반응도 같은 방향으로 확장 시작: `res/infiltration/SecurityBehaviorKeys.cs`, `SecurityBehaviorContext.cs`, `SecurityBehaviorNode.cs`, `SecurityBehaviorFactory.cs`, `SecurityBehaviorExecutor.cs`를 추가하고, cursor crossing / folder navigation / search sweep 반응을 인라인 분기 대신 behavior executor 경유로 연결함
- 이어서 `SecurityAgentType`별 key 분해를 추가해 `IndexerScout`, `AiMonitor`, `FirewallSentinel`, `GuardScanner`가 동일 이벤트에도 서로 다른 awareness/log/trace 보너스를 가질 수 있게 만듦
- 추가로 security behavior context에 현재 폴더/커서/목표 경로와 objective-route 여부를 넣어, 목표선 근처 행동일수록 더 공격적인 반응(`ActiveScan`/`Quarantine`/`Purge`, trace bonus)을 타게 확장함
- 여기서 한 단계 더 나아가 반응 결과를 실제 상태 효과로 연결: `TrackedPathTurns`, `ForcedLockTurns`, `ScanPressureTurns`를 도입해 Indexer/AiMonitor/Firewall 반응이 경로 추적, 임시 잠금, 스캔 압박을 남기고, UI에서도 `Security FX`로 가시화함
- 이어서 `Tracked` 상태는 실제 행동 비용도 올리게 확장: 추적된 노드/폴더에서 오퍼레이션 시작 시 추가 Trace가 붙어, 표식 경로를 계속 쓰는 플레이를 직접 압박함
- 대응 수단도 연결: `Rewrite Log` 완료 시 선택 노드/현재 폴더의 `Tracked` 및 `Scan Pressure`를 제거하도록 해, 보안 압박을 해소하는 전술 행동으로 승격시킴
- 가시성 강화: 새 보안 효과를 탐색기 카드/리포트/상태 요약에서 바로 읽게 하도록 `[WATCH]`, `[LOCK]`, `[OVERRIDE nT]`, `[TRACKED nT]`, `[PRESSURE nT]` 텍스트 배지 표시를 추가함
- 이어서 placeholder SVG 배지도 추가하고 `BattleScene`에 연결: file tree의 status 컬럼에 대표 배지 아이콘을 얹고, explorer 카드 보조 텍스트에 compact badge summary를 붙여 실제 badge-like 가시성을 강화함
- 인스펙터 / 상태 문자열 / 툴팁에 `Access: Locked/Overridden/Open` 및 `Pouch Hidden`, `Pouch Exposed`, `Pouch Safe`, `Oversize` 상태 표시
- oversized 파일은 인스펙터 힌트에서 pouch 한계값 대비 초과 크기를 직접 안내
- 상단 상태/전술 요약에 `Windows N · Focus X` 요약 노출
- `status` 콘솔 요약에도 현재 윈도우 상태 포함
- 창 열기 시 Trace +1 규칙 시범 적용
