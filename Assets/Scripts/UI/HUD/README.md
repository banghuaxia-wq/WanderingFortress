# HUD/ 模块文档

## 模块概述
- **主要职责**: HUD 面板开关、快捷栏显示、准星与体力条等即时信息展示
- **设计原则**: 只做表现层；数据来源通过事件或系统接口获取
- **依赖关系**: 依赖 `Systems/*` 与 `Core/Events`

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `HUDPanelController.cs` | `HUDPanelController` | 管理 Package/Box 面板开关与模态输入 | ✅ |
| `HotbarController.cs` | `HotbarController` | 快捷栏槽位选择与高亮 | ✅ |
| `HotbarSlot.cs` | `HotbarSlot` | 快捷栏槽位组件（选中背景引用） | ✅ |
| `CrosshairController.cs` | `CrosshairController` | 准星跟随与偏移反馈 | ✅ |
| `StaminaHUDController.cs` | `StaminaHUDController` | 体力条材质驱动填充与显隐 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Events`, `Systems/Player`, `Systems/InventorySystem`
- **对外提供的接口**:
  - `CrosshairController.SetExternalOffset()`

