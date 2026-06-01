# C# → GDScript 전환 범위 인벤토리

> 작성일: 2026-06-02  
> 대상 분기: `main`

---

## 1. 프로젝트 레벨 파일

| 파일 | 역할 | 처리 방침 |
|------|------|-----------|
| `ProjectFR.csproj` | .NET 빌드 정의 | GDScript 전환 완료 후 제거 |
| `global.json` | .NET SDK 버전 고정 | GDScript 전환 완료 후 제거 |
| `GlobalUsings.cs` | `System`, `System.Collections.Generic`, `System.Linq` 전역 임포트 | GDScript 전환 완료 후 제거 (GDScript에선 불필요) |

---

## 2. `.tscn` ExtResource 참조 목록

| 씬 파일 | 참조하는 `.cs` | 전환 후 교체 대상 |
|---------|---------------|-------------------|
| `res/scenes/TitleScene.tscn` | `res://res/scenes/TitleScene.cs` | `res://res/scenes/title_scene.gd` |
| `res/scenes/main.tscn` | `res://res/scenes/MainMenu.cs` | `res://res/scenes/main_menu.gd` |
| `res/scenes/BattleScene.tscn` | `res://res/scenes/BattleScene.cs` | `res://res/scenes/battle_scene.gd` |

---

## 3. 모듈별 파일 분류

### 3.1 `core` (2개 파일)

| 파일 | 설명 |
|------|------|
| `res/core/DebugLog.cs` | 정적 로깅 헬퍼 |
| `res/core/GameManager.cs` | Autoload 싱글톤 — ClipboardSystem, StatusEffectSystem, CurrentBattle 소유 |

**전환 형태**: `class_name GameManager` + Autoload 등록 / `class_name DebugLog` 정적 유틸

---

### 3.2 `data` (5개 파일)

| 파일 | 설명 |
|------|------|
| `res/data/TargetType.cs` | enum (Single, AoE, Self, Multiple, All, Adjacent, Line) |
| `res/data/ActorState.cs` | HP/AP 값 객체 (battle layer) |
| `res/data/nodes/NodeData.cs` | 파일 시스템 노드 추상 베이스 |
| `res/data/nodes/SpecialFileNode.cs` | `NodeData` 구체 구현 |
| `res/data/actions/ContextActionData.cs` | 직렬화 가능 액션 메타데이터 |

**전환 형태**: `class_name` 기반 값 클래스 또는 `Resource` 서브클래스

---

### 3.3 `systems` (2개 파일)

| 파일 | 설명 |
|------|------|
| `res/systems/ClipboardSystem.cs` | Copy/Cut 상태 보유, Cut 후 Paste 시 클리어 (battle layer) |
| `res/systems/StatusEffectSystem.cs` | 액터별 Quarantine/Compressed/Corrupted 효과 추적 (battle layer) |

**전환 형태**: `class_name` 일반 클래스; `GameManager` 내부에서 인스턴스화

---

### 3.4 `infiltration` (20개 파일)

#### 열거형 / 소형 값 객체 (우선 이식)

| 파일 | 설명 |
|------|------|
| `res/infiltration/ExplorerNodeKind.cs` | enum (File, Folder, Archive …) |
| `res/infiltration/ExplorerNodeRole.cs` | enum (Objective, Resource, Exit …) |
| `res/infiltration/OperationStatus.cs` | enum (Pending, InProgress, Completed, Failed) |
| `res/infiltration/OperationType.cs` | enum (Copy, Cut, Paste, Delete, Compress …) |
| `res/infiltration/RunState.cs` | enum (Active, Succeeded, Failed, TimedOut) |
| `res/infiltration/InfiltrationTuning.cs` | 밸런스 상수 모음 |
| `res/infiltration/SecurityBehaviorTuning.cs` | 보안 에이전트 밸런스 상수 |

#### 데이터 클래스

| 파일 | 설명 |
|------|------|
| `res/infiltration/ClipboardEntry.cs` | 클립보드 항목 값 객체 |
| `res/infiltration/CommandQueueEntry.cs` | 큐 명령 항목 |
| `res/infiltration/CursorAgent.cs` | 플레이어 에이전트 상태 |
| `res/infiltration/ExplorerWindowState.cs` | 열린 서브윈도우 상태 |
| `res/infiltration/FileOperation.cs` | 진행 중 파일 오퍼레이션 |
| `res/infiltration/SecurityAgent.cs` | 경비 AI 상태 및 패트롤 경로 |

