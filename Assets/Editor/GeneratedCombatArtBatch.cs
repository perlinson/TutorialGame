using System.IO;
using UnityEditor;
using UnityEngine;

public static class GeneratedCombatArtBatch
{
    private const string EnemyOutputDirectory = "Assets/Resources/Generated/Enemies";
    private const string ArenaBackdropOutputDirectory = "Assets/Resources/Generated/ArenaBackdrops";

    [MenuItem("Cultivation/Generated Art/Generate Combat Resource Batch")]
    public static void GenerateAll()
    {
        EnsureDirectory(EnemyOutputDirectory);
        EnsureDirectory(ArenaBackdropOutputDirectory);

        GenerateEnemyPortrait("bandit_common", new Color(0.48f, 0.31f, 0.21f), new Color(0.88f, 0.74f, 0.58f), false);
        GenerateEnemyPortrait("bandit_elite", new Color(0.5f, 0.27f, 0.16f), new Color(0.98f, 0.84f, 0.56f), true);
        GenerateEnemyPortrait("cultivator_common", new Color(0.52f, 0.14f, 0.22f), new Color(0.92f, 0.8f, 0.9f), false);
        GenerateEnemyPortrait("cultivator_elite", new Color(0.62f, 0.12f, 0.22f), new Color(1f, 0.86f, 0.72f), true);
        GenerateEnemyPortrait("beast_common", new Color(0.24f, 0.42f, 0.22f), new Color(0.84f, 0.92f, 0.74f), false);
        GenerateEnemyPortrait("beast_elite", new Color(0.16f, 0.4f, 0.18f), new Color(0.94f, 0.88f, 0.56f), true);
        GenerateEnemyPortrait("heart_demon_common", new Color(0.38f, 0.18f, 0.46f), new Color(0.9f, 0.8f, 0.98f), false);
        GenerateEnemyPortrait("heart_demon_elite", new Color(0.48f, 0.16f, 0.56f), new Color(1f, 0.8f, 0.58f), true);

        GenerateArenaBackdrop("green_stone_gate", new Color(0.08f, 0.14f, 0.12f), new Color(0.24f, 0.42f, 0.34f), new Color(0.64f, 0.84f, 0.72f), BackdropMotif.Gate);
        GenerateArenaBackdrop("misty_forest", new Color(0.06f, 0.11f, 0.12f), new Color(0.16f, 0.34f, 0.28f), new Color(0.72f, 0.9f, 0.86f), BackdropMotif.Forest);
        GenerateArenaBackdrop("crimson_valley", new Color(0.16f, 0.07f, 0.08f), new Color(0.44f, 0.14f, 0.12f), new Color(0.96f, 0.56f, 0.34f), BackdropMotif.Valley);
        GenerateArenaBackdrop("deep_springs", new Color(0.05f, 0.09f, 0.13f), new Color(0.12f, 0.26f, 0.4f), new Color(0.58f, 0.9f, 1f), BackdropMotif.Springs);
        GenerateArenaBackdrop("celestial_ruins", new Color(0.08f, 0.08f, 0.12f), new Color(0.28f, 0.26f, 0.38f), new Color(0.9f, 0.82f, 0.62f), BackdropMotif.Ruins);
        GenerateArenaBackdrop("northern_pass", new Color(0.08f, 0.08f, 0.14f), new Color(0.18f, 0.2f, 0.34f), new Color(0.82f, 0.88f, 0.98f), BackdropMotif.Pass);

        AssetDatabase.Refresh();
        ConfigureEnemyImporters();
        ConfigureBackdropImporters();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private enum BackdropMotif
    {
        Gate,
        Forest,
        Valley,
        Springs,
        Ruins,
        Pass
    }

    private static void GenerateEnemyPortrait(string key, Color primary, Color accent, bool elite)
    {
        const int size = 256;
        var texture = NewTexture(size, size);
        var background = Color.Lerp(primary, Color.black, 0.78f);
        var frame = Color.Lerp(primary, Color.white, elite ? 0.28f : 0.14f);
        var body = Color.Lerp(primary, Color.white, elite ? 0.44f : 0.22f);
        var eyes = elite ? new Color(1f, 0.9f, 0.62f) : new Color(0.94f, 0.92f, 0.8f);

        Fill(texture, background);
        FillRoundedRect(texture, 12, 12, size - 24, size - 24, 22, frame);
        FillRoundedRect(texture, 28, 28, size - 56, size - 56, 18, Color.Lerp(background, primary, 0.22f));
        FillCircle(texture, size / 2, 152, elite ? 72 : 66, body);
        FillCircle(texture, size / 2, 88, elite ? 56 : 48, body);
        FillDiamond(texture, size / 2, 88, elite ? 56 : 42, Color.Lerp(body, accent, 0.22f));
        FillRect(texture, 84, 124, 88, 18, new Color(0f, 0f, 0f, 0.18f));
        FillRect(texture, 92, 108, 28, elite ? 12 : 10, eyes);
        FillRect(texture, 136, 108, 28, elite ? 12 : 10, eyes);
        FillRect(texture, 108, 72, 40, 10, Color.Lerp(body, Color.white, 0.2f));
        FillRect(texture, 72, 180, 112, elite ? 18 : 12, Color.Lerp(accent, Color.white, elite ? 0.18f : 0.06f));
        FillRing(texture, size / 2, 152, elite ? 94 : 84, elite ? 10 : 6, new Color(accent.r, accent.g, accent.b, elite ? 0.55f : 0.26f));

        if (elite)
        {
            FillDiamond(texture, size / 2, 208, 20, new Color(1f, 0.86f, 0.52f, 0.92f));
            FillDiamond(texture, 76, 194, 12, new Color(1f, 0.82f, 0.44f, 0.72f));
            FillDiamond(texture, 180, 194, 12, new Color(1f, 0.82f, 0.44f, 0.72f));
        }

        SaveTexture(texture, EnemyOutputDirectory + "/" + key + ".png");
    }

    private static void GenerateArenaBackdrop(string key, Color skyTop, Color skyBottom, Color accent, BackdropMotif motif)
    {
        const int width = 1024;
        const int height = 512;
        var texture = NewTexture(width, height);

        FillVerticalGradient(texture, skyTop, skyBottom);
        FillCircle(texture, width / 2, 360, 78, new Color(accent.r, accent.g, accent.b, 0.22f));
        DrawHorizonBands(texture, skyBottom);

        switch (motif)
        {
            case BackdropMotif.Gate:
                DrawMountainLayer(texture, 210, 78, Color.Lerp(skyBottom, Color.black, 0.2f), 0.16f);
                DrawMountainLayer(texture, 150, 52, Color.Lerp(skyBottom, Color.black, 0.38f), 0.11f);
                DrawGateSilhouette(texture, accent);
                break;
            case BackdropMotif.Forest:
                DrawMountainLayer(texture, 180, 74, Color.Lerp(skyBottom, Color.black, 0.18f), 0.17f);
                DrawForestSilhouette(texture, accent);
                break;
            case BackdropMotif.Valley:
                DrawMountainLayer(texture, 230, 96, Color.Lerp(skyBottom, Color.black, 0.22f), 0.2f);
                DrawMountainLayer(texture, 150, 64, Color.Lerp(skyBottom, Color.black, 0.42f), 0.14f);
                DrawValleySpire(texture, accent);
                break;
            case BackdropMotif.Springs:
                DrawMountainLayer(texture, 200, 82, Color.Lerp(skyBottom, Color.black, 0.18f), 0.17f);
                DrawWaterTerraces(texture, accent);
                break;
            case BackdropMotif.Ruins:
                DrawMountainLayer(texture, 220, 86, Color.Lerp(skyBottom, Color.black, 0.3f), 0.18f);
                DrawRuins(texture, accent);
                break;
            case BackdropMotif.Pass:
                DrawMountainLayer(texture, 248, 108, Color.Lerp(skyBottom, Color.black, 0.26f), 0.18f);
                DrawMountainLayer(texture, 176, 72, Color.Lerp(skyBottom, Color.black, 0.46f), 0.12f);
                DrawPassColumns(texture, accent);
                break;
        }

        SaveTexture(texture, ArenaBackdropOutputDirectory + "/" + key + ".png");
    }

    private static void ConfigureEnemyImporters()
    {
        ConfigureDirectoryImporters(EnemyOutputDirectory, 100f);
    }

    private static void ConfigureBackdropImporters()
    {
        ConfigureDirectoryImporters(ArenaBackdropOutputDirectory, 100f);
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

    private static void Fill(Texture2D texture, Color color)
    {
        var colors = new Color[texture.width * texture.height];
        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }

        texture.SetPixels(colors);
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
                var dist = dx * dx + dy * dy;
                if (dist <= outer && dist >= inner)
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
                var dx = Mathf.Abs(px - centerX);
                var dy = Mathf.Abs(py - centerY);
                if (dx + dy <= radius)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }
    }

