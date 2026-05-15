using UnityEngine;

public sealed class GameArenaRuntimeBindings
{
    public Transform ArenaRoot;
    public Transform RoomContentRoot;
    public PlayerCultivator Player;
}

public static class GameArenaBuilder
{
    public static GameArenaRuntimeBindings Build(WorldRegionDefinition region)
    {
        var arenaRoot = new GameObject("ArenaRoot").transform;
        BuildBackdrop(arenaRoot, region);
        BuildGround(arenaRoot, region);

        var roomContentRoot = new GameObject("RoomContentRoot").transform;
        roomContentRoot.SetParent(arenaRoot, false);

        var player = CreatePlayer(arenaRoot, region);
        return new GameArenaRuntimeBindings
        {
            ArenaRoot = arenaRoot,
            RoomContentRoot = roomContentRoot,
            Player = player
        };
    }

    private static void BuildBackdrop(Transform parent, WorldRegionDefinition region)
    {
        CreateSprite("BackdropFar", parent, Vector3.zero, new Vector2(region.ArenaSize.x * 1.08f, region.ArenaSize.y * 1.08f), Color.Lerp(region.BackdropColor, Color.black, 0.28f), -30);
        CreateSprite("BackdropGlow", parent, new Vector3(0f, region.ArenaSize.y * 0.1f, 0f), new Vector2(region.ArenaSize.x * 0.72f, region.ArenaSize.y * 0.48f), Color.Lerp(region.AccentColor, Color.white, 0.08f), -29);
        CreateSprite("MistLeft", parent, new Vector3(-region.ArenaSize.x * 0.28f, 0f, 0f), new Vector2(region.ArenaSize.x * 0.18f, region.ArenaSize.y * 0.92f), new Color(region.AccentColor.r, region.AccentColor.g, region.AccentColor.b, 0.08f), -28);
        CreateSprite("MistRight", parent, new Vector3(region.ArenaSize.x * 0.28f, 0f, 0f), new Vector2(region.ArenaSize.x * 0.18f, region.ArenaSize.y * 0.92f), new Color(region.AccentColor.r, region.AccentColor.g, region.AccentColor.b, 0.08f), -28);
    }

    private static void BuildGround(Transform parent, WorldRegionDefinition region)
    {
        CreateSprite("OuterGround", parent, new Vector3(0f, -0.24f, 0f), region.ArenaSize, region.GroundColor, -20);
        CreateSprite("InnerGround", parent, new Vector3(0f, -0.12f, 0f), region.ArenaSize * 0.76f, region.InnerGroundColor, -19);
        CreateSprite("GroundVein", parent, new Vector3(0f, 0f, 0f), new Vector2(region.ArenaSize.x * 0.88f, 0.16f), new Color(region.AccentColor.r, region.AccentColor.g, region.AccentColor.b, 0.26f), -18);
        CreateSprite("LeftStele", parent, new Vector3(-region.ArenaSize.x * 0.36f, region.ArenaSize.y * 0.16f, 0f), new Vector2(0.92f, 2.8f), Color.Lerp(region.AccentColor, Color.black, 0.45f), -17);
        CreateSprite("RightStele", parent, new Vector3(region.ArenaSize.x * 0.36f, region.ArenaSize.y * 0.16f, 0f), new Vector2(0.92f, 2.8f), Color.Lerp(region.AccentColor, Color.black, 0.45f), -17);
    }

    private static PlayerCultivator CreatePlayer(Transform parent, WorldRegionDefinition region)
    {
        var root = new GameObject("PlayerCultivator", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(PlayerCultivator));
        root.transform.SetParent(parent, false);
        root.transform.position = region.PlayerSpawn;

        var renderer = root.GetComponent<SpriteRenderer>();
        renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
        renderer.color = Color.Lerp(region.AccentColor, Color.white, 0.52f);
        renderer.sortingOrder = 18;

        var body = root.GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        var collider = root.GetComponent<CircleCollider2D>();
        collider.radius = 0.44f;
        collider.isTrigger = false;

        root.transform.localScale = new Vector3(0.92f, 1.18f, 1f);
        return root.GetComponent<PlayerCultivator>();
    }

    public static SpiritEnemy CreateEnemy(Transform parent, Vector2 position, Color bodyColor, bool elite)
    {
        var root = new GameObject("SpiritEnemy", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(SpiritEnemy));
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var renderer = root.GetComponent<SpriteRenderer>();
        renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
        renderer.color = elite ? Color.Lerp(bodyColor, Color.white, 0.24f) : bodyColor;
        renderer.sortingOrder = 16;

        var collider = root.GetComponent<CircleCollider2D>();
        collider.radius = elite ? 0.56f : 0.48f;
        collider.isTrigger = true;

        root.transform.localScale = elite ? new Vector3(1.16f, 1.26f, 1f) : new Vector3(0.96f, 1.06f, 1f);
        return root.GetComponent<SpiritEnemy>();
    }

    public static SpiritNode CreateSpiritNode(Transform parent, Vector2 position, Color tint)
    {
        var root = new GameObject("SpiritNode", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(SpiritNode));
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var renderer = root.GetComponent<SpriteRenderer>();
        renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
        renderer.color = tint;
        renderer.sortingOrder = 12;

        var collider = root.GetComponent<CircleCollider2D>();
        collider.radius = 0.34f;
        collider.isTrigger = true;

        root.transform.localScale = new Vector3(0.46f, 0.46f, 1f);
        return root.GetComponent<SpiritNode>();
    }

    public static SpiritHerb CreateSpiritHerb(Transform parent, Vector2 position, Color tint)
    {
        var root = new GameObject("SpiritHerb", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(SpiritHerb));
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var renderer = root.GetComponent<SpriteRenderer>();
        renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
        renderer.color = tint;
        renderer.sortingOrder = 12;

        var collider = root.GetComponent<CircleCollider2D>();
        collider.radius = 0.32f;
        collider.isTrigger = true;

        root.transform.localScale = new Vector3(0.38f, 0.62f, 1f);
        return root.GetComponent<SpiritHerb>();
    }

    public static TrialRelic CreateRelic(Transform parent, Vector2 position, Color tint)
    {
        var root = new GameObject("TrialRelic", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(TrialRelic));
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var renderer = root.GetComponent<SpriteRenderer>();
        renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
        renderer.color = tint;
        renderer.sortingOrder = 13;

        var collider = root.GetComponent<CircleCollider2D>();
        collider.radius = 0.34f;
        collider.isTrigger = true;

        root.transform.localScale = new Vector3(0.54f, 0.54f, 1f);
        return root.GetComponent<TrialRelic>();
    }

    public static void CreateDecor(Transform parent, string name, Vector2 position, Vector2 size, Color tint, int sortingOrder)
    {
        CreateSprite(name, parent, position, size, tint, sortingOrder);
    }

    private static GameObject CreateSprite(string name, Transform parent, Vector2 position, Vector2 size, Color tint, int sortingOrder)
    {
        var root = new GameObject(name, typeof(SpriteRenderer));
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(position.x, position.y, 0f);
        root.transform.localScale = new Vector3(size.x, size.y, 1f);

        var renderer = root.GetComponent<SpriteRenderer>();
        renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
        renderer.color = tint;
        renderer.sortingOrder = sortingOrder;
        return root;
    }
}