#### 보안 행동 파이프라인

| 파일 | 설명 |
|------|------|
| `res/infiltration/SecurityBehaviorContext.cs` | 행동 실행 컨텍스트 |
| `res/infiltration/SecurityBehaviorKeys.cs` | 행동 키 상수 |
| `res/infiltration/SecurityBehaviorNode.cs` | 행동 트리 노드 베이스 |
| `res/infiltration/SecurityBehaviorFactory.cs` | 에이전트 유형별 행동 트리 생성 |
| `res/infiltration/SecurityBehaviorExecutor.cs` | 행동 트리 실행기 |

#### 런타임 핵심

| 파일 | 설명 |
|------|------|
| `res/infiltration/InfiltrationState.cs` | 전체 런 가변 상태 |
| `res/infiltration/InfiltrationManager.cs` | 런 오케스트레이터 (턴 처리, 보안 반응) |

**전환 형태**: 열거형 → GDScript enum 또는 정수 상수, 데이터 클래스 → `class_name` 클래스, `InfiltrationManager` → 씬 노드 또는 Autoload

---

### 3.5 `mission` (8개 파일)

| 파일 | 설명 |
|------|------|
| `res/mission/MissionObjectiveType.cs` | enum (Extract, Delete, Modify, Scan, Escape) |
| `res/mission/CampaignModifiers.cs` | 캠페인 난이도 수정자 |
| `res/mission/MissionClientProfile.cs` | 의뢰인 프로필 데이터 |
| `res/mission/MissionData.cs` | 미션 정의 (목표, 경로, 제한 등) |
| `res/mission/MissionProgress.cs` | 런 중 목표 달성 추적 |
| `res/mission/MissionResult.cs` | 미션 결과 값 객체 |
| `res/mission/MissionBoardFactory.cs` | 미션 보드 생성 팩토리 |
| `res/mission/CampaignState.cs` | 런 간 진행 상황 (Credits, Heat, Reputation, JSON 저장) |

**전환 형태**: enum → GDScript enum, 데이터 클래스 → `class_name`, `CampaignState` → Autoload 또는 `GameManager` 하위

---

### 3.6 `action` (27개 파일)

#### 핵심 인터페이스 / 베이스

| 파일 | 설명 |
|------|------|
| `res/action/IAction.cs` | 액션 인터페이스 |
| `res/action/IActionCondition.cs` | 조건 인터페이스 |
| `res/action/ActionBase.cs` | 공통 보일러플레이트 추상 베이스 |
| `res/action/ActionContext.cs` | 실행 컨텍스트 (caster, target, systems) |
| `res/action/ActionResult.cs` | 실행 결과 값 객체 |
| `res/action/ActionIds.cs` | 액션 ID 문자열 상수 |
| `res/action/ActionMetadata.cs` | UI용 메타데이터 |
| `res/action/ActionRegistry.cs` | 전체 액션 등록 및 필터링 |

#### 조건 (4개)

| 파일 |
|------|
| `res/action/conditions/MinApCondition.cs` |
| `res/action/conditions/TargetAliveCondition.cs` |
| `res/action/conditions/ClipboardNotEmptyCondition.cs` |
| `res/action/conditions/NotStatusCondition.cs` |

#### 구현체 (15개)

| 파일 | AP |
|------|----|
| `res/action/implementations/ActionConstants.cs` | 상수 모음 |
| `res/action/implementations/InspectAction.cs` | 0 |
| `res/action/implementations/OpenAction.cs` | 1 |
| `res/action/implementations/SearchAction.cs` | 1 |
| `res/action/implementations/LogForgeAction.cs` | 1 |
| `res/action/implementations/ShowHiddenAction.cs` | 1 |
| `res/action/implementations/CopyAction.cs` | 1 |
| `res/action/implementations/DeleteAction.cs` | 2 |
| `res/action/implementations/CutAction.cs` | 2 |
| `res/action/implementations/PasteAction.cs` | 2 |
| `res/action/implementations/QuarantineAction.cs` | 2 |
| `res/action/implementations/CompressAction.cs` | 2 |
| `res/action/implementations/PermissionOverrideAction.cs` | 2 |
| `res/action/implementations/StunAction.cs` | 2 |
| `res/action/implementations/CleanAction.cs` | 3 (AoE) |

