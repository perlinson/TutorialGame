using UnityEngine;
using UnityEngine.UI;

public static class GameSpriteLibrary
{
    private static Sprite whiteSquareSprite;

    public static Sprite WhiteSquareSprite
    {
        get
        {
            if (whiteSquareSprite == null)
            {
                whiteSquareSprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }

            return whiteSquareSprite;
        }
    }

    public static void BindSpriteOrPlaceholder(Image image, Text label, Sprite sprite, string caption, Color placeholderColor, bool keepCaptionWhenSpritePresent = true)
    {
        if (image != null)
        {
            image.sprite = sprite != null ? sprite : WhiteSquareSprite;
            image.color = sprite != null ? Color.white : placeholderColor;
        }

        if (label != null)
        {
            label.text = sprite != null && !keepCaptionWhenSpritePresent ? string.Empty : caption;
        }
    }
}
