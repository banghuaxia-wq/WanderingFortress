# Player/ 模块文档

## 模块概述
- **主要职责**: 玩家移动、状态（体力/无敌等）与战斗输入的基础系统
- **设计原则**: 状态数据与表现解耦，通过事件向 UI 广播结果
- **依赖关系**: 依赖 `Core/Events`，并与 `Systems/Camera`、`Systems/Combat` 形成协作

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `PlayerStateManager.cs` | `PlayerStateManager` | 玩家状态管理（体力/无敌），发布体力事件 | ✅ |
| `PlayerStats.cs` | `PlayerStats` | 玩家基础属性与体力参数配置 | ✅ |
| `PlayerMove.cs` | `PlayerMove` | 玩家移动与输入状态联动 | ✅ |
| `PlayerCombat.cs` | `PlayerCombat` | 玩家战斗输入与攻击调用 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Events`, `Systems/Camera`, `Systems/Combat`
- **对外提供的接口**:
  - `PlayerStateManager.Instance`

