# 🎯 WF.Gameplay 项目架构文档

## 项目概述
基于SOC（关注点分离）原则的Unity游戏架构，提供清晰的模块划分和依赖管理。

## 🏗️ 架构层次

### Core/ (核心层)
- **职责**: 基础构建块，与具体游戏逻辑解耦
- **包含**: 数据定义、接口契约、通用工具
- **依赖**: 无（最底层）

### Systems/ (系统层)  
- **职责**: 核心游戏逻辑和业务规则
- **包含**: 玩家、敌人、战斗、物品等系统
- **依赖**: Core/

### UI/ (视图层)
- **职责**: 用户界面显示和交互
- **包含**: HUD、世界空间UI、界面控制器
- **依赖**: Systems/, Core/

## 📂 模块目录

### [Core/](./Core/README.md)
- `Data/` - 数据结构和配置
- `Interfaces/` - 接口契约
- `Utilities/` - 通用工具类

### [Systems/](./Systems/README.md)
- `Player/` - 玩家相关系统
- `Enemy/` - 敌人相关系统  
- `Buffs/` - Buff/Debuff系统
- `Camera/` - 摄像机控制
- `Weapons/` - 武器系统
- `Items/` - 物品系统

### [UI/](./UI/README.md)
- `HUD/` - 平视显示器
- `WorldSpaceInfo/` - 世界空间UI
- `Crosshair/` - 准星控制

## 🔗 依赖规则