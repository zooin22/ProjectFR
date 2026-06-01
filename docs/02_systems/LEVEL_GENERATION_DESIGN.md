# ProjectFR - 레벨 생성 기획서 (Level / Folder Structure Generation)

> Status: design — 침투 무대(폴더 구조) 생성 규칙 명세
> Read with: `COMBAT_SYSTEM_DESIGN.md`, `SECURITY_AI_DESIGN.md`, `LOBBY_MISSION_DESIGN.md`
> Owns: 폴더 트리 생성, 노드 역할(ExplorerNodeRole), 목표/자산/함정/환경 배치, 보안 레벨 매핑, Exit

---

## 1. 설계 선언

ProjectFR의 "던전"은 동굴이나 방이 아니라 **폴더 트리**다.

> 서버는 도시, 폴더는 거리와 건물, 파일은 기억·신분·재산.

레벨 생성의 목표는 "랜덤한 미로"가 아니라, **읽을 수 있는 침투 공간**을 만드는 것이다.
플레이어가 트리 구조를 보고 "목표는 아마 저 깊은 곳, 우회로는 temp를 통해, 위험은 logs 근처"
라고 추론할 수 있어야 한다. 즉, 구조 자체가 정보다.

---

## 2. 노드 모델

### 2.1 ExplorerNodeKind (무엇인가)

| Kind | 의미 |
|---|---|
| Folder | 컨테이너, 진입 가능한 구역 |
| File | 단일 데이터 객체 |
| Archive | zip 등 압축 컨테이너(내부에 노드 보유) |
| RecycleBin | 휴지통(삭제된 노드 수용) |
| ShortcutLink | 다른 경로로의 바로가기(.lnk) |

### 2.2 ExplorerNodeRole (게임에서 무슨 역할인가)

같은 File이라도 Role에 따라 전혀 다르게 동작한다.

| Role | 역할 | 예시 |
|---|---|---|
| Objective | 미션 목표물 | family_photo.jpg, Boss.zip |
| Asset | 부가 가치(보너스 회수, 단서) | diary.txt, employee.db |
| Trap | 함정(honeypot, alert 유발) | bait_login.dat |
| Environment | 배경/구조(이동·은닉 경유) | temp/, logs/ |
| Memory | 삭제된 존재(감정 가치 + 스토리) | unsent_mail.txt |
| Security | 보안 객체/잠금 | firewall.sys, locked/ |
| Decoy | 시스템/플레이어 생성 미끼 | decoy.dat |

---

## 3. 레벨 = 미션 요구사항의 함수

레벨은 무작위가 아니라 **미션이 요구하는 형태로** 생성된다.

```text
입력: ObjectiveType, TargetPath, 보안 레벨, Heat 티어, 세력 톤
   ↓
폴더 골격 생성 (루트 + 주요 구역)
   ↓
목표 노드 배치 (TargetPath 보장)
   ↓
경로·우회로·환경 노드 배치
   ↓
보안 모듈 배치 (보안 레벨/Heat 기반)
   ↓
함정·미끼·자산 산포
   ↓
Exit / extraction zone 설정
   ↓
출력: BattleDungeon (탐색 가능한 트리)
```

핵심 규칙: **목표 경로는 반드시 도달 가능해야 한다.** 우회로가 막혀도 최소 한 개의
유효 경로(Override나 ShortcutLink 포함 가능)가 보장되어야 한다.

---

## 4. 폴더 골격 템플릿

미션 톤에 따라 골격 템플릿을 고른 뒤 변주한다.

### 4.1 표준 서버 템플릿

```text
/root
├── system/         (보안 밀집, 고위험)
│   ├── firewall.sys      [Security]
│   └── logs/             (로그 위조 대상)
├── cache/          (햄스터 생활권, 저위험 진입로)
│   └── archive/
│       └── zone_17/       (기억 파일 밀집 — 스토리 구역)
├── temp/           (Trace 정리 경유지) [Environment]
├── users/          (자산·단서)
├── recycle_bin/    (삭제된 존재 수용) [RecycleBin]
└── [extraction]    (탈출 지점, 목표 완료 시 활성)
```

### 4.2 구역별 성격

| 구역 | 보안 | 역할 | 위험 |
|---|---|---|---|
| system/ | 높음 | 보안 모듈 밀집, 로그 위조 핵심 | Trace 급등 |
| cache/ | 낮음 | 진입로, 기억 파일 | 낮음 |
| temp/ | 낮음 | 경유·은닉·미끼 보관 | Trace 정리 가능 |
| users/ | 중간 | 자산·부가 단서 | 보통 |
| recycle_bin/ | 중간 | Restore 목표 | Backup 반응 |
| archive/zone_17 | 가변 | 스토리 핵심, Memory 노드 | CleanSweep 카운트다운 |

---

## 5. 보안 배치 규칙

(모듈 행동은 `SECURITY_AI_DESIGN.md`)

### 5.1 보안 레벨 → 모듈 구성

| 보안 레벨 | 등장 모듈 `[balance]` |
|---|---|
| 1 (입문) | Guard Scanner ×1, Indexer Scout ×1 |
| 2 | + Backup Repairer ×1 |
| 3 | + Firewall Sentinel(잠긴 폴더 1개) |
| 4 | + Antivirus Heavy |
| 5+ | + AI Monitor / Permission Daemon (Heat 6+에서) |

### 5.2 배치 원칙

