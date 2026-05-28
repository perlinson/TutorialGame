using UnityEditor;
using UnityEngine;

public static class UiPrefabGenerationUtility
{
    private const string WorldMapPrefabPath = "Assets/Resources/UI/WorldMap/WorldMapRoot.prefab";
    private const string GameHubPrefabPath = "Assets/Resources/UI/Game/GameHubPanel.prefab";
    private const string WorldMapRegionPrefabPath = "Assets/Resources/UI/WorldMap/WorldMapRegionPanel.prefab";
    private const string WorldMapSettlementPrefabPath = "Assets/Resources/UI/WorldMap/WorldMapSettlementPanel.prefab";
    private const string PlayerCompendiumPrefabPath = "Assets/Resources/UI/Game/PlayerCompendiumPanel.prefab";
    private const string WorldMapInventoryPrefabPath = "Assets/Resources/UI/WorldMap/WorldMapInventoryPanel.prefab";
    private const string WorldMapWorkshopPrefabPath = "Assets/Resources/UI/WorldMap/WorldMapWorkshopPanel.prefab";
    private const string WorldMapSectResidencePrefabPath = "Assets/Resources/UI/WorldMap/WorldMapSectResidencePanel.prefab";
    private const string WorldMapNpcDialoguePrefabPath = "Assets/Resources/UI/WorldMap/WorldMapNpcDialoguePanel.prefab";
    private const string ExpeditionPrefabPath = "Assets/Resources/UI/Game/ExpeditionRoot.prefab";

    [MenuItem("Cultivation/UI/Generate All Prefabs")]
    public static void GenerateAllPrefabs()
    {
        MainMenuPrefabGenerator.RegeneratePrefabs();
        GenerateOverlayPrefabs();
        GenerateWorldMapPrefabs();
        GenerateExpeditionPrefab();
        CombatPrefabGenerator.GenerateSupportPrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated all UI prefabs.");
    }

    [MenuItem("Cultivation/UI/Generate Overlay Prefabs")]
    public static void GenerateOverlayPrefabs()
    {
        OverlayPrefabGenerator.RegeneratePrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated overlay UI prefabs.");
    }

