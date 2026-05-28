using UnityEngine;

public sealed class GameArenaRuntimeBindings
{
    public Transform ArenaRoot;
    public Transform RoomContentRoot;
    public PlayerCultivator Player;
}

public static class GameArenaBuilder
{
    private const string PlayerPrefabPath = "Prefabs/Combat/PlayerCultivator";
    private const string EnemyPrefabPath = "Prefabs/Combat/SpiritEnemy";
    private const string SpiritNodePrefabPath = "Prefabs/Combat/SpiritNode";
    private const string SpiritHerbPrefabPath = "Prefabs/Combat/SpiritHerb";
    private const string TrialRelicPrefabPath = "Prefabs/Combat/TrialRelic";
    private const string FloatingCombatTextPrefabPath = "Prefabs/Combat/FloatingCombatText";
    private const string SlashEffectPrefabPath = "Prefabs/Combat/CombatSlashEffect";
    private const string ImpactEffectPrefabPath = "Prefabs/Combat/CombatImpactEffect";
    private const string PlayerAnimatorPath = "Animations/Combat/PlayerCultivator";
    private const string EnemyAnimatorPath = "Animations/Combat/SpiritEnemy";

    public static GameArenaRuntimeBindings Build(WorldRegionDefinition region, CultivationSaveData saveData)
    {
        var arenaRoot = new GameObject("ArenaRoot").transform;
        BuildBackdrop(arenaRoot, region);
        BuildGround(arenaRoot, region);

        var roomContentRoot = new GameObject("RoomContentRoot").transform;
        roomContentRoot.SetParent(arenaRoot, false);

        var player = CreatePlayer(arenaRoot, region, saveData);
        return new GameArenaRuntimeBindings
        {
            ArenaRoot = arenaRoot,
            RoomContentRoot = roomContentRoot,
            Player = player
        };
    }

