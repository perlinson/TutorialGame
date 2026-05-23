using System.IO;
using UnityEditor;
using UnityEngine;

public static class GeneratedUiArtBatch
{
    private const string ItemOutputDirectory = "Assets/Resources/Generated/Items";
    private const string SkillOutputDirectory = "Assets/Resources/Generated/Skills";
    private const string InventoryDatabasePath = "Assets/Resources/Data/InventoryDatabase.asset";
    private const string HeroDatabasePath = "Assets/Resources/Data/HeroArchetypeDatabase.asset";

    private enum ItemMotif
    {
        Crystal,
        Token,
        Herb,
        Fruit,
        Ore,
        Bone,
        Scroll,
        BoneCore,
        Incense
    }

    private enum SkillMotif
    {
        Slash,
        Burst,
        Guard,
        Heal,
        Mist,
        Poison,
        Bind,
        Counter
    }

    [MenuItem("Cultivation/Generated Art/Generate UI Icon Batch")]
    public static void GenerateAll()
    {
        EnsureDirectory(ItemOutputDirectory);
        EnsureDirectory(SkillOutputDirectory);

        GenerateItemIcons();
        GenerateSkillIcons();

        AssetDatabase.Refresh();
        ConfigureDirectoryImporters(ItemOutputDirectory, 100f);
        ConfigureDirectoryImporters(SkillOutputDirectory, 100f);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void GenerateItemIcons()
    {
        var database = AssetDatabase.LoadAssetAtPath<InventoryDatabaseAsset>(InventoryDatabasePath);
        if (database == null || database.items == null)
        {
            return;
        }

        for (var i = 0; i < database.items.Length; i++)
        {
            var item = database.items[i];
            if (item == null || string.IsNullOrWhiteSpace(item.id))
            {
                continue;
            }

            var primary = ResolveItemPrimary(item.category, item.rarity);
            var accent = ResolveItemAccent(item.id, primary);
            GenerateItemIcon(item.id, primary, accent, ResolveItemMotif(item.id, item.category));
        }
    }

    private static void GenerateSkillIcons()
    {
        var database = AssetDatabase.LoadAssetAtPath<HeroArchetypeDatabaseAsset>(HeroDatabasePath);
        if (database == null || database.archetypes == null)
        {
            return;
        }

        for (var archetypeIndex = 0; archetypeIndex < database.archetypes.Length; archetypeIndex++)
        {
            var archetype = database.archetypes[archetypeIndex];
            if (archetype == null || archetype.skills == null)
            {
                continue;
            }

            var primary = ResolveSkillPrimary(archetype.id);
            var accent = ResolveSkillAccent(archetype.id);
            for (var skillIndex = 0; skillIndex < archetype.skills.Length; skillIndex++)
            {
                var skill = archetype.skills[skillIndex];
                if (skill == null || string.IsNullOrWhiteSpace(skill.id))
                {
                    continue;
                }

                GenerateSkillIcon(skill.id, primary, accent, ResolveSkillMotif(skill.id));
            }
        }
    }

    private static void GenerateItemIcon(string key, Color primary, Color accent, ItemMotif motif)
    {
        const int size = 256;
        var texture = NewTexture(size, size);
        var background = Color.Lerp(primary, Color.black, 0.8f);
        var panel = Color.Lerp(primary, Color.white, 0.14f);
        var motifColor = Color.Lerp(primary, Color.white, 0.38f);

        FillVerticalGradient(texture, Color.Lerp(background, Color.black, 0.18f), Color.Lerp(background, primary, 0.12f));
        FillRoundedRect(texture, 16, 16, size - 32, size - 32, 28, panel);
        FillRoundedRect(texture, 30, 30, size - 60, size - 60, 22, Color.Lerp(background, primary, 0.18f));
        FillDiamond(texture, size / 2, 200, 18, new Color(accent.r, accent.g, accent.b, 0.38f));
        FillRing(texture, size / 2, 132, 82, 8, new Color(accent.r, accent.g, accent.b, 0.18f));

        switch (motif)
        {
            case ItemMotif.Token:
                FillRing(texture, 128, 128, 48, 16, motifColor);
                FillDiamond(texture, 128, 128, 18, accent);
                FillRect(texture, 122, 70, 12, 24, accent);
                break;
            case ItemMotif.Herb:
                FillDiamond(texture, 112, 132, 28, motifColor);
                FillDiamond(texture, 144, 114, 28, Color.Lerp(motifColor, accent, 0.24f));
                FillRect(texture, 124, 76, 8, 76, accent);
                break;
            case ItemMotif.Fruit:
                FillCircle(texture, 128, 126, 42, motifColor);
                FillCircle(texture, 114, 138, 18, Color.Lerp(motifColor, Color.white, 0.14f));
                FillRect(texture, 124, 170, 8, 22, accent);
                FillDiamond(texture, 150, 176, 16, accent);
                break;
            case ItemMotif.Ore:
                FillDiamond(texture, 128, 128, 52, motifColor);
                FillDiamond(texture, 128, 128, 28, accent);
                FillRect(texture, 118, 168, 20, 8, new Color(1f, 1f, 1f, 0.24f));
                break;
            case ItemMotif.Bone:
                FillCircle(texture, 94, 154, 18, motifColor);
                FillCircle(texture, 162, 102, 18, motifColor);
                FillRect(texture, 98, 110, 60, 36, motifColor);
                FillRect(texture, 112, 126, 34, 8, accent);
                break;
            case ItemMotif.Scroll:
                FillRoundedRect(texture, 86, 84, 84, 96, 14, motifColor);
                FillRect(texture, 98, 154, 60, 6, accent);
                FillRect(texture, 98, 138, 48, 6, accent);
                FillRect(texture, 98, 122, 56, 6, accent);
                break;
            case ItemMotif.BoneCore:
                FillCircle(texture, 128, 126, 40, motifColor);
                FillDiamond(texture, 128, 126, 24, accent);
                FillCircle(texture, 128, 126, 12, new Color(1f, 1f, 1f, 0.22f));
                break;
            case ItemMotif.Incense:
                FillRect(texture, 104, 92, 48, 12, motifColor);
                FillRect(texture, 122, 104, 12, 58, accent);
                FillCircle(texture, 118, 176, 14, new Color(accent.r, accent.g, accent.b, 0.5f));
                FillCircle(texture, 140, 194, 18, new Color(1f, 1f, 1f, 0.12f));
                FillCircle(texture, 154, 214, 22, new Color(1f, 1f, 1f, 0.08f));
                break;
            default:
                FillDiamond(texture, 128, 128, 54, motifColor);
                FillDiamond(texture, 128, 128, 24, accent);
                break;
        }

        SaveTexture(texture, ItemOutputDirectory + "/" + key + ".png");
    }

    private static void GenerateSkillIcon(string key, Color primary, Color accent, SkillMotif motif)
    {
        const int size = 256;
        var texture = NewTexture(size, size);
        var background = Color.Lerp(primary, Color.black, 0.82f);
        var frame = Color.Lerp(primary, Color.white, 0.16f);
        var effect = Color.Lerp(primary, Color.white, 0.32f);

        FillVerticalGradient(texture, Color.Lerp(background, primary, 0.08f), background);
        FillRoundedRect(texture, 16, 16, size - 32, size - 32, 28, frame);
        FillRoundedRect(texture, 30, 30, size - 60, size - 60, 22, Color.Lerp(background, primary, 0.18f));
        FillCircle(texture, 128, 128, 78, new Color(accent.r, accent.g, accent.b, 0.08f));

        switch (motif)
        {
            case SkillMotif.Burst:
                FillDiamond(texture, 128, 128, 56, effect);
                FillRing(texture, 128, 128, 72, 10, accent);
                FillCircle(texture, 128, 128, 18, new Color(1f, 1f, 1f, 0.2f));
                break;
            case SkillMotif.Guard:
                FillRect(texture, 92, 88, 72, 74, effect);
                FillDiamond(texture, 128, 86, 38, effect);
                FillDiamond(texture, 128, 170, 20, accent);
                break;
            case SkillMotif.Heal:
                FillCircle(texture, 128, 128, 48, effect);
                FillRect(texture, 116, 88, 24, 80, accent);
                FillRect(texture, 88, 116, 80, 24, accent);
                break;
            case SkillMotif.Mist:
                FillCircle(texture, 98, 118, 26, effect);
                FillCircle(texture, 130, 138, 38, Color.Lerp(effect, accent, 0.2f));
                FillCircle(texture, 162, 114, 24, effect);
                FillRect(texture, 84, 148, 92, 18, accent);
                break;
            case SkillMotif.Poison:
                FillDiamond(texture, 128, 98, 24, accent);
                FillCircle(texture, 128, 140, 42, effect);
                FillCircle(texture, 112, 124, 8, new Color(1f, 1f, 1f, 0.2f));
                FillCircle(texture, 146, 150, 12, new Color(1f, 1f, 1f, 0.14f));
                break;
            case SkillMotif.Bind:
                FillRect(texture, 84, 116, 88, 18, effect);
                FillRect(texture, 116, 84, 18, 88, effect);
                FillDiamond(texture, 128, 128, 56, accent);
                FillDiamond(texture, 128, 128, 24, background);
                break;
            case SkillMotif.Counter:
                FillDiamond(texture, 94, 128, 28, effect);
                FillDiamond(texture, 150, 128, 28, effect);
                FillRect(texture, 88, 120, 84, 16, accent);
                FillRect(texture, 142, 106, 24, 44, accent);
                break;
            default:
                FillRect(texture, 82, 136, 96, 18, accent);
                FillRect(texture, 104, 100, 96, 18, effect);
                FillRect(texture, 74, 122, 96, 18, effect);
                FillDiamond(texture, 150, 150, 20, new Color(1f, 1f, 1f, 0.2f));
                break;
        }

        SaveTexture(texture, SkillOutputDirectory + "/" + key + ".png");
    }

    private static ItemMotif ResolveItemMotif(string itemId, string category)
    {
        if (!string.IsNullOrWhiteSpace(itemId))
        {
            if (itemId.Contains("token") || itemId.Contains("order"))
            {
                return ItemMotif.Token;
            }

            if (itemId.Contains("mushroom") || itemId.Contains("algae"))
            {
                return ItemMotif.Herb;
            }

            if (itemId.Contains("fruit") || itemId.Contains("jujube"))
            {
                return ItemMotif.Fruit;
            }

            if (itemId.Contains("ore") || itemId.Contains("iron") || itemId.Contains("crystal"))
            {
                return ItemMotif.Ore;
            }

            if (itemId.Contains("bone"))
            {
                return ItemMotif.Bone;
            }

            if (itemId.Contains("script") || itemId.Contains("page") || itemId.Contains("notes") || itemId.Contains("shard"))
            {
                return ItemMotif.Scroll;
            }

            if (itemId.Contains("core") || itemId.Contains("mark"))
            {
                return ItemMotif.BoneCore;
            }

            if (itemId.Contains("incense"))
            {
                return ItemMotif.Incense;
            }
        }

        if (!string.IsNullOrWhiteSpace(category) && category.Contains("凭证"))
        {
            return ItemMotif.Token;
        }

        return ItemMotif.Crystal;
    }

    private static SkillMotif ResolveSkillMotif(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
        {
            return SkillMotif.Slash;
        }

        if (skillId.Contains("burst"))
        {
            return SkillMotif.Burst;
        }

        if (skillId.Contains("barrier"))
        {
            return SkillMotif.Guard;
        }

        if (skillId.Contains("restore") || skillId.Contains("calm"))
        {
            return SkillMotif.Heal;
        }

        if (skillId.Contains("mist"))
        {
            return SkillMotif.Mist;
        }

        if (skillId.Contains("poison") || skillId.Contains("drain"))
        {
            return SkillMotif.Poison;
        }

        if (skillId.Contains("bind"))
        {
            return SkillMotif.Bind;
        }

        if (skillId.Contains("counter"))
        {
            return SkillMotif.Counter;
        }

        return SkillMotif.Slash;
    }

    private static Color ResolveItemPrimary(string category, string rarity)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            if (category.Contains("修炼资源"))
            {
                return new Color(0.19f, 0.38f, 0.32f);
            }

            if (category.Contains("任务凭证"))
            {
                return new Color(0.34f, 0.24f, 0.18f);
            }

            if (category.Contains("天材地宝"))
            {
                return new Color(0.16f, 0.34f, 0.2f);
            }

            if (category.Contains("炼器"))
            {
                return new Color(0.3f, 0.2f, 0.18f);
            }

            if (category.Contains("炼丹"))
            {
                return new Color(0.12f, 0.24f, 0.3f);
            }

            if (category.Contains("妖兽"))
            {
                return new Color(0.3f, 0.26f, 0.14f);
            }

            if (category.Contains("邪修") || category.Contains("尸傀"))
            {
                return new Color(0.32f, 0.14f, 0.18f);
            }

            if (category.Contains("神魂") || category.Contains("遗迹") || category.Contains("传承"))
            {
                return new Color(0.18f, 0.18f, 0.34f);
            }
        }