    [MenuItem("Cultivation/UI/Generate WorldMap Prefabs")]
    public static void GenerateWorldMapPrefabs()
    {
        EnsureFolder("Assets/Resources/UI/WorldMap");
        EnsureFolder("Assets/Resources/UI/Game");

        WorldMapController exportController = null;
        GameHubPanel hudPanel = null;
        WorldMapRegionPanel regionPanel = null;
        WorldMapSettlementPanel settlementPanel = null;
        PlayerCompendiumPanel compendiumPanel = null;
        WorldMapInventoryPanel inventoryPanel = null;
        WorldMapWorkshopPanel workshopPanel = null;
        WorldMapSectResidencePanel sectResidencePanel = null;
        WorldMapNpcDialoguePanel npcDialoguePanel = null;
        try
        {
            exportController = WorldMapPrefabExportBuilder.BuildPrefabExportController();
            if (exportController == null)
            {
                Debug.LogError("Failed to build world map prefab export instance.");
                return;
            }

            exportController.name = "WorldMapRoot";
            PrefabUtility.SaveAsPrefabAsset(exportController.gameObject, WorldMapPrefabPath);
            Debug.Log("Generated UI prefab: " + WorldMapPrefabPath);

            hudPanel = WorldMapPrefabExportBuilder.BuildHudPanelExport();
            hudPanel.name = "GameHubPanel";
            PrefabUtility.SaveAsPrefabAsset(hudPanel.gameObject, GameHubPrefabPath);
            Debug.Log("Generated UI prefab: " + GameHubPrefabPath);

            regionPanel = WorldMapPrefabExportBuilder.BuildRegionPanelExport();
            regionPanel.name = "WorldMapRegionPanel";
            PrefabUtility.SaveAsPrefabAsset(regionPanel.gameObject, WorldMapRegionPrefabPath);
            Debug.Log("Generated UI prefab: " + WorldMapRegionPrefabPath);

            settlementPanel = WorldMapPrefabExportBuilder.BuildSettlementPanelExport();
            settlementPanel.name = "WorldMapSettlementPanel";
            PrefabUtility.SaveAsPrefabAsset(settlementPanel.gameObject, WorldMapSettlementPrefabPath);
            Debug.Log("Generated UI prefab: " + WorldMapSettlementPrefabPath);

            compendiumPanel = WorldMapPrefabExportBuilder.BuildCompendiumPanelExport();
            compendiumPanel.name = "PlayerCompendiumPanel";
            PrefabUtility.SaveAsPrefabAsset(compendiumPanel.gameObject, PlayerCompendiumPrefabPath);
            Debug.Log("Generated UI prefab: " + PlayerCompendiumPrefabPath);

            inventoryPanel = WorldMapPrefabExportBuilder.BuildInventoryPanelExport();
            inventoryPanel.name = "WorldMapInventoryPanel";
            PrefabUtility.SaveAsPrefabAsset(inventoryPanel.gameObject, WorldMapInventoryPrefabPath);
            Debug.Log("Generated UI prefab: " + WorldMapInventoryPrefabPath);

            workshopPanel = WorldMapPrefabExportBuilder.BuildWorkshopPanelExport();
            workshopPanel.name = "WorldMapWorkshopPanel";
            PrefabUtility.SaveAsPrefabAsset(workshopPanel.gameObject, WorldMapWorkshopPrefabPath);
            Debug.Log("Generated UI prefab: " + WorldMapWorkshopPrefabPath);

            sectResidencePanel = WorldMapPrefabExportBuilder.BuildSectResidencePanelExport();
            sectResidencePanel.name = "WorldMapSectResidencePanel";
            PrefabUtility.SaveAsPrefabAsset(sectResidencePanel.gameObject, WorldMapSectResidencePrefabPath);
            Debug.Log("Generated UI prefab: " + WorldMapSectResidencePrefabPath);

            npcDialoguePanel = WorldMapPrefabExportBuilder.BuildNpcDialoguePanelExport();
            npcDialoguePanel.name = "WorldMapNpcDialoguePanel";
            PrefabUtility.SaveAsPrefabAsset(npcDialoguePanel.gameObject, WorldMapNpcDialoguePrefabPath);
            Debug.Log("Generated UI prefab: " + WorldMapNpcDialoguePrefabPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        finally
        {
            if (exportController != null)
            {
                Object.DestroyImmediate(exportController.gameObject);
            }

            if (inventoryPanel != null)
            {
                Object.DestroyImmediate(inventoryPanel.gameObject);
            }

            if (hudPanel != null)
            {
                Object.DestroyImmediate(hudPanel.gameObject);
            }

            if (compendiumPanel != null)
            {
                Object.DestroyImmediate(compendiumPanel.gameObject);
            }

            if (regionPanel != null)
            {
                Object.DestroyImmediate(regionPanel.gameObject);
            }

            if (settlementPanel != null)
            {
                Object.DestroyImmediate(settlementPanel.gameObject);
            }

            if (workshopPanel != null)
            {
                Object.DestroyImmediate(workshopPanel.gameObject);
            }

            if (sectResidencePanel != null)
            {
                Object.DestroyImmediate(sectResidencePanel.gameObject);
            }

            if (npcDialoguePanel != null)
            {
                Object.DestroyImmediate(npcDialoguePanel.gameObject);
            }
        }
    }

    [MenuItem("Cultivation/UI/Generate Player Compendium Prefab")]
    public static void GeneratePlayerCompendiumPrefab()
    {
        EnsureFolder("Assets/Resources/UI/Game");

        PlayerCompendiumPanel compendiumPanel = null;
        try
        {
            compendiumPanel = WorldMapPrefabExportBuilder.BuildCompendiumPanelExport();
            if (compendiumPanel == null)
            {
                Debug.LogError("Failed to build player compendium prefab export instance.");
                return;
            }

            compendiumPanel.name = "PlayerCompendiumPanel";
            PrefabUtility.SaveAsPrefabAsset(compendiumPanel.gameObject, PlayerCompendiumPrefabPath);
            Debug.Log("Generated UI prefab: " + PlayerCompendiumPrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        finally
        {
            if (compendiumPanel != null)
            {
                Object.DestroyImmediate(compendiumPanel.gameObject);
            }
        }
    }

    [MenuItem("Cultivation/UI/Generate Expedition Prefab")]
    public static void GenerateExpeditionPrefab()
    {
        EnsureFolder("Assets/Resources/UI/Game");

        ExpeditionView exportView = null;
        try
        {
            var region = WorldRegionLibrary.GetStartingRegion();
            if (region == null)
            {
                Debug.LogError("Failed to resolve starting region for expedition prefab export.");
                return;
            }

            var roomCount = Mathf.Clamp(4 + region.DangerRank, 5, 8);
            exportView = GameSceneBootstrap.BuildPrefabExportView(region, roomCount);
            if (exportView == null)
            {
                Debug.LogError("Failed to build runtime expedition view for prefab export.");
                return;
            }

            exportView.name = "ExpeditionRoot";
            PrefabUtility.SaveAsPrefabAsset(exportView.gameObject, ExpeditionPrefabPath);
            Debug.Log("Generated UI prefab: " + ExpeditionPrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        finally
        {
            if (exportView != null)
            {
                Object.DestroyImmediate(exportView.gameObject);
            }
        }
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        var segments = folderPath.Split('/');
        var current = segments[0];
        for (var i = 1; i < segments.Length; i++)
        {
            var next = current + "/" + segments[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, segments[i]);
            }

            current = next;
        }
    }
}
