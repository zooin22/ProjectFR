# GDScript 네이밍/구조 규칙

> 작성일: 2026-06-02  
> 목적: C# → GDScript 전환 시 파일명, class_name, Autoload, 타입 변환 패턴을 통일하는 기준선

---

## 1. 파일명 규칙

| C# 파일 패턴 | GDScript 파일 패턴 | 규칙 |
|---|---|---|
| `PascalCase.cs` | `snake_case.gd` | 모든 `.gd` 파일명은 snake_case |
| `IAction.cs` (인터페이스) | 별도 파일 없음 | 인터페이스는 duck typing으로 대체; 파일 불필요 |
| `ActionBase.cs` (추상 클래스) | `action_base.gd` | 동일 변환, class_name 선언 |
| `InfiltrationTuning.cs` (정적 상수 클래스) | `infiltration_tuning.gd` | 동일 변환, class_name + const 선언 |
| `ActionIds.cs` (문자열 상수) | `action_ids.gd` | 동일 변환, class_name + const 선언 |

### 구체적 변환 예시

```
GameManager.cs          → game_manager.gd
InfiltrationManager.cs  → infiltration_manager.gd
InfiltrationState.cs    → infiltration_state.gd
BattleScene.cs          → battle_scene.gd
CursorAgent.cs          → cursor_agent.gd
SecurityAgent.cs        → security_agent.gd
MissionBoardFactory.cs  → mission_board_factory.gd
ExplorerNodeRole.cs     → explorer_node_role.gd
OperationType.cs        → operation_type.gd
```

---

## 2. Namespace → 폴더 경로 매핑

| C# Namespace | 폴더 경로 | GDScript class_name 접두사 |
|---|---|---|
| `ProjectFR.Core` | `res://res/core/` | 없음 (GameManager는 Autoload) |
| `ProjectFR.Data` | `res://res/data/` | 없음 |
| `ProjectFR.Data.Nodes` | `res://res/data/nodes/` | 없음 |
| `ProjectFR.Systems` | `res://res/systems/` | 없음 |
| `ProjectFR.Infiltration` | `res://res/infiltration/` | 없음 |
| `ProjectFR.Mission` | `res://res/mission/` | 없음 |
| `ProjectFR.Action` | `res://res/action/` | 없음 |
| `ProjectFR.Action.Conditions` | `res://res/action/conditions/` | 없음 |
| `ProjectFR.Action.Implementations` | `res://res/action/implementations/` | 없음 |
| `ProjectFR.Skills` | `res://res/skills/` | 없음 |
| `ProjectFR.Battle` | `res://res/battle/` | 없음 (레거시, 전환 후 제거) |
| `ProjectFR.Scenes` | `res://res/scenes/` | 없음 (씬 스크립트) |

> **class_name 접두사 없음**: Godot의 `class_name`은 전역 등록이므로 namespace 접두사 없이 원본 C# 클래스명을 그대로 사용한다. 충돌 시에만 접두사를 고려한다.

---

## 3. class_name 선언 기준

### 선언해야 하는 경우

- 다른 스크립트에서 타입으로 참조하는 모든 클래스
- 데이터 클래스 / 값 객체 (예: `CursorAgent`, `FileOperation`, `MissionData`)
- 상수 모음 파일 (예: `InfiltrationTuning`, `ActionIds`, `SecurityBehaviorKeys`)
- 베이스 클래스 (예: `ActionBase`, `SecurityBehaviorNode`, `SkillBehaviorNode`)

### 선언하지 않아도 되는 경우

- 씬 스크립트 (`BattleScene`, `MainMenu`, `TitleScene`): Node 계층 구조로 참조되므로 불필요
- 씬 전용 내부 헬퍼 클래스 (해당 씬 스크립트 파일 내 inner class로 처리)

---

## 4. Autoload 등록 대상

| C# 클래스 | GDScript 파일 | Autoload 이름 | 비고 |
|---|---|---|---|
| `GameManager` | `res://res/core/game_manager.gd` | `GameManager` | 현재와 동일; `Instance` 패턴 제거 (`get_node("/root/GameManager")` 사용) |
| `CampaignState` | `res://res/mission/campaign_state.gd` | `CampaignState` | C#에서는 GameManager가 소유했지만 GDScript에서는 독립 Autoload로 승격 권장 |
| `DebugLog` | `res://res/core/debug_log.gd` | `DebugLog` | 정적 유틸 → Autoload 싱글톤으로 전환 |

