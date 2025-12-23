# WorldSpaceInfo/ 模块文档

## 模块概述
- **主要职责**: 世界空间信息显示（例如角色血条/眩晕条/提示信息），并跟随目标更新屏幕位置
- **设计原则**: 仅负责表现；目标绑定与数据来源通过系统或组件引用提供
- **依赖关系**: 依赖 `Systems/Camera` 与对应业务系统（如 `Systems/Pochie`）

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `PochieStatusUI.cs` | `PochieStatusUI` | Pochie状态UI：血条/眩晕条/提示信息 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Systems/Camera`, `Systems/Pochie`
- **对外提供的接口**:
  - `PochieStatusUI.*`（绑定与刷新）

