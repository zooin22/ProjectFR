# ProjectFR - 보안 AI 시스템 기획서 (Security Modules AI)

> Status: design — 보안 모듈 행동·반응 명세 (Behavior Tree 확장 포함)
> Read with: `COMBAT_SYSTEM_DESIGN.md`, `GAME_CONCEPT.md`, `STORY_WORLD.md`
> Owns: 8종 보안 모듈, 5단계 반응, 감시·순찰·추적 행동 트리, 상태효과(Tracked/Lock/Pressure)

---

## 1. 설계 선언

보안 모듈은 몬스터가 아니다. **시스템 프로세스가 캐릭터로 의인화된 존재**다.
표면상 시스템 안정과 보안을 담당하지만, 실제로는 Tail Signal이 없는 존재와
오래된 기억 파일을 "정리"하는 Cursor Authority·CleanSweep의 집행자다.

설계 목표:

1. **즉시 공격하지 않는다.** 보안은 때리지 않고 *관찰 → 추적 → 격리 → 제거* 한다.
2. **읽을 수 있어야 한다.** 다음 행동을 예고해, 플레이어가 계획형 턴제에서 대응하게 한다.
3. **모듈마다 성격이 달라야 한다.** 같은 이벤트라도 모듈별로 다른 반응을 보인다.
4. **데이터+행동 조합으로 확장 가능해야 한다.** 인라인 분기 대신 Behavior Tree 경유.

---

## 2. 보안 모듈 8종

| 모듈 | 시스템 의미 | 전술 역할 | 스토리 의미 |
|---|---|---|---|
| **Guard Scanner** | 기본 감시 프로세스 | 파일 주변 순찰, 접근 감지 | Tail Signal 없는 접근자 감시 |
| **Indexer Scout** | 인덱싱 서비스 | 최근 수정/검색/접근 추적 | 미등록 흔적·검색 로그 추적 |
| **Antivirus Heavy** | 백신 핵심 모듈 | 위험 작업 강제 중단, 감염 제거 | 비인가 생명체·감염 코드 제거 |
| **Backup Repairer** | 백업 서비스 | 삭제 파일 복원, 무결성 유지 | 공식 기록만 복구, 비등록 기록 무시 |
| **Firewall Sentinel** | 방화벽 | 폴더 입구 차단, 권한 검사 | 시민권 없는 존재 차단 |
| **AI Monitor** | 이상 행동 분석기 | 반복 패턴 감지, 고급 대응 | 무선 커서 패턴 연구 |
| **Permission Daemon** | 권한 관리자 | 접근 권한 검증, 관리자 인증 요구 | Tail Signal 인증 강제 |
| **CleanSweep Agent** | 정리 프로세스 | 오래된 파일 삭제 대기열 이동 | 기억·미등록 존재 제거 |

### 2.1 모듈별 핵심 스탯

| 모듈 | SightRange `[balance]` | 이동 | 위협도 | 대응 수단 |
|---|---|---|---|---|
| Guard Scanner | 1칸(인접) | 순찰 경로 | 낮음 | Stun, Decoy |
| Indexer Scout | 2칸(원거리 감지) | 최근 수정 폴더로 끌림 | 중간 | Rewrite Log, 은닉 |
| Antivirus Heavy | 1칸 | 위험 작업으로 수렴 | 높음 | 작업 회피, Inject 차단 |
| Backup Repairer | 폴더 단위 | 삭제·복원 대상으로 | 중간 | 빠른 회수, Overwrite 선점 |
| Firewall Sentinel | 폴더 입구 고정 | 거의 고정 | 차단형 | Permission Override |
| AI Monitor | 전역 패턴 | 비이동(분석) | 매우 높음 | 패턴 분산, Wireless Pulse |
| Permission Daemon | 권한 게이트 | 고정 | 차단형 | 권한 위조, 소유자 변경 |
| CleanSweep Agent | 오래된 파일 | 삭제 대기열로 | 시한폭탄 | Mark Important, 조기 회수 |

---

## 3. 5단계 반응 모델

보안은 전역 Trace(런 단위)와 국소 Alert(모듈/폴더 단위)를 함께 본다.

```text
행동 발생 → 로그 생성 → 이상 감지 → 추적 시작 → 위치 특정 → 격리 → 제거
```

