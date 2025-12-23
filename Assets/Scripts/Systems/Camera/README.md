# Camera/ 模块文档

## 模块概述
- **主要职责**: 提供游戏摄像机访问入口与俯视角跟随控制
- **设计原则**: 业务系统获取摄像机优先走提供者，避免分散查找
- **依赖关系**: 依赖 `Core`（间接）与 Unity 引擎组件

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `GameplayCameraProvider.cs` | `GameplayCameraProvider` | 游戏摄像机提供者（入口统一） | ✅ |
| `TopDownCameraFollow.cs` | `TopDownCameraFollow` | 俯视角摄像机跟随控制器 | ✅ |

## 对外接口/依赖
- **依赖的模块**: 无（系统层内部使用）
- **对外提供的接口**:
  - `GameplayCameraProvider.TryGetGameplayCamera()`

