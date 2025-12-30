# Building/ 模块文档

## 模块概述
- **主要职责**: 提供基于精细网格的建筑占格、预览与放置入口
- **设计原则**: 数据（`Core/Data`）与逻辑（`Systems/Building`）分离；占格为权威数据写入点
- **依赖关系**: 依赖 `Core/Data`

## 脚本清单与说明
| 脚本文件 | 核心类名 | 职责描述 | 状态 |
|:---|:---|:---|:---|
| `BuildGridSystem.cs` | `BuildGridSystem` | 网格占用写入/查询，支持Footprint与旋转 | ✅ |
| `BuildModeController.cs` | `BuildModeController` | 建造模式输入：预览、旋转、点击放置 | ✅ |
| `GridOverlayController.cs` | `GridOverlayController` | 网格覆盖与预览高亮显示控制 | ✅ |

## 对外接口/依赖
- **依赖的模块**: `Core/Data`
- **对外提供的接口**:
  - `BuildGridSystem.CanPlace()/TryPlace()/TryRemove()`
  - `GridOverlayController.SetVisible()/SetPreview()`

