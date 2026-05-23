/// <summary>
/// 属性分支类型枚举（10个基础属性 × 4个分支 = 40个分支）
/// </summary>
public enum BranchType
{
    // 神识分支
    DivineSense_Strength,      // 强度 → 探测范围、穿透力、感知隐藏
    DivineSense_Control,        // 掌控 → 操控精度、远程取物、炼丹辅助
    DivineSense_Attack,         // 攻击 → 神魂伤害、眩晕概率
    DivineSense_Defense,        // 防御 → 心魔抗性、幻术抵抗

    // 根骨分支
    Constitution_Physique,     // 体质 → 生命上限、物理防御
    Constitution_Recovery,      // 恢复 → 血量/灵力自然恢复
    Constitution_Resistance,    // 抗性 → 毒抗、眩晕抗、即死抗
    Constitution_Tempering,     // 炼体 → 肉身攻击、反伤

    // 悟性分支
    Comprehension_Learning,     // 学习 → 功法熟练度获取速度
    Comprehension_Insight,      // 领悟 → 顿悟新神通概率
    Comprehension_Integration,  // 融会 → 功法组合效果
    Comprehension_Deduction,    // 推演 → 战斗预判、闪避

    // 魅力分支
    Charm_Favor,                // 好感 → NPC初始友好度
    Charm_Persuasion,           // 说服 → 交易价格、任务接取
    Charm_Deterrence,           // 威慑 → 低等级敌人逃跑
    Charm_Charm,                // 魅惑 → 战斗控制效果

    // 魂力分支
    SoulPower_Strength,         // 强度 → 魂力总量、鬼物强度
    SoulPower_Toughness,        // 韧性 → 灵魂防御、心魔抵抗
    SoulPower_Enchant,          // 附魔 → 符箓威力、法宝附灵
    SoulPower_Resonance,        // 共鸣 → 音波效果、群体增益

    // 精元分支
    VitalEnergy_Storage,        // 储量 → 炼器/炼丹持续次数
    VitalEnergy_Purity,         // 纯度 → 法宝/丹药品质
    VitalEnergy_Recovery,       // 恢复 → 精元恢复速度
    VitalEnergy_Conversion,     // 转化 → 精元→灵力/生命转换效率

    // 意志分支
    Willpower_Steadfast,        // 坚定 → 心魔抵抗、突破成功率
    Willpower_Focus,            // 专注 → 修炼效率、技能释放稳定性
    Willpower_Deterrence,       // 威慑 → 压制低意志敌人
    Willpower_Sacrifice,        // 牺牲 → 濒死时爆发潜力

    // 机巧分支
    Dexterity_Precision,        // 精度 → 符箓绘制成功率、阵法布设精度
    Dexterity_Speed,            // 速度 → 绘制/炼器速度
    Dexterity_Innovation,       // 创新 → 自创符箓/阵法概率
    Dexterity_Flexibility,      // 灵巧 → 同时操控多个精细操作
}

/// <summary>
/// 基础属性类型枚举
/// </summary>
public enum BaseAttributeType
{
    DivineSense,    // 神识
    Constitution,   // 根骨
    Comprehension,  // 悟性
    Fortune,        // 福缘（无分支）
    Charm,          // 魅力
    SoulPower,      // 魂力
    VitalEnergy,    // 精元
    Willpower,      // 意志
    Dexterity,      // 机巧
    SpiritRoot,     // 灵根（无分支）
}
