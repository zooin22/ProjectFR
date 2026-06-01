# Gameplay — 게임 루프와 핵심 규칙

## 1. 한 줄 정체성

> 파일 탐색기를 플레이하는 로그라이크.
> 적을 때려잡는 것이 아니라, 파일 작업이 끝날 때까지 버티고, 숨기고, 속인다.

---

## 2. 게임 흐름

```
[메인 메뉴 / 미션 보드]
    ↓ 미션 선택
[침투 런 시작]
    ↓ 명령 큐 구성 (Copy / Delete / Compress / Rewrite Log 등)
    ↓ Queue 실행 → FileOperation 시작
    ↓ 턴 진행 (AdvanceTurn)
        - 파일 작업 Tick (진행률 +)
        - 보안 에이전트 이동/반응
        - Trace 변화
    ↓ 목표 달성 (ObjectiveCompleted)
    ↓ 탈출 (Extract)
[미션 결과 정산]
    ↓ Credits / Reputation / Heat 반영
[다음 미션 선택 또는 캠페인 계속]
```

---

## 3. 플레이어 캐릭터 — CursorAgent

플레이어는 **꼬리 없는 해커 햄스터**를 조종하는 커서 에이전트다.

| 속성 | 설명 | 기본값 |
|---|---|---|
| `CurrentNodePath` | 현재 커서 위치 | 시작 폴더 |
| `ActionPoints` | 턴당 사용 가능한 명령 점수 | 3 |
| `ClipboardCapacity` | 클립보드 슬롯 수 | 3 |
| `PouchCapacity` | 볼주머니 캐시 슬롯 수 | 2 |
| `PouchMaxFileSize` | 볼주머니에 넣을 수 있는 최대 파일 크기 | 10 |
| `IsDetected` | 보안 에이전트에게 발각 여부 | false |

`IsDetected`는 보안 에이전트가 커서와 같은 경로에 있을 때 세팅된다.
**해제 방법**: Trace ≤ `DetectionClearTraceThreshold(40)` 상태에서
`RewriteLog` 완료 또는 `CleanAction` 성공 시 자동으로 `false`로 리셋된다.

### 볼주머니 캐시 (Cheek Pouch Cache)

햄스터의 볼주머니에서 착안한 특수 은닉 슬롯.
- 클립보드와 별개로 소형 파일을 낮은 Trace로 숨길 수 있다.
- 볼주머니에 숨긴 파일은 `IndexerScout` / `AiMonitor`의 감시 및 Search 정확도에서 제외된다.
- `PouchMaxFileSize` 초과 파일은 보관 불가.
- `ShowHidden` / `PermissionOverride` 액션이 실행되면 일부 은닉이 해제(`Pouch Exposed`)된다.

---

## 4. 파일 작업 진행률

액션을 즉시 완료하지 않고, **FileOperation** 단위로 진행률을 가진다.

```
Copy 중... 50%
Compress 중... 75%
Rewrite Log 중... 30%
```

| 작업 | 기본 소요 턴 |
|---|---|
| MoveCursor | 1 |
| Copy | 2 |
| Compress | 2 |
| RewriteLog | 2 |
| Delete | 1 |
| Stun | 1 |
| 기타 | 1 |

작업 중 보안 에이전트에게 발각되면 해당 작업이 **중단(Failed)**된다.

---

## 5. Trace (흔적)

Trace는 `0~100` 범위의 수치. 높을수록 보안 반응이 강해진다.

| Trace 구간 | AlertStage | 효과 |
|---|---|---|
| 0–14 | Passive | 기본 순찰 |
| 15–34 | Suspicious | 의심 폴더 감시 강화 |
| 35–59 | ActiveScan | 최근 작업 경로 추적 |
| 60–84 | Quarantine | 폴더 봉쇄 가능 |
| 85–100 | Purge | 제거 시도 |

### Trace 증가 행동

