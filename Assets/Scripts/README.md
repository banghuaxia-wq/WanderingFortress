# 🎯 WF.Gameplay 项目架构文档

## 项目概述
基于SOC（关注点分离）原则的Unity游戏架构，提供清晰的模块划分和依赖管理。
- 项目整体运行方式与对外接口总览见 `../../PROJECT_OVERVIEW.md`

## 🏗️ 架构层次

### Core/ (核心层)
- **职责**: 基础构建块，与具体游戏逻辑解耦
- **包含**: 数据定义、接口契约、通用工具
- **依赖**: 无（最底层）

### Systems/ (系统层)  
- **职责**: 核心游戏逻辑和业务规则
- **包含**: 玩家、敌人、战斗、物品等系统
- **依赖**: Core/

### UI/ (视图层)
- **职责**: 用户界面显示和交互
- **包含**: HUD、世界空间UI、界面控制器
- **依赖**: Systems/, Core/

## 📂 模块目录

### [Core/](./Core/README.md)
- `Data/` - 数据结构和配置
- `Interfaces/` - 接口契约
- `Utilities/` - 通用工具类

### [Systems/](./Systems/README.md)
- `Player/` - 玩家相关系统
- `Enemy/` - 敌人相关系统  
- `Buffs/` - Buff/Debuff系统
- `Camera/` - 摄像机控制
- `Weapons/` - 武器系统
- `Items/` - 物品系统
- `Building/` - 建筑网格与放置系统
- `WorldSimulation/` - 动态实体距离分级与唤醒系统

### [UI/](./UI/README.md)
- `HUD/` - 平视显示器
- `WorldSpaceInfo/` - 世界空间UI
- `Crosshair/` - 准星控制

## 🔗 依赖规则
- 严格分层：`Core` → `Systems` → `UI`，不得跨层直接引用非相邻层。
- 程序集定义隔离：使用 `.asmdef` 强制物理依赖方向（Core 不引用任何层；Systems 仅引用 Core；UI 引用 Core 和 Systems）。
- 事件总线通信：跨模块交互通过 `EventBus` 的发布/订阅实现，事件定义在 `Core/Events/`。
- 依赖注入：纯 C# 类使用构造函数注入；`MonoBehaviour` 通过容器属性/方法注入，禁止在 `MonoBehaviour` 使用构造函数。
- 异步规范：优先使用 `UniTask`，所有异步方法以 `Async` 结尾并接受 `CancellationToken`。
- 资源管理：禁止硬编码路径，动态资源统一使用 Addressables 或 `AssetReference`。
- 文档优先：实现前先阅读根与子模块 README；重大变更后更新相关 README 的脚本清单与职责说明。
- 文档补充：当 `PROJECT_OVERVIEW.md` 中的接口/流程发生变化时，同步维护该文件。
