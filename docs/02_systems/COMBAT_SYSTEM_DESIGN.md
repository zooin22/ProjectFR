# ProjectFR - 전투 시스템 기획서 (Combat / Infiltration Core)

> Status: design — 코어 전투 루프 명세 (프로토타입 1차 기준 + 확장 설계)
> Read with: `GAME_CONCEPT.md`, `SECURITY_AI_DESIGN.md`, `EXPLORER_INTERACTION_DESIGN.md`
> Owns: 명령 큐, AP/명령 슬롯, FileOperation 진행률, 턴 흐름, Trace 정산 규칙, 승패 판정

---

## 1. 설계 선언

ProjectFR의 "전투"는 적의 HP를 깎는 행위가 아니다.
**파일 작업이 끝날 때까지 보안 모듈을 막고, 속이고, 우회하며 흔적을 관리하는 행위**다.

> 기존 전투: 적 발견 → 공격 → HP 감소 → 처치 → 보상
> ProjectFR: 목표 발견 → 작업 시작 → 보안 반응 → Trace 관리 → 작업 완료 → 탈출

따라서 이 문서는 "데미지 공식"이 아니라 **시간·진행률·발각의 삼각 균형**을 정의한다.

핵심 긴장 구조:

```text
나는 작업을 끝내야 한다 (시간 필요)
   ↕
작업할수록 Trace가 오른다 (시간이 적이 된다)
   ↕
Trace가 오르면 보안이 작업을 방해한다 (시간이 더 필요해진다)
```

이 악순환을 끊는 도구가 곧 전술이다: 스턴, 미끼, 로그 위조, 은닉, 우회.

---

## 2. 턴 구조 — 명령 큐 기반 계획형 턴제

채택 방식: **계획형 턴제 (Plan → Execute)**.
한 줄 원칙: *전략은 턴제처럼, 표현은 실시간처럼.*

입력은 **포인터 전용**이다. 계획형 턴제의 리듬상 타이핑이 필요 없도록, 모든 명령은
마우스 클릭/드래그로 큐에 적재한다. (조작 모델 상세는 `EXPLORER_INTERACTION_DESIGN.md §1.1`)

### 2.1 한 턴의 단계

```text
[Plan Phase]   플레이어가 명령을 큐에 적재 (AP 소모 예약)
      ↓
[Lock-in]      Execute 버튼 → 큐 확정
      ↓
[Resolve]      큐를 순서대로 처리 (짧은 애니메이션)
      ↓
[Security]     보안 에이전트가 순찰/감시/추적 1스텝 진행
      ↓
[Upkeep]       진행 중 작업 % 갱신, Trace 정산, 상태효과 만료, 승패 체크
      ↓
다음 턴
```

### 2.2 AP / 명령 슬롯

한 턴에 예약 가능한 명령 수를 **AP(Action Points)**로 표현한다.

| 항목 | 기본값 `[balance]` | 비고 |
|---|---|---|
| 턴당 기본 AP | 3 | 빌드/장비로 +1~2 |
| 즉시 액션 비용 | 1 AP | Access, Sort 등 |
| 작업 시작 비용 | 1 AP | 진행률 작업은 "시작"에만 AP 소모, 진행 자체는 자동 |
| 무거운 액션 비용 | 2 AP | Permission Override, Inject |
| 미사용 AP | 소멸 (이월 없음) | 빌드 "AP Bank"로 일부 이월 가능 |

> 설계 의도: 작업 "시작"에만 AP가 들고 진행은 턴마다 자동으로 흐르게 해서,
> 플레이어가 여러 작업을 병렬로 굴리며 그 사이에 방해를 끼워 넣는 멀티태스킹 감각을 만든다.

### 2.3 큐 예시

```text
작업 큐 (AP 3/3)
1. [1 AP] Guard Scanner 기절
2. [1 AP] family_photo.jpg 복원 시작 (2턴 작업)
3. [1 AP] deletion_report.log 위조 예약

[명령 실행]
```

실행 연출:

```text
작업 1/3: Scanner.stun 실행 중...
작업 2/3: family_photo.jpg 복원 시작 (0% → 진행 큐 등록)
작업 3/3: deletion_report.log 위조 시작 (0% → 진행 큐 등록)
— Security Phase —
Guard Scanner: 기절 (2턴)
Indexer Scout: 최근 수정 폴더로 순찰 이동
— Upkeep —
family_photo.jpg 복원 50%
deletion_report.log 위조 50%
Trace +3% (작업 2건 진행 중)
```

---

## 3. FileOperation — 진행률 작업 시스템

