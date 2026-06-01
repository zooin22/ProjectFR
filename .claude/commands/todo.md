---
description: "ProjectFR docs/TODO.md의 첫 번째 unchecked 항목을 정확히 하나 처리한다. 남은 항목이 없으면 wiki를 갱신한다."
---

# ProjectFR TODO 실행

작업 대상 프로젝트는 항상 다음 경로다:

```text
/mnt/e/Agent/workspaces/Godot/projects/ProjectFR
```

TODO 파일은 항상 다음 경로다:

```text
/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/TODO.md
```

## 절대 규칙

- 이 커맨드는 **한 번에 첫 번째 `- [ ]` 항목 하나만** 처리한다.
- 여러 TODO를 연속 처리하지 않는다.
- 실제 코드 변경 없이 체크박스를 `- [x]`로 바꾸지 않는다.
- 사용자가 다른 경로에서 Claude를 실행했더라도 반드시 위 ProjectFR 경로에서 작업한다.
- 다른 사람/에이전트의 기존 변경을 되돌리지 않는다. 수정 전후로 변경 범위를 확인한다.
- `/wiki` 같은 다른 slash command를 문자 그대로 호출하지 않는다. 필요하면 아래에 적힌 wiki 갱신 절차를 직접 수행한다.

## Step 1 — 현재 상태 확인

1. `/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/TODO.md`를 읽는다.
2. 파일이 없으면 다음만 보고하고 중단한다:
   - `docs/TODO.md 파일이 없습니다. /op <요청>으로 먼저 항목을 추가해주세요.`
3. 첫 번째 `- [ ]` 항목을 찾는다.
4. `- [ ]` 항목이 없으면 Step 5의 wiki 갱신으로 이동한다.

## Step 2 — 첫 번째 TODO 하나 구현

첫 번째 `- [ ]` 항목만 처리한다.

처리 순서:

1. 해당 항목 텍스트를 그대로 인용해서 어떤 TODO를 처리할지 먼저 확인한다.
2. 관련 소스 파일을 읽고 필요한 최소 변경을 구현한다.
3. 코드 변경은 ProjectFR 내부 파일로 제한한다.
4. 가능한 가장 싼 검증을 실행한다. 기본 검증은 다음 중 가능한 것:
   - `dotnet build ProjectFR.csproj`
   - 또는 환경 문제로 build가 불가능하면 수정 파일의 정적 검토와 실패 이유 보고
5. 구현과 검증이 끝난 뒤에만 `docs/TODO.md`의 해당 항목을 `- [x]`로 변경한다.
6. `docs/TODO.md` 상단의 `> Updated:` 날짜가 있으면 오늘 날짜로 갱신한다.

## Step 3 — 완료 보고

다음 형식으로 짧게 보고한다:

```text
처리한 TODO: <첫 번째 항목 요약>
변경 파일: <파일 목록>
검증: <명령 및 결과>
남은 TODO: <개수>
```

## Step 4 — 실패/중단 조건

다음 경우에는 체크박스를 바꾸지 말고 중단한다:

- 어떤 파일을 바꿔야 하는지 확실하지 않음
- build/검증 실패가 수정 때문인지 판단 불가
- 항목 범위가 너무 커서 한 번에 안전하게 처리 불가
- 필요한 파일/경로가 없음

중단 시에는 이유와 다음에 필요한 조치를 보고한다.

## Step 5 — Wiki 갱신 절차 (TODO가 하나도 없을 때만)

`docs/TODO.md`에 `- [ ]` 항목이 하나도 없을 때만 수행한다.

1. `docs/wiki/` 디렉토리가 없으면 생성한다.
2. 다음 파일들을 읽어 현재 상태를 파악한다:
   - `docs/기획서/*.md`
   - `docs/TODO.md`
   - `res/infiltration/*.cs`
   - `res/mission/*.cs`
   - `res/action/*.cs`, `res/action/implementations/*.cs`
   - `res/scenes/BattleScene.cs`, `res/scenes/MainMenu.cs`
   - `res/infiltration/InfiltrationTuning.cs`, `SecurityBehaviorTuning.cs`
3. 다음 wiki 파일을 현재 코드와 설계 문서에서 확인한 실제 값으로 갱신한다:
   - `docs/wiki/README.md`
   - `docs/wiki/gameplay.md`
   - `docs/wiki/systems.md`
   - `docs/wiki/architecture.md`
   - `docs/wiki/content-pipeline.md`
   - `docs/wiki/dev-guide.md`
   - `docs/wiki/changelog-from-todo.md`
4. 갱신한 파일 목록을 보고한다.
