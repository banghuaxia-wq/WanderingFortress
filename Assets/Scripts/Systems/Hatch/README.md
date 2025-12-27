# Hatch/ 模块文档

## 模块概述
- **主要职责**: Hatch 的生成/回收、战斗控制与状态绑定
- **设计原则**: 生成/回收通过服务抽象复用；尽量避免多入口重复创建逻辑
- **依赖关系**: 依赖 `Core/Interfaces`、`Core/Data`，并与对象池协作

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `HatchSpawner.cs` | `HatchSpawner` | 生成器：管理生成点与状态UI绑定 | ✅ |
| `HatchService.cs` | `HatchService` | 生成/回收服务：统一生成与对象池回收逻辑 | ✅ |
| `HatchFactory.cs` | `HatchFactory` | 兼容入口：转发到 `HatchService` | ✅ |
| `HatchCombatController.cs` | `HatchCombatController` | 战斗逻辑，实现 `IDamageable` 等契约 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Interfaces`, `Core/Data`
- **对外提供的接口**:
  - `IHatchService` / `IHatchFactory`
  - `HatchCombatController.TakeDamage()`


