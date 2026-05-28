using System;
using System.IO;
using UnityEngine;

public static class CultivationLocalSaveStore
{
    [Serializable]
    private sealed class LocalStoreData
    {
        public bool audioSettingsConfigured;
        public float musicVolume = 0.8f;
        public float sfxVolume = 0.8f;
        public float voiceVolume = 0.8f;
        public bool fullscreen = true;
        public int currentSlot = -1;
        public int selectedArchetype;
        public CultivationSaveData[] saveSlots = new CultivationSaveData[SaveSlotCount];
        public PersistentExpeditionRuntimeSnapshot expeditionRuntime;
    }

    public const int SaveSlotCount = 3;

    private const string SaveFolderName = "Saves";
    private const string SaveFileName = "main_menu_store.json";
    private const string BackupFileName = "main_menu_store.bak.json";

    private static LocalStoreData cachedStore;
    private static bool storeLoaded;

    public static float LoadMusicVolume()
    {
        return Mathf.Clamp(GetStore().musicVolume, 0f, 1f);
    }

    public static void SaveMusicVolume(float value)
    {
        var store = GetStore();
        store.audioSettingsConfigured = true;
        store.musicVolume = Mathf.Clamp01(value);
        PersistStore(store, true);
    }

    public static float LoadSfxVolume()
    {
        return Mathf.Clamp(GetStore().sfxVolume, 0f, 1f);
    }

    public static void SaveSfxVolume(float value)
    {
        var store = GetStore();
        store.audioSettingsConfigured = true;
        store.sfxVolume = Mathf.Clamp01(value);
        PersistStore(store, true);
    }

    public static float LoadVoiceVolume()
    {
        return Mathf.Clamp(GetStore().voiceVolume, 0f, 1f);
    }

    public static void SaveVoiceVolume(float value)
    {
        var store = GetStore();
        store.audioSettingsConfigured = true;
        store.voiceVolume = Mathf.Clamp01(value);
        PersistStore(store, true);
    }

    public static bool LoadFullscreen()
    {
        return GetStore().fullscreen;
    }

    public static void SaveFullscreen(bool fullscreen)
    {
        var store = GetStore();
        store.fullscreen = fullscreen;
        PersistStore(store, true);
    }

    public static int LoadSelectedArchetype()
    {
        return Mathf.Max(0, GetStore().selectedArchetype);
    }

    public static void SaveSelectedArchetype(int index)
    {
        var store = GetStore();
        store.selectedArchetype = Mathf.Max(0, index);
        PersistStore(store, true);
    }

    public static void SaveSlot(int slotIndex, CultivationSaveData data)
    {
        var store = GetStore();
        var clampedIndex = Mathf.Clamp(slotIndex, 0, SaveSlotCount - 1);

        if (data != null)
        {
            data.EnsureDefaults();
        }

        EnsureSlotArray(store);
        store.saveSlots[clampedIndex] = CloneSaveData(data);
        store.currentSlot = clampedIndex;
        PersistStore(store, true);
    }

    public static void DeleteSlot(int slotIndex)
    {
        var store = GetStore();
        var clampedIndex = Mathf.Clamp(slotIndex, 0, SaveSlotCount - 1);

        EnsureSlotArray(store);
        store.saveSlots[clampedIndex] = null;
        if (store.expeditionRuntime != null && store.expeditionRuntime.slotIndex == clampedIndex)
        {
            store.expeditionRuntime = null;
        }

        if (store.currentSlot == clampedIndex)
        {
            store.currentSlot = -1;
        }

        PersistStore(store, true);
    }

    public static bool TryLoadSlot(int slotIndex, out CultivationSaveData data)
    {
        data = null;
        if (slotIndex < 0 || slotIndex >= SaveSlotCount)
        {
            return false;
        }

        var store = GetStore();
        EnsureSlotArray(store);

        var source = store.saveSlots[slotIndex];
        if (source == null)
        {
            return false;
        }

        data = CloneSaveData(source);
        return data != null && !string.IsNullOrWhiteSpace(data.heroName);
    }

    public static bool TryGetCurrentSave(out int slotIndex, out CultivationSaveData data)
    {
        var store = GetStore();
        slotIndex = store.currentSlot;
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

    public static void SaveCurrent(int slotIndex, CultivationSaveData data)
    {
        SaveSlot(slotIndex, data);
    }

    public static bool TryLoadCurrentSave(out CultivationSaveData data)
    {
        return TryGetCurrentSave(out _, out data);
    }

    public static void SaveExpeditionRuntime(PersistentExpeditionRuntimeSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        snapshot.EnsureDefaults();
        if (!snapshot.IsUsable())
        {
            return;
        }

        var store = GetStore();
        store.expeditionRuntime = CloneSnapshot(snapshot);
        PersistStore(store, true);
    }

    public static bool TryLoadExpeditionRuntime(out PersistentExpeditionRuntimeSnapshot snapshot)
    {
        snapshot = CloneSnapshot(GetStore().expeditionRuntime);
        return snapshot != null && snapshot.IsUsable();
    }

    public static void ClearExpeditionRuntime()
    {
        var store = GetStore();
        if (store.expeditionRuntime == null)
        {
            return;
        }

        store.expeditionRuntime = null;
        PersistStore(store, true);
    }

    private static LocalStoreData GetStore()
    {
        if (storeLoaded)
        {
            return cachedStore;
        }

        cachedStore = LoadStore();
        storeLoaded = true;
        return cachedStore;
    }

    private static LocalStoreData LoadStore()
    {
        var path = GetSaveFilePath();
        var store = TryReadStore(path);
        if (store != null)
        {
            PersistStore(store, false);
            return store;
        }

        var backup = TryReadStore(GetBackupFilePath());
        if (backup != null)
        {
            PersistStore(backup, false);
            return backup;
        }

        store = CreateEmptyStore();
        PersistStore(store, false);
        return store;
    }

    private static LocalStoreData TryReadStore(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var store = JsonUtility.FromJson<LocalStoreData>(json);
            return NormalizeStore(store);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Failed to read main menu save store: " + path + "\n" + exception.Message);
            return null;
        }
    }

