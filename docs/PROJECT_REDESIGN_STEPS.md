# ProjectFR Redesign Steps

## Goal
현재 배틀 프로토타입을 `docs/GAME_CONCEPT.md` 기준의 **Explorer-Based Tactical Roguelike**로 재설계한다.

핵심 목표:
- 탐색기 조작 자체를 전투 시스템으로 사용
- 파일 작업 진행률을 전술 중심축으로 전환
- 보안 모듈을 시스템 프로세스 캐릭터로 분리
- 커서형 플레이어 + 명령 큐 기반 계획형 턴제로 전환

---

## Current -> Target

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

---

## Step-by-step Plan

### Step 1. Domain Split [진행중]
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
- `CursorAgent`
- `ExplorerNodeKind`
- `ExplorerNodeRole`
- `FileOperation`
- `OperationType`
- `OperationStatus`
- `CommandQueueEntry`

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
- RunStatus / ObjectiveState
- Queue 실행 API
- 기본 Copy 완료 -> Clipboard 반영
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
- 상단 상태/전술 요약에 `Windows N · Focus X` 요약 노출
- `status` 콘솔 요약에도 현재 윈도우 상태 포함
- 창 열기 시 Trace +1 규칙 시범 적용

---

## Immediate Next Step
바로 진행할 1차 작업은 **Step 1: Domain Split**.

이번 단계에서는 기능을 다 갈아엎지 않고, 아래 타입을 먼저 추가한다.
- `CursorAgent`
- `ExplorerNodeKind`
- `ExplorerNodeRole`
- `FileOperation`
- `OperationType`
- `OperationStatus`
- `CommandQueueEntry`

이렇게 해두면 기존 BattleScene을 유지한 채 점진적으로 침투형 구조로 옮길 수 있다.