### 3.1 작업 상태 머신

```text
Queued → InProgress → (Completed | Interrupted | Cancelled)
                 ↑__________|  (방해 후 재개 가능)
```

| 상태 | 의미 | UI 표현 |
|---|---|---|
| Queued | 큐에 적재됨, 아직 미시작 | 큐 패널 항목 |
| InProgress | 진행 중 (%) | `복사 중... 40%` |
| Completed | 완료, 효과 발동 | `✓ 완료` |
| Interrupted | 보안에 의해 일시 중단 | `‖ 중단 80%` (재개 가능) |
| Cancelled | 플레이어/시스템이 취소 | 진행률 소멸 |

**중요 규칙:** 효과는 오직 `Completed` 시점에만 발동한다. 복원 90%는 "거의 복원"이 아니라 "아직 복원 안 됨"이다. 이 규칙이 마지막 1턴을 버티는 긴장을 만든다.

### 3.2 작업 시간 결정 공식

```text
작업 턴 수 = ceil( BaseTime × SizeFactor × SecurityFactor / BuildBonus )
```

| 변수 | 설명 | 예시 값 `[balance]` |
|---|---|---|
| BaseTime | 작업 종류 기본 시간 | Copy 1, Restore 2, Rewrite Log 1, Decompress 2 |
| SizeFactor | 파일 크기 계수 | 소형 ×1, 중형 ×1.5, 대형 ×2 |
| SecurityFactor | 파일/폴더 보안 가중 | 일반 ×1, 보안 ×1.5, 시스템 ×2 |
| BuildBonus | 빌드/툴 가속 | 기본 1, Archive 빌드 압축 ×0.5 등 |

### 3.3 작업별 레퍼런스 테이블

| 파일 | 작업 | 기본 소요 | 완료 효과 | 위험 |
|---|---|---|---|---|
| notes.txt | Copy | 즉시 | 클립보드 적재 | Trace 낮음 |
| family_photo.jpg | Restore | 2턴 | 기억 파일 회수, 스토리 단서 | 기억 파일·감정 가치 |
| employee.db | Copy | 3턴 | 대용량 자료 회수 | 대량 복사 → Trace |
| secrets.enc | Access | 분석 1턴 후 가능 | 암호 해제 후 열람 | 암호화, 실패 시 alert |
| deletion_report.log | Rewrite Log | 1~2턴 | Trace/Tracked 제거 | 위조 행위 자체가 Trace |
| zone17_backup.zip | Decompress | 2턴 | 은닉된 기억 노출 | Indexer 반응 |
| shortcut_to_home.lnk | Repair | 2턴 | 숨겨진 경로 개방 | 새 구역 노출 |

### 3.4 작업 방해 (Interrupt)

진행 중 작업은 보안 반응으로 중단될 수 있다. (상세 트리거는 `SECURITY_AI_DESIGN.md` §4)

| 방해원 | 트리거 | 결과 |
|---|---|---|
| Guard Scanner | 작업 노드 감시 진입 | 작업 일시 중단, Trace +`[balance]` |
| Antivirus Heavy | 위험 작업(Inject/Shift Delete) 포착 | 강제 중단 + 진행률 일부 손실 |
| Backup Repairer | 삭제/복원 작업 대상 | 원본 무결성 복구 시도(작업 무효화 가능) |
| Firewall Sentinel | 잠긴 폴더 내 작업 | 폴더 봉쇄, 작업 Cancelled |

방어 수단: 작업 시작 전 또는 도중에 스턴/미끼/은닉/로그 위조로 방해원을 무력화한다.

---

## 4. Trace — 런 단위 발각도

Trace는 "시스템이 꼬리 없는 나를 얼마나 정확히 포착했는가"를 0~100%로 나타낸다.
일정 임계값마다 보안 반응 단계가 오른다.

### 4.1 Trace ↔ 보안 단계 매핑

| Trace 구간 `[balance]` | 보안 단계 | 효과 요약 |
|---|---|---|
| 0–19% | Passive | 기본 순찰, 로그만 수집 |
| 20–39% | Suspicious | 의심 폴더 감시, 스캔 주기 단축 |
| 40–59% | Active Scan | 최근 조작 파일 추적, 검색 로그 분석 |
| 60–79% | Quarantine | 폴더 봉쇄, 이동 경로 차단 |
| 80–99% | Purge | 플레이어 제거 시도 |
| 100% | CleanSweep | 오래된·미등록 파일 즉시 삭제 실행 |

### 4.2 Trace 증감 테이블