> `project.godot` `[autoload]` 섹션에 추가:
> ```
> GameManager="*res://res/core/game_manager.gd"
> CampaignState="*res://res/mission/campaign_state.gd"
> DebugLog="*res://res/core/debug_log.gd"
> ```

---

## 5. 타입 전환 패턴

### 5.1 일반 클래스 → class_name 클래스

```csharp
// C#
namespace ProjectFR.Infiltration;
public class CursorAgent {
    public string CurrentNodePath { get; set; } = "";
    public int ActionPoints { get; set; }
}
```

```gdscript
# cursor_agent.gd
class_name CursorAgent

var current_node_path: String = ""
var action_points: int = 0
```

### 5.2 인터페이스 → duck typing (별도 파일 없음)

```csharp
// C#
public interface IAction {
    string ActionId { get; }
    bool CanExecute(ActionContext context);
    ActionResult Execute(ActionContext context);
}
```

```gdscript
# GDScript — IAction 파일 없음; 구현 클래스에서 직접 메서드 정의
# 아래 메서드를 가지면 ActionBase를 상속하지 않아도 동작
# action_id: String (get)
# can_execute(ctx: ActionContext) -> bool
# execute(ctx: ActionContext) -> ActionResult
```

### 5.3 추상 클래스 → 베이스 class_name

```csharp
// C#
public abstract class ActionBase : IAction {
    public abstract string ActionId { get; }
    public virtual bool CanExecute(ActionContext context)
        => Conditions.All(c => c.Check(context));
    public abstract ActionResult Execute(ActionContext context);
}
```

```gdscript
# action_base.gd
class_name ActionBase

var conditions: Array = []

func can_execute(ctx: ActionContext) -> bool:
    return conditions.all(func(c): return c.check(ctx))

func execute(_ctx: ActionContext) -> ActionResult:
    push_error("ActionBase.execute() must be overridden")
    return ActionResult.new()
```

### 5.4 정적 상수 클래스 → class_name + const

```csharp
// C#
public static class InfiltrationTuning {
    public const int StunDurationTurns = 2;
    public const int DetectionContactDamage = 3;
}
```

```gdscript
# infiltration_tuning.gd
class_name InfiltrationTuning

const STUN_DURATION_TURNS = 2
const DETECTION_CONTACT_DAMAGE = 3
```

> 사용: `InfiltrationTuning.STUN_DURATION_TURNS`

### 5.5 enum → GDScript unnamed enum in class_name

```csharp
// C#
public enum ExplorerNodeRole {
    Objective, Resource, Hazard, Evidence, Utility, Decoy, Exit, SecurityAnchor
}
```

```gdscript
# explorer_node_role.gd
class_name ExplorerNodeRole

enum { OBJECTIVE, RESOURCE, HAZARD, EVIDENCE, UTILITY, DECOY, EXIT, SECURITY_ANCHOR }
```

> 사용: `ExplorerNodeRole.OBJECTIVE`  
> **규칙**: enum 값은 `SCREAMING_SNAKE_CASE` 사용

### 5.6 C# record / 값 객체 → class_name + `_init()`

```csharp
// C#
public record ActionResult(bool Success, string Message);
```

```gdscript
# action_result.gd
class_name ActionResult

var success: bool
var message: String

func _init(p_success: bool = false, p_message: String = "") -> void:
    success = p_success
    message = p_message
```

---

## 6. 변수·메서드 명명 규칙

### 프로퍼티 / 변수

| C# 패턴 | GDScript 패턴 |
|---|---|
| `public int ActionPoints { get; set; }` | `var action_points: int` |
| `public string CurrentNodePath { get; private set; }` | `var current_node_path: String` (setter 노출 최소화는 함수로) |
| `private int _turnCount;` | `var _turn_count: int` |
| `public static readonly int MaxHp = 30;` | `const MAX_HP = 30` |
| `public const int StunDurationTurns = 2;` | `const STUN_DURATION_TURNS = 2` |

### 메서드