| 행동 | 증가량 |
|---|---|
| 감시 중 노드에서 작업 시작 | +4 |
| 추적된 경로에서 작업 | +2 |
| 커서가 감시 노드 통과 | +6 |
| 폴더 이동 (일반) | +3 |
| 폴더 직접 점프 | +5 |
| Permission Override 사용 | +4 |
| Search 사용 | +7 |
| 파일 이동 | +4 |
| 창 열기 | +1 |
| 멀티 윈도우 병렬 작업 (창 초과분 1개당) | +1/턴 |

### Trace 감소 행동

| 행동 | 감소량 |
|---|---|
| Rewrite Log 완료 | -8 |
| 볼주머니 캐시에 파일 은닉 | -2 |
| Compress (Archive) | -2 |

---

## 6. Heat (캠페인 추적도)

Heat는 캠페인 전체에 걸친 장기 위험 수치.

| Heat 구간 | CampaignModifiers 요약 |
|---|---|
| 0–2 | TRACE LOW — 기본 상태 |
| 3–5 | TRACE ELEVATED — 순찰 강화, 턴 제한 -1 |
| 6+ | TRACE CRITICAL — 고급 보안 강화, 턴 제한 -2 |

미션 결과로 Heat 변동:
- 성공 시: `Trace / 10` 포인트 추가, 이후 -1 감소
- 실패 시: `MissionData.FailureHeat + Trace / 10` 추가

---

## 7. 미션 구조

각 미션은 `MissionData`로 정의된다.

| 필드 | 설명 |
|---|---|
| `ObjectiveType` | Extract / Delete / Scan / Modify / Escape |
| `TargetPath` | 목표 파일/폴더 경로 |
| `TurnLimit` | 최대 턴 수 (Heat에 따라 조정) |
| `RewardCredits` / `RewardReputation` | 성공 보상 |
| `FailurePenaltyCredits` / `FailureHeat` | 실패 패널티 |
| `PrerequisiteMissionId` | 선행 미션 ID (null = 항상 해금) |
| `ConflictGroup` | 같은 대상을 두고 대립하는 미션 그룹 |
| `RequiredFactionReputation` | 특정 세력 평판 요구치 |

### ConflictGroup 뮤텍스

같은 `ConflictGroup`을 공유하는 미션은 그룹 내 다른 미션이 이미 완료된 경우 잠금 처리된다.
예: `"readme_conflict"` 그룹의 "Loose End Cleanup"을 완료하면 "Mirror Snatch"는 더 이상 선택할 수 없다.

### 완료 미션 표시

미션 보드에서 완료한 미션은 제목 앞에 `(완료)` 접두어가 붙는다.
`CampaignState.IsMissionCompleted(missionId)` 로 완료 여부를 확인한다.

### 목표 타입별 완료 조건

| ObjectiveType | 완료 액션 |
|---|---|
| Extract | `copy` 액션을 TargetPath에 사용 |
| Delete | `delete` 액션을 TargetPath에 사용 |
| Scan | `inspect` 액션을 TargetPath에 사용 |
| Modify | `logforge` 액션을 TargetPath에 사용 |
| Escape | `extract` 또는 `RegisterEscape()` 호출 |

### 성공 조건

- `PlayerSurvived` = true
- `ObjectiveCompleted` = true
- `Extracted` = true (탈출 완료)
- `TurnCount < TurnLimit`

---

## 8. 의뢰 세력 (Factions)

| 세력 | 분류 | 주요 의뢰 톤 |
|---|---|---|
| Morrow Proxy | Corporate Espionage | 차갑고 실무적. 보상 크지만 실패 시 흔적 큼 |
| Northline Legal | Legal Fixers | 겉은 합법적, 실상은 진실 은폐 |
| Helix Ops | Security Contractors | 건조하고 전문적. 플레이어를 소모품 취급 가능 |
| Ember Circuit | Leak Brokers | 빠르고 공격적. 고수익이지만 신뢰 불가 |
| Glass Key Collective | Civic Leakers | 이상주의적. 정확한 회수에 압박 큼 |

