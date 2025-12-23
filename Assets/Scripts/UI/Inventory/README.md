# Inventory/（UI）模块文档

## 模块概述
- **主要职责**: 背包/容器/装备/快捷栏等界面渲染与拖拽交互元数据绑定
- **设计原则**: UI 不直接写业务状态；通过事件订阅刷新显示
- **依赖关系**: 依赖 `Systems/InventorySystem`、`Systems/ContainerSystem`、`Core/Events`

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `PackagePanelManager.cs` | `PackagePanelManager` | 背包格子渲染：订阅背包更新事件刷新 | ✅ |
| `PackagePanelToggle.cs` | `PackagePanelToggle` | 背包面板按钮：开关显示 | ✅ |
| `PackageUISlotController.cs` | `PackageUISlotController` | 槽位UI：绑定物品与拖拽元数据 | ✅ |
| `BoxPanelManager.cs` | `BoxPanelManager` | 容器格子渲染：订阅容器事件刷新 | ✅ |
| `EquipmentPanelManager.cs` | `EquipmentPanelManager` | 固定装备槽位绑定与更新 | ✅ |
| `HotbarPanelManager.cs` | `HotbarPanelManager` | 快捷栏内容父节点管理 | ✅ |
| `UISlotPoolManager.cs` | `UISlotPoolManager` | Slot 对象池出入管理 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Events`, `Systems/InventorySystem`, `Systems/ContainerSystem`
- **对外提供的接口**: 无（UI内部使用）

