# WF.Gameplay 项目概述

## 目标与范围
- **项目类型**：Unity 游戏项目（2.5D俯视角，偏“搜打撤”玩法）
- **核心目标**：在保持清晰分层的前提下，快速迭代玩法系统，并且让后续 AI/新成员能低成本理解工程结构与调用方式

## 架构总览（SOC + 严格分层）
- **Core（基础层）**：纯数据（`Core/Data`）、接口契约（`Core/Interfaces`）、事件（`Core/Events`）、通用工具（`Core/Utilities`）
- **Systems（逻辑层）**：游戏业务规则与运行时系统（`Systems/*`）
- **UI（视图层）**：界面显示与交互（`UI/*`）
- **依赖方向**：`Core` → `Systems` → `UI`（禁止跨层直接引用非相邻层）

## 通信方式约定
- **命令（必须可控顺序/需返回结果）**：使用方法调用
  - 示例：容器打开、物品转移、装备/卸下、建筑放置/移除
- **通知（广播结果给多个订阅者）**：使用 `EventBus` 发布事件
  - 示例：容器已打开/更新、背包已更新、体力变化、快捷栏选择变化

## 运行时关键入口（常见“从哪进/到哪去”）
- **事件总线**：`Assets/Scripts/Core/Events/EventBus.cs`
- **对象池**：`Assets/Scripts/Core/Utilities/Pooling/PoolManager.cs`
- **玩家状态**：`Assets/Scripts/Systems/Player/PlayerStateManager.cs`
- **容器系统**：`Assets/Scripts/Systems/ContainerSystem/ContainerManager.cs`
- **背包系统**：`Assets/Scripts/Systems/InventorySystem/PlayerInventory.cs`
- **建筑系统（网格占格）**：`Assets/Scripts/Systems/Building/BuildGridSystem.cs`
- **世界动态分级（距离LOD）**：`Assets/Scripts/Systems/WorldSimulation/SimulationRangeSystem.cs`

## 主要系统如何运行（摘要）
### 容器与背包/UI联动
- **容器打开/更新**：`ContainerManager` 维护 `CurrentOpened`，并通过 `EventBus` 发布 `ContainerOpenedEvent` / `ContainerUpdatedEvent` / `ContainerClosedEvent`
- **UI刷新策略**：UI 在 `OnEnable` 订阅事件，收到事件后刷新渲染（例如 `Assets/Scripts/UI/Inventory/BoxPanelManager.cs`）

### 建筑（网格占格 + 预览/放置）
- **占格写入/查询**：`BuildGridSystem.CanPlace()` / `TryPlace()` / `TryRemove()`（`Assets/Scripts/Systems/Building/BuildGridSystem.cs`）
- **建造模式输入**：`BuildModeController` 负责开关模式、旋转、射线选格、点击放置（`Assets/Scripts/Systems/Building/BuildModeController.cs`）
- **网格覆盖显示**：`GridOverlayController` 负责网格线与预览高亮（`Assets/Scripts/Systems/Building/GridOverlayController.cs`）

### 世界动态实体距离分级（Active/Passive/Dormant）
- **分级系统**：`SimulationRangeSystem` 以中心点为基准，按配置回滞阈值切换状态（`Assets/Scripts/Systems/WorldSimulation/SimulationRangeSystem.cs`）
- **实体代理**：实体挂载 `SimulationLodAgent`，由系统驱动批量开关组件/对象（`Assets/Scripts/Systems/WorldSimulation/SimulationLodAgent.cs`）
- **短时唤醒**：`ForceMinimumState()` 用于枪声/爆炸/受击等场景的临时唤醒

## 当前对外接口清单（会随迭代维护）
### 单例/系统入口（不代表推荐用法，仅列出现状）
- `EventBus.Publish()/Subscribe()/Unsubscribe()`：`Assets/Scripts/Core/Events/EventBus.cs`
- `PoolManager.Instance`：`Assets/Scripts/Core/Utilities/Pooling/PoolManager.cs`
- `PlayerStateManager.Instance`：`Assets/Scripts/Systems/Player/PlayerStateManager.cs`
- `PlayerInventory.Instance`：`Assets/Scripts/Systems/InventorySystem/PlayerInventory.cs`
- `ContainerManager.Instance`：`Assets/Scripts/Systems/ContainerSystem/ContainerManager.cs`
- `SimulationRangeSystem.Instance`：`Assets/Scripts/Systems/WorldSimulation/SimulationRangeSystem.cs`

### 契约层接口（Core/Interfaces）
- `IContainerManager`：`Assets/Scripts/Core/Interfaces/IContainerManager.cs`
- `IPlayerInventory`：`Assets/Scripts/Core/Interfaces/IPlayerInventory.cs`
- `ISimulationLodAgent`：`Assets/Scripts/Core/Interfaces/ISimulationLodAgent.cs`
- 其余接口以 `Assets/Scripts/Core/Interfaces/` 为准

### 数据配置（Core/Data）
- `SimulationRangeConfig`：动态实体距离分级配置（`Assets/Scripts/Core/Data/SimulationRangeConfig.cs`）
- `BuildGridConfig`：建筑网格尺寸/原点配置（`Assets/Scripts/Core/Data/BuildGridConfig.cs`）
- `BuildingDefinition`：建筑定义（Footprint/Prefab/旋转开关）（`Assets/Scripts/Core/Data/BuildingDefinition.cs`）

## 待办与计划（Backlog）
- 敌人 AI 组件实现与性能分级接入（与 `SimulationRangeSystem` 协同）
- 建筑系统：资源消耗、拆除返还、保存/读档与回放（以 `BuildingDefinition.Id` 为键）
- 世界交互统一入口与交互反馈规范（动画/特效/音效的分级策略）
- 分层依赖的 `.asmdef` 物理隔离落地（Core/Systems/UI 三层）