    private static void DrawHorizonBands(Texture2D texture, Color baseColor)
    {
        FillRect(texture, 0, 0, texture.width, 96, Color.Lerp(baseColor, Color.black, 0.5f));
        FillRect(texture, 0, 84, texture.width, 18, new Color(baseColor.r, baseColor.g, baseColor.b, 0.18f));
        FillRect(texture, 0, 132, texture.width, 14, new Color(baseColor.r, baseColor.g, baseColor.b, 0.12f));
    }

    private static void DrawMountainLayer(Texture2D texture, int baseHeight, int amplitude, Color color, float frequency)
    {
        for (var x = 0; x < texture.width; x++)
        {
            var t = x * frequency * Mathf.Deg2Rad;
            var height = baseHeight
                         + Mathf.Sin(t * 1.7f) * amplitude
                         + Mathf.Sin(t * 0.65f + 1.2f) * amplitude * 0.45f
                         + Mathf.Sin(t * 2.4f + 0.6f) * amplitude * 0.18f;
            FillRect(texture, x, 0, 1, Mathf.Clamp(Mathf.RoundToInt(height), 0, texture.height), color);
        }
    }

    private static void DrawGateSilhouette(Texture2D texture, Color accent)
    {
        var color = Color.Lerp(accent, Color.black, 0.54f);
        FillRect(texture, 444, 92, 32, 206, color);
        FillRect(texture, 548, 92, 32, 206, color);
        FillRect(texture, 400, 262, 224, 32, color);
        FillRect(texture, 416, 294, 192, 16, Color.Lerp(accent, Color.white, 0.16f));
    }