| C# 패턴 | GDScript 패턴 |
|---|---|
| `public void StartOperation(...)` | `func start_operation(...)` |
| `private bool TryExtract(...)` | `func _try_extract(...)` |
| `public override void _Ready()` | `func _ready()` |
| `public override void _EnterTree()` | `func _enter_tree()` |
| `public override void _Process(double delta)` | `func _process(delta: float)` |
| `public override void _Input(InputEvent e)` | `func _input(event: InputEvent)` |

### 시그널

| C# 이벤트 패턴 | GDScript 패턴 |
|---|---|
| `public event Action<string> OnOperationCompleted;` | `signal operation_completed(path: String)` |

---

## 7. 전체 파일 매핑표 (C# → GDScript)

### core

| C# 파일 | GDScript 파일 | class_name |
|---|---|---|
| `res/core/GameManager.cs` | `res/core/game_manager.gd` | `GameManager` (Autoload) |
| `res/core/DebugLog.cs` | `res/core/debug_log.gd` | `DebugLog` (Autoload) |

### data

| C# 파일 | GDScript 파일 | class_name |
|---|---|---|
| `res/data/TargetType.cs` | `res/data/target_type.gd` | `TargetType` |
| `res/data/ActorState.cs` | `res/data/actor_state.gd` | `ActorState` |
| `res/data/nodes/NodeData.cs` | `res/data/nodes/node_data.gd` | `NodeData` |
| `res/data/nodes/SpecialFileNode.cs` | `res/data/nodes/special_file_node.gd` | `SpecialFileNode` |
| `res/data/actions/ContextActionData.cs` | `res/data/actions/context_action_data.gd` | `ContextActionData` |

### systems

| C# 파일 | GDScript 파일 | class_name |
|---|---|---|
| `res/systems/ClipboardSystem.cs` | `res/systems/clipboard_system.gd` | `ClipboardSystem` |
| `res/systems/StatusEffectSystem.cs` | `res/systems/status_effect_system.gd` | `StatusEffectSystem` |

### infiltration

| C# 파일 | GDScript 파일 | class_name |
|---|---|---|
| `res/infiltration/ExplorerNodeKind.cs` | `res/infiltration/explorer_node_kind.gd` | `ExplorerNodeKind` |
| `res/infiltration/ExplorerNodeRole.cs` | `res/infiltration/explorer_node_role.gd` | `ExplorerNodeRole` |
| `res/infiltration/OperationStatus.cs` | `res/infiltration/operation_status.gd` | `OperationStatus` |
| `res/infiltration/OperationType.cs` | `res/infiltration/operation_type.gd` | `OperationType` |
| `res/infiltration/RunState.cs` | `res/infiltration/run_state.gd` | `RunState` |
| `res/infiltration/InfiltrationTuning.cs` | `res/infiltration/infiltration_tuning.gd` | `InfiltrationTuning` |
| `res/infiltration/SecurityBehaviorTuning.cs` | `res/infiltration/security_behavior_tuning.gd` | `SecurityBehaviorTuning` |
| `res/infiltration/ClipboardEntry.cs` | `res/infiltration/clipboard_entry.gd` | `ClipboardEntry` |
| `res/infiltration/CommandQueueEntry.cs` | `res/infiltration/command_queue_entry.gd` | `CommandQueueEntry` |
| `res/infiltration/CursorAgent.cs` | `res/infiltration/cursor_agent.gd` | `CursorAgent` |
| `res/infiltration/ExplorerWindowState.cs` | `res/infiltration/explorer_window_state.gd` | `ExplorerWindowState` |
| `res/infiltration/FileOperation.cs` | `res/infiltration/file_operation.gd` | `FileOperation` |
| `res/infiltration/SecurityAgent.cs` | `res/infiltration/security_agent.gd` | `SecurityAgent` |
| `res/infiltration/SecurityBehaviorContext.cs` | `res/infiltration/security_behavior_context.gd` | `SecurityBehaviorContext` |
| `res/infiltration/SecurityBehaviorKeys.cs` | `res/infiltration/security_behavior_keys.gd` | `SecurityBehaviorKeys` |
| `res/infiltration/SecurityBehaviorNode.cs` | `res/infiltration/security_behavior_node.gd` | `SecurityBehaviorNode` |
| `res/infiltration/SecurityBehaviorFactory.cs` | `res/infiltration/security_behavior_factory.gd` | `SecurityBehaviorFactory` |
| `res/infiltration/SecurityBehaviorExecutor.cs` | `res/infiltration/security_behavior_executor.gd` | `SecurityBehaviorExecutor` |
| `res/infiltration/InfiltrationState.cs` | `res/infiltration/infiltration_state.gd` | `InfiltrationState` |
| `res/infiltration/InfiltrationManager.cs` | `res/infiltration/infiltration_manager.gd` | `InfiltrationManager` |

