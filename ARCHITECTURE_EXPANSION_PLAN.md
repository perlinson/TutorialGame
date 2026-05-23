# 架构扩展规划（修仙游戏属性系统 V3.0）

> 目标：在现有 QFramework `Utility / Model / System / Command / Event` 分层之上，
> 实现"修仙模拟 + 回合制战斗"的完整属性体系，包含基础属性、分支系统、流派、神通等核心玩法。
> 当前已就绪部分见 `REFACTOR_CHECKLIST.md`，本文只列**新增/拆分**项。

---

## 现状速查

- **Utility（6）**：`IGameResourceService` / `IGameSettingsService` / `IGameAudioService` / `IGameLogService` / `IGameUiService` / `IGameInputService`
- **Model（7）**：`Archive` / `Inventory` / `Player` / `Game` / `TaskBoard` / `WorldMap` / `Expedition`
- **System（17）**：`Sound` / `Save` / `Condition` / `Story` / `MindState` / `Faction` / `Reward` / `Task` / `Settlement` / `Sect` / `Npc` / `EncounterDirector` / `EnemyAi` / `WorldMap` / `Battle` / `Expedition` / `ExpeditionEvent`

---

## P0 · 核心架构（已完成）

### Utility
- [x] **U1 `IGameDataService` / `IConfigService`**：统一加载 `ScriptableObject / Json / Csv` 配置表（功法、丹方、怪物、地图、事件、技能、Buff）。
- [x] **U2 `IGameInputService`**：抽象 `New Input System`，封装键盘、鼠标输入。
- [x] **U3 `IGameTimeService`**：把 `CultivationGameTime` 升格为服务。
- [x] **U4 `IGameRandomService`**：带种子的随机源。

### Model
- [x] **M1 `CultivationRealmModel`**：境界 / 修为 / 瓶颈 / 突破 / 心魔。
- [x] **M2 `CultivationAttributeModel`**：根骨 / 悟性 / 神识 / 气血基底 / 法力基底 / 魅力。
- [x] **M3 `CultivationSkillModel`**：已学功法 / 装配槽位 / 冷却。
- [x] **M4 `CultivationCombatStatsModel`**：战斗运行时属性中央表。

### System
- [x] **S1 `CultivationRealmSystem`**：修炼增益结算、突破判定、瓶颈、心魔。
- [x] **S2 `CultivationSkillCastSystem`**：回合内技能释放管线。
- [x] **S3 `CultivationDamageSystem`**：伤害公式 + 元素克制 + 暴击 + 抗性。
- [x] **S4 `CultivationBuffSystem`**：状态层运行时管理。

---

## P1 · 属性系统扩展（新增）

### 一、基础属性体系（10个）

| 属性 | 英文 | 核心作用 | 主要影响流派 | 成长方式 |
|------|------|---------|-------------|---------|
| **神识** | Divine Sense | 神魂强度、感知范围 | 法修、音修、阵修 | 境界突破、功法、丹药 |
| **根骨** | Constitution | 肉身强度、生命成长 | 体修、剑修、器修 | 先天、炼体功法、洗髓丹 |
| **悟性** | Comprehension | 学习速度、领悟能力 | 所有流派（通用） | 先天、悟道、开悟丹 |
| **福缘** | Fortune | 奇遇概率、掉落品质 | 所有流派（通用） | 先天、功德、奇遇（不可主动修炼） |
| **魅力** | Charm | NPC好感、社交效果 | 社交、双修 | 先天、事件、驻颜丹 |
| **魂力** | Soul Power | 灵魂强度、附魔能力 | 音修、符修、鬼修 | 修炼魂道功法、击杀鬼物、特殊丹药 |
| **精元** | Vital Energy | 生命力、炼器火候 | 器修、体修、丹修 | 炼体、服用天材地宝、特殊功法 |
| **意志** | Willpower | 心魔抵抗、突破成功率 | 所有流派（通用） | 历练、渡劫、心魔考验 |
| **机巧** | Dexterity | 手工精细度、绘制能力 | 符修、阵修、器修 | 练习制符/布阵/炼器、特殊功法 |
| **灵根** | Spirit Root | 五行亲和、灵气吸收 | 法修、丹修 | 先天（极难改变） |

