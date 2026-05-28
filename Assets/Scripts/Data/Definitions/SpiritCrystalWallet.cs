using System;
using UnityEngine;

[Serializable]
public struct SpiritCrystalWallet
{
    public int low;
    public int mid;
    public int high;
    public int supreme;

    public bool IsEmpty => low <= 0 && mid <= 0 && high <= 0 && supreme <= 0;

    public int GetGradeValue(SpiritCrystalGrade grade)
    {
        switch (grade)
        {
            case SpiritCrystalGrade.Low: return low;
            case SpiritCrystalGrade.Mid: return mid;
            case SpiritCrystalGrade.High: return high;
            case SpiritCrystalGrade.Supreme: return supreme;
            default: return 0;
        }
    }

    public void Add(SpiritCrystalGrade grade, int amount)
    {
        if (amount <= 0) return;
        switch (grade)
        {
            case SpiritCrystalGrade.Low: low += amount; break;
            case SpiritCrystalGrade.Mid: mid += amount; break;
            case SpiritCrystalGrade.High: high += amount; break;
            case SpiritCrystalGrade.Supreme: supreme += amount; break;
        }
    }

    public bool CanAfford(SpiritCrystalGrade grade, int amount)
    {
        if (amount <= 0) return true;
        return GetGradeValue(grade) >= amount;
    }

    public bool Spend(SpiritCrystalGrade grade, int amount)
    {
        if (amount <= 0) return true;
        if (!CanAfford(grade, amount)) return false;

        switch (grade)
        {
            case SpiritCrystalGrade.Low: low -= amount; break;
            case SpiritCrystalGrade.Mid: mid -= amount; break;
            case SpiritCrystalGrade.High: high -= amount; break;
            case SpiritCrystalGrade.Supreme: supreme -= amount; break;
        }

        return true;
    }

    /// <summary>
    /// 自动进位：低级灵石满100自动转换为上级
    /// </summary>
    public void Normalize()
    {
        if (low >= 100) { mid += low / 100; low %= 100; }
        if (mid >= 100) { high += mid / 100; mid %= 100; }
        if (high >= 100) { supreme += high / 100; high %= 100; }
    }

    public string ToDisplayString()
    {
        Normalize();
        var parts = new System.Text.StringBuilder();
        if (supreme > 0) parts.Append(supreme).Append("极品 ");
        if (high > 0) parts.Append(high).Append("上品 ");
        if (mid > 0) parts.Append(mid).Append("中品 ");
        if (low > 0 || parts.Length == 0) parts.Append(low).Append("下品");
        return parts.ToString().Trim();
    }
}