| 행동 | ΔTrace `[balance]` |
|---|---|
| Delete (휴지통) | +5% |
| Shift Delete (완전 삭제) | +12% |
| 대량 복사(대형 파일) | +6% |
| Search 사용 | +4% |
| 주소창 이동 실패 | +8% |
| Permission Override | +10% |
| 같은 폴더 반복 행동(3회+) | +3%/회 |
| Tracked 노드에서 작업 시작 | +추가 4% |
| — | — |
| Rewrite Log 완료 | −8% + Tracked/Pressure 제거 |
| temp 폴더 경유 | −3% |
| 압축 은닉 | −2% |
| 볼주머니 은닉(ghost화) | −2% (소폭, 지속) |
| 보안 모듈 스턴 | 직접 감소 X, 추적 지연 |

### 4.3 설계 노트: Trace는 양방향 자원

Trace는 단순 페널티 게이지가 아니라 **밀고 당기는 자원**이다.
공격적 빌드는 일부러 Trace를 올려 보안을 한곳으로 유인하고, Ghost 빌드는
Trace를 바닥에 붙여 무반응 침투를 노린다. 두 플레이가 모두 유효해야 한다.

---

## 5. CursorAgent — 플레이어 본체

| 속성 | 설명 |
|---|---|
| Position | 현재 폴더 내 노드 좌표 |
| CurrentFolderPath | 현재 열려 있는 폴더 경로 |
| AP | 턴당 명령 슬롯 |
| Pulse Stability | 무선 펄스 안정도. 낮으면 Wireless Pulse 액션 실패율↑ |
| Status FX | Tracked / Exposed / 등 일시 상태 |

플레이어는 전사가 아니라 **조작자**다. 직접 타격 능력이 없고, 모든 영향력은 파일 작업과 보안 교란을 경유한다. 이동(MoveCursor)은 그 자체로 감시·Trace를 유발할 수 있는 행동이다.

---

## 6. 액션 카탈로그 (전투 관점)

`GAME_CONCEPT §17`의 액션을 **전투 역할**로 재분류한다.

### 6.1 진행률 작업 (시간 소요)

| 액션 | 역할 | 완료 효과 |
|---|---|---|
| Copy | 회수 | 클립보드 적재 |
| Restore | 구출 | 삭제/손상 파일 복원 |
| Compress | 은닉 | 파일을 zip에 담아 스캔 회피 |
| Decompress | 개방 | zip 내부 접근 |
| Rewrite Log | 흔적 관리 | Trace 감소 + Tracked/Pressure 제거 |

### 6.2 즉시 액션 (1턴 내 발동)

| 액션 | 역할 | 효과 |
|---|---|---|
| Access | 정찰 | 노드 정보 확인 |
| Move | 재배치 | 노드/커서 이동 |
| Stun | 무력화 | 보안 모듈 N턴 정지 |
| Decoy | 유인 | 미끼 생성, 보안 경로 변경 |
| Search | 탐지 | 목표 후보 표시(Trace↑) |
| Show Hidden | 개방 | 숨김 파일 노출 |

### 6.3 고위험 액션 (Trace 급등)

| 액션 | 역할 | 위험 |
|---|---|---|
| Shift Delete | 완전 제거 | Trace 급등, Backup 반응 |
| Inject | 감염 | 보안 모듈 교란, Antivirus 반응 |
| Permission Override | 강제 접근 | 잠긴 폴더 3턴 임시 개방 후 재잠금 |
| Wireless Pulse | 추적 회피 | 불안정, 실패 시 Trace↑ |

### 6.4 행동 판정 — 운 요소 (Action Rolls)

로그라이크답게 일부 액션은 **확정이 아니라 확률 판정**을 거친다.
단, 계획형 턴제이므로 **확률은 커밋(실행) 전에 항상 공개**된다 — 정보 없는 도박은 없다.
(전체 RNG 프레임워크와 완화 수단은 `RANDOMNESS_DESIGN.md`)

| 액션 | 판정 여부 | 기본 성공률 `[balance]` | 결과 갈래 |
|---|---|---|---|
| Copy / Restore / Compress | 확정(판정 없음) | — | 진행률만 흐름 |
| Rewrite Log | 확정 | — | 안정적인 흔적 관리 수단 |
| Wireless Pulse | 판정 | Pulse Stability 기반 60~85% | 성공: 감시 회피 / 실패: Trace↑ |
| Inject | 판정 | 70% | 성공: 모듈 교란 / 실패: Antivirus 반응 |
| Permission Override | 판정 | 75% | 성공: 3턴 개방 / 실패: Trace↑ + 즉시 alert |
| Decoy | 판정 | 80% | 성공: 유인 / 실패: 미끼 간파됨 |
| ShortcutLink 점프(미검증) | 판정 | 50% | 성공: 깊은 구역 / 실패: honeypot |

