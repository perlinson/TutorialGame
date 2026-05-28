using System;

[Serializable]
public sealed class CultivationAttributes
{
    public CultivationLevel level;
    public int cultivationProgress;     // 当前修为值
    public int breakthroughRequired;    // 突破所需修为
    public int lifespan;                // 寿元（剩余可活年份）
    public int usedLifespan;            // 已用寿元（当前年龄）
    public int heartDemonValue;         // 心魔值（0-100）
    public int pillPoison;              // 丹毒（0-100）
}
