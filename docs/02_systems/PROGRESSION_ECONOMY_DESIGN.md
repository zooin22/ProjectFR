# ProjectFR - 재화 · 메타 진행 · 빌드 기획서 (Economy / Meta / Builds)

> Status: design — 캠페인 경제 + 로그라이크 진행 + 빌드 아키타입 명세
> Read with: `LOBBY_MISSION_DESIGN.md`, `COMBAT_SYSTEM_DESIGN.md`, `MISSION_DATA_MODEL.md`
> Owns: Credits/Reputation/Heat 경제, 로그라이크 루프, 빌드 보상 규칙, 8종 빌드 아키타입

---

## 1. 설계 선언

반복 플레이의 재미는 **수치 증가가 아니라 행동 규칙의 변화**에서 나온다.

> 나쁜 업그레이드: "복사 속도 +10%"
> 좋은 업그레이드: "복사 시 원본 위치에 미끼가 자동 생성된다"

후자는 *무엇을 할 수 있는가*를 바꾼다. ProjectFR의 빌드는 항상 후자를 지향한다.

이 문서는 두 개의 진행 축을 정의한다.

```text
런 내 자원 (Trace, AP, Clipboard) ──→ COMBAT_SYSTEM 소관
캠페인 자원 (Credits, Reputation, Heat) + 빌드 ──→ 이 문서 소관
```

---

## 2. 캠페인 경제 — 3대 자원

### 2.1 Credits

의뢰 보상 화폐. 빌드/도구 구매에 사용.

| 항목 | 값 `[balance]` |
|---|---|
| 초기 Credits | 100 |
| 미션 성공 보상 | 55~95cr (미션별) |
| 미션 실패 페널티 | FailurePenaltyCredits 차감 |
| 부분 성공 | 보상 차감 정산 |

용도: 영구/임시 빌드 해금, 소비 도구 보충, 위험 의뢰 보험.

### 2.2 Reputation

신뢰도. **전역 Reputation**과 **세력별 Reputation** 두 층위.

| 층위 | 코드 | 역할 |
|---|---|---|
| 전역 | `CampaignState.Reputation` | 보드 전반 접근성, UI 표시 |
| 세력별 | `_factionReputation : Dict<FactionId,int>` | 세력 단위 신뢰 (기본 0) |

`ApplyMissionResult`가 성공 시 의뢰 세력의 평판을 함께 올린다.

> 설계 의도(데이터는 있으나 미연결): 세력 평판 임계값으로 잠금 해제·위험도 보정을 거는 것.
> 예: Glass Key 평판 ≥ 5면 폭로 계열 의뢰 해금, Northline 평판 ≥ 5면 은폐 계열 할인.
> 두 대립 세력 평판을 동시에 올리기 어렵게 만들어 **노선 선택**을 유도한다.

### 2.3 Heat

당국(Cursor Authority·CleanSweep Division)이 햄스터 공동체를 위험 집단으로 보는 장기 추적도.

Heat 증가 원인: 미션 실패 · 높은 Trace 탈출 · 제한 턴 초과 · 완전 삭제 남발 ·
보안 대량 제거 · 위험 의뢰 수락 · CleanSweep 기록 탈취 · Tail Registry 조작.

#### Heat 효과 (CampaignModifiers 3티어)

| Heat | 티어 | 효과 |
|---|---|---|
| 0–2 | LOW (<3) | 기본 |
| 3–5 | ELEVATED (3–5) | 스캔 주기 단축, 모듈 능력 소폭↑, EnemyAttack/Ap/Hp 보너스 |
| 6+ | CRITICAL (≥6) | 고급 모듈 등장, 제한 턴 감소, 보상 대비 위험↑ |
| 9+ | — | OPTIMA 직접 패턴 분석, CleanSweep Agent 조기 등장 |

> `CampaignModifiers`는 `HeatTurnPenalty`, `EnemyAttackBonus`, `EnemyApBonus`, `EnemyHpBonus`를
> 티어별로 스케일하며 `BattleScene`과 `MainMenu` 양쪽에서 읽힌다.

#### Heat는 빌드 자원이기도 하다

