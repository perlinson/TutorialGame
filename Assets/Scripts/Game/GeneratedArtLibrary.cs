using System.Collections.Generic;
using UnityEngine;

public static class GeneratedArtLibrary
{
    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
    private static readonly Dictionary<string, Sprite> GeneratedEnemyPortraitCache = new Dictionary<string, Sprite>();
    private static readonly Dictionary<string, Sprite> RuntimeArtCache = new Dictionary<string, Sprite>();

    public static Sprite GetHeroPortrait(string archetypeId)
    {
        return LoadSprite("Heroes/" + archetypeId);
    }

    public static Sprite GetWorldRegionIllustration(string regionId)
    {
        return LoadSprite("WorldRegions/" + regionId);
    }

    public static Sprite GetArenaBackdrop(string regionId)
    {
        return LoadSprite("ArenaBackdrops/" + regionId);
    }

    public static Sprite GetEnemyPortrait(ExpeditionEnemyFaction faction, bool elite)
    {
        var cacheKey = faction + ":" + elite;
        Sprite sprite;
        if (GeneratedEnemyPortraitCache.TryGetValue(cacheKey, out sprite))
        {
            return sprite;
        }

        var customSprite = LoadRuntimeArtSprite(ResolveCustomEnemyPortraitPath(faction, elite));
        if (customSprite != null)
        {
            GeneratedEnemyPortraitCache[cacheKey] = customSprite;
            return customSprite;
        }

        var generatedSprite = LoadSprite("Enemies/" + ResolveEnemyPortraitKey(faction, elite));
        if (generatedSprite != null)
        {
            GeneratedEnemyPortraitCache[cacheKey] = generatedSprite;
            return generatedSprite;
        }

        var primary = ResolveEnemyPrimaryColor(faction);
        var secondary = Color.Lerp(primary, Color.white, elite ? 0.38f : 0.18f);
        var background = Color.Lerp(primary, Color.black, 0.72f);
        var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        FillRect(texture, 0, 0, 128, 128, background);
        FillRect(texture, 8, 8, 112, 112, Color.Lerp(primary, Color.black, 0.18f));
        FillRect(texture, 18, 20, 92, 18, new Color(0f, 0f, 0f, 0.18f));
        FillRect(texture, 38, 42, 52, 54, secondary);
        FillRect(texture, 28, 54, 16, 26, secondary);
        FillRect(texture, 84, 54, 16, 26, secondary);
        FillRect(texture, 48, 62, 32, 10, new Color(1f, 0.93f, 0.72f, 0.95f));
        FillRect(texture, 44, 78, 40, 8, Color.Lerp(secondary, Color.white, 0.12f));

        if (elite)
        {
            FillRect(texture, 20, 102, 88, 10, new Color(0.94f, 0.8f, 0.46f, 0.96f));
            FillRect(texture, 52, 94, 24, 12, new Color(0.96f, 0.88f, 0.62f, 0.96f));
        }

        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        GeneratedEnemyPortraitCache[cacheKey] = sprite;
        return sprite;
    }

    public static Sprite GetItemIcon(string itemId)
    {
        return LoadSprite("Items/" + itemId);
    }

    public static Sprite GetSkillIcon(string skillId)
    {
        return LoadSprite("Skills/" + skillId);
    }

    public static Sprite GetRuntimeArtSprite(string relativePath)
    {
        return LoadRuntimeArtSprite(relativePath);
    }

    private static string ResolveEnemyPortraitKey(ExpeditionEnemyFaction faction, bool elite)
    {
        var suffix = elite ? "_elite" : "_common";
        switch (faction)
        {
            case ExpeditionEnemyFaction.Bandit:
                return "bandit" + suffix;
            case ExpeditionEnemyFaction.Cultivator:
                return "cultivator" + suffix;
            case ExpeditionEnemyFaction.Beast:
                return "beast" + suffix;
            case ExpeditionEnemyFaction.HeartDemon:
                return "heart_demon" + suffix;
            default:
                return string.Empty;
        }
    }

    private static string ResolveCustomEnemyPortraitPath(ExpeditionEnemyFaction faction, bool elite)
    {
        if (elite)
        {
            return string.Empty;
        }

        switch (faction)
        {
            case ExpeditionEnemyFaction.Cultivator:
                return "Enemies/Portraits/enemy_portrait_bloodcultist_a";
            default:
                return string.Empty;
        }
    }

    private static Sprite LoadSprite(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        Sprite sprite;
        if (SpriteCache.TryGetValue(relativePath, out sprite))
        {
            return sprite;
        }

        sprite = GameResource.Load<Sprite>("Generated/" + relativePath);
        if (sprite == null)
        {
            var texture = GameResource.Load<Texture2D>("Generated/" + relativePath);
            if (texture != null)
            {
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }
        }

        SpriteCache[relativePath] = sprite;
        return sprite;
    }

    private static Sprite LoadRuntimeArtSprite(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        Sprite sprite;
        if (RuntimeArtCache.TryGetValue(relativePath, out sprite))
        {
            return sprite;
        }

        sprite = GameResource.Load<Sprite>("Art/" + relativePath);
        if (sprite == null)
        {
            var texture = GameResource.Load<Texture2D>("Art/" + relativePath);
            if (texture != null)
            {
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }
        }

        RuntimeArtCache[relativePath] = sprite;
        return sprite;
    }

    private static Color ResolveEnemyPrimaryColor(ExpeditionEnemyFaction faction)
    {
        switch (faction)
        {
            case ExpeditionEnemyFaction.Bandit:
                return new Color(0.48f, 0.31f, 0.21f, 1f);
            case ExpeditionEnemyFaction.Cultivator:
                return new Color(0.56f, 0.14f, 0.22f, 1f);
            case ExpeditionEnemyFaction.Beast:
                return new Color(0.24f, 0.42f, 0.22f, 1f);
            case ExpeditionEnemyFaction.HeartDemon:
                return new Color(0.4f, 0.2f, 0.48f, 1f);
            default:
                return new Color(0.34f, 0.38f, 0.44f, 1f);
        }
    }

    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        var xMax = Mathf.Min(texture.width, x + width);
        var yMax = Mathf.Min(texture.height, y + height);
        for (var px = Mathf.Max(0, x); px < xMax; px++)
        {
            for (var py = Mathf.Max(0, y); py < yMax; py++)
            {
                texture.SetPixel(px, py, color);
            }
        }
    }
}
