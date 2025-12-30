# ContainerSystem/ 模块文档

## 模块概述
- **主要职责**: 管理场景容器的注册、打开/关闭、内容变更与事件通知
- **设计原则**: `ContainerManager` 为容器数据权威写入点；UI 通过事件订阅刷新
- **依赖关系**: 依赖 `Core/Data`、`Core/Events`、`Core/Interfaces`

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `ContainerManager.cs` | `ContainerManager` | 容器管理：注册、打开、更新、关闭，并发布事件 | ✅ |
| `ContainerActor.cs` | `ContainerActor` | 场景容器实体：注册与描边控制、提供容器ID | ✅ |
| `ContainerGenerator.cs` | `ContainerGenerator` | 容器与物品生成辅助：按类型创建/填充 | ✅ |
| `ContainerLootInitializer.cs` | `ContainerLootInitializer` | 容器掉落初始化：按配置填充容器内容 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Data`, `Core/Events`, `Core/Interfaces`
- **对外提供的接口**:
  - `ContainerManager.Open()/Close()/AddItem()/RemoveItem()`
  - `IContainerManager`（契约）

