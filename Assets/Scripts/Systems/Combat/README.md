# Combat/ 模块文档

## 模块概述
- **主要职责**: 统一攻击调度与武器联动，将攻击行为抽象为可扩展策略
- **设计原则**: 攻击行为通过契约与可替换实现扩展；避免将“命令链路”绑定到事件顺序
- **依赖关系**: 依赖 `Core/Interfaces`、`Core/Data`

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `AttackManager.cs` | `AttackManager` | 攻击调度：选择/驱动攻击行为 | ✅ |
| `WeaponManager.cs` | `WeaponManager` | 武器管理：当前武器与攻击联动 | ✅ |
| `AttackBehaviors/*` | `*AttackBehavior` | 具体攻击策略：近战/远程/徒手等 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Data`, `Core/Interfaces`
- **对外提供的接口**:
  - `IAttackBehavior`（策略契约）

