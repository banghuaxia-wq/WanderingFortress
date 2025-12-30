# WorldSimulation/ 模块文档

## 模块概述
- **主要职责**: 以玩家（或指定中心）为基准，对动态实体进行距离分级（Active/Passive/Dormant）
- **设计原则**: 分级系统只做“状态驱动”，具体降级策略由实体代理（组件/对象开关列表）决定
- **依赖关系**: 依赖 `Core/Data`、`Core/Interfaces`

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `SimulationRangeSystem.cs` | `SimulationRangeSystem` | 距离分级系统：分桶筛选、回滞阈值切换、短时唤醒 | ✅ |
| `SimulationLodAgent.cs` | `SimulationLodAgent` | 分级实体代理：批量开关组件/对象 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Data`, `Core/Interfaces`
- **对外提供的接口**:
  - `SimulationRangeSystem.Register()/Unregister()`
  - `SimulationRangeSystem.ForceMinimumState()`
  - `ISimulationLodAgent.ApplyState()`