세력별 평판(`GetFactionReputation(FactionId)`)이 추적되며,
`RequiredFactionReputation`이 설정된 미션은 해당 세력 평판이 충족되어야 해금된다.

---

## 9. 보안 반응 단계

보안 에이전트는 AlertStage에 따라 행동이 달라진다.

| 단계 | 이름 | 행동 |
|---|---|---|
| 0 | Passive | 기본 순찰 |
| 1 | Suspicious | 의심 폴더 감시 강화 |
| 2 | ActiveScan | 최근 수정 경로 추적 |
| 3 | Quarantine | 폴더 봉쇄 + **커서 방향 수렴 이동 시작** |
| 4 | Purge | 제거 시도 + **커서 방향 수렴 이동** |

`AlertStage >= Quarantine`이 되면 `AdvanceSecurityAgents()`에서 각 에이전트가
`CursorAgent.CurrentNodePath` 방향으로 순찰 경로 인덱스를 한 칸씩 전진시킨다.
커서 경로가 순찰 경로 밖이면 커서 위치로 직접 이동한다.

---

## 10. 보안 에이전트 종류

| 타입 | 역할 | 특수 반응 |
|---|---|---|
| GuardScanner | 기본 감시 순찰 | 커서/폴더 이동 감지 |
| IndexerScout | 수정/검색 기록 추적 | Search 시 ActiveScan으로 전환, 목표 경로 TrackedPath 표식 |
| AiMonitor | 이상 행동 패턴 분석 | 볼주머니 은닉 파일도 일부 감지, ScanPressure 적용. Temp ↔ Boss.zip 구역 순찰. |
| FirewallSentinel | 폴더 입구 차단 | 잠긴 컨테이너 접근 차단, ForcedLock 적용 |
| AntivirusHeavy | 고강도 바이러스 제거 | 커서 감지 시 Quarantine/Purge 에스컬레이션, 목표 경로 ForcedLock |
| BackupRepairer | 파일 무결성 복원 감시 | 커서 감지 시 항상 ScanPressure(3턴), 조용하지만 끈질김 |

---

## 11. 캠페인 진행 — New Game & Reset

`CampaignState`는 씬 전환에도 유지되는 정적 클래스다.
- "새 게임" 버튼 → `CampaignState.Reset()` 호출
- Credits(100), Reputation(0), Heat(0), 완료 미션 목록, 세력 평판 전부 초기화
- 리셋 후 미션 보드 재구성 (`EnsureInitialized()` 재호출)

---

## 12. UI 턴 제한 경고

턴 수가 제한에 2턴 이내로 근접하면 (`TurnCount >= EffectiveTurnLimit - 2`):
- `_turnStateLabel` 색상 → 빨간색
- `_traceBar` 색상 → 빨간색

---

---

## 13. 키보드 단축키

`BattleScene._Input()`에서 처리하는 단축키 목록.

| 키 | 동작 |
|---|---|
| `Enter` | Execute Queue (명령 큐 실행) |
| `Delete` | Delete 액션 큐에 적재 |
| `Backspace` | Navigate Up (상위 폴더로 이동) |
| `F5` | 현재 컨테이너 새로 고침 (LoadContainer 재호출) |

배틀 종료 상태(`_battleManager.IsBattleEnd`)이거나 선택 노드가 없으면 대부분의 단축키는 무시된다.
`ShowConsoleHelp()` 출력에 단축키 목록이 포함된다.

---

## 14. 미션별 던전 변형

모든 런이 동일한 던전을 사용하는 대신, 현재 미션에 따라 노드 프로파일이 변형된다.

| 미션 ID | 변형 내용 |
|---|---|
| `mission_delete/extract_readme` | Readme.txt HP+3, 위협도 High |
| `mission_modify_syslog` | system.log HP+2, 위협도 High + audit_snapshot.dat 추가 |
| `mission_extract/delete_boss` | Boss.zip HP+4, 위협도 Critical |
| `mission_scan_cache` | BuildCache에 index.db · scan_queue.tmp 추가 |
| `mission_escape_only` | 전체 노드 HP-2 (가벼운 런) |