### 二、属性分支系统（核心设计）

#### 2.1 核心概念
- 每个基础属性拆分为**4个可独立成长的分支**
- 分支等级和基础属性是**独立维度**，没有数学等式关系
- 基础属性代表天赋，影响修炼效率
- 分支代表后天修炼成果

#### 2.2 分支等级计算公式
```
分支最终等级 = 大境界基数 + 功法侧重加成 + 玩家主动修炼投入 + 临时加成
```

#### 2.3 分支定义

**神识分支**：
- 强度 → 探测范围、穿透力、感知隐藏
- 掌控 → 操控精度、远程取物、炼丹辅助
- 攻击 → 神魂伤害、眩晕概率
- 防御 → 心魔抗性、幻术抵抗

**根骨分支**：
- 体质 → 生命上限、物理防御
- 恢复 → 血量/灵力自然恢复
- 抗性 → 毒抗、眩晕抗、即死抗
- 炼体 → 肉身攻击、反伤

**悟性分支**：
- 学习 → 功法熟练度获取速度
- 领悟 → 顿悟新神通概率
- 融会 → 功法组合效果
- 推演 → 战斗预判、闪避

**魅力分支**：
- 好感 → NPC初始友好度
- 说服 → 交易价格、任务接取
- 威慑 → 低等级敌人逃跑
- 魅惑 → 战斗控制效果

**魂力分支**：
- 强度 → 魂力总量、鬼物强度
- 韧性 → 灵魂防御、心魔抵抗
- 附魔 → 符箓威力、法宝附灵
- 共鸣 → 音波效果、群体增益

**精元分支**：
- 储量 → 炼器/炼丹持续次数
- 纯度 → 法宝/丹药品质
- 恢复 → 精元恢复速度
- 转化 → 精元→灵力/生命转换效率

**意志分支**：
- 坚定 → 心魔抵抗、突破成功率
- 专注 → 修炼效率、技能释放稳定性
- 威慑 → 压制低意志敌人
- 牺牲 → 濒死时爆发潜力

**机巧分支**：
- 精度 → 符箓绘制成功率、阵法布设精度
- 速度 → 绘制/炼器速度
- 创新 → 自创符箓/阵法概率
- 灵巧 → 同时操控多个精细操作

**福缘（特殊，无分支）**：
- 先天出身（开局选择）
- 奇遇事件（随机获得）
- 功德累积（行善积德）
- 特殊天命（剧情获得）

### 三、流派系统（9大流派）

| 流派 | 神识 | 根骨 | 悟性 | 福缘 | 魅力 | 魂力 | 精元 | 意志 | 机巧 | 灵根 |
|------|:----:|:----:|:----:|:----:|:----:|:----:|:----:|:----:|:----:|:----:|
| 法修 | ⭐⭐⭐ | ⭐ | ⭐⭐ | ⭐ | - | - | - | ⭐ | - | ⭐⭐⭐ |
| 体修 | - | ⭐⭐⭐ | ⭐ | ⭐ | - | - | ⭐⭐ | ⭐⭐ | - | - |
| 剑修 | ⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐ | - | - | - | ⭐⭐⭐ | - | ⭐ |
| 音修 | ⭐⭐⭐ | - | ⭐⭐ | ⭐ | ⭐ | ⭐⭐⭐ | - | ⭐ | - | - |
| 器修 | ⭐ | ⭐⭐ | ⭐ | ⭐ | - | - | ⭐⭐⭐ | - | ⭐⭐⭐ | - |
| 符修 | ⭐ | - | ⭐⭐ | ⭐ | - | ⭐⭐⭐ | - | - | ⭐⭐⭐ | - |
| 阵修 | ⭐⭐⭐ | - | ⭐⭐⭐ | ⭐ | - | ⭐⭐ | - | ⭐ | ⭐⭐ | - |
| 丹修 | ⭐⭐⭐ | - | ⭐⭐ | ⭐⭐ | - | - | ⭐⭐ | - | ⭐⭐ | ⭐⭐ |
| 鬼修 | ⭐⭐⭐ | - | ⭐ | ⭐⭐ | - | ⭐⭐⭐ | - | ⭐⭐⭐ | - | - |

