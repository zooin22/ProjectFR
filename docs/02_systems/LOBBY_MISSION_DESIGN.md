# ProjectFR - 로비 & 미션 보드 기획서 (Lobby & Mission Board)

> Status: design — 캠페인 허브(로비) + 미션 구조 명세
> Read with: `MISSION_FACTIONS.md`, `MISSION_DATA_MODEL.md`, `PROGRESSION_ECONOMY_DESIGN.md`
> Owns: 로비 흐름, 미션 보드, 의뢰 선택/브리핑, 출격 준비, 미션 구조, 목표 타입, 보스 미션

---

## 1. 설계 선언

로비는 단순 메뉴가 아니라 **캠페인 루프의 결정 지점**이다.
플레이어는 여기서 "어떤 세력의 일을, 어떤 빌드로, 어떤 위험을 감수하고 할 것인가"를 고른다.

> 침투 루프가 "어떻게 작전을 수행하는가"라면,
> 로비는 "무슨 작전을, 누구를 위해, 무엇을 걸고 하는가"를 다룬다.

로비의 세 가지 책임:

1. **선택지 제시** — 가용 미션을 의뢰인/위험/보상과 함께 보여준다.
2. **상태 요약** — Credits/Reputation/Heat와 그로 인한 난이도 보정을 명확히 보여준다.
3. **출격 준비** — 빌드/장비/소비 도구를 점검하고 침투 루프로 진입시킨다.

---

## 2. 로비 흐름

```text
[로비 진입]
   ↓
캠페인 상태 표시 (Credits · Reputation · Heat · 보안 보정)
   ↓
미션 보드 탐색 (가용 미션만 노출 — 잠긴 미션은 숨김)
   ↓
의뢰 선택 → 브리핑 열람 (의뢰인 · 목표 · 위험 · 보상 · 숨은 진실 힌트)
   ↓
출격 준비 (빌드 확인 · 소비 도구 장착 · 예상 보안 모듈 확인)
   ↓
[침투 루프 진입]
   ↓ (귀환)
결과 정산 (보상/페널티/평판/Heat) → 빌드 갱신 → 로비 복귀
```

---

## 3. 미션 보드

### 3.1 보드 구성

`MissionBoardFactory.CreateDefaultBoard`가 생성하는 의뢰 목록.
`CampaignState.GetAvailableMissions()`로 **선행 조건을 만족한 미션만** 노출한다.

### 3.2 기본 보드 (현재)

| Id | 제목 | 의뢰인 | 세력 | 타입 | 보상 | 선행 |
|---|---|---|---|---|---|---|
| mission_extract_boss | Archive Lift | Morrow Proxy | Corporate Espionage | Extract | 90cr rep+2 | — |
| mission_delete_readme | Loose End Cleanup | Northline Legal | Legal Fixers | Delete | 65cr rep+1 | — |
| mission_scan_cache | Cache Recon | Helix Ops | Security Contractors | Scan | 55cr rep+1 | — |
| mission_extract_readme | Mirror Snatch | Glass Key Collective | Civic Leakers | Extract | 60cr rep+2 | — |
| mission_delete_boss | Burn Notice | Ember Circuit | Leak Brokers | Delete | 95cr rep+1 | mission_extract_boss |

### 3.3 의뢰 충돌 구조 (설계의 핵심)

같은 타깃을 두고 상반된 의뢰가 존재하는 것이 ProjectFR 서사의 긴장축이다.

- `Archive Lift`(회수) → `Burn Notice`(소각): 같은 `BossZipPath`. Morrow Proxy가 회수를 시키고,
  나중에 Ember Circuit이 *같은 파일을* 태우라 의뢰한다 — 단, "그곳에 닿을 수 있음"을 증명한 뒤에야.
- `Mirror Snatch`(Glass Key, Extract) ↔ `Loose End Cleanup`(Northline, Delete): 같은 `RootReadmePath`.
  한쪽은 폭로를 위해 회수, 한쪽은 은폐를 위해 삭제.

> 설계 의도(데이터는 있으나 미연결): 같은 `TargetPath`를 가진 대립 의뢰를 보드에서 시각적으로
> 강조하는 `ConflictGroup` 필드를 추후 추가. 플레이어가 "어느 편을 들 것인가"를 의식하게 만든다.

---

## 4. 의뢰인 세력 (요약)

상세 톤·스토리 훅은 `MISSION_FACTIONS.md` 참조.