    private static void BuildBackdrop(Transform parent, WorldRegionDefinition region)
    {
        var backdropSprite = GeneratedArtLibrary.GetArenaBackdrop(region.Id);
        if (backdropSprite != null)
        {
            CreateSprite(
                "BackdropIllustration",
                parent,
                Vector2.zero,
                new Vector2(region.ArenaSize.x * 1.06f, region.ArenaSize.y * 0.92f),
                new Color(1f, 1f, 1f, 0.72f),
                -31,
                backdropSprite);
        }

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

    private static PlayerCultivator CreatePlayer(Transform parent, WorldRegionDefinition region, CultivationSaveData saveData)
    {
        var playerPortrait = GeneratedArtLibrary.GetHeroPortrait(saveData != null ? saveData.archetypeId : string.Empty);
        var prefabPlayer = GameResource.InstantiatePrefab(PlayerPrefabPath, parent);
        if (prefabPlayer != null)
        {
            prefabPlayer.name = "PlayerCultivator";
            prefabPlayer.transform.position = region.PlayerSpawn;
            var playerComponent = prefabPlayer.GetComponent<PlayerCultivator>();
            if (playerComponent != null)
            {
                playerComponent.ApplyPresentation(playerPortrait, region.AccentColor);
            }

            ConfigurePlayerPhysics(prefabPlayer);
            EnsureAnimator(prefabPlayer.transform.Find("VisualPivot/VisualBody"), PlayerAnimatorPath);
            return playerComponent;
        }

        var root = new GameObject("PlayerCultivator", typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(PlayerCultivator));
        root.transform.SetParent(parent, false);
        root.transform.position = region.PlayerSpawn;

        var visualPivot = new GameObject("VisualPivot").transform;
        visualPivot.SetParent(root.transform, false);

        var visualBody = new GameObject("VisualBody", typeof(SpriteRenderer), typeof(Animator)).transform;
        visualBody.SetParent(visualPivot, false);
        visualBody.localScale = new Vector3(0.92f, 1.18f, 1f);

        var renderer = visualBody.GetComponent<SpriteRenderer>();
        renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
        renderer.color = Color.Lerp(region.AccentColor, Color.white, 0.52f);
        renderer.sortingOrder = 18;

        var animator = visualBody.GetComponent<Animator>();
        animator.runtimeAnimatorController = GameResource.Load<RuntimeAnimatorController>(PlayerAnimatorPath);

        ConfigurePlayerPhysics(root);
        var player = root.GetComponent<PlayerCultivator>();
        player.ApplyPresentation(playerPortrait, region.AccentColor);
        return player;
    }

    public static SpiritEnemy CreateEnemy(Transform parent, Vector2 position, ExpeditionEnemyFaction faction, Color bodyColor, bool elite)
    {
        var enemyPortrait = GeneratedArtLibrary.GetEnemyPortrait(faction, elite);
        var prefabEnemy = GameResource.InstantiatePrefab(EnemyPrefabPath, parent);
        if (prefabEnemy != null)
        {
            prefabEnemy.name = "SpiritEnemy";
            prefabEnemy.transform.position = position;
            ConfigureEnemyPhysics(prefabEnemy, elite);
            EnsureAnimator(prefabEnemy.transform.Find("VisualPivot/VisualBody"), EnemyAnimatorPath);
            var enemyComponent = prefabEnemy.GetComponent<SpiritEnemy>();
            if (enemyComponent != null)
            {
                enemyComponent.ApplyPresentation(enemyPortrait, bodyColor, elite);
            }

            return enemyComponent;
        }

        var root = new GameObject("SpiritEnemy", typeof(CircleCollider2D), typeof(SpiritEnemy));
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var visualPivot = new GameObject("VisualPivot").transform;
        visualPivot.SetParent(root.transform, false);

        var visualBody = new GameObject("VisualBody", typeof(SpriteRenderer), typeof(Animator)).transform;
        visualBody.SetParent(visualPivot, false);
        visualBody.localScale = elite ? new Vector3(1.16f, 1.26f, 1f) : new Vector3(0.96f, 1.06f, 1f);

        var renderer = visualBody.GetComponent<SpriteRenderer>();
        renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
        renderer.color = elite ? Color.Lerp(bodyColor, Color.white, 0.24f) : bodyColor;
        renderer.sortingOrder = 16;

        var animator = visualBody.GetComponent<Animator>();
        animator.runtimeAnimatorController = GameResource.Load<RuntimeAnimatorController>(EnemyAnimatorPath);

        ConfigureEnemyPhysics(root, elite);

        CreateSprite("Core", visualBody, new Vector2(0f, 0f), elite ? new Vector2(0.68f, 0.84f) : new Vector2(0.54f, 0.7f), Color.Lerp(bodyColor, Color.white, elite ? 0.24f : 0.14f), 17);
        CreateSprite("EyeLine", visualBody, new Vector2(0f, 0.18f), elite ? new Vector2(0.4f, 0.1f) : new Vector2(0.32f, 0.08f), new Color(1f, 0.92f, 0.7f, 0.96f), 18);

        var enemy = root.GetComponent<SpiritEnemy>();
        enemy.ApplyPresentation(enemyPortrait, bodyColor, elite);
        return enemy;
    }

    public static SpiritNode CreateSpiritNode(Transform parent, Vector2 position, Color tint)
    {
        var prefabNode = GameResource.InstantiatePrefab(SpiritNodePrefabPath, parent);
        if (prefabNode != null)
        {
            prefabNode.name = "SpiritNode";
            prefabNode.transform.position = position;
            ConfigurePickupRenderer(prefabNode, tint, 12, new Vector3(0.46f, 0.46f, 1f));
            ConfigurePickupCollider(prefabNode, 0.34f);
            var prefabNodeComponent = prefabNode.GetComponent<SpiritNode>();
            if (prefabNodeComponent != null)
            {
                return prefabNodeComponent;
            }

            Object.Destroy(prefabNode);
        }

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
        var prefabHerb = GameResource.InstantiatePrefab(SpiritHerbPrefabPath, parent);
        if (prefabHerb != null)
        {
            prefabHerb.name = "SpiritHerb";
            prefabHerb.transform.position = position;
            ConfigurePickupRenderer(prefabHerb, tint, 12, new Vector3(0.38f, 0.62f, 1f));
            ConfigurePickupCollider(prefabHerb, 0.32f);
            var prefabHerbComponent = prefabHerb.GetComponent<SpiritHerb>();
            if (prefabHerbComponent != null)
            {
                return prefabHerbComponent;
            }

            Object.Destroy(prefabHerb);
        }

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
        var prefabRelic = GameResource.InstantiatePrefab(TrialRelicPrefabPath, parent);
        if (prefabRelic != null)
        {
            prefabRelic.name = "TrialRelic";
            prefabRelic.transform.position = position;
            ConfigurePickupRenderer(prefabRelic, tint, 13, new Vector3(0.54f, 0.54f, 1f));
            ConfigurePickupCollider(prefabRelic, 0.34f);
            var prefabRelicComponent = prefabRelic.GetComponent<TrialRelic>();
            if (prefabRelicComponent != null)
            {
                return prefabRelicComponent;
            }

            Object.Destroy(prefabRelic);
        }

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

    public static FloatingCombatText CreateFloatingCombatText(Transform parent, Vector3 position, string message, Color color, int sortingOrder = 40, float characterSize = 0.12f)
    {
        var prefabText = GameResource.InstantiatePrefab(FloatingCombatTextPrefabPath, parent);
        if (prefabText != null)
        {
            prefabText.name = "FloatingCombatText";
            prefabText.transform.position = position;
            var floatingTextComponent = prefabText.GetComponent<FloatingCombatText>();
            if (floatingTextComponent != null)
            {
                floatingTextComponent.Configure(message, color, new Vector3(0f, 1.1f, 0f), 0.8f, sortingOrder, characterSize);
                return floatingTextComponent;
            }

            Object.Destroy(prefabText);
        }

        var root = new GameObject("FloatingCombatText", typeof(TextMesh), typeof(FloatingCombatText));
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var floatingText = root.GetComponent<FloatingCombatText>();
        floatingText.Configure(message, color, new Vector3(0f, 1.1f, 0f), 0.8f, sortingOrder, characterSize);
        return floatingText;
    }

    public static TransientSpriteEffect CreateSlashEffect(Transform parent, Vector3 position, Vector2 direction, Color color, bool emphasized)
    {
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        var drift = new Vector3(direction.x, direction.y, 0f).normalized * (emphasized ? 0.45f : 0.3f);
        var slashSprite = GeneratedArtLibrary.GetRuntimeArtSprite("VFX/Combat/vfx_hit_ink_slash");
        var prefabEffect = GameResource.InstantiatePrefab(SlashEffectPrefabPath, parent);
        if (prefabEffect != null)
        {
            prefabEffect.name = "CombatSlashEffect";
            prefabEffect.transform.position = position;
            var prefabTransientEffect = prefabEffect.GetComponent<TransientSpriteEffect>();
            if (prefabTransientEffect != null)
            {
                prefabTransientEffect.Configure(
                    slashSprite != null ? slashSprite : GameSpriteLibrary.WhiteSquareSprite,
                    slashSprite != null ? Color.white : color,
                    emphasized ? new Vector3(0.72f, 0.72f, 1f) : new Vector3(0.56f, 0.56f, 1f),
                    emphasized ? new Vector3(0.34f, 1.08f, 1f) : new Vector3(0.26f, 0.82f, 1f),
                    emphasized ? 0.24f : 0.18f,
                    emphasized ? 39 : 37,
                    angle,
                    drift,
                    emphasized ? 120f : 80f);
                return prefabTransientEffect;
            }

            Object.Destroy(prefabEffect);
        }

        var root = new GameObject("CombatSlashEffect", typeof(SpriteRenderer), typeof(TransientSpriteEffect));
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        var effect = root.GetComponent<TransientSpriteEffect>();
        effect.Configure(
            slashSprite != null ? slashSprite : GameSpriteLibrary.WhiteSquareSprite,
            slashSprite != null ? Color.white : color,
            emphasized ? new Vector3(0.72f, 0.72f, 1f) : new Vector3(0.56f, 0.56f, 1f),
            emphasized ? new Vector3(0.34f, 1.08f, 1f) : new Vector3(0.26f, 0.82f, 1f),
            emphasized ? 0.24f : 0.18f,
            emphasized ? 39 : 37,
            angle,
            drift,
            emphasized ? 120f : 80f);
        return effect;
    }

    public static TransientSpriteEffect CreateImpactBurst(Transform parent, Vector3 position, Color color, bool emphasized)
    {
        var prefabEffect = GameResource.InstantiatePrefab(ImpactEffectPrefabPath, parent);
        if (prefabEffect != null)
        {
            prefabEffect.name = "CombatImpactEffect";
            prefabEffect.transform.position = position;
            var prefabTransientEffect = prefabEffect.GetComponent<TransientSpriteEffect>();
            if (prefabTransientEffect != null)
            {
                prefabTransientEffect.Configure(
                    GameSpriteLibrary.WhiteSquareSprite,
                    color,
                    emphasized ? new Vector3(0.28f, 0.28f, 1f) : new Vector3(0.22f, 0.22f, 1f),
                    emphasized ? new Vector3(0.92f, 0.14f, 1f) : new Vector3(0.66f, 0.12f, 1f),
                    emphasized ? 0.2f : 0.16f,
                    emphasized ? 38 : 36,
                    45f,
                    new Vector3(0f, emphasized ? 0.18f : 0.12f, 0f),
                    emphasized ? 150f : 110f);
                return prefabTransientEffect;
            }

            Object.Destroy(prefabEffect);
        }

        var root = new GameObject("CombatImpactEffect", typeof(SpriteRenderer), typeof(TransientSpriteEffect));
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var effect = root.GetComponent<TransientSpriteEffect>();
        effect.Configure(
            GameSpriteLibrary.WhiteSquareSprite,
            color,
            emphasized ? new Vector3(0.28f, 0.28f, 1f) : new Vector3(0.22f, 0.22f, 1f),
            emphasized ? new Vector3(0.92f, 0.14f, 1f) : new Vector3(0.66f, 0.12f, 1f),
            emphasized ? 0.2f : 0.16f,
            emphasized ? 38 : 36,
            45f,
            new Vector3(0f, emphasized ? 0.18f : 0.12f, 0f),
            emphasized ? 150f : 110f);
        return effect;
    }

    private static GameObject CreateSprite(string name, Transform parent, Vector2 position, Vector2 size, Color tint, int sortingOrder, Sprite sprite = null)
    {
        var root = new GameObject(name, typeof(SpriteRenderer));
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(position.x, position.y, 0f);
        root.transform.localScale = new Vector3(size.x, size.y, 1f);

        var renderer = root.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite != null ? sprite : GameSpriteLibrary.WhiteSquareSprite;
        renderer.color = tint;
        renderer.sortingOrder = sortingOrder;
        return root;
    }

    private static void ConfigurePickupRenderer(GameObject root, Color tint, int sortingOrder, Vector3 scale)
    {
        if (root == null)
        {
            return;
        }

        var renderer = root.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = root.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = GameSpriteLibrary.WhiteSquareSprite;
        renderer.color = tint;
        renderer.sortingOrder = sortingOrder;
        root.transform.localScale = scale;
    }

    private static void ConfigurePickupCollider(GameObject root, float radius)
    {
        if (root == null)
        {
            return;
        }

        var collider = root.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = root.AddComponent<CircleCollider2D>();
        }

        collider.radius = radius;
        collider.isTrigger = true;
    }

    private static void ConfigurePlayerPhysics(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        var body = root.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            body = root.AddComponent<Rigidbody2D>();
        }

        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        var collider = root.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = root.AddComponent<CircleCollider2D>();
        }

        collider.radius = 0.44f;
        collider.isTrigger = false;
    }

    private static void ConfigureEnemyPhysics(GameObject root, bool elite)
    {
        if (root == null)
        {
            return;
        }

        var collider = root.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = root.AddComponent<CircleCollider2D>();
        }

        collider.radius = elite ? 0.56f : 0.48f;
        collider.isTrigger = true;
    }

    private static void EnsureAnimator(Transform visualRoot, string resourcePath)
    {
        if (visualRoot == null)
        {
            return;
        }

        var animator = visualRoot.GetComponent<Animator>();
        if (animator == null)
        {
            animator = visualRoot.gameObject.AddComponent<Animator>();
        }

        if (animator.runtimeAnimatorController == null && !string.IsNullOrWhiteSpace(resourcePath))
        {
            animator.runtimeAnimatorController = GameResource.Load<RuntimeAnimatorController>(resourcePath);
        }
    }
}
