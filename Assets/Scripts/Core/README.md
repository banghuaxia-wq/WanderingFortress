# Core/ 模块文档

## 模块概述
- **主要职责**: 提供游戏无关的基础构建块，确保高可复用性
- **设计原则**: 与具体游戏逻辑完全解耦，可在其他项目中直接复用
- **依赖关系**: 无外部依赖，为最底层模块

## 脚本清单与说明

### Data/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `AttackData.cs` | `AttackData` | 攻击数据结构：攻击参数与配置 |
| `AttackType.cs` | `AttackType` | 攻击类型枚举定义 |
| `DamageInfo.cs` | `DamageInfo` | 伤害信息数据结构，包含伤害源、类型、数值等 |
| `DamageType.cs` | `DamageType` | 伤害类型枚举定义 |
| `BuffData.cs` | `BuffData` | Buff配置数据(ScriptableObject) |
| `BuffRunTimeInfo.cs` | `BuffRunTimeInfo` | Buff运行时信息容器 |
| `BuildGridConfig.cs` | `BuildGridConfig` | 建筑网格配置：单元尺寸与世界原点 |
| `BuildingDefinition.cs` | `BuildingDefinition` | 建筑定义(SO)：占格Footprint、预制体、旋转开关 |
| `GridRotation.cs` | `GridRotation` | 网格旋转枚举：R0/R90/R180/R270 |
| `HatchType.cs` | `HatchType` | Hatch类型枚举定义 |
| `HatchState.cs` | `HatchState` | Hatch状态枚举：Active/Stunned/Ally |
| `TameMethod.cs` | `TameMethod` | 驯服方式枚举定义 |
| `ItemType.cs` | `ItemType` | 物品类型枚举：消耗品/工具/武器/防具/材料/任务 |
| `ItemRarity.cs` | `ItemRarity` | 物品稀有度枚举 |
| `UISlotType.cs` | `UISlotType` | UI格子类型枚举：背包/装备/快捷栏 |
| `EquipmentSlotType.cs` | `EquipmentSlotType` | 装备槽位枚举：头/甲/手/裤/鞋 |
| `ContainerType.cs` | `ContainerType` | 容器类型枚举：普通箱/食物箱/武器箱/防具箱/大宝箱/仓库 |
| `ItemStack.cs` | `ItemStack` | 物品堆栈数据结构：数量、重量、图标等 |
| `ContainerData.cs` | `ContainerData` | 独立容器数据：类型、格子上限、物品列表 |
| `ContainerLootConfigSO.cs` | `ContainerLootConfigSO` | 容器掉落配置(SO)：容器类型到掉落表映射 |
| `ContainerLootMapping.cs` | `ContainerLootMapping` | 容器类型与掉落表映射条目 |
| `LootTableSO.cs` | `LootTableSO` | 掉落表(SO)：条目列表与抽取规则 |
| `LootEntry.cs` | `LootEntry` | 掉落表条目：物品/子表/权重等 |
| `LootEntryKind.cs` | `LootEntryKind` | 掉落表条目类型枚举 |
| `LootTier.cs` | `LootTier` | 掉落层级枚举 |
| `ResourceCost.cs` | `ResourceCost` | 资源消耗数据结构 |
| `SimulationRangeConfig.cs` | `SimulationRangeConfig` | 动态实体距离分级配置：半径/回滞/分桶/刷新间隔 |
| `SimulationLodState.cs` | `SimulationLodState` | 距离分级状态枚举：Dormant/Passive/Active |
| `TransferRequest.cs` | `TransferRequest` | 拖拽转移请求数据：来源/目标/索引 |
| `WeaponCategory.cs` | `WeaponCategory` | 武器分类枚举 |
| `ProgressEndReason.cs` | `ProgressEndReason` | 进度流程结束原因枚举：完成/取消/失败 |
| `ProgressViewMode.cs` | `ProgressViewMode` | 进度条视图模式枚举：全屏/HUD |
| `ProgressSnapshot.cs` | `ProgressSnapshot` | 进度流程快照数据：标题/进度/可取消等 |

### Events/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `EventBus.cs` | `EventBus` | 全局事件总线（发布/订阅模式） |
| `GameEventDefinitions.cs` | `ContainerOpenedEvent...` | 定义各类事件数据结构 (Struct)：包含 `StaminaChangedEvent`、`HotbarSelectionChangedEvent`、`CrosshairOffsetEvent`、`ProgressStartedEvent` 等 |

### Interfaces/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `IAttackBehavior.cs` | `IAttackBehavior` | 攻击行为契约：供Combat系统策略实现 |
| `IDamageable.cs` | `IDamageable` | 可受伤对象接口，定义受伤行为 |
| `IPoolable.cs` | `IPoolable` | 对象池可回收对象接口 |
| `ICapturable.cs` | `ICapturable` | 可捕获对象接口 |
| `IRecallable.cs` | `IRecallable` | 可召回对象接口 |
| `IConsumable.cs` | `IConsumable` | 可消耗物品接口 |
| `IStackable.cs` | `IStackable` | 可堆叠物品接口 |
| `IEquippable.cs` | `IEquippable` | 可装备物品接口 |
| `IWeaponItem.cs` | `IWeaponItem` | 武器物品契约 |
| `IItem.cs` | `IItem` | 通用物品契约 |
| `IUsableItem.cs` | `IUsableItem` | 可使用物品接口：定义 `Use()` 行为 |
| `IContainerManager.cs` | `IContainerManager` | 容器管理器接口 |
| `IPlayerInventory.cs` | `IPlayerInventory` | 玩家库存接口 |
| `IPlayerCombatState.cs` | `IPlayerCombatState` | 玩家战斗状态契约 |
| `IHatchFactory.cs` | `IHatchFactory` | Hatch工厂契约：从数据/预制体创建实体 |
| `IHatchService.cs` | `IHatchService` | Hatch生成/回收服务契约：供Spawner与道具复用 |
| `ISimulationLodAgent.cs` | `ISimulationLodAgent` | 动态实体距离分级代理契约 |

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