### 四、大境界系统

#### 4.1 境界序列
1. 炼气期（Qi Condensation）
2. 筑基期（Foundation）
3. 金丹期（Golden Core）
4. 元婴期（Nascent Soul）
5. 化神期（Deity Transformation）

#### 4.2 境界突破效果

**A. 必然提升（修仙铁律）**：
| 境界 | 神识提升 | 生命上限 | 灵力上限 |
|------|---------|---------|---------|
| 筑基 | +30 | +100 | +80 |
| 金丹 | +50 | +200 | +150 |
| 元婴 | +80 | +350 | +250 |
| 化神 | +120 | +500 | +400 |

**B. 分支基数提升（所有分支）**：
| 境界 | 全分支基数 | 神识分支额外 | 根骨分支额外 |
|------|-----------|-------------|-------------|
| 炼气 | 5 | - | - |
| 筑基 | 15 | +5 | +3 |
| 金丹 | 35 | +10 | +5 |
| 元婴 | 70 | +15 | +8 |
| 化神 | 120 | +20 | +10 |

**C. 可选突破奖励（玩家选择一项）**：
| 境界 | 可选奖励值 |
|------|-----------|
| 筑基 | 选择一项+5 |
| 金丹 | 选择一项+10 |
| 元婴 | 选择一项+15 |
| 化神 | 选择一项+20 |

### 五、神通领悟系统

#### 5.1 神通类型
- 小神通：单一效果（铁骨、轻身、明目）
- 大神通：复合效果+动画（法天象地、五行遁术）
- 被动神通：常驻增益（金刚不坏）
- 先天神通：开局自带的特殊能力

#### 5.2 神通触发方式
1. 属性阈值检测（根骨>100 → 铁骨）
2. 行为累计触发（被暴击100次 → 不屈）
3. 剧情触发（第一次死亡 → 涅槃）
4. 随机顿悟（修炼时有概率）

---

## P2 · 属性系统实现计划

### Model（新增）
- [ ] **M11 `CultivationBaseAttributeModel`**：10个基础属性（神识、根骨、悟性、福缘、魅力、魂力、精元、意志、机巧、灵根）
- [ ] **M12 `CultivationBranchModel`**：40个分支属性（每个基础属性4个分支）
- [ ] **M13 `CultivationSchoolModel`**：流派选择、流派加成
- [ ] **M14 `CultivationDivinePowerModel`**：已领悟神通列表
- [ ] **M15 `CultivationStatusModel`**：状态属性（饱食度、精力、心情、伤势、煞气）
- [ ] **M16 `CultivationSocialModel`**：缘分属性（声望、功德、杀气、道侣数量、宗门贡献）

### System（新增）
- [ ] **S5 `CultivationBranchSystem`**：分支修炼、分支等级计算、分支与战斗属性映射
- [ ] **S6 `CultivationSchoolSystem`**：流派选择、流派加成应用、流派技能解锁
- [ ] **S7 `CultivationDivinePowerSystem`**：神通领悟判定、神通效果应用
- [ ] **S8 `CultivationStatusSystem`**：状态属性衰减、状态对修炼效率影响
- [ ] **S9 `CultivationTrainingSystem`**：玩家主动修炼分支、修炼效率计算

### Data（新增）
- [ ] **D1 `BranchConfigAsset`**：分支配置（基础属性→分支映射、分支效果）
- [ ] **D2 `SchoolConfigAsset`**：流派配置（流派属性权重、流派特色技能）
- [ ] **D3 `DivinePowerConfigAsset`**：神通配置（触发条件、效果、分支前置）
- [ ] **D4 `ArtifactConfigAsset`**：功法配置（属性加成、分支侧重）