    private static void DrawForestSilhouette(Texture2D texture, Color accent)
    {
        var trunk = Color.Lerp(accent, Color.black, 0.58f);
        var crown = Color.Lerp(accent, Color.black, 0.28f);
        for (var i = 0; i < 18; i++)
        {
            var x = 34 + i * 54;
            var h = 90 + (i % 4) * 18;
            FillRect(texture, x, 88, 10, h, trunk);
            FillDiamond(texture, x + 5, 112 + h, 40 + (i % 3) * 10, crown);
            FillDiamond(texture, x + 5, 150 + h, 30 + (i % 2) * 8, crown);
        }
    }

    private static void DrawValleySpire(Texture2D texture, Color accent)
    {
        var color = Color.Lerp(accent, Color.black, 0.42f);
        for (var i = 0; i < 7; i++)
        {
            var x = 160 + i * 110;
            var width = 22 + (i % 3) * 12;
            var height = 140 + (i % 4) * 24;
            for (var step = 0; step < height; step++)
            {
                var currentWidth = Mathf.Max(4, width - step / 8);
                FillRect(texture, x, 90 + step, currentWidth, 1, color);
            }
        }
    }

    private static void DrawWaterTerraces(Texture2D texture, Color accent)
    {
        var water = new Color(accent.r, accent.g, accent.b, 0.3f);
        for (var i = 0; i < 5; i++)
        {
            FillRect(texture, 110 + i * 82, 86 + i * 26, 780 - i * 120, 12, water);
            FillRect(texture, 126 + i * 82, 72 + i * 26, 748 - i * 120, 8, Color.Lerp(accent, Color.white, 0.12f));
        }
    }

    private static void DrawRuins(Texture2D texture, Color accent)
    {
        var stone = Color.Lerp(accent, Color.black, 0.46f);
        FillRect(texture, 208, 94, 34, 132, stone);
        FillRect(texture, 304, 94, 34, 176, stone);
        FillRect(texture, 446, 94, 34, 148, stone);
        FillRect(texture, 632, 94, 34, 188, stone);
        FillRect(texture, 760, 94, 34, 164, stone);
        FillRect(texture, 186, 226, 170, 18, stone);
        FillRect(texture, 606, 246, 210, 18, stone);
    }

    private static void DrawPassColumns(Texture2D texture, Color accent)
    {
        var column = Color.Lerp(accent, Color.black, 0.5f);
        FillRect(texture, 168, 92, 38, 228, column);
        FillRect(texture, 820, 92, 38, 228, column);
        FillRect(texture, 150, 280, 76, 18, column);
        FillRect(texture, 800, 280, 76, 18, column);
        FillRect(texture, 456, 94, 112, 26, Color.Lerp(accent, Color.black, 0.38f));
    }
}
