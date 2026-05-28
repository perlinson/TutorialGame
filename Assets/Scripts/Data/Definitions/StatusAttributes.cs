using System;

[Serializable]
public sealed class StatusAttributes
{
    public int hunger;      // 饱食度（0-100）
    public int energy;      // 精力（0-100）
    public int mood;        // 心情（0-100）
    public int injury;      // 伤势（0-100）
    public int killingIntent;// 煞气（0-100）
}
