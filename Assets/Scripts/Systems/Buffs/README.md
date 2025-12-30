# Buffs/ 模块文档

## 模块概述
- **主要职责**: Buff/Debuff 的添加、移除、刷新与模块化执行
- **设计原则**: Buff 行为可拆分为 `Modules`，按配置组合扩展
- **依赖关系**: 依赖 `Core/Data`、`Core/Events`

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `BuffManager.cs` | `BuffManager` | Buff管理器：添加/移除/刷新与驱动 | ✅ |
| `BuffModule.cs` | `BuffModule` | Buff模块基类（ScriptableObject） | ✅ |
| `Modules/README.md` | - | Buff模块清单与说明 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Data`, `Core/Events`
- **对外提供的接口**:
  - `BuffManager.AddBuff()/RemoveBuff()`

