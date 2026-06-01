# Content Pipeline — 콘텐츠/데이터 추가 방법

## 1. 미션 추가

미션은 `res/mission/MissionBoardFactory.cs`의 `CreateDefaultBoard()`에서 `MissionData` 인스턴스를 추가한다.

```csharp
new MissionData(
    id: "mission_my_new_mission",
    title: "My New Mission",
    client: new MissionClientProfile(
        factionId: FactionId.MorrowProxy,
        name: "Morrow Proxy Handler",
        agenda: "Acquire target data silently.",
        riskNote: "High trace environment."
    ),
    briefing: "The target file is located in the archive server.",
    objectiveType: MissionObjectiveType.Extract,
    targetPath: "res://archive/secret.dat",
    turnLimit: 12,
    rewardCredits: 80,
    rewardReputation: 2,
    failurePenaltyCredits: 30,
    failureHeat: 2,
    prerequisiteMissionId: null,          // null = 항상 해금
    conflictGroup: null,                   // 같은 타깃 대립 시 그룹명 입력
    requiredFactionReputation: null        // 특정 세력 평판 요구 시 int 입력
)
```

### 선행 조건 미션 설정

```csharp
prerequisiteMissionId: "mission_extract_boss"
// "mission_extract_boss" 성공 후에만 이 미션이 해금됨
```

### 세력 평판 요건 설정

```csharp
requiredFactionReputation: 3
// 해당 미션 의뢰인의 세력 평판이 3 이상일 때만 해금
```

### 충돌 미션 그룹

같은 TargetPath를 두고 대립하는 미션들에 같은 `conflictGroup`을 설정.
`MainMenu.RefreshMenu()`에서 충돌 표시를 위해 사용된다.

---

## 2. 세력(Faction) 추가

`res/mission/MissionClientProfile.cs`의 `FactionId` enum에 새 세력 추가.

```csharp
public enum FactionId
{
    MorrowProxy,
    NorthlineLegal,
    HelixOps,
    EmberCircuit,
    GlassKeyCollective,
    MyNewFaction   // 추가
}
```

`MissionClientProfile` 생성 시 새 `FactionId`를 사용하면 된다.
평판은 `CampaignState._factionReputation`에 자동으로 등록된다 (기본 0).

---

## 3. 보안 에이전트 추가

### 에이전트 인스턴스 생성

`BattleScene`에서 `InfiltrationManager.AddSecurityAgent(agent)`로 추가.

```csharp
var agent = new SecurityAgent
{
    AgentType = SecurityAgentType.GuardScanner,
    CurrentNodePath = "res://server/zone_a/",
    PatrolRoute = new List<string>
    {
        "res://server/zone_a/",
        "res://server/zone_b/",
        "res://server/zone_c/"
    },
    SightRange = 1,
    AwarenessStage = SecurityAwarenessStage.Passive
};
infManager.AddSecurityAgent(agent);
```

### 미션 목표 경로 자동 연동

`BattleScene.SeedSecurityAgents()` 내부의 `EnsureMissionTargetPatrolled(agents, targetPath)`가
현재 미션의 `TargetPath`를 순찰하는 에이전트가 없으면 가장 가까운 조상 경로를 순찰하는
에이전트에 자동으로 `TargetPath`를 추가한다. 새 미션을 추가할 때 별도로 에이전트 경로를
수동 수정할 필요가 없다.

### 새 에이전트 타입 추가

`SecurityAgentType` enum에 추가하고,
`SecurityBehaviorFactory`에 해당 타입의 행동 키를 등록한다.
`InfiltrationManager.ResolveSecurityBehaviorKey()`에도 매핑을 추가한다.

---

## 4. 액션 추가

### Step 1 — ActionIds에 상수 추가

`res/action/ActionIds.cs`

```csharp
public static class ActionIds
{
    // ...기존 상수들...
    public const string MyNewAction = "mynewaction";
}
```

### Step 2 — 액션 클래스 구현

`res/action/implementations/MyNewAction.cs`

```csharp
using ProjectFR.Action;

namespace ProjectFR.Action;

public sealed class MyNewAction : ActionBase
{
    public override string ActionId => ActionIds.MyNewAction;
    public override string DisplayName => "My New Action";
    public override int ApCost => 2;
    public override TargetType Scope => TargetType.Single;

    protected override IEnumerable<IActionCondition> GetConditions() =>
    [
        new MinApCondition(ApCost),
        new TargetAliveCondition()
    ];

    protected override ActionResult ExecuteCore(ActionContext context)
    {
        // 구현
        return ActionResult.Success("My action executed.");
    }
}
```

### Step 3 — ActionRegistry에 등록

`res/action/ActionRegistry.cs`의 등록 목록에 추가:

```csharp
Register(new MyNewAction());
```

### Step 4 — BattleScene 액션 바에 버튼 추가 (선택)

BattleScene에서 퀵바 버튼과 연결하거나 컨텍스트 메뉴에 추가.

---

## 5. 새 창(Window Type) 추가

### Step 1 — ExplorerWindowType enum에 추가

`res/infiltration/ExplorerWindowState.cs` 또는 별도 enum 파일.

```csharp
public enum ExplorerWindowType
{
    Main,
    Clipboard,
    TempFolder,
    LogViewer,
    MyNewWindow   // 추가
}
```

### Step 2 — InfiltrationManager에 헬퍼 추가

```csharp
public ExplorerWindowState OpenMyNewWindow()
{
    return OpenWindow(ExplorerWindowType.MyNewWindow, "My Window Title", "bound://path", traceModifier: 1);
}
```

### Step 3 — BattleScene에 UI 연결

하단 패널에 버튼 추가 → 클릭 시 `InfiltrationManager.OpenMyNewWindow()` 호출.
창 타입별 렌더링 로직을 BattleScene의 창 업데이트 코드에 추가.

---

## 6. 밸런스 수치 조정

**Infiltration 관련 수치** → `res/infiltration/InfiltrationTuning.cs`
**보안 행동 관련 수치** → `res/infiltration/SecurityBehaviorTuning.cs`

이 두 파일 외에는 수치를 직접 하드코딩하지 않는다.

---

## 7. 파일 노드 데이터

`res/data/nodes/NodeData.cs` (abstract) / `SpecialFileNode.cs`

새 파일 타입이나 특수 노드를 추가할 때 `NodeData`를 상속하거나 `SpecialFileNode`를 활용.
`BattleDungeon`에서 노드를 구성해 `InfiltrationManager.Initialize()`에 전달한다.
