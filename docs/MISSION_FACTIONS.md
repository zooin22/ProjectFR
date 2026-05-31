# ProjectFR - Mission Factions

> Status: active — design intent canonical; in-game wiring partial (see §Implementation Status)
> Read with: `STORY_WORLD.md`, `GAME_CONCEPT.md`, `MISSION_DATA_MODEL.md`
> Related: `res/mission/`


## Purpose

이 문서는 의뢰인 세력과 그들이 주는 미션 톤을 정리한 초안이다.
나중에 스토리라인, 평판 분기, 연속 의뢰 체인 설계의 기준점으로 쓴다.

---

## 1. Morrow Proxy

- **세력 분류:** Corporate Espionage
- **역할:** 기업 정보 절도 브로커
- **주요 의뢰:** 자료 회수, 연구 패키지 탈취, 사전 유출
- **톤:** 차갑고 실무적. 돈은 잘 주지만 뒤처리는 플레이어 몫.
- **위험:** 큰 보상 대신 실패 시 흔적이 크게 남음

### Story Hooks
- 훔친 자료가 단순 연구가 아니라 생체/감시 프로젝트일 수 있음
- 경쟁사 의뢰처럼 보였지만 사실 원 소유주 측의 내부 싸움일 수 있음

---

## 2. Northline Legal

- **세력 분류:** Legal Fixers
- **역할:** 기업/권력층 리스크 제거 대행
- **주요 의뢰:** 문서 삭제, 로그 정리, 증거 은폐
- **톤:** 겉으로는 합법적이고 깔끔하지만, 실상은 진실 은폐와 가깝다.
- **위험:** 조용한 작전을 원하며 실패를 매우 싫어함

### Story Hooks
- 플레이어가 지우는 문서가 내부고발 증거일 수 있음
- 후반에 “누구를 위해 흔적을 지웠는가”가 되돌아올 수 있음

---

## 3. Helix Ops

- **세력 분류:** Security Contractors
- **역할:** 시스템 정찰 및 보안 평가 용역
- **주요 의뢰:** 정찰, 구조 파악, 감염 샘플 분석
- **톤:** 전문적이고 건조함. 노골적인 악당은 아니지만 목적 중심.
- **위험:** 플레이어를 소모품처럼 쓰는 태도를 보일 수 있음

### Story Hooks
- 정찰 의뢰가 사실 선제 공격 준비일 수 있음
- 클라이언트가 보안업체인지 공격업체인지 경계가 흐려질 수 있음

---

## 4. Ember Circuit

- **세력 분류:** Leak Brokers
- **역할:** 위험 자료를 거래하는 암시장 브로커
- **주요 의뢰:** 핵심 파일 삭제, 회수, 교체
- **톤:** 빠르고 공격적. 고수익이지만 신뢰하기 어려움.
- **위험:** 실패 시 추적도 상승이 큼

### Story Hooks
- 같은 타깃을 회수해 달라는 의뢰와 삭제해 달라는 의뢰가 충돌할 수 있음
- 고객이 여러 명인 척하지만 실제론 한 조직일 수도 있음

---

## 5. Glass Key Collective

- **세력 분류:** Civic Leakers
- **역할:** 공익 제보 네트워크 / 시민 폭로 집단
- **주요 의뢰:** 증거 회수, 안전한 추출, 정보 공개 준비
- **톤:** 이상주의적이지만 현실적으로는 위험한 일을 시킨다.
- **위험:** 자료를 파괴하지 않고 정확히 회수해야 한다는 압박이 큼

### Story Hooks
- 진실 폭로가 더 큰 혼란을 부를 수 있음
- 플레이어가 어느 순간 “고용된 해커”가 아니라 사건의 공범이 될 수 있음

---

## Relationship Directions

### Morrow Proxy vs Glass Key Collective
- 같은 자료를 두고
  - 한쪽은 팔기 위해 회수
  - 한쪽은 폭로하기 위해 회수
- 동일 타깃, 다른 윤리 구조를 만들기 좋음

### Northline Legal vs Glass Key Collective
- 한쪽은 증거 삭제
- 한쪽은 증거 확보
- 가장 직접적인 대립축