Heat를 단순 페널티가 아니라 **자원**으로 쓰는 빌드(Virus Build 등)가 존재해야 한다.
"위험할수록 강해지는" 하이리스크 플레이를 보상한다.

---

## 3. 로그라이크 루프

```text
런 시작 (현재 빌드로 침투)
   ↓
런 중 임시 강화 획득 가능 (그 런에만 유효)
   ↓
런 종료 → 보상 정산 (Credits/Reputation/Heat)
   ↓
로비에서 영구 빌드 해금/교체
   ↓
다음 런 (바뀐 행동 규칙으로)
```

### 3.1 두 종류의 강화

| 종류 | 범위 | 예시 | 자원 |
|---|---|---|---|
| **임시 강화 (Run Mod)** | 현재 런만 | 이번 런 동안 압축 시간 -1턴 | 런 중 발견 |
| **영구 빌드 (Build)** | 캠페인 지속 | 복사 시 미끼 자동 생성 | Credits로 해금 |

> 강화 획득은 단순 지급이 아니라 **무작위 후보 중 선택(드래프트)** 으로 이뤄진다.
> 드래프트 규칙·희귀도 가중치·불운 보호는 `RANDOMNESS_DESIGN.md §4`가 정의한다.

### 3.2 실패의 가치

실패해도 다음 런에서 더 많은 도구·전략으로 재시도한다. 실패는 Heat를 올리지만,
그 Heat가 특정 빌드를 강화하거나 새 의뢰를 여는 분기가 되도록 설계한다.

---

## 4. 빌드 아키타입 8종

각 아키타입은 **행동 규칙을 바꾸는** 빌드 요소 묶음이다. 혼합 빌드도 가능하다.

### 4.1 Ghost Build — 흔적 최소화
- Trace 증가량 감소
- 숨김 속성 활용 강화
- 로그 위조 강화
- temp 폴더 경유 보너스
- 보안 모듈과 직접 충돌 회피
- 무선 펄스 안정화
> 플레이 감각: 아무도 모르게 들어갔다 나온다.

### 4.2 Clipboard / Pouch Build — 운반·은닉
- 클립보드 용량↑
- 볼주머니에 작은 파일 은닉
- 복사 시 decoy 생성
- 붙여넣기 시 감염/로그 위조
- 클립보드 내부 파일 스캔 회피
> 플레이 감각: 모든 걸 입에 물고 빠져나온다.

### 4.3 Virus Build — 감염·확산
- Inject 강화
- 감염된 보안 모듈이 주변 적 방해
- 붙여넣기 시 바이러스 확산
- **높은 Heat에서 강해짐**
> 플레이 감각: 시스템을 안에서부터 무너뜨린다.

### 4.4 Decoy Build — 미끼·유도
- 가짜 파일 생성
- 가짜 Trace 생성
- 보안 모듈 경로 변경
- 검색 결과 조작
- 가짜 Tail Signal 흔적 생성
> 플레이 감각: 보안을 엉뚱한 곳으로 보낸다.

### 4.5 Archive Build — 압축·운반
- zip 내부 스캔 회피
- 압축 작업 시간 감소(×0.5)
- 압축 파일 안에 목표 숨기기
- 큰 파일 운반 효율↑
- 오래된 파일을 압축 보관소에 보호
> 플레이 감각: 전부 압축해서 통째로 들고 간다.

### 4.6 Admin Build — 권한 탈취·정면 돌파
- Permission Override 강화
- 잠긴 폴더 접근
- 보안 모듈 일시 정지
- 높은 Trace 감수하고 빠르게 돌파
> 플레이 감각: 문을 부수고 들어간다.

### 4.7 Search / Analyst Build — 정보·탐지
- 검색 Trace 감소
- 위험 파일 자동 표시
- 숨김 파일 탐지
- 정렬/필터 사용 시 보너스
- 목표 경로 추론 강화
- 오래된 기억 파일 우선 탐지
> 플레이 감각: 들어가기 전에 이미 다 알고 있다.