### mission

| C# 파일 | GDScript 파일 | class_name |
|---|---|---|
| `res/mission/MissionObjectiveType.cs` | `res/mission/mission_objective_type.gd` | `MissionObjectiveType` |
| `res/mission/CampaignModifiers.cs` | `res/mission/campaign_modifiers.gd` | `CampaignModifiers` |
| `res/mission/MissionClientProfile.cs` | `res/mission/mission_client_profile.gd` | `MissionClientProfile` |
| `res/mission/MissionData.cs` | `res/mission/mission_data.gd` | `MissionData` |
| `res/mission/MissionProgress.cs` | `res/mission/mission_progress.gd` | `MissionProgress` |
| `res/mission/MissionResult.cs` | `res/mission/mission_result.gd` | `MissionResult` |
| `res/mission/MissionBoardFactory.cs` | `res/mission/mission_board_factory.gd` | `MissionBoardFactory` |
| `res/mission/CampaignState.cs` | `res/mission/campaign_state.gd` | `CampaignState` (Autoload) |

### action

| C# 파일 | GDScript 파일 | class_name |
|---|---|---|
| `res/action/IAction.cs` | — (duck typing, 파일 없음) | — |
| `res/action/IActionCondition.cs` | — (duck typing, 파일 없음) | — |
| `res/action/ActionBase.cs` | `res/action/action_base.gd` | `ActionBase` |
| `res/action/ActionContext.cs` | `res/action/action_context.gd` | `ActionContext` |
| `res/action/ActionResult.cs` | `res/action/action_result.gd` | `ActionResult` |
| `res/action/ActionIds.cs` | `res/action/action_ids.gd` | `ActionIds` |
| `res/action/ActionMetadata.cs` | `res/action/action_metadata.gd` | `ActionMetadata` |
| `res/action/ActionRegistry.cs` | `res/action/action_registry.gd` | `ActionRegistry` |
| `res/action/conditions/MinApCondition.cs` | `res/action/conditions/min_ap_condition.gd` | `MinApCondition` |
| `res/action/conditions/TargetAliveCondition.cs` | `res/action/conditions/target_alive_condition.gd` | `TargetAliveCondition` |
| `res/action/conditions/ClipboardNotEmptyCondition.cs` | `res/action/conditions/clipboard_not_empty_condition.gd` | `ClipboardNotEmptyCondition` |
| `res/action/conditions/NotStatusCondition.cs` | `res/action/conditions/not_status_condition.gd` | `NotStatusCondition` |
| `res/action/implementations/ActionConstants.cs` | `res/action/implementations/action_constants.gd` | `ActionConstants` |
| `res/action/implementations/InspectAction.cs` | `res/action/implementations/inspect_action.gd` | `InspectAction` |
| `res/action/implementations/OpenAction.cs` | `res/action/implementations/open_action.gd` | `OpenAction` |
| `res/action/implementations/SearchAction.cs` | `res/action/implementations/search_action.gd` | `SearchAction` |
| `res/action/implementations/LogForgeAction.cs` | `res/action/implementations/log_forge_action.gd` | `LogForgeAction` |
| `res/action/implementations/ShowHiddenAction.cs` | `res/action/implementations/show_hidden_action.gd` | `ShowHiddenAction` |
| `res/action/implementations/CopyAction.cs` | `res/action/implementations/copy_action.gd` | `CopyAction` |
| `res/action/implementations/DeleteAction.cs` | `res/action/implementations/delete_action.gd` | `DeleteAction` |
| `res/action/implementations/CutAction.cs` | `res/action/implementations/cut_action.gd` | `CutAction` |
| `res/action/implementations/PasteAction.cs` | `res/action/implementations/paste_action.gd` | `PasteAction` |
| `res/action/implementations/QuarantineAction.cs` | `res/action/implementations/quarantine_action.gd` | `QuarantineAction` |
| `res/action/implementations/CompressAction.cs` | `res/action/implementations/compress_action.gd` | `CompressAction` |
| `res/action/implementations/PermissionOverrideAction.cs` | `res/action/implementations/permission_override_action.gd` | `PermissionOverrideAction` |
| `res/action/implementations/StunAction.cs` | `res/action/implementations/stun_action.gd` | `StunAction` |
| `res/action/implementations/CleanAction.cs` | `res/action/implementations/clean_action.gd` | `CleanAction` |

