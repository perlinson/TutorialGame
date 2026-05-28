using System;
using System.Collections.Generic;
using QFramework;

/// <summary>
/// M3 SkillModel：玩家已学功法/招式 + 装配槽位 + 当前冷却。
/// - 已学技能集合按 ID 存（数据本体走 SkillDatabaseAsset）。
/// - 装配槽位是有序 list（slot index 决定快捷键 1-N）。
/// - 冷却以"剩余回合数"维护，0 表示就绪；战斗结束后由 <see cref="CultivationSkillCastSystem"/> 重置。
/// 持久化（已学/装配）后续接入 <see cref="CultivationSaveData"/>，本轮先以运行时状态承载。
/// </summary>
public sealed class CultivationSkillModel : AbstractModel
{
    private readonly HashSet<string> learnedSkillIds = new HashSet<string>(StringComparer.Ordinal);
    private readonly List<EquippedSkillSlot> equipped = new List<EquippedSkillSlot>();

    public readonly EasyEvent SkillsChanged = new EasyEvent();

    protected override void OnInit()
    {
    }

    public IReadOnlyCollection<string> LearnedSkillIds => learnedSkillIds;
    public IReadOnlyList<EquippedSkillSlot> EquippedSlots => equipped;

    public bool HasLearned(string skillId)
    {
        return !string.IsNullOrWhiteSpace(skillId) && learnedSkillIds.Contains(skillId);
    }

    public void Learn(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
        {
            return;
        }

        if (learnedSkillIds.Add(skillId))
        {
            SkillsChanged.Trigger();
        }
    }

    public void Forget(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
        {
            return;
        }

        var changed = learnedSkillIds.Remove(skillId);
        for (var i = equipped.Count - 1; i >= 0; i--)
        {
            if (equipped[i].SkillId == skillId)
            {
                equipped.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
        {
            SkillsChanged.Trigger();
        }
    }

    public bool Equip(int slotIndex, string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId) || !learnedSkillIds.Contains(skillId))
        {
            return false;
        }

        while (equipped.Count <= slotIndex)
        {
            equipped.Add(new EquippedSkillSlot(string.Empty, 0));
        }

        equipped[slotIndex] = new EquippedSkillSlot(skillId, 0);
        SkillsChanged.Trigger();
        return true;
    }

    public void Unequip(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equipped.Count)
        {
            return;
        }

        equipped[slotIndex] = new EquippedSkillSlot(string.Empty, 0);
        SkillsChanged.Trigger();
    }

    public int GetCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equipped.Count)
        {
            return 0;
        }

        return equipped[slotIndex].RemainingCooldown;
    }

    public void SetCooldown(int slotIndex, int remainingTurns)
    {
        if (slotIndex < 0 || slotIndex >= equipped.Count)
        {
            return;
        }

        equipped[slotIndex] = new EquippedSkillSlot(equipped[slotIndex].SkillId, remainingTurns < 0 ? 0 : remainingTurns);
        SkillsChanged.Trigger();
    }

    public void TickCooldowns(int turns = 1)
    {
        if (turns <= 0)
        {
            return;
        }

        var changed = false;
        for (var i = 0; i < equipped.Count; i++)
        {
            if (equipped[i].RemainingCooldown > 0)
            {
                var next = equipped[i].RemainingCooldown - turns;
                equipped[i] = new EquippedSkillSlot(equipped[i].SkillId, next < 0 ? 0 : next);
                changed = true;
            }
        }

        if (changed)
        {
            SkillsChanged.Trigger();
        }
    }

    public void ResetAllCooldowns()
    {
        var changed = false;
        for (var i = 0; i < equipped.Count; i++)
        {
            if (equipped[i].RemainingCooldown != 0)
            {
                equipped[i] = new EquippedSkillSlot(equipped[i].SkillId, 0);
                changed = true;
            }
        }

        if (changed)
        {
            SkillsChanged.Trigger();
        }
    }

    public void Clear()
    {
        learnedSkillIds.Clear();
        equipped.Clear();
        SkillsChanged.Trigger();
    }
}

public struct EquippedSkillSlot
{
    public string SkillId;
    public int RemainingCooldown;

    public EquippedSkillSlot(string skillId, int remainingCooldown)
    {
        SkillId = skillId ?? string.Empty;
        RemainingCooldown = remainingCooldown < 0 ? 0 : remainingCooldown;
    }

    public bool IsEmpty => string.IsNullOrEmpty(SkillId);
    public bool IsReady => !IsEmpty && RemainingCooldown <= 0;
}
