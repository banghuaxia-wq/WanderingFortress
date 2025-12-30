# Interaction/ 模块文档

## 模块概述
- **主要职责**: 玩家交互选择与触发（例如对容器的描边与按键打开）
- **设计原则**: 交互只做“选择/触发”，业务处理交由对应权威系统完成
- **依赖关系**: 依赖 `Systems/ContainerSystem`、`Core/Events`

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `InteractiveSelector.cs` | `InteractiveSelector` | 最近交互对象选择、描边控制与按键触发 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Systems/ContainerSystem`, `Core/Events`
- **对外提供的接口**: 无（通过事件/系统调用触发）