**전환 형태**: 인터페이스 → duck typing 또는 base `class_name`, 조건/구현체 → `class_name` 클래스

---

### 3.7 `skills` (7개 파일)

| 파일 | 설명 |
|------|------|
| `res/skills/SkillDefinition.cs` | 스킬 데이터 정의 |
| `res/skills/SkillCatalog.cs` | 스킬 등록/조회 |
| `res/skills/SkillBehaviorKeys.cs` | 행동 키 상수 |
| `res/skills/SkillBehaviorNode.cs` | 행동 트리 노드 베이스 |
| `res/skills/SkillBehaviorFactory.cs` | 스킬별 행동 트리 생성 |
| `res/skills/SkillExecutionContext.cs` | 스킬 실행 컨텍스트 |
| `res/skills/SkillExecutor.cs` | 스킬 실행기 |

**전환 형태**: `class_name` 기반; 행동 트리 패턴 유지

---

### 3.8 `battle` — 레거시 (5개 파일)

| 파일 | 설명 |
|------|------|
| `res/battle/BattleConstants.cs` | 경로 상수 (일부 `BattleFactory`가 참조) |
| `res/battle/DungeonFolderMetadata.cs` | 던전 폴더 메타 |
| `res/battle/BattleDungeon.cs` | 던전 구조 |
| `res/battle/BattleFactory.cs` | 던전 생성 팩토리 (미션별 레이아웃 포함) |
| `res/battle/BattleManager.cs` | HP 추적·게임 종료 전용으로 격리된 레거시 매니저 |

**전환 방침**: `BattleConstants` → `InfiltrationTuning`/상수 파일로 통합; `BattleFactory`/`BattleDungeon` → GDScript로 전환 또는 `InfiltrationManager` 내부로 흡수; `BattleManager` → HP 최소 인터페이스 후 제거

---

### 3.9 `scenes` (3개 파일)

| 파일 | 씬 | 설명 |
|------|----|------|
| `res/scenes/TitleScene.cs` | `TitleScene.tscn` | 타이틀 화면 |
| `res/scenes/MainMenu.cs` | `main.tscn` | 미션 보드 / 캠페인 메뉴 |
| `res/scenes/BattleScene.cs` | `BattleScene.tscn` | 침투 런 메인 화면 (가장 큰 씬) |

**전환 형태**: `.gd` 씬 스크립트로 1:1 교체 후 `.tscn` ExtResource 경로 갱신

---

## 4. 전환 순서 (권장)

아래 순서는 의존성이 낮은 레이어에서 높은 레이어로 올라가는 원칙을 따른다.

| 단계 | 모듈 | 이유 |
|------|------|------|
| 1 | `data` (열거형·값 객체) | 의존성 없음, 나머지 모든 모듈이 참조 |
| 2 | `core` (`DebugLog`, `GameManager` 스켈레톤) | 모든 런타임 모듈이 로깅에 의존 |
| 3 | `systems` (`ClipboardSystem`, `StatusEffectSystem`) | `action`이 참조 |
| 4 | `infiltration` (열거형 → 데이터 클래스 → 보안 파이프라인 순) | 내부 순서: 열거형 → 값 객체 → 파이프라인 → 상태 → 매니저 |
| 5 | `mission` | `CampaignState`는 `MissionData`·`MissionBoardFactory`에 의존 |
| 6 | `action` (인터페이스 → 베이스 → 조건 → 구현체 순) | `infiltration`·`skills`이 참조 |
| 7 | `skills` | `action` 완성 후 |
| 8 | `battle` (레거시 정리) | `BattleManager` 최소화 후 제거 경로 확정 |
| 9 | `scenes` | 모든 하위 레이어 완성 후 마지막 |
| 10 | 프로젝트 파일 정리 | `.csproj`, `global.json`, `GlobalUsings.cs` 제거, docs 갱신 |

---

## 5. 파일 수 요약

| 모듈 | 파일 수 |
|------|---------|
| core | 2 |
| data | 5 |
| systems | 2 |
| infiltration | 20 |
| mission | 8 |
| action | 27 |
| skills | 7 |
| battle (레거시) | 5 |
| scenes | 3 |
| **소계 (res/)** | **79** |
| 프로젝트 레벨 | 3 |
| **합계** | **82** |