### Helix Ops vs Ember Circuit
- 한쪽은 시스템 파악
- 한쪽은 시스템 파괴/탈취
- 플레이어가 양쪽을 오가며 더 큰 그림을 보게 만들 수 있음

---

## Recommended Early Story Arc

### Act 1 - Small Contracts
- Readme 삭제
- BuildCache 정찰
- Boss.zip 회수

### Act 2 - Contradictions
- 같은 경로를 두고 상반된 의뢰 등장
- 어느 세력이 진실을 말하는지 흔들기

### Act 3 - Exposure
- 단순 문서/캐시가 아니라 대형 프로젝트 실체 노출
- 플레이어가 돈, 평판, 진실 중 무엇을 우선할지 묻기

---

## Implementation Status

이 섹션은 문서의 설계 의도(Design Intent)와 실제 코드에 연결된 것(Wired)을 구분한다.
상세 코드 매핑은 `MISSION_DATA_MODEL.md`를 참고.

### 현재 코드에 연결된 것 (Wired)

- `MissionClientProfile` — `FactionId` (typed enum), `Faction` (derived display string), `Name`, `Agenda`, `RiskNote`. 5개 세력 인스턴스 `MissionBoardFactory`에 정의.
- `MissionData` — 제목, 목적 타입(`MissionObjectiveType`), 타깃 경로, 턴 제한, 보상/페널티. `PrerequisiteMissionId : string?` 추가 — null이면 항상 해금, 값이 있으면 해당 미션 ID 완료 후 해금.
- `MissionObjectiveType` 열거형 — `Extract`, `Delete`, `Scan`, `Modify`, `Escape` 정의됨. 기본 보드는 `Extract/Delete/Scan` 사용.
- `CampaignState` — `Credits`(초기 100), `Reputation`(전역), `Heat` 전역 관리. **세력별 평판**: `GetFactionReputation(FactionId)` (딕셔너리 기반, 기본 0). **완료 미션 추적**: `_completedMissionIds`; `ApplyMissionResult`가 성공 시 미션 ID 기록. **가용 미션 필터**: `IsMissionAvailable(MissionData)` + `GetAvailableMissions()` — 네비게이션/선택이 잠긴 미션을 건너뜀.
- `MissionProgress` — 단일 목표 완료 추적; `RegisterAction` 매핑 (Extract→copy, Delete→delete, Scan→inspect, Modify→logforge) + `RegisterEscape` 지원.
- `MissionResult` + `ApplyMissionResult` — 결과 스냅샷; Credits/전역 Reputation/Heat 반영, 세력 평판도 함께 업데이트.
- `MainMenu` — 세력명, 브리핑, 의뢰 성향, 리스크, 보상, 오퍼레이터 상태 표시. 계약 선택(이전/다음)은 가용 미션 범위 내에서만 순환.
- **미션 선행 예시**: "Burn Notice" (`mission_delete_boss`) — "Archive Lift" (`mission_extract_boss`) 성공 후 해금. 같은 `BossZipPath`를 두고 회수(Morrow Proxy) → 소각(Ember Circuit) 의뢰 충돌 구조.

### 설계 의도만 있고 아직 미연결 (Design Intent Only)

- 세력 평판 수치가 아직 런타임 분기에 활용되지 않음 — 데이터 구조는 존재하나 잠금 조건이나 위험도 보정 등에 연결되지 않았다.
- 복합 선행 조건 — 현재 `PrerequisiteMissionId`는 단일 ID만 지원. 복수 조건·평판 임계값 기반 해금은 미구현.
- `MissionObjectiveType.Modify`·`Escape` 사용 미션 없음 — 열거형 및 `MissionProgress` 처리는 구현됨, 기본 보드에 예시 없음.
- 충돌 감지 — 같은 `TargetPath`를 두고 대립하는 세력 의뢰 간 긴장이 코드로 표면화되지 않음.

> 이 섹션은 구현이 진행됨에 따라 갱신한다. Wired 항목 추가 시 관련 클래스/파일명을 함께 기록할 것.
