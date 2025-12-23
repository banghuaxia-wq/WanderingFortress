# Inventory/ 模块文档

## 模块概述
- **主要职责**: 运行时物品体系：物品创建、物品类型实现与武器物品的派生结构
- **设计原则**: 物品“纯数据”在 `Core/Data`（如 `ItemType`），具体行为以接口拆分并由运行时类型实现
- **依赖关系**: 依赖 `Core/Data`、`Core/Interfaces`

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `ItemFactory.cs` | `ItemFactory` | 物品创建入口：由配置/类型创建运行时物品实例 | ✅ |
| `Items/ItemBase.cs` | `ItemBase` | 物品基类：通用字段与基础能力 | ✅ |
| `Items/*Item.cs` | `*Item` | 具体物品：Ammo/Consumable/Tool/Material/Quest/Weapon等 | ✅ |
| `Items/Weapons/*` | `*WeaponItem` | 武器物品派生：近战/远程/徒手等 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Data`, `Core/Interfaces`
- **对外提供的接口**:
  - `ItemFactory.Create*()`（物品创建入口）