    private static LocalStoreData NormalizeStore(LocalStoreData store)
    {
        store = store ?? CreateEmptyStore();
        if (!store.audioSettingsConfigured)
        {
            store.musicVolume = 0.8f;
            store.sfxVolume = 0.8f;
            store.voiceVolume = 0.8f;
            store.audioSettingsConfigured = true;
        }

        store.musicVolume = Mathf.Clamp01(store.musicVolume);
        store.sfxVolume = Mathf.Clamp01(store.sfxVolume);
        store.voiceVolume = Mathf.Clamp01(store.voiceVolume);
        store.selectedArchetype = Mathf.Max(0, store.selectedArchetype);
        store.expeditionRuntime = NormalizeSnapshot(store.expeditionRuntime);
        EnsureSlotArray(store);

        for (var i = 0; i < store.saveSlots.Length; i++)
        {
            if (store.saveSlots[i] == null)
            {
                continue;
            }

            store.saveSlots[i].EnsureDefaults();
            if (string.IsNullOrWhiteSpace(store.saveSlots[i].heroName))
            {
                store.saveSlots[i] = null;
            }
        }

        if (store.currentSlot < 0 || store.currentSlot >= SaveSlotCount || store.saveSlots[store.currentSlot] == null)
        {
            store.currentSlot = ResolveFirstOccupiedSlot(store.saveSlots);
        }

        return store;
    }

    private static LocalStoreData CreateEmptyStore()
    {
        return new LocalStoreData
        {
            saveSlots = new CultivationSaveData[SaveSlotCount]
        };
    }

    private static void EnsureSlotArray(LocalStoreData store)
    {
        if (store.saveSlots == null || store.saveSlots.Length != SaveSlotCount)
        {
            var resized = new CultivationSaveData[SaveSlotCount];
            if (store.saveSlots != null)
            {
                Array.Copy(store.saveSlots, resized, Mathf.Min(store.saveSlots.Length, SaveSlotCount));
            }

            store.saveSlots = resized;
        }
    }

    private static int ResolveFirstOccupiedSlot(CultivationSaveData[] saveSlots)
    {
        if (saveSlots == null)
        {
            return -1;
        }

        for (var i = 0; i < saveSlots.Length; i++)
        {
            var slot = saveSlots[i];
            if (slot != null && !string.IsNullOrWhiteSpace(slot.heroName))
            {
                return i;
            }
        }

        return -1;
    }

    private static void PersistStore(LocalStoreData store, bool createBackup)
    {
        store = NormalizeStore(store);

        var folderPath = GetSaveFolderPath();
        var filePath = GetSaveFilePath();
        var backupPath = GetBackupFilePath();

        try
        {
            Directory.CreateDirectory(folderPath);
            if (createBackup && File.Exists(filePath))
            {
                File.Copy(filePath, backupPath, true);
            }

            var json = JsonUtility.ToJson(store, true);
            File.WriteAllText(filePath, json);
            cachedStore = store;
            storeLoaded = true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to persist main menu save store: " + exception.Message);
        }
    }

    private static CultivationSaveData CloneSaveData(CultivationSaveData data)
    {
        if (data == null)
        {
            return null;
        }

        data.EnsureDefaults();
        var json = JsonUtility.ToJson(data);
        var clone = JsonUtility.FromJson<CultivationSaveData>(json);
        if (clone != null)
        {
            clone.EnsureDefaults();
        }

        return clone;
    }

    private static PersistentExpeditionRuntimeSnapshot NormalizeSnapshot(PersistentExpeditionRuntimeSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return null;
        }

        snapshot.EnsureDefaults();
        return snapshot.IsUsable() ? snapshot : null;
    }

    private static PersistentExpeditionRuntimeSnapshot CloneSnapshot(PersistentExpeditionRuntimeSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return null;
        }

        snapshot.EnsureDefaults();
        var json = JsonUtility.ToJson(snapshot);
        var clone = JsonUtility.FromJson<PersistentExpeditionRuntimeSnapshot>(json);
        if (clone != null)
        {
            clone.EnsureDefaults();
        }

        return clone;
    }

    private static string GetSaveFolderPath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFolderName);
    }

    private static string GetSaveFilePath()
    {
        return Path.Combine(GetSaveFolderPath(), SaveFileName);
    }

    private static string GetBackupFilePath()
    {
        return Path.Combine(GetSaveFolderPath(), BackupFileName);
    }
}
