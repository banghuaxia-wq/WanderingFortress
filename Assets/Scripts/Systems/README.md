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
| `PlayerShooter.cs` | `PlayerShooter` | 玩家射击行为控制 |

### Enemy/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `EnemyCombatController.cs` | `EnemyCombatController` | 敌人战斗逻辑，实现IDamageable接口 |
| `EnemySpawner.cs` | `EnemySpawner` | 敌人生成器，管理生成点和状态UI绑定 |

### Buffs/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `BuffManager.cs` | `BuffManager` | Buff管理器，处理Buff的添加、移除、刷新 |
| `BuffModule.cs` | `BuffModule` | Buff模块基类(ScriptableObject) |

#### Buffs/Modules/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `ApplySlowMovementByStack.cs` | `ApplySlowMovementByStack` | 按层数减速移动的Buff模块 |
| `CastDamageToEnemyByStack.cs` | `CastDamageToEnemyByStack` | 按层数造成伤害的Buff模块 |
| `SedativeStunModule.cs` | `SedativeStunModule` | 镇静剂眩晕效果模块 |

### Camera/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `GameplayCameraProvider.cs` | `GameplayCameraProvider` | 游戏摄像机提供者(单例) |
| `TopDownCameraFollow.cs` | `TopDownCameraFollow` | 俯视角摄像机跟随控制器 |

### UI/HUD/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `HUDPanelController.cs` | `HUDPanelController` | E打开箱子与背包；I关闭两者或仅切背包；模态禁用操作 |

### EventSystem/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `GameEvents.cs` | `GameEvents` | 全局事件总线：容器/背包/装备/快捷栏/重量/转移 |

### ContainerSystem/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `ContainerManager.cs` | `ContainerManager` | 容器管理单例：注册、打开、更新、关闭 |
| `ContainerGenerator.cs` | `ContainerGenerator` | 容器与物品生成辅助：按类型创建/填充 |
| `ContainerActor.cs` | `ContainerActor` | 场景容器实体：注册与描边控制、提供容器ID |

### InventorySystem/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `PlayerInventory.cs` | `PlayerInventory` | 玩家背包：容量、物品列表、总重量、事件通知 |
| `EquipmentSystem.cs` | `EquipmentSystem` | 装备管理：槽位映射、装备/卸下、事件通知 |
| `HotbarSystem.cs` | `HotbarSystem` | 快捷栏：槽位设置与事件通知、可用性限制 |
| `StorageSystem.cs` | `StorageSystem` | 仓库：确保仓库容器存在与初始化 |
| `WeightBuffSystem.cs` | `WeightBuffSystem` | 重量事件驱动速度倍率（超重/超载） |

### Interaction/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `InteractiveSelector.cs` | `InteractiveSelector` | 玩家交互选择器：最近容器描边与按E打开 |

### Developer/
| 脚本文件 | 核心类名 | 职责描述 |
|:---|:---|:---|
| `DeveloperManager.cs` | `DeveloperManager` | 开发者工具：加物品、调数值、施加Buff、存/读档 |

## 对外接口/依赖
- **依赖的模块**: `Core/`
- **对外提供的接口**:
  - `PlayerStateManager.Instance` (单例访问)
  - `EnemyCombatController.TakeDamage()`
  - `BuffManager.AddBuff()/RemoveBuff()`
  - `GameEvents.*` 事件派发与订阅接口
  - `ContainerManager.Open()/AddItem()/RemoveItem()`
  - `PlayerInventory.Add()/RemoveAt()/GetTotalWeight()`
