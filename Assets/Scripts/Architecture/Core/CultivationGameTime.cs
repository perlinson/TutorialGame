using UnityEngine;

public static class CultivationGameTime
{
    private static readonly string[] TimeLabels =
    {
        "子时",
        "丑时",
        "寅时",
        "卯时",
        "辰时",
        "巳时",
        "午时",
        "未时",
        "申时",
        "酉时",
        "戌时",
        "亥时"
    };

    public static void EnsureDefaults(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        if (saveData.worldDay <= 0)
        {
            saveData.worldDay = 1;
        }

        if (saveData.worldTimeIndex < 0 || saveData.worldTimeIndex >= TimeLabels.Length)
        {
            saveData.worldTimeIndex = 4;
        }
    }

    public static void Advance(MainMenuSaveData saveData, int segments)
    {
        if (saveData == null || segments <= 0)
        {
            return;
        }

        EnsureDefaults(saveData);
        var total = saveData.worldTimeIndex + segments;
        saveData.worldDay += total / TimeLabels.Length;
        saveData.worldTimeIndex = total % TimeLabels.Length;
    }

    public static string Format(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return "太初历 · 未定时";
        }

        EnsureDefaults(saveData);
        return "太初历 第" + saveData.worldDay + "日 · " + GetTimeLabel(saveData.worldTimeIndex);
    }

    public static string GetTimeLabel(int timeIndex)
    {
        var clamped = Mathf.Clamp(timeIndex, 0, TimeLabels.Length - 1);
        return TimeLabels[clamped];
    }
}