| 단계 | 이름 | 전체 행동 | 플레이어 체감 |
|---|---|---|---|
| 0 | Passive | 기본 순찰, 로그 수집 | 자유롭게 행동 가능 |
| 1 | Suspicious | 의심 폴더 감시, 스캔 주기 단축 | "누가 의심하기 시작했다" |
| 2 | Active Scan | 최근 조작 파일 추적, 검색 로그 분석 | 작업이 들킬 위험↑ |
| 3 | Quarantine | 폴더 봉쇄, 이동 경로 차단 | 갇힐 수 있다 |
| 4 | Purge | 플레이어 제거 시도 | 직접 위협 |
| 5 | CleanSweep | 오래된·미등록 파일 즉시 삭제 | 목표가 사라진다 |

단계는 **모듈별로 다르게 진행될 수 있다.** 예: 같은 Trace 50%에서 Indexer는 Active Scan,
Firewall은 아직 Suspicious일 수 있다(모듈 가중치 차이).

---

## 4. Behavior Tree — 모듈 행동 명세

인라인 분기 대신 **Behavior Tree(BT)** 로 행동을 구성한다.
공통 컨텍스트와 모듈별 키(key) 분해로 같은 이벤트에 서로 다른 반응을 만든다.

### 4.1 공통 컨텍스트 (SecurityBehaviorContext)

```text
{
  currentFolderPath, cursorPath, objectivePath,
  isOnObjectiveRoute,        // 목표선 근처 행동인가?
  triggerEvent,              // cursor_crossing | folder_navigation | search_sweep | op_start
  agentType, alertStage, trace
}
```

> 목표선(objective route) 근처 행동일수록 더 공격적인 반응(ActiveScan/Quarantine/Purge,
> Trace 보너스)을 타게 한다. "목표에 가까울수록 위험하다"는 긴장을 코드로 보장.

### 4.2 표준 BT 골격

```text
Selector(root)
├─ Sequence: [IsPlayerVisible?] → [Pursue/Quarantine]
├─ Sequence: [HasTrackedTarget?] → [MoveToTracked] → [ApplyPressure]
├─ Sequence: [HeardEvent?(search/op)] → [RaiseAlert] → [InvestigateSource]
└─ Fallback: [Patrol along route]
```

### 4.3 모듈별 BT 차이 (같은 이벤트, 다른 반응)

이벤트 = `search_sweep`(플레이어가 Search 사용)일 때:

| 모듈 | 반응 |
|---|---|
| Indexer Scout | 즉시 Active Scan 전환 + 검색 키워드 인근 폴더로 이동 + Trace 보너스 |
| AI Monitor | 검색 패턴을 기록, 3회 누적 시 예측 차단(다음 Search 무효화) |
| Guard Scanner | 반응 약함, 인근이면 순찰 주기만 단축 |
| Firewall Sentinel | 무반응(입구 고정) |

이벤트 = `cursor_crossing`(커서가 가드 노드 진입)일 때:

| 모듈 | 반응 |
|---|---|
| Guard Scanner | 즉시 alert, Trace↑, 해당 노드 감시 |
| AI Monitor | 침투 경로 패턴에 좌표 추가 |
| Permission Daemon | 권한 인증 요구 팝업(미인증 시 Trace↑) |

---

## 5. 상태효과 — 반응을 실제 압박으로

보안 반응이 말뿐이 되지 않도록, 결과를 **지속 상태효과**로 노드/폴더에 남긴다.

| 상태효과 | 부여원 | 지속 `[balance]` | 효과 |
|---|---|---|---|
| **Tracked** | Indexer / AI Monitor | 3턴 | 해당 노드/폴더에서 작업 시작 시 추가 Trace |
| **Forced Lock** | Firewall / Permission Daemon | 2턴 | 폴더 진입 차단 (Override로만 통과) |
| **Scan Pressure** | Antivirus / AI Monitor | 3턴 | 진행 중 작업 속도 감소, 방해 확률↑ |
| **Quarantine** | 단계3 전역 | 단계 유지 동안 | 폴더 간 이동 경로 봉쇄 |
| **Marked for Sweep** | CleanSweep Agent | N턴 카운트다운 | 만료 시 파일 삭제 |

### 5.1 가시화 (UI 배지)

상태효과는 탐색기 카드/리스트/툴팁/인스펙터에서 즉시 읽혀야 한다.

```text
[WATCH]        감시 중 (SightRange 내)
[LOCK]         폴더 봉쇄
[OVERRIDE 3T]  권한 우회 잔여 턴
[TRACKED 2T]   표식 잔여 턴
[PRESSURE 3T]  스캔 압박 잔여 턴
[SWEEP 2T]     삭제 카운트다운
```