| 세력 | 분류 | 주는 일의 톤 | 위험 |
|---|---|---|---|
| Morrow Proxy | Corporate Espionage | 차갑고 실무적, 돈 잘 줌 | 실패 시 흔적 큼 |
| Northline Legal | Legal Fixers | 깔끔한 합법의 외피, 실상은 은폐 | 조용한 작전 강요 |
| Helix Ops | Security Contractors | 전문적·건조 | 플레이어를 소모품 취급 |
| Ember Circuit | Leak Brokers | 빠르고 공격적, 고수익 | 신뢰 어려움, 추적도↑ |
| Glass Key Collective | Civic Leakers | 이상주의적 | 자료 무손상 회수 압박 |

---

## 5. 미션 데이터 구조 (로비 표시 관점)

코드 형태는 `MISSION_DATA_MODEL.md`가 캐논. 로비는 아래 필드를 화면에 렌더한다.

| 필드 | 로비 표시 |
|---|---|
| Title + Client.Name | 의뢰 제목 + 의뢰인 |
| Client.Faction | 세력 분류 |
| Briefing | 작전 브리핑 |
| ObjectiveType + TargetPath | 목표 타입 + 목표 경로 |
| TurnLimit (heat 보정) | 제한 턴(Heat 티어 반영) |
| Client.Agenda / RiskNote | 의뢰 성향 / 리스크 노트 |
| Reward / Penalty | 보상 / 실패 페널티 |
| (operator status) | Credits·Reputation·Heat + 적 보정 |

### 5.1 브리핑 카드 예시

```text
┌─ Mirror Snatch ─────────────────────────────┐
│ 의뢰인: Glass Key Collective (Civic Leakers) │
│ 목표:   Extract  /root/README.md             │
│ 제한:   8턴 (Heat ELEVATED: -1턴 적용)        │
│ 성향:   "있는 그대로 회수해. 한 글자도 바꾸지 마." │
│ 리스크: 무손상 회수 압박. 위조/손상 시 실패.     │
│ 보상:   60cr · 평판 +2                        │
│ ── 단서 ──                                    │
│ Northline Legal도 같은 파일을 노린다는 소문.    │
└─────────────────────────────────────────────┘
```

---

## 6. 목표 타입 (Objective Types)

| 타입 | 설명 | 완료 판정(RegisterAction 매핑) | 구현 |
|---|---|---|---|
| Extract | 특정 파일 회수 | copy | 기본 보드 사용 |
| Delete | 특정 파일/폴더 삭제 | delete | 기본 보드 사용 |
| Scan | 특정 폴더/파일 분석 | inspect | 기본 보드 사용 |
| Modify | 내용/속성 변조 | logforge | enum/처리 O, 예시 미션 X |
| Escape | 제한 조건 내 탈출 | extract(RegisterEscape) | enum/처리 O, 예시 미션 X |
| Restore | 휴지통/손상 복원 | (restore) | 설계 |
| Replace | 가짜 파일로 교체 | — | 설계 |
| Plant | 특정 위치에 파일 심기 | — | 설계 |
| Rescue | 삭제 예정 파일 보호 후 회수 | — | 설계 |
| Sabotage | 프로세스/폴더 기능 마비 | — | 설계 |
| Escort Data | 데이터 패킷 경로 이동 | — | 설계 |
| Prove Existence | 미등록 존재의 증거 확보 | — | 설계(서사 핵심) |

> 우선 구현 권장: `Modify`/`Escape`는 enum·처리는 이미 있으니 **예시 미션 1개씩** 추가하면
> 즉시 활용 가능. `Prove Existence`는 메인 스토리의 정체성 목표로 별도 연출 필요.

---

## 7. 미션 구성 요소 (전체)

각 미션이 정의하는 항목:

```text
- 미션 이름 / 의뢰인 / 브리핑
- 목표 타입 / 목표 경로
- 제한 턴 또는 제한 작업 수
- 보안 레벨 / 활성 보안 모듈
- 목표 파일 크기·속성
- 성공 보상 / 실패 페널티
- 선택 보너스 목표
- 숨겨진 진실 또는 반전 요소
- 삭제된 존재/기억 파일과의 연결
```

### 7.1 보너스 목표

본 목표 외 선택 목표로 보상↑ vs 위험↑를 제공한다. 예:

| 보너스 | 보상 | 추가 위험 |
|---|---|---|
| 삭제 대기열의 기억 파일 1개 추가 구출 | 평판↑ | 체류 턴↑, Trace↑ |
| 흔적 0으로 탈출(Trace < 10%) | Credits↑ | 위조 비용↑ |
| 보안 모듈 무제거 클리어 | Heat↓ | 우회 난이도↑ |

