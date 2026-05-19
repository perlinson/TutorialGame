#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CombatPrefabGenerator
{
    private const string RootFolder = "Assets/Resources/Prefabs/Combat";
    private const string SpiritNodePrefabPath = RootFolder + "/SpiritNode.prefab";
    private const string SpiritHerbPrefabPath = RootFolder + "/SpiritHerb.prefab";
    private const string TrialRelicPrefabPath = RootFolder + "/TrialRelic.prefab";
    private const string FloatingCombatTextPrefabPath = RootFolder + "/FloatingCombatText.prefab";
    private const string SlashEffectPrefabPath = RootFolder + "/CombatSlashEffect.prefab";
    private const string ImpactEffectPrefabPath = RootFolder + "/CombatImpactEffect.prefab";

    [MenuItem("Cultivation/Combat/Generate Support Prefabs")]
    public static void GenerateSupportPrefabs()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Prefabs");
        EnsureFolder(RootFolder);

        CreatePickupPrefab<SpiritNode>("SpiritNode", SpiritNodePrefabPath, 0.34f, new Vector3(0.46f, 0.46f, 1f), 12);
        CreatePickupPrefab<SpiritHerb>("SpiritHerb", SpiritHerbPrefabPath, 0.32f, new Vector3(0.38f, 0.62f, 1f), 12);
        CreatePickupPrefab<TrialRelic>("TrialRelic", TrialRelicPrefabPath, 0.34f, new Vector3(0.54f, 0.54f, 1f), 13);
        CreateFloatingCombatTextPrefab();
        CreateEffectPrefab("CombatSlashEffect", SlashEffectPrefabPath, 37);
        CreateEffectPrefab("CombatImpactEffect", ImpactEffectPrefabPath, 36);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated combat support prefabs.");
    }

    private static void CreatePickupPrefab<T>(string name, string path, float colliderRadius, Vector3 localScale, int sortingOrder)
        where T : Component
    {
        var root = new GameObject(name, typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(T));
        try
        {
            var renderer = root.GetComponent<SpriteRenderer>();
            renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
            renderer.color = Color.white;
            renderer.sortingOrder = sortingOrder;

            var collider = root.GetComponent<CircleCollider2D>();
            collider.radius = colliderRadius;
            collider.isTrigger = true;

            root.transform.localScale = localScale;
            SavePrefab(root, path);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static void CreateFloatingCombatTextPrefab()
    {
        var root = new GameObject("FloatingCombatText", typeof(TextMesh), typeof(FloatingCombatText));
        try
        {
            var textMesh = root.GetComponent<TextMesh>();
            textMesh.text = string.Empty;
            textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.12f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            var meshRenderer = root.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = 40;

            var shadow = new GameObject("Shadow", typeof(TextMesh), typeof(MeshRenderer));
            shadow.transform.SetParent(root.transform, false);
            shadow.transform.localPosition = new Vector3(0.0216f, -0.0216f, 0.01f);

            var shadowTextMesh = shadow.GetComponent<TextMesh>();
            shadowTextMesh.text = string.Empty;
            shadowTextMesh.font = textMesh.font;
            shadowTextMesh.fontSize = textMesh.fontSize;
            shadowTextMesh.characterSize = textMesh.characterSize;
            shadowTextMesh.anchor = textMesh.anchor;
            shadowTextMesh.alignment = textMesh.alignment;
            shadowTextMesh.color = new Color(0f, 0f, 0f, 0.6f);

            var shadowRenderer = shadow.GetComponent<MeshRenderer>();
            shadowRenderer.sortingOrder = 39;

            SavePrefab(root, FloatingCombatTextPrefabPath);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static void CreateEffectPrefab(string name, string path, int sortingOrder)
    {
        var root = new GameObject(name, typeof(SpriteRenderer), typeof(TransientSpriteEffect));
        try
        {
            var renderer = root.GetComponent<SpriteRenderer>();
            renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
            renderer.color = Color.white;
            renderer.sortingOrder = sortingOrder;
            SavePrefab(root, path);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static void SavePrefab(GameObject root, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(root, path);
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
#endif
