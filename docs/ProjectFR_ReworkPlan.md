# ProjectFR Rework Plan

## Goal
기존 전투 프로토타입을 **파일 시스템 침투형 전술 로그라이크**로 재해석한다.

핵심 전환:

- 파일 = 적 → **파일 = 목표물 / 자산 / 함정 / 규칙**
- 적 = 전투 대상 → **보안 캐릭터 = 시스템 프로세스의 의인화**
- 승리 = 적 전멸 → **승리 = 목표 달성 + 흔적 관리 + 탈출**

---

## Keep / Replace

### 유지
- 타이틀 / 로비 / 던전 3단 씬 구조
- 미션 보드, Credits / Reputation / Heat 루프
- 파일 탐색기형 화면 방향
- 액션을 파일 조작 명령으로 해석하는 축

### 교체/축소
- `파일 = 몬스터` 전투 해석
- `NodeCombatProfile` 중심의 직접 전투 설계
- `BattleManager.Enemies` 중심 턴 흐름
- 파일을 공격해서 없애는 단순 배틀 루프

---

## New Runtime Model

### 1. InfiltrationNode
파일/폴더/로그/함정/탈출지점 같은 환경 객체.

예시 필드:
- Id
- Path
- DisplayName
- NodeKind (Folder/File/Log/Temp/Exit/Shortcut/...)
- NodeRole (Objective/Resource/Hazard/Decoy/Utility)
- IsContainer
- IsLocked
- ThreatWeight
- Tags

### 2. SecurityAgent
보안 프로세스를 의인화한 캐릭터.

예시 필드:
- Id
- AgentType (GuardScanner/AntivirusHeavy/IndexerScout/...)
- CurrentNodePath
- PatrolRoute
- Awareness
- DisabledTurns
- StatusEffects
- ReactionProfile

### 3. CursorAgent
플레이어 본체.

예시 필드:
- CurrentNodePath
- ActionPoints
- Trace
- ActiveTools
- PassiveMods
- Daemons

### 4. InfiltrationState
전투 상태 대신 작전 상태를 관리.

예시 필드:
- CurrentMission
- CurrentFolderPath
- Trace
- AlertStage
- HeatModifier
- TurnCount
- KnownNodes
- VisibleSecurityAgents
- LogsGeneratedThisTurn
- ExitUnlocked

---

## BattleScene Re-Role
기존 BattleScene은 이름은 유지할 수 있지만 실제 역할은 아래로 변경.

### 좌측
- 폴더 트리
- 경로 구조
- 봉쇄/격리 상태 표시

### 중앙
- 현재 폴더 내부 파일/폴더 표시
- 각 파일 주변에 보안 캐릭터가 붙어 보이게 표현
- 커서 에이전트 위치 표시

### 우측
- 선택한 파일/보안 캐릭터 상세 정보
- Trace / Alert / 이벤트 로그

### 하단
- 액션 바
- Access / Copy / Move / Delete / Compress / Stun / Rewrite Log / Override 등

---

## Implementation Order

### Phase 1
- 현재 구조에서 파일 노드와 보안 캐릭터 데이터를 분리
- `SecurityAgent` / `InfiltrationState` 도입
- 기존 전투용 적 리스트를 보안 캐릭터 리스트로 대체 시작

### Phase 2
- 중앙 파일 영역에 파일 + 보안 캐릭터 배치 모델 도입
- 파일 선택 / 보안 캐릭터 선택 구분
- Trace / Alert 단계 반응 구조 추가

### Phase 3
- 목표 달성 + 탈출 중심 미션 판정으로 변경
- 적 전멸 조건 제거
- 로그 생성 / 스캔 / 봉쇄 같은 시스템 반응 구현

### Phase 4
- 빌드 시스템(툴/패시브/데몬) 확장
- 캐주얼한 시각 연출 추가

---

## First Concrete Refactor
가장 먼저 할 일:

1. 기존 `NodeData`는 환경 노드 쪽으로 유지
2. 보안 캐릭터용 새 타입 추가
3. `BattleManager` 대체 또는 래핑할 `InfiltrationManager` 초안 추가
4. BattleScene에서 "파일 선택"과 "보안 캐릭터 선택"을 분리할 준비
