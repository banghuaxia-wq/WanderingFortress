# InventorySystem/ 模块文档

## 模块概述
- **主要职责**: 玩家背包、装备、快捷栏、仓库与转移等“库存业务规则”
- **设计原则**: 核心状态由系统层写入；UI 通过事件被动刷新
- **依赖关系**: 依赖 `Core/Data`、`Core/Events`、`Core/Interfaces`、`Systems/Inventory`

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `PlayerInventory.cs` | `PlayerInventory` | 玩家背包：容量、物品列表、重量统计、事件通知 | ✅ |
| `EquipmentSystem.cs` | `EquipmentSystem` | 装备管理：槽位映射、装备/卸下、事件通知 | ✅ |
| `HotbarSystem.cs` | `HotbarSystem` | 快捷栏：槽位设置与选择、事件通知 | ✅ |
| `StorageSystem.cs` | `StorageSystem` | 仓库：确保仓库容器存在与初始化 | ✅ |
| `WeightBuffSystem.cs` | `WeightBuffSystem` | 重量事件驱动速度倍率（超重/超载） | ✅ |
| `InventoryTransferSystem.cs` | `InventoryTransferSystem` | 监听转移请求事件，处理容器与背包的转移 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Data`, `Core/Events`, `Core/Interfaces`, `Systems/Inventory`
- **对外提供的接口**:
  - `PlayerInventory.Add()/RemoveAt()/GetTotalWeight()`

