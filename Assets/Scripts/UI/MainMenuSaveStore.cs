using UnityEngine;

public static class MainMenuSaveStore
{
    public const int SaveSlotCount = 3;

    private const string VolumeKey = "main_menu.master_volume";
    private const string FullscreenKey = "main_menu.fullscreen";
    private const string CurrentSlotKey = "main_menu.current_slot";
    private const string SelectedArchetypeKey = "main_menu.selected_archetype";
    private const string SaveSlotKeyPrefix = "main_menu.save_slot_";

    public static float LoadVolume()
    {
        return PlayerPrefs.GetFloat(VolumeKey, 0.8f);
    }

    public static void SaveVolume(float value)
    {
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    public static bool LoadFullscreen()
    {
        return PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
    }

    public static void SaveFullscreen(bool fullscreen)
    {
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static int LoadSelectedArchetype()
    {
        return PlayerPrefs.GetInt(SelectedArchetypeKey, 0);
    }

    public static void SaveSelectedArchetype(int index)
    {
        PlayerPrefs.SetInt(SelectedArchetypeKey, index);
        PlayerPrefs.Save();
    }

    public static void SaveSlot(int slotIndex, MainMenuSaveData data)
    {
        var clampedIndex = Mathf.Clamp(slotIndex, 0, SaveSlotCount - 1);
        if (data != null)
        {
            data.EnsureDefaults();
        }

        PlayerPrefs.SetString(GetSaveSlotKey(clampedIndex), JsonUtility.ToJson(data));
        PlayerPrefs.SetInt(CurrentSlotKey, clampedIndex);
        PlayerPrefs.Save();
    }

    public static void DeleteSlot(int slotIndex)
    {
        var clampedIndex = Mathf.Clamp(slotIndex, 0, SaveSlotCount - 1);
        PlayerPrefs.DeleteKey(GetSaveSlotKey(clampedIndex));

        if (PlayerPrefs.GetInt(CurrentSlotKey, -1) == clampedIndex)
        {
            PlayerPrefs.DeleteKey(CurrentSlotKey);
        }

        PlayerPrefs.Save();
    }

    public static bool TryLoadSlot(int slotIndex, out MainMenuSaveData data)
    {
        data = null;
        if (slotIndex < 0 || slotIndex >= SaveSlotCount)
        {
            return false;
        }

        var json = PlayerPrefs.GetString(GetSaveSlotKey(slotIndex), string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        data = JsonUtility.FromJson<MainMenuSaveData>(json);
        if (data != null)
        {
            data.EnsureDefaults();
        }

        return data != null && !string.IsNullOrWhiteSpace(data.heroName);
    }

    public static bool TryGetCurrentSave(out int slotIndex, out MainMenuSaveData data)
    {
        slotIndex = PlayerPrefs.GetInt(CurrentSlotKey, -1);
        if (slotIndex >= 0 && slotIndex < SaveSlotCount && TryLoadSlot(slotIndex, out data))
        {
            return true;
        }

        for (var i = 0; i < SaveSlotCount; i++)
        {
            if (TryLoadSlot(i, out data))
            {
                slotIndex = i;
                return true;
            }
        }

        slotIndex = -1;
        data = null;
        return false;
    }

    public static bool HasAnySave()
    {
        for (var i = 0; i < SaveSlotCount; i++)
        {
            if (TryLoadSlot(i, out _))
            {
                return true;
            }
        }

        return false;
    }

    public static int GetPreferredLoadSlot()
    {
        if (TryGetCurrentSave(out var slotIndex, out _))
        {
            return slotIndex;
        }

        for (var i = 0; i < SaveSlotCount; i++)
        {
            if (TryLoadSlot(i, out _))
            {
                return i;
            }
        }

        return 0;
    }

    public static int GetPreferredNewGameSlot()
    {
        for (var i = 0; i < SaveSlotCount; i++)
        {
            if (!TryLoadSlot(i, out _))
            {
                return i;
            }
        }

        return 0;
    }

    public static void SaveCurrent(int slotIndex, MainMenuSaveData data)
    {
        SaveSlot(slotIndex, data);
    }

    public static bool TryLoadCurrentSave(out MainMenuSaveData data)
    {
        return TryGetCurrentSave(out _, out data);
    }

    private static string GetSaveSlotKey(int slotIndex)
    {
        return SaveSlotKeyPrefix + slotIndex;
    }
}
