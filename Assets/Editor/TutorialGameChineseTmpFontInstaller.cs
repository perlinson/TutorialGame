#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TutorialGameChineseTmpFontInstaller
{
    private const string SourceSystemFontPath = "/usr/share/fonts/truetype/droid/DroidSansFallbackFull.ttf";
    private const string ProjectFontFolder = "Assets/Fonts";
    private const string ProjectFontPath = ProjectFontFolder + "/DroidSansFallbackFull.ttf";
    private const string TmpFontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/TutorialGameCJK SDF.asset";
    private const string TmpFontAssetName = "TutorialGameCJK SDF";

    [MenuItem("Cultivation/Text/Fix Chinese TMP Font")]
    public static void EnsureInstalledAfterReload()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += EnsureInstalledAfterReload;
            return;
        }

        if (!EnsureFontFileAvailable())
        {
            EditorApplication.delayCall += EnsureInstalledAfterReload;
            return;
        }

        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(ProjectFontPath);
        if (sourceFont == null)
        {
            EditorApplication.delayCall += EnsureInstalledAfterReload;
            return;
        }

        var fontAsset = EnsureTmpFontAsset(sourceFont);
        if (fontAsset == null)
        {
            Debug.LogError("Failed to create Chinese TMP font asset from " + ProjectFontPath);
            return;
        }

        var changed = false;
        changed |= ConfigureTmpSettings(fontAsset);
        changed |= RepairOpenSceneFonts(fontAsset);

        if (changed)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Chinese TMP font repaired with asset: " + TmpFontAssetPath);
        }
    }

    private static bool EnsureFontFileAvailable()
    {
        if (File.Exists(Path.GetFullPath(ProjectFontPath)))
        {
            return true;
        }

        if (!File.Exists(SourceSystemFontPath))
        {
            Debug.LogError("Chinese system font not found: " + SourceSystemFontPath);
            return false;
        }

        EnsureFolder("Assets");
        EnsureFolder(ProjectFontFolder);
        File.Copy(SourceSystemFontPath, Path.GetFullPath(ProjectFontPath), true);
        AssetDatabase.ImportAsset(ProjectFontPath, ImportAssetOptions.ForceSynchronousImport);
        return File.Exists(Path.GetFullPath(ProjectFontPath));
    }

    private static TMP_FontAsset EnsureTmpFontAsset(Font sourceFont)
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontAssetPath);
        if (existing != null)
        {
            return existing;
        }

        var fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,
            9,
            UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
            2048,
            2048,
            AtlasPopulationMode.Dynamic,
            true);

        if (fontAsset == null)
        {
            return null;
        }

        fontAsset.name = TmpFontAssetName;
        AssetDatabase.CreateAsset(fontAsset, TmpFontAssetPath);

        if (fontAsset.atlasTextures != null)
        {
            for (var i = 0; i < fontAsset.atlasTextures.Length; i++)
            {
                var atlas = fontAsset.atlasTextures[i];
                if (atlas == null)
                {
                    continue;
                }

                atlas.name = TmpFontAssetName + " Atlas";
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(atlas)))
                {
                    AssetDatabase.AddObjectToAsset(atlas, fontAsset);
                }
            }
        }

        if (fontAsset.material != null)
        {
            fontAsset.material.name = TmpFontAssetName + " Material";
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(fontAsset.material)))
            {
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        return fontAsset;
    }

    private static bool ConfigureTmpSettings(TMP_FontAsset fontAsset)
    {
        var settings = TMP_Settings.instance;
        if (settings == null)
        {
            return false;
        }

        var serializedObject = new SerializedObject(settings);
        var defaultFontProperty = serializedObject.FindProperty("m_defaultFontAsset");
        var fallbackFontsProperty = serializedObject.FindProperty("m_fallbackFontAssets");

        var changed = false;
        if (defaultFontProperty != null && defaultFontProperty.objectReferenceValue != fontAsset)
        {
            defaultFontProperty.objectReferenceValue = fontAsset;
            changed = true;
        }

        if (fallbackFontsProperty != null)
        {
            var alreadyPresent = false;
            for (var i = 0; i < fallbackFontsProperty.arraySize; i++)
            {
                if (fallbackFontsProperty.GetArrayElementAtIndex(i).objectReferenceValue == fontAsset)
                {
                    alreadyPresent = true;
                    break;
                }
            }

            if (!alreadyPresent)
            {
                fallbackFontsProperty.InsertArrayElementAtIndex(fallbackFontsProperty.arraySize);
                fallbackFontsProperty.GetArrayElementAtIndex(fallbackFontsProperty.arraySize - 1).objectReferenceValue = fontAsset;
                changed = true;
            }
        }

        if (!changed)
        {
            return false;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
        return true;
    }

    private static bool RepairOpenSceneFonts(TMP_FontAsset fontAsset)
    {
        var sceneTexts = Object.FindObjectsOfType<TMP_Text>(true);
        var changed = false;
        for (var i = 0; i < sceneTexts.Length; i++)
        {
            var text = sceneTexts[i];
            if (!ApplyFont(text, fontAsset))
            {
                continue;
            }

            EditorUtility.SetDirty(text);
            if (text.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(text.gameObject.scene);
            }

            changed = true;
        }

        return changed;
    }

    private static bool ApplyFontToHierarchy(GameObject root, TMP_FontAsset fontAsset)
    {
        var changed = false;
        var texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (var i = 0; i < texts.Length; i++)
        {
            changed |= ApplyFont(texts[i], fontAsset);
        }

        return changed;
    }

    private static bool ApplyFont(TMP_Text text, TMP_FontAsset fontAsset)
    {
        if (text == null || fontAsset == null)
        {
            return false;
        }

        var changed = false;
        if (text.font != fontAsset)
        {
            text.font = fontAsset;
            changed = true;
        }

        if (text.fontSharedMaterial != fontAsset.material)
        {
            text.fontSharedMaterial = fontAsset.material;
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(text);
        }

        return changed;
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
