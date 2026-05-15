using System.Collections.Generic;
using UnityEngine;

public static class GeneratedArtLibrary
{
    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

    public static Sprite GetHeroPortrait(string archetypeId)
    {
        return LoadSprite("Heroes/" + archetypeId);
    }

    public static Sprite GetWorldRegionIllustration(string regionId)
    {
        return LoadSprite("WorldRegions/" + regionId);
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

        sprite = Resources.Load<Sprite>("Generated/" + relativePath);
        if (sprite == null)
        {
            var texture = Resources.Load<Texture2D>("Generated/" + relativePath);
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
}
