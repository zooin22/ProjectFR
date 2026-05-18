# ProjectFR Multi-Window Expansion Plan

## Why this fits
ProjectFR는 단일 전투 화면보다 **여러 탐색기 창을 동시에 운용하는 침투 감각**과 더 잘 맞는다.

핵심 아이디어:
- 해커는 보통 한 창만 띄워두지 않는다.
- 메인 침투 경로, 임시 은닉 경로, 로그 위조 창, 클립보드/백업 창을 병렬로 다룬다.
- 따라서 서브창은 장식이 아니라 **전술 자원**이어야 한다.

---

## Design Goal
멀티윈도우는 다음 세 가지를 만족해야 한다.

1. **실제 플레이 이득이 있어야 함**
   - 이동 동선 단축
   - 은닉 루트 분산
   - 로그 위조와 본 작업 병행
   - 백업/복원 방해와 목표 회수를 분리

2. **위험도도 커져야 함**
   - 창이 많아질수록 관리 난이도 상승
   - 창 간 이동 흔적 증가 가능
   - 동시 작업은 Trace를 더 빠르게 올릴 수 있음

3. **UI가 감당 가능해야 함**
   - 1차는 소수의 기능성 창만
   - 너무 빨리 풀 다중창으로 가면 복잡도 폭증

---

## Recommended Scope

### Phase A - Single Auxiliary Window
첫 확장은 **서브창 1개만 허용**.

추천 후보:
- Clipboard Window
- Temp Window
- Log Viewer Window

가장 추천:
- **Clipboard Window**
  - 지금 구현된 Copy/Clipboard 시스템과 바로 연결 가능
  - 회수 파일을 별도 창에서 관리하는 감각이 좋음
  - 메인 창과 분리된 “운반/보관 창” 역할이 명확함

---

## Window Types

### 1. Main Infiltration Window
기본 창. 항상 존재.

역할:
- 현재 침투 경로 탐색
- 목표 접근
- 보안 감시 확인
- 기본 명령 실행

### 2. Clipboard Window
복사한 파일과 임시 운반 대상을 관리.

역할:
- 클립보드 슬롯 표시
- 붙여넣기 후보 선택
- Ghost Clipboard/Decoy Copy 같은 빌드와 연동

전술 의미:
- 회수한 파일을 메인 창 시야 밖에서 관리하는 느낌
- “복사해둔 상태” 자체를 별도 공간으로 보여줄 수 있음

### 3. Temp Window
임시 경유/은닉용.

역할:
- temp 폴더를 별도 창으로 고정 표시
- 미끼 파일 은닉
- Trace 정리 전용 경유지

### 4. Log Viewer Window
로그 관련 조작 전용.

역할:
- 접근 기록 표시
- Rewrite Log / 위조 / 정리
- 감시 대응 작업 전용 창

### 5. Backup Window
복원/무결성 관련 대응 전용.

역할:
- Backup Repairer 대응
- 복원 예정 파일 추적
- 백업 방해 또는 역이용

### 6. Archive Window
압축 내부 관리 전용.

역할:
- zip 내부 파일 표시
- 은닉/압축 운반
- 스캔 회피 루트 제공

---

## Gameplay Rules

### Window Slot Rule
- 기본: 메인 창 1개
- 1차 확장: 보조창 1개
- 2차 확장: 최대 2~3개

### Focus Rule
- 한 번에 **포커스된 창**은 하나
- 포커스 창에서만 직접 명령 가능
- 비포커스 창은 상태 확인/대기열 용도

### Risk Rule
- 창 열기 자체가 무료가 아니어야 함
- 창 전환/동시 작업은 Trace나 관리 부담을 유발 가능

예시:
- Clipboard Window 열기: Trace +1
- Log Viewer 창 유지 중 Search 사용: Trace +1 추가
- 여러 창에서 동시에 Active Operation 유지: Alert 가속

### Persistence Rule
- 창은 닫아도 상태를 일부 유지할 수 있음
- 단, 특정 창은 보안 모듈에 의해 강제 종료될 수 있음

---

## UI Direction

### Layout Concept
- 메인 BattleScene은 그대로 메인 창 역할
- 서브창은 떠 있는 보조 패널 또는 내부 드래그 가능한 미니 윈도우
- 초반엔 OS 수준 자유 배치보다 **고정형 보조 패널 팝업**이 안전함

### Stage 1 UI Recommendation
- 화면 우측/하단에 작은 보조창 하나
- 열기/닫기 버튼 제공
- 메인 창과 동시에 내용 표시

### Stage 2 UI Recommendation
- 드래그 가능한 보조창
- 창마다 제목바/닫기/핀/고정 경로 제공

---

## System Design

### Needed Runtime Types
- `ExplorerWindowType`
- `ExplorerWindowState`
- `ExplorerWindowManager` 또는 `InfiltrationState.Windows`

필드 예시:
- WindowId
- WindowType
- BoundPath
- Title
- IsOpen
- IsFocused
- SlotIndex
- TraceModifier
- LinkedClipboard / LinkedFolder / LinkedArchive

---

## Recommended Implementation Order

### Step 1
문서 + 상태 스캐폴딩
- window type/state 추가
- InfiltrationState에 창 목록 추가

### Step 2
Clipboard Window 1차 구현
- 열기/닫기
- 현재 clipboard 내용 표시
- paste 대상 선택 연결

### Step 3
Temp / Log Viewer 창 추가
- temp 은닉과 log 조작 분리

### Step 4
동시 작업/병렬 운영 규칙 추가
- 창별 active operation
- 다중창 운영 리스크 추가

---

## Recommendation
가장 좋은 첫 구현은:

> **Clipboard Window를 첫 서브창으로 도입**

이유:
- 이미 있는 시스템과 가장 자연스럽게 연결됨
- 다중창 감각을 가장 싸게 구현 가능
- 플레이어가 “회수/운반/보관”을 별도 공간으로 인식하게 만들 수 있음

그 다음 순서는:
1. Clipboard Window
2. Temp Window
3. Log Viewer Window
4. Backup / Archive Window

---

## Final Direction
멀티윈도우는 단순 UI 확장이 아니라, ProjectFR의 핵심 감각을 강화하는 시스템이 될 수 있다.

목표는 다음 감각이다.

> 메인 침투 창에서 목표를 건드리고,
> 옆 창에서 로그를 지우고,
> 다른 창에선 회수 파일을 임시 보관하는 해커식 병렬 작업.

즉,
**“한 창의 탐색기 전투”**에서
**“여러 창을 동시에 운영하는 침투 작전”**으로 확장하는 방향이다.
