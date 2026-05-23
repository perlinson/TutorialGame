#if UNITY_EDITOR
using UnityEditor;

public static class WorldMapUiPrefabExporter
{
    [MenuItem("Cultivation/UI/Export WorldMap Prefabs")]
    public static void ExportWorldMapPrefabs()
    {
        UiPrefabGenerationUtility.GenerateWorldMapPrefabs();
    }

    public static void ExportWorldMapPrefabsBatch()
    {
        ExportWorldMapPrefabs();
        EditorApplication.Exit(0);
    }
}
#endif
