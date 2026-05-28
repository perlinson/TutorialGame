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
        if (!CultivationLocalSaveStore.TryGetCurrentSave(out var slotIndex, out var saveData))
        {
            ClearModels();
            return null;
        }

        return BuildSnapshotAndSync(slotIndex, saveData);
    }

    public void SaveArchive(int slotIndex, CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        PrepareWorldState(saveData);
        CultivationLocalSaveStore.SaveSlot(slotIndex, saveData);
        SyncModels(slotIndex, saveData);
    }

    public void DeleteArchive(int slotIndex)
    {
        CultivationLocalSaveStore.DeleteSlot(slotIndex);

        if (CultivationLocalSaveStore.TryGetCurrentSave(out var currentSlotIndex, out var currentSave))
        {
            SyncModels(currentSlotIndex, currentSave);
        }
        else
        {
            ClearModels();
        }
    }

    public string ResolveTaskBoard(int slotIndex, CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            taskBoardModel.Apply(null, string.Empty);
            return string.Empty;
        }

        PrepareWorldState(saveData);
        var message = this.GetSystem<CultivationTaskSystem>().ResolveTaskBoard(saveData);
        CultivationLocalSaveStore.SaveCurrent(slotIndex, saveData);
        SyncModels(slotIndex, saveData, message);
        return message;
    }

    public TaskContextSnapshot GetActiveTaskContext(CultivationSaveData saveData)
    {
        return this.GetSystem<CultivationTaskSystem>().GetActiveTaskContext(saveData);
    }

    public string ClaimActiveTask(int slotIndex, CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return "当前没有可领取的委托奖励。";
        }

        PrepareWorldState(saveData);
        var message = this.GetSystem<CultivationTaskSystem>().ClaimActiveTask(saveData);
        CultivationLocalSaveStore.SaveCurrent(slotIndex, saveData);
        SyncModels(slotIndex, saveData, message);
        return message;
    }

    public TaskProgressResult RecordTaskProgress(int slotIndex, CultivationSaveData saveData, TaskProgressSignal signal)
    {
        if (saveData == null)
        {
            return new TaskProgressResult();
        }

        PrepareWorldState(saveData);
        var result = this.GetSystem<CultivationTaskSystem>().RecordProgress(saveData, signal);
        CultivationLocalSaveStore.SaveCurrent(slotIndex, saveData);
        SyncModels(slotIndex, saveData);
        return result;
    }

    public void SyncArchiveState(int slotIndex, CultivationSaveData saveData)
    {
        SyncModels(slotIndex, saveData);
    }

    private CultivationArchiveSnapshot BuildSnapshotAndSync(int slotIndex, CultivationSaveData saveData)
    {
        var syncedCopy = CloneSaveData(saveData);
        SyncModels(slotIndex, syncedCopy);
        return new CultivationArchiveSnapshot(slotIndex, CloneSaveData(syncedCopy));
    }

    private void SyncModels(int slotIndex, CultivationSaveData saveData, string boardMessage = null)
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

    private CultivationSaveData CloneSaveData(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return null;
        }

        PrepareWorldState(saveData);
        var json = JsonUtility.ToJson(saveData);
        var clone = JsonUtility.FromJson<CultivationSaveData>(json);
        if (clone != null)
        {
            clone.EnsureDefaults();
        }

        return clone;
    }

    private void PrepareWorldState(CultivationSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.EnsureDefaults();
        var worldGenerationSystem = this.GetSystem<CultivationWorldGenerationSystem>();
        worldGenerationSystem?.EnsureWorldGenerated(saveData);
        var incidentSystem = this.GetSystem<CultivationWorldIncidentSystem>();
        incidentSystem?.EnsureIncidents(saveData);
    }
}
