# UI/ 模块文档

## 模块概述
- **主要职责**: 处理所有用户界面显示和交互逻辑
- **设计原则**: 与业务逻辑分离，只负责表现层
- **依赖关系**: 依赖Systems/和Core/模块

## 脚本清单与说明

### Crosshair/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `CrosshairController.cs` | `CrosshairController` | 准星UI控制，跟随鼠标和受后坐力影响 |

### EnemyUI/  
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `EnemyStatusUI.cs` | `EnemyStatusUI` | 敌人状态UI(血条、眩晕条)，世界空间显示 |

### HUD/
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `HUDPanelController.cs` | `HUDPanelController` | 管理 Package/Box 面板开关与模态输入 | ✅ |
| `HotbarController.cs` | `HotbarController` | 快捷栏槽位选择与高亮 | ✅ |
| `CrosshairController.cs` | `CrosshairController` | 准星跟随与系统光标控制 | ✅ |
| `StaminaHUDController.cs` | `StaminaHUDController` | 材质驱动体力条填充与显隐逻辑 | ✅ |
| `HotbarSlot.cs` | `HotbarSlot` | 快捷栏槽位组件，引用选中背景 | ✅ |

### Inventory/
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `PackagePanelManager.cs` | `PackagePanelManager` | 根据背包物品数量渲染槽位 | ✅ |
| `BoxPanelManager.cs` | `BoxPanelManager` | 根据容器物品数量渲染槽位 | ✅ |
| `EquipmentPanelManager.cs` | `EquipmentPanelManager` | 固定装备槽位绑定与更新 | ✅ |
| `HotbarPanelManager.cs` | `HotbarPanelManager` | 快捷栏内容父节点管理 | ✅ |
| `UISlotPoolManager.cs` | `UISlotPoolManager` | 统一 Slot 对象池出入管理 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Systems/Player`, `Systems/Enemy`, `Core/Interfaces`
- **对外提供的接口**: 
  - `CrosshairController.SetExternalOffset()` (后坐力反馈)
  - `EnemyStatusUI.Initialize()` (绑定敌人控制器)
