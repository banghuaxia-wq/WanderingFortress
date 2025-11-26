代码架构与规范规则文档 (Agent AI Guide)
0. 响应效率优化规则 (Response Efficiency Rules)
规则 0：文档优先原则

在响应任何用户需求时，Agent AI 必须首先阅读相关的README.md文件，以快速获取架构上下文和模块状态。

执行流程：

🥇 首先阅读根 Scripts/README.md 了解整体架构

🥈 按需阅读子模块README获取详细信息

🥉 仅在必要时深入分析具体代码文件

阅读目标：

✅ 快速理解模块职责和当前状态

✅ 定位相关文件和依赖关系

✅ 避免重复工作和架构冲突

✅ 提升响应速度60-80%

I. 核心设计原则：职责分离（Separation of Concerns - SOC）
规则 1：所有代码必须严格分层，分为 数据层 (Data)、契约层 (Interface)、逻辑层 (Systems) 和 视图层 (UI)。

规则 2：命名空间必须使用 WF.Gameplay，并根据其文件夹路径进行扩展。

II. 强制代码放置规则 (Mandatory Placement Rules)
规则名称	规则描述	目标路径	示例文件
契约统一	任何 interface 必须放在此文件夹中。严禁存放任何实现代码。	Scripts/Core/Interfaces/	IDamageable.cs, IPoolable.cs
纯数据放置	任何不包含 MonoBehaviour 且只定义数据的 class、struct 或 enum 必须放在此文件夹。	Scripts/Core/Data/	DamageInfo.cs, DamageType.cs
逻辑隔离	任何业务逻辑、核心管理器必须放在 Systems 文件夹中。严禁直接引用 UI 组件。	Scripts/Systems/[ModuleName]/	InventoryManager.cs, HealthSystem.cs
视图分离	任何继承自 MonoBehaviour 且包含 UI 组件引用的脚本必须放在 UI 文件夹中。	Scripts/UI/[ModuleName]/	PackageUISlotController.cs
III. 文档维护规则 (Documentation Maintenance Rules)
规则：实时README更新

在每次代码变更后必须立即更新对应的README.md文件

新增文件时必须在README的"脚本清单"中添加条目

修改文件功能时必须更新README中的职责描述

删除文件时必须从README中移除对应条目

规则：README文件结构

text
# [文件夹名] 模块文档

## 模块概述
- **主要职责**：[简要描述]
- **设计原则**：[架构原则]
- **依赖关系**：[依赖模块]

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `FileName.cs` | `ClassName` | 功能描述 | ✅/🚧 |

## 对外接口/依赖
- **依赖的模块**：[模块列表]
- **对外提供的接口**：[方法签名]
IV. 枚举文件规范 (Enum File Specifications)
规则：枚举独立放置

所有 enum 定义必须放在独立的 .cs 文件中

枚举文件必须放置在 Core/Data/ 文件夹中

文件名必须与枚举名完全一致

规则：ScriptableObject 分类

纯配置数据的 ScriptableObject 放在 Core/Data/

包含逻辑实现的 ScriptableObject 放在 Systems/[ModuleName]/

V. 文件拆分规范 (File Separation Rules)
规则：单一职责文件

每个 .cs 文件只能包含一个主要的 class、struct 或 enum

严禁在同一个文件中定义多个不相关的类型

VI. 命名空间映射规则 (Namespace Mapping Rules)
规则：严格路径映射

Scripts/Systems/Player/ → WF.Gameplay.Systems.Player

Scripts/UI/Crosshair/ → WF.Gameplay.UI.Crosshair

Scripts/Core/Data/ → WF.Gameplay.Core.Data

VII. UI 交互与模态规范
规则：拖放统一

所有拖放交互的UI元素必须共享同一个根Canvas

规则：模态控制

模态窗口通过根节点 SetActive 控制显示/隐藏

规则：输入阻断

模态窗口必须包含全屏 ModalBlockerPanel 阻挡输入