### skills

| C# 파일 | GDScript 파일 | class_name |
|---|---|---|
| `res/skills/SkillDefinition.cs` | `res/skills/skill_definition.gd` | `SkillDefinition` |
| `res/skills/SkillCatalog.cs` | `res/skills/skill_catalog.gd` | `SkillCatalog` |
| `res/skills/SkillBehaviorKeys.cs` | `res/skills/skill_behavior_keys.gd` | `SkillBehaviorKeys` |
| `res/skills/SkillBehaviorNode.cs` | `res/skills/skill_behavior_node.gd` | `SkillBehaviorNode` |
| `res/skills/SkillBehaviorFactory.cs` | `res/skills/skill_behavior_factory.gd` | `SkillBehaviorFactory` |
| `res/skills/SkillExecutionContext.cs` | `res/skills/skill_execution_context.gd` | `SkillExecutionContext` |
| `res/skills/SkillExecutor.cs` | `res/skills/skill_executor.gd` | `SkillExecutor` |

### battle (레거시)

| C# 파일 | GDScript 파일 | class_name | 비고 |
|---|---|---|---|
| `res/battle/BattleConstants.cs` | `res/battle/battle_constants.gd` | `BattleConstants` | `InfiltrationTuning`으로 통합 후 제거 |
| `res/battle/DungeonFolderMetadata.cs` | `res/battle/dungeon_folder_metadata.gd` | `DungeonFolderMetadata` | |
| `res/battle/BattleDungeon.cs` | `res/battle/battle_dungeon.gd` | `BattleDungeon` | |
| `res/battle/BattleFactory.cs` | `res/battle/battle_factory.gd` | `BattleFactory` | |
| `res/battle/BattleManager.cs` | `res/battle/battle_manager.gd` | `BattleManager` | HP 최소 인터페이스 후 제거 |

### scenes

| C# 파일 | GDScript 파일 | 씬 |
|---|---|---|
| `res/scenes/TitleScene.cs` | `res/scenes/title_scene.gd` | `TitleScene.tscn` |
| `res/scenes/MainMenu.cs` | `res/scenes/main_menu.gd` | `main.tscn` |
| `res/scenes/BattleScene.cs` | `res/scenes/battle_scene.gd` | `BattleScene.tscn` |

---

## 8. 추가 규칙 요약

| 항목 | C# 규칙 | GDScript 규칙 |
|---|---|---|
| 파일명 | `PascalCase.cs` | `snake_case.gd` |
| 클래스명 (`class_name`) | `PascalCase` | `PascalCase` (동일) |
| 인스턴스 변수 | `PascalCase` (프로퍼티) / `_camelCase` (private) | `snake_case` / `_snake_case` |
| 메서드명 | `PascalCase` | `snake_case` |
| 상수 | `PascalCase` (C#) | `SCREAMING_SNAKE_CASE` |
| enum 값 | `PascalCase` (C#) | `SCREAMING_SNAKE_CASE` |
| Godot 콜백 | `_Ready()`, `_Process()` | `_ready()`, `_process()` (동일 snake) |
| 시그널 | `event Action<T> OnX` | `signal x_happened(...)` |
| null 처리 | `?` nullable, `!` null-forgiving | `if x == null:` 또는 `@warning_ignore` |
| 인터페이스 | `interface IFoo` | duck typing (별도 파일 없음) |
| 정적 유틸 | `static class Foo` | `class_name Foo` + `const` (인스턴스화 없이 사용) |
