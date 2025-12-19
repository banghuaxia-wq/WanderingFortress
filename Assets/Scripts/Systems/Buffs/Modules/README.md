# Buffs/Modules/ 模块文档

## 模块概述
- **主要职责**: 具体的Buff效果实现，基于ScriptableObject的可配置模块
- **设计原则**: 每个模块只负责单一效果，通过组合实现复杂行为
- **依赖关系**: 依赖父级Buffs/模块和Core/Data

## 脚本清单与说明

| 脚本文件 | 核心类名 | 职责描述 | 回调类型 |
|:---|:---|:---|:---|
| `ApplySlowMovementByStack.cs` | `ApplySlowMovementByStack` | 按层数减速移动速度 | OnCreate, OnRemove, OnAddStack |
| `CastDamageToPochieByStack.cs` | `CastDamageToPochieByStack` | 按层数造成持续伤害 | OnTick |
| `SedativeStunModule.cs` | `SedativeStunModule` | 镇静剂效果，累积眩晕值 | OnTick, OnCreate, OnRemove |

## 对外接口/依赖
- **依赖的模块**: `Systems/Buffs/`, `Core/Data`, `Systems/Player`, `Systems/Enemy`
- **对外提供的接口**: 无（通过Buff系统间接调用）
- **配置方式**: 通过Unity Inspector配置参数