판정 4단계(크리티컬/성공/실패/펌블) 모델과 변동성 완화는 `RANDOMNESS_DESIGN.md §3`을 따른다.

> 설계 원칙: **확정 작업(회수·복원·위조)이 게임의 뼈대**이고, **판정 액션은 양념**이다.
> 운으로 한 방에 죽거나 무조건 이기지 않으며, 운은 "더 빠르고 위험한 길"을 여는 선택지로만 쓰인다.

---

## 7. 승패 판정

### 7.1 승리 조건

```text
승리 = (목표 달성) AND (Exit 도달/탈출 완료) AND (제한 턴 이내) AND (플레이어 생존)
```

- 목표 달성: 미션 ObjectiveType 충족 (Extract/Delete/Scan/Modify/Restore 등 — `LOBBY_MISSION_DESIGN.md`)
- Exit 해금: 목표 완료 시 `UnlockExit()` 호출 → 루트 컨테이너의 extraction zone 활성화
- 탈출: 루트에서 Extract 실행

### 7.2 패배 / 미완 조건

| 조건 | 결과 |
|---|---|
| 제한 턴 초과 | 미션 실패, Heat↑ |
| Trace 100% 후 Purge에 포착 | 강제 종료, 큰 페널티 |
| 목표 미달 상태 탈출 | 부분 실패(보상 없음, Heat 소폭↑) |

### 7.3 선택적 결과: 부분 성공

목표는 달성했으나 보너스 목표 실패, 또는 높은 Trace로 탈출한 경우 **부분 성공**으로
보상을 차감 정산한다. 로그라이크 특성상 "완벽 vs 생존"의 선택지를 남긴다.

---

## 8. 모먼트 예시 (정상 플레이)

미션 목표: `/recycle_bin/cache_zone_17/family_photo.jpg` 복원

상황:
- family_photo.jpg는 휴지통의 손상 사진, 복원 2턴
- deletion_report.log가 접근 기록을 남김
- 3턴 뒤 CleanSweep Agent가 오래된 파일 삭제 예정

플레이어 플랜:

```text
[T1] 1. Guard Scanner에 Decoy 사용 (유인)
     2. family_photo.jpg 복원 시작
     3. deletion_report.log Rewrite Log 예약
[T2] 1. (복원/위조 자동 진행)
     2. 복원 완료된 사진을 볼주머니 캐시에 은닉
     3. temp 폴더로 이동
[T3] 1. Exit(루트) 이동
     2. Extract → 탈출
```

결과:

```text
Guard Scanner가 decoy.dat 쪽으로 이동
family_photo.jpg 복원 100% 완료 → ghost화
deletion_report.log 위조 성공 → Trace −8%
최종 Trace 14% (Passive 유지)
기억 파일 회수 완료 · 탈출 성공
```

이것이 ProjectFR의 이상적 전투 감각이다: 한 발도 쏘지 않고 시스템을 속여 이겼다.

---

## 9. 인접 시스템 경계 (Interfaces)

| 경계 상대 | 이 문서가 넘기는 것 | 받는 것 |
|---|---|---|
| `SECURITY_AI_DESIGN` | Trace 값, 작업 진행 이벤트 | 반응 단계, 방해/추적 결과 |
| `EXPLORER_INTERACTION` | "선택→명령" 입력 결과 | 큐에 적재할 액션 정의 |
| `LOBBY_MISSION_DESIGN` | 승패 판정 결과 | ObjectiveType, TurnLimit, 활성 모듈 |
| `PROGRESSION_ECONOMY` | 런 종료 스냅샷 | AP 보너스, 빌드 효과(작업 규칙 변경) |

---

## 10. 구현 우선순위 (전투 한정)

| 순위 | 항목 | 현재 상태(REDESIGN_STEPS 기준) |
|---|---|---|
| P0 | 명령 큐 + Execute/Clear | 착수(Step 4) |
| P0 | FileOperation 진행률 + Completed-only 효과 | 착수(Step 4) |
| P0 | Trace 5단계 매핑 | 부분 |
| P1 | Interrupt(방해) 처리 | 부분(감시 중 Trace↑) |
| P1 | AP/명령 슬롯 정식화 | 미구현 |
| P2 | 부분 성공 정산 | 미구현 |
| P2 | Wireless Pulse 불안정 모델 | 미구현 |
