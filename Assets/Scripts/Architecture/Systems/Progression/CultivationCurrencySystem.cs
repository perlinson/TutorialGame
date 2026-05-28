using QFramework;

public sealed class CultivationCurrencySystem : AbstractSystem
{
    protected override void OnInit()
    {
    }

    /// <summary>
    /// 按玩家当前境界自动加对应等级灵石
    /// </summary>
    public void AddCrystals(CultivationSaveData saveData, int amount)
    {
        if (saveData == null || amount <= 0) return;
        saveData.EnsureDefaults();
        var grade = GetPlayerGrade(saveData);
        saveData.wallet.Add(grade, amount);
    }

    /// <summary>
    /// 按玩家当前境界扣除灵石
    /// </summary>
    public bool SpendCrystals(CultivationSaveData saveData, int amount)
    {
        if (saveData == null || amount <= 0) return true;
        saveData.EnsureDefaults();
        var grade = GetPlayerGrade(saveData);
        return saveData.wallet.Spend(grade, amount);
    }

    /// <summary>
    /// 检查玩家当前境界的灵石是否足够
    /// </summary>
    public bool CanAfford(CultivationSaveData saveData, int amount)
    {
        if (saveData == null || amount <= 0) return true;
        saveData.EnsureDefaults();
        var grade = GetPlayerGrade(saveData);
        return saveData.wallet.CanAfford(grade, amount);
    }

    /// <summary>
    /// 获取格式化显示字符串
    /// </summary>
    public string GetDisplayString(CultivationSaveData saveData)
    {
        if (saveData == null) return "0下品";
        saveData.EnsureDefaults();
        return saveData.wallet.ToDisplayString();
    }

    /// <summary>
    /// 境界 → 对应灵石等级
    /// </summary>
    public SpiritCrystalGrade GetPlayerGrade(CultivationSaveData saveData)
    {
        if (saveData == null) return SpiritCrystalGrade.Low;
        return RealmToGrade(saveData.realmTier);
    }

    public static SpiritCrystalGrade RealmToGrade(int realmTier)
    {
        if (realmTier >= 9) return SpiritCrystalGrade.Supreme;
        if (realmTier >= 6) return SpiritCrystalGrade.High;
        if (realmTier >= 3) return SpiritCrystalGrade.Mid;
        return SpiritCrystalGrade.Low;
    }

    public static string GradeName(SpiritCrystalGrade grade)
    {
        switch (grade)
        {
            case SpiritCrystalGrade.Low: return "下品灵石";
            case SpiritCrystalGrade.Mid: return "中品灵石";
            case SpiritCrystalGrade.High: return "上品灵石";
            case SpiritCrystalGrade.Supreme: return "极品灵石";
            default: return "灵石";
        }
    }
}
