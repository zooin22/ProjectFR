# ProjectFR Docs - Cleanup Notes

> 2026-06-01 통합 정리 패스 기록. 원본 6종 + 신규 시스템 기획서 9종을 하나의 일관된
> 문서 세트로 정리했다.

## 1. 폴더 구조로 재편

전체 15개 문서를 3계층으로 분류:
- `01_vision/` — GAME_CONCEPT, STORY_WORLD, MISSION_FACTIONS
- `02_systems/` — COMBAT, SECURITY_AI, EXPLORER_INTERACTION, LOBBY_MISSION,
  PROGRESSION_ECONOMY, LEVEL_GENERATION, RANDOMNESS, UI_UX, MULTI_WINDOW_EXPANSION
- `03_data_impl/` — MISSION_DATA_MODEL, PROJECT_REDESIGN_STEPS
- 루트: `00_DESIGN_INDEX.md`(허브), 본 파일

## 2. 정식 설계 결정을 전 문서에 반영

두 가지 결정을 캐논으로 확정하고, 충돌하던 원본 서술을 정리했다.

### 2.1 포인터 전용 (No-Keyboard)
- 전투가 계획형 턴제이므로 타이핑 없이 마우스만으로 조작 완결.
- `GAME_CONCEPT.md` 정리:
  - §6.2 퀵바 원칙에서 "단축키"를 보조 수단으로 격하.
  - §9.3 검색창 → "검색(타이핑 없는 클릭 기반)": Search Sweep / Filter Chips / Known Keywords.
  - §9.4 주소창 경로 입력 → "내비게이션(클릭 기반)": 브레드크럼 / 북마크 / ShortcutLink 점프.
- 캐논 명세: `EXPLORER_INTERACTION_DESIGN.md §1.1, §7, §8`.

### 2.2 운은 양념, 실력이 뼈대 (Roguelike RNG)
- 확정 작업이 뼈대, 운은 부가 선택지에만. 모든 판정 확률은 커밋 전 공개. 불운 보호 장치.
- `GAME_CONCEPT.md §4.4`에 운/무작위성 정식 결정 포인터 추가.
- 캐논 명세: `RANDOMNESS_DESIGN.md`.

## 3. 상호 참조 정리

원본 문서 front-matter의 `Read with`/`Related`에 새 시스템 문서 연결을 추가:
- STORY_WORLD → LOBBY_MISSION_DESIGN
- MISSION_FACTIONS → LOBBY_MISSION_DESIGN
- MISSION_DATA_MODEL → LOBBY_MISSION_DESIGN
- MULTI_WINDOW_EXPANSION → UI_UX_DESIGN, EXPLORER_INTERACTION_DESIGN
- PROJECT_REDESIGN_STEPS → "시스템 명세는 02_systems/가 캐논" 명시

## 4. 인덱스 갱신

`00_DESIGN_INDEX.md`에 폴더 구조 트리(§0.1)와 15개 문서 전체 맵을 반영.

## 5. 변경하지 않은 것

- 스토리·세계관(STORY_WORLD), 세력 설정(MISSION_FACTIONS), 코드 모델(MISSION_DATA_MODEL),
  구현 로그(PROJECT_REDESIGN_STEPS)의 본문 내용은 보존. 상호 참조 헤더만 보강.
- 밸런싱 수치는 여전히 `[balance]` 플레이스홀더(튜닝 단계에서 확정).
