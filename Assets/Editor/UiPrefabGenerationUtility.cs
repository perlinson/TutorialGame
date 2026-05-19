using UnityEditor;
using UnityEngine;

public static class UiPrefabGenerationUtility
{
    private const string WorldMapPrefabPath = "Assets/Resources/UI/WorldMap/WorldMapRoot.prefab";
    private const string ExpeditionPrefabPath = "Assets/Resources/UI/Game/ExpeditionRoot.prefab";

    [MenuItem("Cultivation/UI/Generate All Prefabs")]
    public static void GenerateAllPrefabs()
    {
        MainMenuPrefabGenerator.RegeneratePrefabs();
        GenerateWorldMapPrefab();
        GenerateExpeditionPrefab();
        CombatPrefabGenerator.GenerateSupportPrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated all UI prefabs.");
    }

    [MenuItem("Cultivation/UI/Generate WorldMap Prefab")]
    public static void GenerateWorldMapPrefab()
    {
        EnsureFolder("Assets/Resources/UI/WorldMap");

        WorldMapController exportController = null;
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
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        finally
        {
            if (exportController != null)
            {
                Object.DestroyImmediate(exportController.gameObject);
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