---

## 8. 출격 준비 (Pre-Run Loadout)

침투 진입 전 점검 화면. (빌드/도구 상세는 `PROGRESSION_ECONOMY_DESIGN.md`)

| 점검 항목 | 내용 |
|---|---|
| 활성 빌드 | 현재 보유 업그레이드 요약 |
| 소비 도구 | 일회성 도구 장착(미끼팩, 펄스 안정기 등) |
| 예상 보안 | 미션 보안 레벨·Heat 기반 등장 모듈 미리보기 |
| 목표 요약 | 목표 경로·타입·제한 턴 |
| 추정 난이도 | Heat 티어 반영 위험 등급 표시 |

---

## 9. 캠페인 진행 게이트

### 9.1 선행 조건 (Prerequisite)

`MissionData.PrerequisiteMissionId`(nullable). null이면 항상 가용, 값이 있으면 해당 미션
완료 후 해금. `IsMissionAvailable()`이 게이팅하고 `GetAvailableMissions()`가 선택/네비게이션을
필터링하므로 **잠긴 미션은 보드에서 도달 불가**.

> 현재는 단일 ID 선행만 지원. 향후 복수 조건·평판 임계값 기반 해금은 richer condition model 필요.

### 9.2 추천 초기 스토리 아크

| 막 | 의뢰 톤 |
|---|---|
| Act 1 — Small Contracts | Readme 삭제 · BuildCache 정찰 · Boss.zip 회수 |
| Act 2 — Contradictions | 같은 경로에 상반된 의뢰 등장, 누가 진실인지 흔들기 |
| Act 3 — Exposure | 대형 프로젝트 실체 노출, 돈·평판·진실 중 선택 |

---

## 10. 보스 미션 (OPTIMA)

최종 보스 미션은 일반 미션과 구조가 다르다. (AI 동작은 `SECURITY_AI_DESIGN.md` §8)

| 요소 | 일반 미션 | 보스 미션 |
|---|---|---|
| 목표 | 단일 ObjectiveType | 복합(복원 다수 + Sabotage + Prove Existence) |
| 보안 | 고정 모듈 N종 | OPTIMA가 8종을 동적 지휘 |
| 승리 | 목표 + 탈출 | CleanSweep Final 저지 + 삭제 대기열 복원 |
| 종결 | 결과 정산 | Final Choice(아래) |

### 10.1 Final Choice

엔딩 분기. 로비/캠페인 상태가 선택지에 영향.

```text
- OPTIMA 삭제
- OPTIMA 재학습
- Tail Registry 해체
- 기억 보관소 생성
```

---

## 11. MainMenu 렌더링 (현재 구현)

`MainMenu.RefreshMenu()`가 표시: 미션 제목+의뢰인, 세력, 브리핑, 목표타입+경로,
heat 보정 제한 턴, Agenda, RiskNote, 보상/페널티, 오퍼레이터 상태.
계약 선택(이전/다음)은 **가용 미션 범위 내**에서만 순환.

---

## 12. 인접 시스템 경계

| 경계 상대 | 넘기는 것 | 받는 것 |
|---|---|---|
| `COMBAT_SYSTEM` | ObjectiveType, TurnLimit, 활성 모듈 | 승패 판정 결과 |
| `PROGRESSION_ECONOMY` | 결과 스냅샷 | Credits/Reputation/Heat, 빌드 상태 |
| `LEVEL_GENERATION` | 목표 경로, 보안 레벨 | 생성된 폴더 구조 |
| `MISSION_DATA_MODEL` | (코드 형태 준수) | 데이터 구조 |

---

## 13. 구현 우선순위

| 순위 | 항목 | 현재 상태 |
|---|---|---|
| P0 | 미션 보드 + 가용 미션 필터 | 구현 |
| P0 | 브리핑 렌더(세력·목표·보상) | 구현 |
| P0 | 결과 정산 → 캠페인 반영 | 구현(ApplyMissionResult) |
| P1 | Modify/Escape 예시 미션 추가 | 미구현(enum만) |
| P1 | 보너스 목표 시스템 | 미구현 |
| P1 | 출격 준비 화면(loadout) | 미구현 |
| P2 | ConflictGroup 시각화 | 미구현(데이터 설계만) |
| P2 | 보스 미션 + Final Choice | 미구현 |
