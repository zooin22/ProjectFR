# ProjectFR Wiki

> 이 위키는 `docs/`, `res/` 소스 전체를 기반으로 자동 생성되었습니다. 최종 갱신: 2026-06-01

ProjectFR는 **파일 탐색기 조작 자체를 전투 시스템으로 만든 전술 침투 로그라이크**입니다.
플레이어는 꼬리 없는 해커 햄스터가 되어 마우스 중심 인터넷 세계의 서버에 침투하고,
삭제된 존재들의 기억 파일을 복원합니다.

엔진: **Godot 4.6 + C# (.NET 8)**

---

## 위키 목차

| 문서 | 내용 |
|---|---|
| [gameplay.md](gameplay.md) | 게임 루프, 핵심 규칙, 미션 구조, Trace/Heat |
| [systems.md](systems.md) | 침투 런타임, 창 시스템, 파일 작업, 보안 반응 |
| [architecture.md](architecture.md) | 코드 구조, 네임스페이스, 주요 클래스 관계 |
| [content-pipeline.md](content-pipeline.md) | 미션·세력·보안 에이전트·액션 추가 방법 |
| [dev-guide.md](dev-guide.md) | 빌드·실행·코딩 컨벤션·작업 규칙 |
| [changelog-from-todo.md](changelog-from-todo.md) | TODO 완료 기반 구현 이력 |

---

## 핵심 한 줄 요약

```
선택 → 명령 큐 적재 → 실행 → 파일 작업 진행 → 보안 반응 → Trace 관리 → 탈출
```

## 세계관 한 줄 요약

```
인터넷 세계에서 Tail Signal(꼬리 신호)이 없는 햄스터는 비인가 존재다.
주인공은 그 결핍을 강점으로 바꿔, 삭제된 기억들을 되찾는다.
```

## 설계 문서 링크

| 파일 | 역할 |
|---|---|
| `docs/기획서/GAME_CONCEPT.md` | 핵심 비전 (canonical) |
| `docs/기획서/STORY_WORLD.md` | 세계관/스토리 (canonical) |
| `docs/기획서/MISSION_FACTIONS.md` | 의뢰 세력 설계 |
| `docs/기획서/MISSION_DATA_MODEL.md` | 미션 데이터 구조 및 현황 |
| `docs/기획서/PROJECT_REDESIGN_STEPS.md` | 재설계 단계별 진행 현황 |
| `docs/기획서/MULTI_WINDOW_EXPANSION.md` | 멀티 윈도우 설계 및 구현 현황 |
