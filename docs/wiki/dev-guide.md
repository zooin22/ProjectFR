# Dev Guide — 빌드, 실행, 코딩 규칙

## 1. 빌드 및 실행

| 작업 | 방법 |
|---|---|
| 프로젝트 열기 | Godot 4.6 에디터 → `project.godot` 열기 |
| C# 빌드 | 에디터 자동 컴파일 또는 `Alt+B` |
| 게임 실행 | `F5` (진입점: `res/scenes/main.tscn`) |
| 단일 씬 실행 | 씬 파일 열고 `F6` |
| CLI 빌드 | `dotnet build ProjectFR.csproj` |

> **주의**: 유닛 테스트 없음. 기능 검증은 에디터 실행으로만 가능.

---

## 2. 환경 요건

- Godot **4.6** (C# / .NET 지원 버전)
- .NET SDK **8.0** (`dotnet build` 사용 시)
- WSL2 환경에서는 `.local/bin/dotnet` wrapper 사용

---

## 3. 코딩 컨벤션

### 기본 규칙

- **Nullable 활성화** (`<Nullable>enable</Nullable>`) — null 참조는 명시적으로 처리한다.
- **액션 ID는 상수 사용** — `ActionIds.Copy`, `ActionIds.Delete` 등. 문자열 리터럴 직접 사용 금지.
- **밸런스 숫자는 Tuning 파일에** — `InfiltrationTuning`, `SecurityBehaviorTuning`. 코드 안에 매직 넘버 금지.
- **시스템은 주입받기** — `ActionContext`를 통해 `ClipboardSystem` / `StatusEffectSystem` 수신. 액션 내부에서 직접 생성 금지.
- **Autoload 접근** — `GameManager.Instance`만. 씬에서 직접 싱글톤 생성 금지.

### 새 기능 위치

- 침투 런 관련 → `res/infiltration/`
- 레거시 배틀 관련 → `res/battle/` (새 기능은 여기 넣지 않는다)
- 미션/캠페인 → `res/mission/`
- 공유 데이터 모델 → `res/data/`

### 로그 사용

BattleScene 레이어에서 작업 로그를 남길 때는 `AddOperationLog()` 헬퍼를 사용한다.
이 헬퍼는 BattleManager.AddLog와 InfiltrationState.AddLog를 동시에 기록한다.
`UpdateOperationLog()`는 `InfiltrationState.EventLog`를 단일 소스로 사용한다.

```csharp
// BattleScene 내 작업 로그 (두 레이어 동시 기록)
AddOperationLog("메시지");

// InfiltrationManager/InfiltrationState 내부 직접 기록
State.AddLog("메시지");

// 배틀 레거시 로그 (최대 50줄 유지)
BattleManager.AddLog("메시지");
```

---

## 4. 두 레이어 병행 구조 이해

현재 **BattleManager(레거시)** 와 **InfiltrationManager(활성)** 가 BattleScene에서 동시에 실행된다.

- 새 기능은 **InfiltrationManager/InfiltrationState** 쪽에 추가한다.
- BattleManager는 아직 제거하지 않았으며, 점진적으로 대체 예정.
- BattleScene이 두 레이어를 모두 구동하고 UI를 연결한다.

---

## 5. 작업 흐름 권장 패턴

### 새 액션 추가 시

1. `ActionIds.cs`에 상수 추가
2. `res/action/implementations/`에 클래스 파일 생성
3. `ActionRegistry`에 등록
4. (필요 시) BattleScene 액션 바/컨텍스트 메뉴에 연결
5. `InfiltrationManager`에서 CommandQueue→FileOperation 처리 추가

### 새 보안 반응 추가 시

1. `SecurityBehaviorKeys.cs`에 키 상수 추가
2. `SecurityBehaviorFactory`에 노드 생성 로직 추가
3. `InfiltrationManager.ResolveSecurityBehaviorKey()`에 타입별 분기 추가
4. 밸런스 수치는 `SecurityBehaviorTuning`에 추가

### 밸런스 조정 시

`InfiltrationTuning.cs` 또는 `SecurityBehaviorTuning.cs`만 수정한다.

---

## 6. 파일 노드/던전 구성

`BattleDungeon` 클래스에서 노드 트리를 구성하고,
`BattleFactory`가 초기 배치를 만든다.
`BattleScene`이 `InfiltrationManager.Initialize(startPath, knownNodes)`를 호출해 런을 시작한다.

---

## 7. 씬 전환

```
TitleScene → MainMenu → BattleScene → MainMenu (결과 후)
```

씬 전환은 Godot의 `GetTree().ChangeSceneToFile(path)` 사용.

---

## 8. 주의 사항

- `BattleScene`은 UI 부트스트랩(참조 캐시 / 이벤트 바인딩 / 초기 상태 설정)을 분리해서 관리한다.
  참조 캐시가 누락되면 NullReferenceException이 발생하므로, 새 UI 요소 추가 시 반드시 캐시 단계에 포함할 것.
- `ExplorerWindowState.WindowId`는 Guid 기반 자동 생성이므로 하드코딩하지 않는다.
- `CampaignState`는 static 클래스로 씬 전환 후에도 상태가 유지된다.
  새 게임 시작 시 `EnsureInitialized()`만으로는 리셋되지 않으므로 주의.
