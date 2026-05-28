using PixelCrushers.DialogueSystem;
using System.Reflection;
using UnityEngine;

/// <summary>
/// DSU Lua 函数桥接 — 将 C# 游戏状态读写函数注册到 DSU 的 Lua 环境，
/// 供 DSU 对话节点的 Condition / Script 框使用。
///
/// DSU 对话节点中直接调用示例：
///   Condition: GameState_RealmTier() >= 2
///   Script:    GameState_SetFlag("met_elder")
/// </summary>
public sealed class LuaBridge
{
    private CultivationSaveData saveData;
    private CultivationStorySystem storySystem;

    /// <summary>
    /// 对话开始前注入当前存档引用（后续考虑 Clone 隔离）。
    /// </summary>
    public void PushSaveData(CultivationSaveData data)
    {
        saveData = data;
    }

    /// <summary>
    /// 对话结束后回写（当前直接读写的 live reference，无需 Pull）。
    /// 预留 Clone/WriteBack 模式入口。
    /// </summary>
    public void PullSaveData(CultivationSaveData data)
    {
        saveData = null;
    }

    /// <summary>
    /// 向 DSU Lua 注册所有读写函数。在 CultivationDialogueSystem.OnInit 中调用。
    /// </summary>
    public void RegisterAll(CultivationDialogueSystem system)
    {
        storySystem = system.GetStorySystem();

        // ─── 读取类（DSU Condition 框） ───
        Register("GameState_RealmTier", nameof(GetRealmTier));
        Register("GameState_SpiritCrystals", nameof(GetSpiritCrystals));
        Register("GameState_Qi", nameof(GetQi));
        Register("GameState_WorldDay", nameof(GetWorldDay));
        Register("GameState_HasFlag", nameof(HasStoryFlag));
        Register("GameState_HasItem", nameof(HasItem));
        Register("GameState_ItemCount", nameof(GetItemCount));
        Register("GameState_BagUsedSlots", nameof(GetBagUsedSlots));
        Register("GameState_BagCapacity", nameof(GetBagCapacity));
        Register("GameState_IsSectDisciple", nameof(IsSectDisciple));
        Register("GameState_SectId", nameof(GetSectId));

        // ─── 写入类（DSU Script 框） ───
        Register("GameState_AddQi", nameof(AddQi));
        Register("GameState_AddCrystals", nameof(AddCrystals));
        Register("GameState_GrantItem", nameof(GrantItem));
        Register("GameState_SetFlag", nameof(SetStoryFlag));
        Register("GameState_RecordStory", nameof(RecordStory));
        Register("Story_RecordSignal", nameof(RecordStory));
    }

    private void Register(string luaName, string methodName)
    {
        var method = typeof(LuaBridge).GetMethod(methodName,
            BindingFlags.Public | BindingFlags.Instance);
        if (method == null)
        {
            Debug.LogWarning($"[LuaBridge] Method not found: {methodName}");
            return;
        }

        Lua.RegisterFunction(luaName, this, method);
    }

    // ═══════════════════════════════════════════
    //  读取方法
    // ═══════════════════════════════════════════

    public int GetRealmTier()
    {
        return saveData != null ? saveData.realmTier : 0;
    }

    public int GetSpiritCrystals()
    {
        if (saveData == null)
        {
            return 0;
        }

        saveData.EnsureDefaults();
        return saveData.wallet.GetGradeValue(CultivationCurrencySystem.RealmToGrade(saveData.realmTier));
    }

    public int GetQi()
    {
        return saveData != null ? saveData.qi : 0;
    }

    public int GetWorldDay()
    {
        return saveData != null ? saveData.worldDay : 0;
    }

    public bool HasStoryFlag(string flagId)
    {
        if (saveData == null || saveData.storyFlags == null || string.IsNullOrEmpty(flagId))
            return false;
        for (var i = 0; i < saveData.storyFlags.Length; i++)
        {
            if (saveData.storyFlags[i] == flagId)
                return true;
        }
        return false;
    }

    public bool HasItem(string itemId)
    {
        return saveData != null && saveData.GetItemCount(itemId) > 0;
    }

    public int GetItemCount(string itemId)
    {
        return saveData != null ? saveData.GetItemCount(itemId) : 0;
    }

    public int GetBagUsedSlots()
    {
        return saveData != null ? saveData.GetUsedBagSlots() : 0;
    }

    public int GetBagCapacity()
    {
        return saveData != null ? saveData.bagCapacity : 0;
    }

    public bool IsSectDisciple()
    {
        return saveData != null && saveData.isSectDisciple;
    }

    public string GetSectId()
    {
        return saveData != null ? (saveData.sectId ?? string.Empty) : string.Empty;
    }

    // ═══════════════════════════════════════════
    //  写入方法
    // ═══════════════════════════════════════════

    public void AddQi(double amount)
    {
        if (saveData == null) return;

        var intAmount = ToPositiveInt(amount);
        if (intAmount <= 0) return;
        saveData.qi += intAmount;
    }

    public void AddCrystals(double amount)
    {
        if (saveData == null) return;

        var intAmount = ToPositiveInt(amount);
        if (intAmount <= 0) return;
        saveData.EnsureDefaults();
        saveData.wallet.Add(CultivationCurrencySystem.RealmToGrade(saveData.realmTier), intAmount);
    }

    public void GrantItem(string itemId, double quantity)
    {
        if (saveData == null || string.IsNullOrEmpty(itemId)) return;

        var intQuantity = ToPositiveInt(quantity);
        if (intQuantity <= 0) return;
        saveData.TryAddItem(itemId, intQuantity);
    }

    public void SetStoryFlag(string flagId)
    {
        if (saveData == null || string.IsNullOrEmpty(flagId)) return;
        saveData.EnsureDefaults();
        var merged = new System.Collections.Generic.List<string>(saveData.storyFlags ?? new string[0]);
        if (!merged.Contains(flagId))
            merged.Add(flagId);
        saveData.storyFlags = merged.ToArray();
    }

    public void RecordStory(string storyId, string nodeId, string resultText)
    {
        if (saveData == null || storySystem == null) return;
        storySystem.RecordSignal(saveData, new StorySignal
        {
            StoryId = storyId ?? string.Empty,
            NodeId = nodeId ?? string.Empty,
            Title = "DSU对话",
            ResultText = resultText ?? string.Empty
        });
    }

    private static int ToPositiveInt(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 0;
        }

        return Mathf.Max(0, Mathf.RoundToInt((float)value));
    }
}
