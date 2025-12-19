# Core/ 模块文档

## 模块概述
- **主要职责**: 提供游戏无关的基础构建块，确保高可复用性
- **设计原则**: 与具体游戏逻辑完全解耦，可在其他项目中直接复用
- **依赖关系**: 无外部依赖，为最底层模块

## 脚本清单与说明

### Data/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `DamageInfo.cs` | `DamageInfo` | 伤害信息数据结构，包含伤害源、类型、数值等 |
| `DamageType.cs` | `DamageType` | 伤害类型枚举定义 |
| `BuffData.cs` | `BuffData` | Buff配置数据(ScriptableObject) |
| `BuffRunTimeInfo.cs` | `BuffRunTimeInfo` | Buff运行时信息容器 |
| `PochieType.cs` | `PochieType` | Pochie类型枚举定义 |
| `PochieState.cs` | `PochieState` | Pochie状态枚举：Active/Stunned/Ally |
| `TameMethod.cs` | `TameMethod` | 驯服方式枚举定义 |
| `ItemType.cs` | `ItemType` | 物品类型枚举：消耗品/工具/武器/防具/材料/任务 |
| `EquipmentSlotType.cs` | `EquipmentSlotType` | 装备槽位枚举：头/甲/手/裤/鞋 |
| `ContainerType.cs` | `ContainerType` | 容器类型枚举：普通箱/食物箱/武器箱/防具箱/大宝箱/仓库 |
| `ItemStack.cs` | `ItemStack` | 物品堆栈数据结构：数量、重量、图标等 |
| `ContainerData.cs` | `ContainerData` | 独立容器数据：类型、格子上限、物品列表 |
| `TransferRequest.cs` | `TransferRequest` | 拖拽转移请求数据：来源/目标/索引 |
| `Weapons/SOBullet.cs` | `SOBullet` | 子弹配置数据 (ScriptableObject) |
| `Weapons/SOGun.cs` | `SOGun` | 枪械配置数据 (ScriptableObject) |

### Events/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `EventBus.cs` | `EventBus` | 全局事件总线（发布/订阅模式） |
| `GameEventDefinitions.cs` | `ContainerOpenedEvent...` | 定义各类事件数据结构 (Struct)：包含 `StaminaChangedEvent`、`HotbarSelectionChangedEvent`、`CrosshairOffsetEvent` 等 |

### Interfaces/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `IDamageable.cs` | `IDamageable` | 可受伤对象接口，定义受伤行为 |
| `IPoolable.cs` | `IPoolable` | 对象池可回收对象接口 |
| `ICapturable.cs` | `ICapturable` | 可捕获对象接口 |
| `IRecallable.cs` | `IRecallable` | 可召回对象接口 |
| `IUsableItem.cs` | `IUsableItem` | 可使用物品接口：定义 `Use()` 行为 |
| `IContainerManager.cs` | `IContainerManager` | 容器管理器接口 |
| `IPlayerInventory.cs` | `IPlayerInventory` | 玩家库存接口 |
| `IPochieFactory.cs` | `IPochieFactory` | Pochie工厂契约：从数据/预制体创建实体 |

### Utilities/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `Pooling/PoolManager.cs` | `PoolManager` | 通用对象池管理器 |
| `Pooling/PooledObject.cs` | `PooledObject` | 可池化对象组件 |

## 对外接口/依赖
- **依赖的模块**: 无
- **对外提供的接口**: 
  - `IDamageable.TakeDamage(DamageInfo)`
  - `EventBus.Publish()/Subscribe()`
  - `PoolManager.Get()/Release()`