        return ResolveRarityColor(rarity);
    }

    private static Color ResolveItemAccent(string itemId, Color primary)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return Color.Lerp(primary, Color.white, 0.4f);
        }

        if (itemId.Contains("crystal") || itemId.Contains("jade") || itemId.Contains("dew"))
        {
            return new Color(0.66f, 0.92f, 1f);
        }

        if (itemId.Contains("flame") || itemId.Contains("crimson") || itemId.Contains("blood"))
        {
            return new Color(0.98f, 0.56f, 0.34f);
        }

        if (itemId.Contains("void") || itemId.Contains("array") || itemId.Contains("mark"))
        {
            return new Color(0.82f, 0.74f, 0.98f);
        }

        if (itemId.Contains("north") || itemId.Contains("mist") || itemId.Contains("cold"))
        {
            return new Color(0.76f, 0.9f, 1f);
        }

        return Color.Lerp(primary, Color.white, 0.5f);
    }

    private static Color ResolveSkillPrimary(string archetypeId)
    {
        switch (archetypeId)
        {
            case "alchemist":
                return new Color(0.48f, 0.18f, 0.16f);
            case "wanderer":
                return new Color(0.18f, 0.3f, 0.34f);
            default:
                return new Color(0.16f, 0.24f, 0.42f);
        }
    }

    private static Color ResolveSkillAccent(string archetypeId)
    {
        switch (archetypeId)
        {
            case "alchemist":
                return new Color(0.96f, 0.62f, 0.32f);
            case "wanderer":
                return new Color(0.74f, 0.94f, 0.9f);
            default:
                return new Color(0.78f, 0.9f, 1f);
        }
    }

    private static Color ResolveRarityColor(string rarity)
    {
        if (string.IsNullOrWhiteSpace(rarity))
        {
            return new Color(0.3f, 0.36f, 0.4f);
        }

        if (rarity.Contains("玄"))
        {
            return new Color(0.24f, 0.22f, 0.42f);
        }

        if (rarity.Contains("灵"))
        {
            return new Color(0.16f, 0.34f, 0.24f);
        }

        return new Color(0.34f, 0.32f, 0.24f);
    }

    private static void ConfigureDirectoryImporters(string directory, float ppu)
    {
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { directory });
        for (var i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }

    private static Texture2D NewTexture(int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        return texture;
    }

    private static void SaveTexture(Texture2D texture, string path)
    {
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static void FillVerticalGradient(Texture2D texture, Color top, Color bottom)
    {
        for (var y = 0; y < texture.height; y++)
        {
            var t = texture.height <= 1 ? 0f : (float)y / (texture.height - 1);
            var color = Color.Lerp(bottom, top, t);
            for (var x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }

    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        var xMin = Mathf.Max(0, x);
        var yMin = Mathf.Max(0, y);
        var xMax = Mathf.Min(texture.width, x + width);
        var yMax = Mathf.Min(texture.height, y + height);
        for (var px = xMin; px < xMax; px++)
        {
            for (var py = yMin; py < yMax; py++)
            {
                texture.SetPixel(px, py, color);
            }
        }
    }

    private static void FillRoundedRect(Texture2D texture, int x, int y, int width, int height, int radius, Color color)
    {
        var radiusSquared = radius * radius;
        for (var px = x; px < x + width; px++)
        {
            for (var py = y; py < y + height; py++)
            {
                var dx = px < x + radius ? x + radius - px : px >= x + width - radius ? px - (x + width - radius - 1) : 0;
                var dy = py < y + radius ? y + radius - py : py >= y + height - radius ? py - (y + height - radius - 1) : 0;
                if (dx * dx + dy * dy <= radiusSquared)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }
    }

    private static void FillCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        var radiusSquared = radius * radius;
        var xMin = Mathf.Max(0, centerX - radius);
        var xMax = Mathf.Min(texture.width - 1, centerX + radius);
        var yMin = Mathf.Max(0, centerY - radius);
        var yMax = Mathf.Min(texture.height - 1, centerY + radius);
        for (var px = xMin; px <= xMax; px++)
        {
            for (var py = yMin; py <= yMax; py++)
            {
                var dx = px - centerX;
                var dy = py - centerY;
                if (dx * dx + dy * dy <= radiusSquared)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }
    }

    private static void FillDiamond(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        var xMin = Mathf.Max(0, centerX - radius);
        var xMax = Mathf.Min(texture.width - 1, centerX + radius);
        var yMin = Mathf.Max(0, centerY - radius);
        var yMax = Mathf.Min(texture.height - 1, centerY + radius);
        for (var px = xMin; px <= xMax; px++)
        {
            for (var py = yMin; py <= yMax; py++)
            {
                if (Mathf.Abs(px - centerX) + Mathf.Abs(py - centerY) <= radius)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }
    }

    private static void FillRing(Texture2D texture, int centerX, int centerY, int radius, int thickness, Color color)
    {
        var outer = radius * radius;
        var innerRadius = Mathf.Max(0, radius - thickness);
        var inner = innerRadius * innerRadius;
        var xMin = Mathf.Max(0, centerX - radius);
        var xMax = Mathf.Min(texture.width - 1, centerX + radius);
        var yMin = Mathf.Max(0, centerY - radius);
        var yMax = Mathf.Min(texture.height - 1, centerY + radius);
        for (var px = xMin; px <= xMax; px++)
        {
            for (var py = yMin; py <= yMax; py++)
            {
                var dx = px - centerX;
                var dy = py - centerY;
                var distance = dx * dx + dy * dy;
                if (distance <= outer && distance >= inner)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }
    }
}