- 보안 모듈은 **목표 근처일수록 조밀**하게(목표선 압박, `SECURITY_AI §4.1`과 일치).
- Guard Scanner는 patrol route를 따라 배치 — 순찰 사이의 빈 틈이 플레이어의 기회.
- Firewall Sentinel은 우회로의 "관문"에 두어 Override/ShortcutLink 선택을 만든다.
- CleanSweep Agent는 Memory 노드가 있는 구역에 카운트다운으로 배치(시한 압박).

---

## 6. 목표·함정·자산 산포

### 6.1 목표 노드

- `TargetPath`에 정확히 1개 배치. ObjectiveType에 맞는 Kind/Role 보장.
  (Extract→회수 가능한 File/Archive, Restore→RecycleBin 내 손상 File 등)
- 목표는 탐색기 선택 박스처럼 강조 표시.

### 6.2 함정 (Trap)

| 함정 | 트리거 | 효과 |
|---|---|---|
| honeypot 폴더 | 주소창 오입력/잘못된 Search | 진입 시 Trace↑, 모듈 소환 |
| bait 파일 | Copy/Open | alert 발생 |
| 위장 시스템 파일 | Delete | Antivirus 반응 |

함정은 "정보 부족에 대한 처벌"이다. Analyst 빌드/Scan으로 사전 식별 가능해야 공정하다.

### 6.3 자산 (Asset) & 보너스

- Asset 노드는 보너스 목표(`LOBBY_MISSION §7.1`)와 연결.
- Memory 노드는 회수 시 스토리 단서 + 평판 보너스. 일부는 삭제 카운트다운을 가져
  "구할지 두고 갈지" 선택을 만든다.

---

## 7. Exit / Extraction Zone

```text
초기: extraction zone 잠김 (목표 미완)
목표 완료 → InfiltrationManager.UnlockExit() → 루트 컨테이너의 extraction 활성
탈출: 루트에서 Extract 실행 → 런 종료
```

규칙:
- 추출은 **루트 컨테이너에서만** 가능(1차 규칙). 회수물을 끝까지 운반해야 하는 긴장.
- 탈출 시 잔여 Trace/Heat 정산.
- ShortcutLink를 복구(Repair)하면 빠른 탈출 우회로가 열릴 수 있다.

---

## 8. 절차 생성 vs 수제 (Procedural vs Handcrafted)

| 미션 종류 | 생성 방식 |
|---|---|
| 일반 의뢰 | 템플릿 기반 절차 생성 + 시드 변주 |
| 스토리 핵심(zone_17, Tail Registry) | 수제 레이아웃(서사 연출 보장) |
| 보스(OPTIMA_CORE) | 수제 + 동적 보안 지휘 |

> 권장: 절차 생성으로 반복 플레이 다양성을 확보하되, 스토리 비트가 걸린 구역은 수제로
> 고정해 연출 일관성을 지킨다. 절차 생성도 "구역 성격"은 고정하고 "내부 배치"만 변주.

---

## 9. 생성 파라미터 요약

| 파라미터 | 출처 | 영향 |
|---|---|---|
| ObjectiveType / TargetPath | 미션 | 목표 노드 종류·위치 |
| 보안 레벨 | 미션 | 모듈 구성 |
| Heat 티어 | 캠페인 | 모듈 강화·추가, 제한 턴 |
| 세력 톤 | 의뢰인 | 구역 성격(은폐형/폭로형 레이아웃 뉘앙스) |
| 시드 | 런 | 내부 배치 변주 |

---

## 10. 1차 프로토타입 레벨 (권장)

첫 미션 무대 예시(`GAME_CONCEPT §23.1`):

```text
목표: /recycle_bin/cache_zone_17/family_photo.jpg 복원
또는:  /cache/archive/zone_17/deletion_report.log 회수

구성:
- 진입: cache/ (저위험)
- 핵심: zone_17 (Memory 노드 밀집 + CleanSweep 3턴 카운트다운)
- 위험: logs/deletion_report.log (접근 기록)
- 모듈: Guard Scanner, Indexer Scout, Backup Repairer (3종)
- Exit: 루트 extraction (목표 완료 시 해금)
```

---

## 11. 인접 시스템 경계

| 경계 상대 | 받는 것 | 넘기는 것 |
|---|---|---|
| `LOBBY_MISSION` | ObjectiveType, TargetPath, 보안 레벨 | 생성된 BattleDungeon |
| `SECURITY_AI` | 모듈 구성 요청, patrol route 슬롯 | 배치 좌표 |
| `COMBAT_SYSTEM` | — | 노드 역할, Exit 상태 |
| `EXPLORER_INTERACTION` | — | 트리/노드 데이터(MoveNode 등) |

---

## 12. 구현 우선순위

| 순위 | 항목 | 현재 상태 |
|---|---|---|
| P0 | 수제 1차 프로토타입 레벨 1개 | 부분(배틀 던전 존재) |
| P0 | 노드 Kind/Role 분리 | 구현(Step 1) |
| P0 | Exit/extraction 해금 흐름 | 구현(Step 6) |
| P1 | 표준 템플릿 절차 생성 | 미구현 |
| P1 | 보안 레벨 → 모듈 구성 매핑 | 미구현 |
| P2 | 함정/honeypot 시스템 | 미구현 |
| P2 | 시드 기반 내부 배치 변주 | 미구현 |