### 5.2 대응 수단

| 상태효과 | 해제/완화 방법 |
|---|---|
| Tracked | Rewrite Log 완료 → 선택 노드/현재 폴더의 Tracked 제거 |
| Scan Pressure | Rewrite Log, 모듈 스턴 |
| Forced Lock | Permission Override (3턴 임시 개방) |
| Quarantine | 전역 Trace를 단계 아래로 떨어뜨리기 |
| Marked for Sweep | Mark Important, 카운트다운 만료 전 회수/은닉 |

---

## 6. 순찰·감시·추적 사이클

### 6.1 순찰 (Patrol)

- Guard Scanner는 정해진 patrol route를 따라 이동(Step 5 구현).
- 경계/작업 중 상태에서는 활성 작업 대상으로 수렴.

### 6.2 감시 (Monitoring)

- `IsNodeMonitored(node)` = 해당 노드가 어떤 모듈의 SightRange 안에 있는가.
- 감시 중 노드는 카드/툴팁에 `[WATCH]` 표시.
- 감시 중 노드에서 작업 시작 → Trace 추가 상승.

### 6.3 추적 (Tracking)

- 폴더 진입/트리 점프/상위 이동(`HandleFolderNavigation`)도 감시 반응·Trace를 유발.
- AI Monitor는 이동 좌표를 누적해 침투 경로를 학습 → 후반 단계에서 길목 차단.

---

## 7. Heat에 따른 모듈 스케일링

캠페인 Heat가 높을수록 보안이 강해진다. (`PROGRESSION_ECONOMY_DESIGN.md`와 공유)

| Heat 구간 | 보안 변화 |
|---|---|
| 0–2 | 기본 |
| 3–5 | 스캔 주기 단축, 모듈 능력 소폭↑, EnemyAttack/Ap/Hp 보너스(CampaignModifiers) |
| 6+ | 고급 모듈(AI Monitor, Permission Daemon) 등장, 제한 턴 감소 |
| 9+ | OPTIMA 직접 패턴 분석, CleanSweep Agent 조기 등장 |

> CampaignModifiers 3티어(LOW<3 / ELEVATED 3–5 / CRITICAL≥6)는 `MainMenu`와 전투 양쪽에서 읽힌다.

---

## 8. 보스급 프로세스: OPTIMA

최종 보스 OPTIMA는 단일 유닛이 아니라 **시스템 자체**다.

- HP를 깎는 전투가 아니라, 삭제 대기열의 기억 파일을 복원하고 CleanSweep Final을 막는 작전.
- OPTIMA는 위 8종 모듈을 동시에 지휘하는 "메타 컨트롤러"로 동작.
- 행동 원칙: 패턴을 학습하면 그 패턴을 차단한다. 따라서 보스전은 *같은 수를 두 번 쓰지 못하게* 강제.
- 상징: "나는 삭제하지 않습니다. 정리합니다."

(보스 미션 구조 상세는 `LOBBY_MISSION_DESIGN.md` §보스 미션, 서사는 `STORY_WORLD.md` §9~10)

---

## 9. 인접 시스템 경계

| 경계 상대 | 받는 것 | 넘기는 것 |
|---|---|---|
| `COMBAT_SYSTEM` | Trace, 작업 이벤트 | 반응 단계, 방해/상태효과 |
| `EXPLORER_INTERACTION` | 이동/검색/탐색 입력 | 감시 반응, [WATCH] 표시 |
| `LEVEL_GENERATION` | 노드 배치, patrol route | 활성 모듈 구성 요청 |
| `PROGRESSION_ECONOMY` | Heat 값 | 난이도 티어 적용 결과 |

---

## 10. 구현 우선순위

| 순위 | 항목 | 현재 상태 |
|---|---|---|
| P0 | Guard/Indexer/Backup 3종 + 순찰·감시 | 부분(Step 5) |
| P0 | SightRange / IsNodeMonitored / [WATCH] | 구현 |
| P0 | Tracked/Lock/Pressure 상태효과 + 배지 | 부분(Step 7) |
| P1 | Behavior Tree 경유(SecurityBehavior*) | 스캐폴딩 도입됨 |
| P1 | 모듈별 key 분해(같은 이벤트 다른 반응) | 도입됨 |
| P2 | AI Monitor 패턴 학습 | 미구현 |
| P2 | OPTIMA 메타 컨트롤러 | 미구현 |
