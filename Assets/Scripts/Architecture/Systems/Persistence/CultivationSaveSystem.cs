using UnityEngine;
using QFramework;

public sealed class CultivationSaveSystem : AbstractSystem
{
    private CultivationArchiveModel archiveModel;
    private CultivationInventoryModel inventoryModel;
    private CultivationPlayerModel playerModel;
    private CultivationRealmModel realmModel;
    private CultivationAttributeModel attributeModel;
    private CultivationGameModel gameModel;
    private CultivationTaskBoardModel taskBoardModel;
    private CultivationWorldMapModel worldMapModel;

    protected override void OnInit()
    {
        archiveModel = this.GetModel<CultivationArchiveModel>();
        inventoryModel = this.GetModel<CultivationInventoryModel>();
        playerModel = this.GetModel<CultivationPlayerModel>();
        realmModel = this.GetModel<CultivationRealmModel>();
        attributeModel = this.GetModel<CultivationAttributeModel>();
        gameModel = this.GetModel<CultivationGameModel>();
        taskBoardModel = this.GetModel<CultivationTaskBoardModel>();
        worldMapModel = this.GetModel<CultivationWorldMapModel>();
    }

    public CultivationArchiveSnapshot BootstrapCurrentArchive()
    {
        if (!MainMenuSaveStore.TryGetCurrentSave(out var slotIndex, out var saveData))
        {
            ClearModels();
            return null;
        }

        return BuildSnapshotAndSync(slotIndex, saveData);
    }

    public void SaveArchive(int slotIndex, MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.EnsureDefaults();
        MainMenuSaveStore.SaveSlot(slotIndex, saveData);
        SyncModels(slotIndex, saveData);
    }

    public void DeleteArchive(int slotIndex)
    {
        MainMenuSaveStore.DeleteSlot(slotIndex);

        if (MainMenuSaveStore.TryGetCurrentSave(out var currentSlotIndex, out var currentSave))
        {
            SyncModels(currentSlotIndex, currentSave);
        }
        else
        {
            ClearModels();
        }
    }

    public string ResolveTaskBoard(int slotIndex, MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            taskBoardModel.Apply(null, string.Empty);
            return string.Empty;
        }

        saveData.EnsureDefaults();
        var message = this.GetSystem<CultivationTaskSystem>().ResolveTaskBoard(saveData);
        MainMenuSaveStore.SaveCurrent(slotIndex, saveData);
        SyncModels(slotIndex, saveData, message);
        return message;
    }

    public TaskContextSnapshot GetActiveTaskContext(MainMenuSaveData saveData)
    {
        return this.GetSystem<CultivationTaskSystem>().GetActiveTaskContext(saveData);
    }

    public string ClaimActiveTask(int slotIndex, MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return "当前没有可领取的委托奖励。";
        }

        saveData.EnsureDefaults();
        var message = this.GetSystem<CultivationTaskSystem>().ClaimActiveTask(saveData);
        MainMenuSaveStore.SaveCurrent(slotIndex, saveData);
        SyncModels(slotIndex, saveData, message);
        return message;
    }

    public TaskProgressResult RecordTaskProgress(int slotIndex, MainMenuSaveData saveData, TaskProgressSignal signal)
    {
        if (saveData == null)
        {
            return new TaskProgressResult();
        }

        saveData.EnsureDefaults();
        var result = this.GetSystem<CultivationTaskSystem>().RecordProgress(saveData, signal);
        MainMenuSaveStore.SaveCurrent(slotIndex, saveData);
        SyncModels(slotIndex, saveData);
        return result;
    }

    public void SyncArchiveState(int slotIndex, MainMenuSaveData saveData)
    {
        SyncModels(slotIndex, saveData);
    }

    private CultivationArchiveSnapshot BuildSnapshotAndSync(int slotIndex, MainMenuSaveData saveData)
    {
        var syncedCopy = CloneSaveData(saveData);
        SyncModels(slotIndex, syncedCopy);
        return new CultivationArchiveSnapshot(slotIndex, CloneSaveData(syncedCopy));
    }

    private void SyncModels(int slotIndex, MainMenuSaveData saveData, string boardMessage = null)
    {
        var syncedCopy = CloneSaveData(saveData);
        archiveModel.Apply(slotIndex, syncedCopy);
        inventoryModel.Apply(syncedCopy);
        playerModel.Apply(syncedCopy);
        realmModel.Apply(syncedCopy);
        attributeModel.Apply(syncedCopy);
        gameModel.Apply(syncedCopy);
        taskBoardModel.Apply(syncedCopy, boardMessage);
        worldMapModel.Apply(syncedCopy);
        this.SendEvent(new CultivationArchiveChangedEvent(slotIndex, CloneSaveData(syncedCopy)));
    }

    private void ClearModels()
    {
        archiveModel.Clear();
        inventoryModel.Apply(null);
        playerModel.Clear();
        realmModel.Clear();
        attributeModel.Clear();
        gameModel.Clear();
        taskBoardModel.Apply(null, string.Empty);
        worldMapModel.Apply(null);
    }

    private static MainMenuSaveData CloneSaveData(MainMenuSaveData saveData)
    {
        if (saveData == null)
        {
            return null;
        }

        saveData.EnsureDefaults();
        var json = JsonUtility.ToJson(saveData);
        var clone = JsonUtility.FromJson<MainMenuSaveData>(json);
        if (clone != null)
        {
            clone.EnsureDefaults();
        }

        return clone;
    }
}