---

## P3 · MVP 优先级实现顺序

### 第一阶段：基础属性扩展（必须有）
1. 扩展 `CultivationAttributeModel`：添加魂力、精元、意志、机巧、灵根
2. 创建 `CultivationBranchModel`：实现分支数据结构
3. 创建 `CultivationBranchSystem`：实现分支修炼逻辑
4. 更新 `CultivationRealmSystem`：境界突破时提升分支基数

### 第二阶段：流派系统（中优先级）
1. 创建 `CultivationSchoolModel`：流派选择
2. 创建 `CultivationSchoolSystem`：流派加成应用
3. 创建 `SchoolConfigAsset`：流派配置数据
4. 更新战斗属性计算：考虑流派加成

### 第三阶段：神通系统（中优先级）
1. 创建 `CultivationDivinePowerModel`：神通列表
2. 创建 `CultivationDivinePowerSystem`：神通领悟判定
3. 创建 `DivinePowerConfigAsset`：神通配置数据
4. 实现神通效果（小神通优先）

### 第四阶段：状态与社交（低优先级）
1. 创建 `CultivationStatusModel`：状态属性
2. 创建 `CultivationStatusSystem`：状态衰减与影响
3. 创建 `CultivationSocialModel`：缘分属性
4. 实现社交系统（好感度、功德等）

---

## 命名与目录约定

- `Utility` → `Assets/Scripts/Architecture/Core/Services/{Domain}/`
- `Model` → `Assets/Scripts/Architecture/Core/Models/{Domain}/`
- `System` → `Assets/Scripts/Architecture/Systems/{Domain}/`
- `Data` → `Assets/Scripts/Data/{Domain}/`
- 每个新增 System / Utility 都需在 `@/Users/perlinson/develop/TutorialGame/Assets/Scripts/Architecture/Core/CultivationApp.cs` 的 `Init()` 注册

---

## 代码结构要求

```csharp
// 命名空间
namespace CultivationRPG.Attributes

// 枚举定义
public enum SpiritRootType { Mortal, Pseudo, True, Heavenly, Variant }
public enum CultivationLevel { QiCondensation, Foundation, GoldenCore, NascentSoul, DeityTransformation }
public enum BranchType
{
    DivineSense_Strength, DivineSense_Control, DivineSense_Attack, DivineSense_Defense,
    Constitution_Physique, Constitution_Recovery, Constitution_Resistance, Constitution_Tempering,
    Comprehension_Learning, Comprehension_Insight, Comprehension_Integration, Comprehension_Deduction,
    Charm_Favor, Charm_Persuasion, Charm_Deterrence, Charm_Charm,
    SoulPower_Strength, SoulPower_Toughness, SoulPower_Enchant, SoulPower_Resonance,
    VitalEnergy_Storage, VitalEnergy_Purity, VitalEnergy_Recovery, VitalEnergy_Conversion,
    Willpower_Steadfast, Willpower_Focus, Willpower_Deterrence, Willpower_Sacrifice,
    Dexterity_Precision, Dexterity_Speed, Dexterity_Innovation, Dexterity_Flexibility
}
public enum DivinePowerType { Minor, Major, Passive, Innate }
public enum SchoolType
{
    Mage, Body, Sword, Music, Artifact, Talisman, Formation, Pill, Ghost
}

// 数据结构
[Serializable] public class PlayerAttributes
[Serializable] public class InnateAttributes
[Serializable] public class CombatAttributes
[Serializable] public class CultivationAttributes
[Serializable] public class StatusAttributes
[Serializable] public class SocialAttributes
[Serializable] public class AttributeBranch

// 管理器
public class BranchGrowthManager
public class DivinePowerManager
public class RealmBreakthroughSystem
public class BranchTrainingManager
public class SchoolManager

// ScriptableObject配置
[CreateAssetMenu] public class DivinePowerData
[CreateAssetMenu] public class AttributeBranchConfig
[CreateAssetMenu] public class ArtifactData
[CreateAssetMenu] public class SchoolData
```

