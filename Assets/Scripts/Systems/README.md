# Systems/ 模块文档

## 模块概述
- **主要职责**: 实现核心游戏逻辑和业务规则
- **设计原则**: 高内聚、低耦合，每个系统职责单一
- **依赖关系**: 依赖Core/模块，严禁依赖UI/

## 脚本清单与说明

### Player/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `PlayerStateManager.cs` | `PlayerStateManager` | 玩家状态管理(单例)，处理体力、无敌状态等 |
| `PlayerStats.cs` | `PlayerStats` | 玩家基础属性管理 |
| `PlayerMove.cs` | `PlayerMove` | 玩家移动和旋转控制 |
| `PlayerCombat.cs` | `PlayerCombat` | 玩家战斗输入与攻击调用 |

### Pochie/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `PochieCombatController.cs` | `PochieCombatController` | Pochie战斗逻辑，实现IDamageable接口 |
| `PochieSpawner.cs` | `PochieSpawner` | Pochie生成器，管理生成点和状态UI绑定 |
| `PochieService.cs` | `PochieService` | Pochie生成/回收服务：统一生成与对象池回收逻辑 |
| `PochieFactory.cs` | `PochieFactory` | 兼容入口：转发到PochieService（保留旧调用方式） |

### Buffs/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `BuffManager.cs` | `BuffManager` | Buff管理器，处理Buff的添加、移除、刷新 |
| `BuffModule.cs` | `BuffModule` | Buff模块基类(ScriptableObject) |

#### Buffs/Modules/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `ApplySlowMovementByStack.cs` | `ApplySlowMovementByStack` | 按层数减速移动的Buff模块 |
| `CastDamageToPochieByStack.cs` | `CastDamageToPochieByStack` | 按层数造成伤害的Buff模块 |
| `SedativeStunModule.cs` | `SedativeStunModule` | 镇静剂眩晕效果模块 |

### Camera/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `GameplayCameraProvider.cs` | `GameplayCameraProvider` | 游戏摄像机提供者(单例) |
| `TopDownCameraFollow.cs` | `TopDownCameraFollow` | 俯视角摄像机跟随控制器 |

### Combat/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `AttackManager.cs` | `AttackManager` | 攻击调度：选择/驱动攻击行为 |
| `WeaponManager.cs` | `WeaponManager` | 武器管理：当前武器与攻击联动 |
| `AttackBehaviors/*` | `*AttackBehavior` | 具体攻击行为：近战/远程/徒手等策略 |

### Inventory/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `ItemFactory.cs` | `ItemFactory` | 物品创建入口：由数据创建运行时物品实例 |
| `Items/*` | `*Item` | 物品实现：Ammo/Consumable/Tool/Material/Quest/Weapon等 |

### Building/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `BuildGridSystem.cs` | `BuildGridSystem` | 网格占用写入/查询，支持Footprint与旋转 |
| `BuildModeController.cs` | `BuildModeController` | 建造模式输入：预览、旋转、点击放置 |
| `GridOverlayController.cs` | `GridOverlayController` | 网格覆盖与预览高亮显示控制 |

### WorldSimulation/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `SimulationRangeSystem.cs` | `SimulationRangeSystem` | 动态实体距离分级：Active/Passive/Dormant切换 |
| `SimulationLodAgent.cs` | `SimulationLodAgent` | 距离分级实体代理：批量开关组件/对象 |


### ContainerSystem/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `ContainerManager.cs` | `ContainerManager` | 容器管理单例：注册、打开、更新、关闭 |
| `ContainerGenerator.cs` | `ContainerGenerator` | 容器与物品生成辅助：按类型创建/填充 |
| `ContainerActor.cs` | `ContainerActor` | 场景容器实体：注册与描边控制、提供容器ID |
| `ContainerLootInitializer.cs` | `ContainerLootInitializer` | 容器掉落初始化：按配置填充容器内容 |

### InventorySystem/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `PlayerInventory.cs` | `PlayerInventory` | 玩家背包：容量、物品列表、总重量、事件通知 |
| `EquipmentSystem.cs` | `EquipmentSystem` | 装备管理：槽位映射、装备/卸下、事件通知 |
| `HotbarSystem.cs` | `HotbarSystem` | 快捷栏：槽位设置与事件通知、可用性限制 |
| `StorageSystem.cs` | `StorageSystem` | 仓库：确保仓库容器存在与初始化 |
| `WeightBuffSystem.cs` | `WeightBuffSystem` | 重量事件驱动速度倍率（超重/超载） |
| `InventoryTransferSystem.cs` | `InventoryTransferSystem` | 监听转移请求事件，处理容器到背包的转移 |

### Interaction/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `InteractiveSelector.cs` | `InteractiveSelector` | 玩家交互选择器：最近容器描边与按E打开 |

### Developer/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `DeveloperManager.cs` | `DeveloperManager` | 开发者工具：加物品、调数值、施加Buff、存/读档 |

### Weapons/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `Projectile/Projectile.cs` | `Projectile` | 投射物：飞行、碰撞、命中处理 |

## 子模块文档
- `Player/README.md` - 玩家系统文档
- `Pochie/README.md` - Pochie 系统文档
- `Buffs/README.md` - Buff 系统文档
- `Camera/README.md` - 摄像机系统文档
- `Combat/README.md` - 战斗系统文档
- `Inventory/README.md` - 物品系统文档
- `InventorySystem/README.md` - 背包/装备/快捷栏系统文档
- `ContainerSystem/README.md` - 容器系统文档
- `Interaction/README.md` - 交互系统文档
- `Building/README.md` - 建筑系统文档
- `WorldSimulation/README.md` - 世界动态分级系统文档
- `Weapons/README.md` - 武器通用模块文档

## 对外接口/依赖
- **依赖的模块**: `Core/`
- **对外提供的接口**:
  - `PlayerStateManager.Instance` (单例访问)
  - `PochieCombatController.TakeDamage()`
  - `BuffManager.AddBuff()/RemoveBuff()`
  - `EventBus.*` 事件发布与订阅 (Core/Events)
  - `ContainerManager.Open()/AddItem()/RemoveItem()`
  - `PlayerInventory.Add()/RemoveAt()/GetTotalWeight()` (玩家背包)
  - `BuildGridSystem.CanPlace()/TryPlace()/TryRemove()` (建筑占格写入/移除)
  - `SimulationRangeSystem.Register()/ForceMinimumState()` (动态实体距离分级)