### 4.8 Restore Build — 복원·보호
- 휴지통 파일 복원 속도↑
- CleanSweep 삭제 대기열 지연
- 오래된 파일 중요 표시 강화
- Backup Repairer 조작
- 손상 파일 복구 성공률↑
> 플레이 감각: 사라지는 것을 붙잡아 되살린다.

---

## 5. 빌드 요소 카탈로그 (예시)

행동 규칙을 바꾸는 개별 업그레이드 풀. 런 보상/상점에서 등장.

| 빌드 요소 | 아키타입 | 효과 |
|---|---|---|
| Decoy Copy | Decoy/Pouch | 복사 시 원본 위치에 미끼 생성 |
| Viral Paste | Virus | 붙여넣기 시 감염 확산 |
| Hard Wipe | Admin | 삭제가 휴지통 거치지 않고 즉시 완전 삭제 |
| Sealed Archive | Archive | 압축 파일 내부 스캔 노출↓ |
| Quiet Search | Analyst | 검색 Trace 증가량 감소 |
| Ghost Clipboard | Pouch/Ghost | 클립보드 파일 감시 대상 제외 |
| Log Cascade | Ghost | 로그 위조가 주변 폴더까지 적용 |
| Low-Priority Hide | Ghost | 숨김 파일이 Guard 감시 우선순위에서 제외 |
| Pulse Cloak | Ghost | 무선 펄스 사용 시 Tail Signal 감시 회피 |
| Memory Recall | Analyst/Restore | 오래된 파일 복원 성공 시 다음 미션 보안 예측 정보 획득 |

> 모든 항목 공통 규칙: **수치보다 행동을 바꾼다.** 새 요소 추가 시 "이게 새로운 선택지를
> 여는가, 아니면 기존 행동을 더 효율적으로 만들 뿐인가?"를 먼저 검증.

---

## 6. 경제 밸런스 가이드라인

| 원칙 | 설명 |
|---|---|
| Credits는 항상 부족하게 | 모든 빌드를 다 살 수 없어 선택이 강제되도록 |
| 고위험 의뢰 = 고보상 + 고Heat | Ember Circuit 의뢰는 돈 많지만 Heat 부담 |
| Heat는 되돌릴 수 있어야 | 무조건 누적만 하면 데스 스파이럴. 일부 행동/시간으로 감소 경로 제공 |
| 평판은 양립 어렵게 | 대립 세력 평판 동시 최대치 불가 → 노선 선택 |
| 부분 성공 = 안전망 | 완벽 실패와 완벽 성공 사이의 회색지대 제공 |

---

## 7. 현재 구현 상태 (코드 기준)

| 항목 | 상태 |
|---|---|
| Credits(초기 100) / 전역 Reputation / Heat | 구현 |
| 세력별 평판 `GetFactionReputation` | 구현(데이터만, 분기 미연결) |
| 완료 미션 추적 `_completedMissionIds` | 구현 |
| `ApplyMissionResult` 정산 | 구현 |
| CampaignModifiers 3티어 | 구현 |
| 빌드 아키타입/요소 | **미구현(설계 단계)** |
| 임시 강화(Run Mod) | 미구현 |
| 세력 평판 게이팅 | 미구현(설계 의도) |

---

## 8. 인접 시스템 경계

| 경계 상대 | 넘기는 것 | 받는 것 |
|---|---|---|
| `LOBBY_MISSION` | 자원 상태, 빌드 상태 | 미션 결과 스냅샷 |
| `COMBAT_SYSTEM` | AP 보너스, 작업 규칙 변경(빌드) | 런 종료 스냅샷 |
| `SECURITY_AI` | Heat 값 | 난이도 티어 적용 |

---

## 9. 구현 우선순위

| 순위 | 항목 |
|---|---|
| P0 | 영구 빌드 해금 골격 + 행동 규칙 훅 1~2개(Decoy Copy, Quiet Search) |
| P1 | 임시 강화(Run Mod) 보상 풀 |
| P1 | 세력 평판 게이팅(임계값 해금) |
| P2 | 8종 아키타입 풀 채우기(요소 10~20개) |
| P2 | Heat 감소 경로(데스 스파이럴 방지) |
