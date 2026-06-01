---
description: "ProjectFR 작업 계획 수립. 인자가 있으면 요청 기반으로, 인자가 없으면 기획서/소스/현재 TODO를 자동 스캔해 docs/TODO.md에 실행 가능한 항목을 추가하고 성공 시 커밋/푸시한다."
---

# ProjectFR OP 계획 수립

작업 대상 프로젝트는 항상 다음 경로다:

```text
/mnt/e/Agent/workspaces/Godot/projects/ProjectFR
```

TODO 파일은 항상 다음 경로다:

```text
/mnt/e/Agent/workspaces/Godot/projects/ProjectFR/docs/TODO.md
```

## 작업 요청

```text
$ARGUMENTS
```

## 절대 규칙

- 이 커맨드는 **계획만** 작성한다. 소스 코드를 실제로 수정하지 않는다.
- 수정 가능한 파일은 원칙적으로 `docs/TODO.md` 하나뿐이다.
- 인자값이 없거나 `$ARGUMENTS` 플레이스홀더가 그대로 남아 있으면 사용자에게 다시 묻지 말고 **자동 스캔 모드**로 실행한다.
- 실제 인자값이 있으면 **요청 기반 모드**로 실행한다.
- 이미 완료된 `- [x]` 항목은 되돌리거나 중복 작성하지 않는다.
- 이미 존재하는 `- [ ]` 항목과 의미가 같은 작업은 중복 추가하지 않는다.
- 추가 항목은 `/todo`가 하나씩 처리할 수 있게 작고 구체적인 `- [ ]` 체크박스로 작성한다.
- 계획 작성이 성공했고 `docs/TODO.md`에 변경사항이 있으면 반드시 자동으로 commit 후 push한다.
- 분석만 했고 추가할 항목이 없으면 commit/push를 하지 않고 이유를 보고한다.

## Step 1 — 시작 상태 확인

1. 다음 경로로 이동한다:

```bash
cd /mnt/e/Agent/workspaces/Godot/projects/ProjectFR
```

2. 시작 전 git 상태를 확인한다:

```bash
git status --short
```

3. `docs/TODO.md`가 있으면 읽고, 없으면 새로 만들 준비를 한다.
4. `docs/TODO.md`의 기존 섹션, 완료 항목, 미완료 항목을 파악한다.

## Step 2A — 요청 기반 모드 (`$ARGUMENTS` 있음)

`$ARGUMENTS`에 실제 내용이 있으면 다음을 수행한다. 값이 비어 있거나 리터럴 `$ARGUMENTS` 그대로면 요청 기반 모드가 아니라 자동 스캔 모드다:

1. 요청 내용을 구현 작업 단위로 분해한다.
2. 기존 `docs/TODO.md`와 중복되는 항목을 제거한다.
3. 필요한 경우 기존 섹션에 추가하거나 새 `## <섹션명>`을 만든다.
4. 각 항목은 아래 형식으로 작성한다:

```markdown
- [ ] <구체적인 구현 지시>: <수정해야 할 파일/클래스/메서드 후보와 완료 조건>
```

## Step 2B — 자동 스캔 모드 (`$ARGUMENTS` 없음)

`$ARGUMENTS`가 비어 있거나 리터럴 `$ARGUMENTS` 그대로면 사용자에게 묻지 말고 자동으로 다음 파일들을 읽는다:

필수 문서:
- `CLAUDE.md`
- `docs/README.md`
- `docs/TODO.md` (있으면)
- `docs/기획서/*.md`
- `docs/wiki/*.md` (있으면)

필수 소스 영역:
- `res/infiltration/*.cs`
- `res/mission/*.cs`
- `res/action/*.cs`
- `res/action/implementations/*.cs`
- `res/scenes/BattleScene.cs`
- `res/scenes/MainMenu.cs`
- `res/battle/*.cs`

자동 스캔 기준:

1. 기획서/위키에는 있는데 코드에 빠진 기능을 찾는다.
2. TODO의 완료 항목(`- [x]`) 이후 자연스럽게 이어지는 다음 구현 단계를 찾는다.
3. 코드에 TODO/임시 구현/죽은 경로/중복 상태가 있는지 찾는다.
4. 빌드 가능한 작은 단위의 개선만 후보로 삼는다.
5. 이미 존재하는 미완료 TODO와 중복되면 추가하지 않는다.
6. 우선순위가 높은 3~7개 항목만 추가한다.

자동 스캔으로 추가할 항목의 좋은 예:

```markdown
- [ ] `CampaignState` 저장 파일 로드 실패 시 복구 경로를 추가한다: `CampaignState.Load()`에서 JSON 파싱 예외가 발생하면 기존 파일을 `.bak`으로 보존하고 새 상태로 초기화하도록 구현한 뒤, `MainMenu`의 캠페인 초기화 흐름에서 오류 로그를 남긴다.
```

나쁜 예:

```markdown
- [ ] 게임을 개선한다
- [ ] 리팩터링한다
- [ ] 버그를 고친다
```

## Step 3 — TODO 파일 갱신 규칙

1. `docs/TODO.md`가 없으면 생성한다.
2. 상단에 `> Updated: YYYY-MM-DD` 라인이 있으면 오늘 날짜로 갱신한다.
3. 상단 날짜가 없으면 제목 아래에 추가한다.
4. 새 항목은 기존 섹션 구조에 맞게 넣는다.
5. 자동 스캔 모드에서 섹션이 애매하면 `## 자동 스캔 후보` 섹션을 사용한다.
6. 모든 새 작업은 `- [ ]`로 시작한다.
7. 완료 항목은 수정하지 않는다.

## Step 4 — 검증

계획만 수정했으므로 빌드는 필수는 아니다. 대신 다음을 확인한다:

```bash
git diff -- docs/TODO.md
python3 - <<'PY'
from pathlib import Path
p = Path('docs/TODO.md')
text = p.read_text(encoding='utf-8')
assert '- [ ]' in text or '- [x]' in text
print('TODO markdown looks readable:', p)
PY
```

## Step 5 — 자동 commit/push

`docs/TODO.md`가 변경되었을 때만 실행한다.

1. 변경사항 확인:

```bash
git status --short
```

2. `docs/TODO.md` 외의 파일이 변경되었다면, 사용자가 명시하지 않은 한 되돌리거나 중단하고 보고한다.
3. 변경사항이 없으면 다음처럼 보고하고 종료한다:

```text
추가할 새 TODO가 없어 commit/push를 건너뜀.
```

4. 변경사항이 있으면 커밋한다:

```bash
git add docs/TODO.md
git commit -m "docs: update ProjectFR todo plan"
```

5. 현재 브랜치로 push한다:

```bash
BRANCH="$(git branch --show-current)"
git push origin "$BRANCH"
```

6. push 후 동기화 확인:

```bash
git rev-list --left-right --count origin/"$BRANCH"...HEAD
```

## Step 6 — 최종 보고 형식

짧게 다음만 보고한다:

```text
모드: 자동 스캔 | 요청 기반
추가한 TODO: <개수>
주요 항목:
- <항목 1 요약>
- <항목 2 요약>
커밋: <hash 또는 skipped>
푸시: <완료 또는 skipped/실패>
다음 단계: /todo
```
