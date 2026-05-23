using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class TutorialGameDataHubWindow : EditorWindow
{
    private enum IssueSeverity
    {
        Error,
        Warning
    }

    private sealed class DataEntry
    {
        public string Group;
        public string Label;
        public string Path;
        public Type AssetType;
        public Func<UnityEngine.Object, string> SummaryBuilder;
    }

    private sealed class ValidationIssue
    {
        public IssueSeverity Severity;
        public string Scope;
        public string Message;
        public string Path;
        public UnityEngine.Object Asset;
    }

    private static readonly DataEntry[] Entries =
    {
        new DataEntry
        {
            Group = "世界",
            Label = "区域总表",
            Path = "Assets/Resources/Data/WorldRegionDatabase.asset",
            AssetType = typeof(WorldRegionDatabaseAsset),
            SummaryBuilder = asset =>
            {
                var database = asset as WorldRegionDatabaseAsset;
                return database == null
                    ? "无效资源"
                    : "区域 " + GetLength(database.regions) + " / 境界名 " + GetLength(database.realmNames);
            }
        },
        new DataEntry
        {
            Group = "世界",
            Label = "房间事件表",
            Path = "Assets/Resources/Data/RoomEventTable.asset",
            AssetType = typeof(RoomEventTableAsset),
            SummaryBuilder = asset =>
            {
                var table = asset as RoomEventTableAsset;
                return table == null ? "无效资源" : "中间房文案 " + GetLength(table.roomCopies);
            }
        },
        new DataEntry
        {
            Group = "战斗",
            Label = "主角流派表",
            Path = "Assets/Resources/Data/HeroArchetypeDatabase.asset",
            AssetType = typeof(HeroArchetypeDatabaseAsset),
            SummaryBuilder = asset =>
            {
                var database = asset as HeroArchetypeDatabaseAsset;
                return database == null ? "无效资源" : "流派 " + GetLength(database.archetypes);
            }
        },
        new DataEntry
        {
            Group = "战斗",
            Label = "敌人原型表",
            Path = "Assets/Resources/Data/EnemyArchetypeDatabase.asset",
            AssetType = typeof(EnemyArchetypeDatabaseAsset),
            SummaryBuilder = asset =>
            {
                var database = asset as EnemyArchetypeDatabaseAsset;
                return database == null ? "无效资源" : "原型 " + GetLength(database.archetypes);
            }
        },
        new DataEntry
        {
            Group = "战斗",
            Label = "区域敌群表",
            Path = "Assets/Resources/Data/RegionEncounterDatabase.asset",
            AssetType = typeof(RegionEncounterDatabaseAsset),
            SummaryBuilder = asset =>
            {
                var database = asset as RegionEncounterDatabaseAsset;
                return database == null ? "无效资源" : "地域配置 " + GetLength(database.profiles);
            }
        },
        new DataEntry
        {
            Group = "战斗",
            Label = "掉落总表",
            Path = "Assets/Resources/Data/LootTable.asset",
            AssetType = typeof(LootTableAsset),
            SummaryBuilder = asset =>
            {
                var table = asset as LootTableAsset;
                return table == null
                    ? "无效资源"
                    : "阵营 " + GetLength(table.factionLoots) + " / 房间 " + GetLength(table.roomLoots) + " / 地域 " + GetLength(table.regionLoots);
            }
        },
        new DataEntry
        {
            Group = "养成",
            Label = "物品总表",
            Path = "Assets/Resources/Data/InventoryDatabase.asset",
            AssetType = typeof(InventoryDatabaseAsset),
            SummaryBuilder = asset =>
            {
                var database = asset as InventoryDatabaseAsset;
                return database == null ? "无效资源" : "物品 " + GetLength(database.items);
            }
        },
        new DataEntry
        {
            Group = "养成",
            Label = "委托总表",
            Path = "Assets/Resources/Data/TaskDatabase.asset",
            AssetType = typeof(TaskDatabaseAsset),
            SummaryBuilder = asset =>
            {
                var database = asset as TaskDatabaseAsset;
                return database == null ? "无效资源" : "委托 " + GetLength(database.tasks);
            }
        },
        new DataEntry
        {
            Group = "养成",
            Label = "洞府配方表",
            Path = "Assets/Resources/Data/WorkshopRecipeDatabase.asset",
            AssetType = typeof(WorkshopRecipeDatabaseAsset),
            SummaryBuilder = asset =>
            {
                var database = asset as WorkshopRecipeDatabaseAsset;
                return database == null ? "无效资源" : "配方 " + GetLength(database.recipes);
            }
        }
    };

    private Vector2 scrollPosition;
    private Vector2 validationScrollPosition;
    private readonly List<ValidationIssue> validationIssues = new List<ValidationIssue>();
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Main.unity",
        "Assets/Scenes/WorldMap.unity",
        "Assets/Scenes/Game.unity"
    };

    private static readonly string[] QuickAssetPaths =
    {
        "Assets/Resources/UI/MainMenu/MainMenuRoot.prefab",
        "Assets/Resources/UI/MainMenu/LoadSlotItem.prefab",
        "Assets/Resources/UI/MainMenu/CharacterSlotItem.prefab",
        "Assets/Resources/UI/MainMenu/ArchetypeCard.prefab",
        "Assets/Resources/Data"
    };

    [MenuItem("Tools/Cultivation/Open Data Hub")]
    private static void OpenWindow()
    {
        var window = GetWindow<TutorialGameDataHubWindow>("Data Hub");
        window.minSize = new Vector2(860f, 520f);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("TutorialGame 数据总览", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("集中查看和管理 Resources/Data 下的核心表。缺失资源可直接创建；现有运行时仍保留 fallback，不会因为单张表缺失而完全失效。", MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("选中 Data 目录", GUILayout.Width(120f)))
            {
                SelectDataFolder();
            }

            if (GUILayout.Button("刷新", GUILayout.Width(80f)))
            {
                AssetDatabase.Refresh();
                RunValidation();
                Repaint();
            }

            if (GUILayout.Button("运行校验", GUILayout.Width(100f)))
            {
                RunValidation();
            }
        }

        EditorGUILayout.Space(8f);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawQuickNavigation();
        DrawGroup("世界");
        DrawGroup("战斗");
        DrawGroup("养成");
        DrawValidationPanel();
        EditorGUILayout.EndScrollView();
    }

    private void DrawQuickNavigation()
    {
        EditorGUILayout.LabelField("快捷跳转", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("场景", EditorStyles.miniBoldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawSceneButton("Main", ScenePaths[0]);
            DrawSceneButton("WorldMap", ScenePaths[1]);
            DrawSceneButton("Game", ScenePaths[2]);
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("关键资源", EditorStyles.miniBoldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawAssetButton("主菜单 Root", QuickAssetPaths[0]);
            DrawAssetButton("加载档位项", QuickAssetPaths[1]);
            DrawAssetButton("新建档位项", QuickAssetPaths[2]);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawAssetButton("职业卡", QuickAssetPaths[3]);
            DrawAssetButton("Data 目录", QuickAssetPaths[4]);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(8f);
    }

    private void DrawGroup(string groupName)
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField(groupName, EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        for (var i = 0; i < Entries.Length; i++)
        {
            if (Entries[i].Group != groupName)
            {
                continue;
            }

            DrawEntry(Entries[i]);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawEntry(DataEntry entry)
    {
        var asset = AssetDatabase.LoadAssetAtPath(entry.Path, entry.AssetType);
        var exists = asset != null;

        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(entry.Label, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                var statusStyle = new GUIStyle(EditorStyles.miniBoldLabel)
                {
                    normal =
                    {
                        textColor = exists ? new Color(0.2f, 0.7f, 0.25f) : new Color(0.75f, 0.25f, 0.22f)
                    }
                };
                GUILayout.Label(exists ? "已加载" : "缺失", statusStyle, GUILayout.Width(48f));
            }

            EditorGUILayout.LabelField(entry.Path, EditorStyles.miniLabel);
            EditorGUILayout.LabelField(exists ? entry.SummaryBuilder(asset) : "当前资源不存在；运行时会落回代码内置表。", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(4f);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = exists;
                if (GUILayout.Button("选中", GUILayout.Width(72f)))
                {
                    Selection.activeObject = asset;
                }

                if (GUILayout.Button("Ping", GUILayout.Width(72f)))
                {
                    EditorGUIUtility.PingObject(asset);
                }

                if (GUILayout.Button("在 Inspector 打开", GUILayout.Width(128f)))
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }

                GUI.enabled = !exists;
                if (GUILayout.Button("创建缺失资源", GUILayout.Width(110f)))
                {
                    CreateMissingAsset(entry);
                }

                GUI.enabled = true;
            }
        }
    }

    private void DrawValidationPanel()
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("数据校验", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        if (validationIssues.Count == 0)
        {
            EditorGUILayout.HelpBox("当前没有发现校验问题。点击“运行校验”可重新扫描所有数据表。", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        var errorCount = 0;
        var warningCount = 0;
        for (var i = 0; i < validationIssues.Count; i++)
        {
            if (validationIssues[i].Severity == IssueSeverity.Error)
            {
                errorCount++;
            }
            else
            {
                warningCount++;
            }
        }

        EditorGUILayout.HelpBox("发现 " + errorCount + " 个错误，" + warningCount + " 个警告。", errorCount > 0 ? MessageType.Error : MessageType.Warning);
        validationScrollPosition = EditorGUILayout.BeginScrollView(validationScrollPosition, GUILayout.MinHeight(220f));

        for (var i = 0; i < validationIssues.Count; i++)
        {
            DrawValidationIssue(validationIssues[i]);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawValidationIssue(ValidationIssue issue)
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var severityLabel = issue.Severity == IssueSeverity.Error ? "错误" : "警告";
                var severityStyle = new GUIStyle(EditorStyles.miniBoldLabel)
                {
                    normal =
                    {
                        textColor = issue.Severity == IssueSeverity.Error
                            ? new Color(0.82f, 0.22f, 0.18f)
                            : new Color(0.82f, 0.58f, 0.12f)
                    }
                };
                GUILayout.Label(severityLabel, severityStyle, GUILayout.Width(32f));
                EditorGUILayout.LabelField(issue.Scope, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (!string.IsNullOrWhiteSpace(issue.Path))
                {
                    GUILayout.Label(issue.Path, EditorStyles.miniLabel, GUILayout.Width(320f));
                }
            }

            EditorGUILayout.LabelField(issue.Message, EditorStyles.wordWrappedMiniLabel);

            if (issue.Asset != null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("选中资源", GUILayout.Width(88f)))
                    {
                        Selection.activeObject = issue.Asset;
                    }

                    if (GUILayout.Button("Ping", GUILayout.Width(60f)))
                    {
                        EditorGUIUtility.PingObject(issue.Asset);
                    }
                }
            }
        }
    }

    private void CreateMissingAsset(DataEntry entry)
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Data");

        var asset = ScriptableObject.CreateInstance(entry.AssetType);
        AssetDatabase.CreateAsset(asset, entry.Path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
        RunValidation();
    }

    private void DrawSceneButton(string label, string scenePath)
    {
        if (GUILayout.Button(label, GUILayout.Width(120f)))
        {
            OpenScene(scenePath);
        }
    }

    private void DrawAssetButton(string label, string assetPath)
    {
        if (GUILayout.Button(label, GUILayout.Width(120f)))
        {
            SelectAsset(assetPath);
        }
    }

    private static void OpenScene(string scenePath)
    {
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogWarning("Scene not found: " + scenePath);
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }

    private static void SelectAsset(string assetPath)
    {
        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
        if (asset == null)
        {
            Debug.LogWarning("Asset not found: " + assetPath);
            return;
        }

        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }

    private static void SelectDataFolder()
    {
        SelectAsset("Assets/Resources/Data");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        var parts = path.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static int GetLength(Array array)
    {
        return array != null ? array.Length : 0;
    }

    private void OnEnable()
    {
        RunValidation();
    }

    private void RunValidation()
    {
        validationIssues.Clear();

        var inventory = AssetDatabase.LoadAssetAtPath<InventoryDatabaseAsset>("Assets/Resources/Data/InventoryDatabase.asset");
        var world = AssetDatabase.LoadAssetAtPath<WorldRegionDatabaseAsset>("Assets/Resources/Data/WorldRegionDatabase.asset");
        var hero = AssetDatabase.LoadAssetAtPath<HeroArchetypeDatabaseAsset>("Assets/Resources/Data/HeroArchetypeDatabase.asset");
        var enemy = AssetDatabase.LoadAssetAtPath<EnemyArchetypeDatabaseAsset>("Assets/Resources/Data/EnemyArchetypeDatabase.asset");
        var encounter = AssetDatabase.LoadAssetAtPath<RegionEncounterDatabaseAsset>("Assets/Resources/Data/RegionEncounterDatabase.asset");
        var loot = AssetDatabase.LoadAssetAtPath<LootTableAsset>("Assets/Resources/Data/LootTable.asset");
        var tasks = AssetDatabase.LoadAssetAtPath<TaskDatabaseAsset>("Assets/Resources/Data/TaskDatabase.asset");
        var workshop = AssetDatabase.LoadAssetAtPath<WorkshopRecipeDatabaseAsset>("Assets/Resources/Data/WorkshopRecipeDatabase.asset");
        var roomEvents = AssetDatabase.LoadAssetAtPath<RoomEventTableAsset>("Assets/Resources/Data/RoomEventTable.asset");

        var itemIds = ValidateInventory(inventory);
        var regionIds = ValidateWorld(world);
        ValidateHero(hero);
        ValidateEnemies(enemy);
        ValidateEncounters(encounter, regionIds);
        ValidateLoot(loot, itemIds, regionIds);
        ValidateTasks(tasks, itemIds, regionIds);
        ValidateWorkshop(workshop, itemIds);
        ValidateRoomEvents(roomEvents);

        Repaint();
    }

    private HashSet<string> ValidateInventory(InventoryDatabaseAsset database)
    {
        var itemIds = new HashSet<string>();
        if (database == null)
        {
            AddIssue(IssueSeverity.Error, "物品总表", "缺少 InventoryDatabase.asset。", "Assets/Resources/Data/InventoryDatabase.asset", null);
            return itemIds;
        }

        if (database.items == null || database.items.Length == 0)
        {
            AddIssue(IssueSeverity.Warning, "物品总表", "当前没有任何物品记录。", AssetDatabase.GetAssetPath(database), database);
            return itemIds;
        }

        for (var i = 0; i < database.items.Length; i++)
        {
            var item = database.items[i];
            if (item == null)
            {
                AddIssue(IssueSeverity.Warning, "物品总表", "第 " + i + " 条物品记录为空。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.id))
            {
                AddIssue(IssueSeverity.Error, "物品总表", "第 " + i + " 条物品缺少 id。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (!itemIds.Add(item.id))
            {
                AddIssue(IssueSeverity.Error, "物品总表", "物品 id 重复：" + item.id, AssetDatabase.GetAssetPath(database), database);
            }

            if (string.IsNullOrWhiteSpace(item.displayName))
            {
                AddIssue(IssueSeverity.Warning, "物品总表", "物品 " + item.id + " 缺少展示名。", AssetDatabase.GetAssetPath(database), database);
            }
        }

        return itemIds;
    }

    private HashSet<string> ValidateWorld(WorldRegionDatabaseAsset database)
    {
        var regionIds = new HashSet<string>();
        if (database == null)
        {
            AddIssue(IssueSeverity.Error, "区域总表", "缺少 WorldRegionDatabase.asset。", "Assets/Resources/Data/WorldRegionDatabase.asset", null);
            return regionIds;
        }

        if (database.regions == null || database.regions.Length == 0)
        {
            AddIssue(IssueSeverity.Error, "区域总表", "当前没有任何区域定义。", AssetDatabase.GetAssetPath(database), database);
            return regionIds;
        }

        for (var i = 0; i < database.regions.Length; i++)
        {
            var region = database.regions[i];
            if (region == null)
            {
                AddIssue(IssueSeverity.Warning, "区域总表", "第 " + i + " 条区域记录为空。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (string.IsNullOrWhiteSpace(region.Id))
            {
                AddIssue(IssueSeverity.Error, "区域总表", "第 " + i + " 条区域缺少 Id。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (!regionIds.Add(region.Id))
            {
                AddIssue(IssueSeverity.Error, "区域总表", "区域 Id 重复：" + region.Id, AssetDatabase.GetAssetPath(database), database);
            }

            if (string.IsNullOrWhiteSpace(region.DisplayName))
            {
                AddIssue(IssueSeverity.Warning, "区域总表", "区域 " + region.Id + " 缺少显示名。", AssetDatabase.GetAssetPath(database), database);
            }
        }

        if (string.IsNullOrWhiteSpace(database.startingRegionId))
        {
            AddIssue(IssueSeverity.Error, "区域总表", "startingRegionId 为空。", AssetDatabase.GetAssetPath(database), database);
        }
        else if (!regionIds.Contains(database.startingRegionId))
        {
            AddIssue(IssueSeverity.Error, "区域总表", "startingRegionId 指向不存在的区域：" + database.startingRegionId, AssetDatabase.GetAssetPath(database), database);
        }

        for (var i = 0; i < database.regions.Length; i++)
        {
            var region = database.regions[i];
            if (region == null || region.UnlockRegionIds == null)
            {
                continue;
            }

            for (var unlockIndex = 0; unlockIndex < region.UnlockRegionIds.Length; unlockIndex++)
            {
                var unlockId = region.UnlockRegionIds[unlockIndex];
                if (!string.IsNullOrWhiteSpace(unlockId) && !regionIds.Contains(unlockId))
                {
                    AddIssue(IssueSeverity.Error, "区域总表", "区域 " + region.Id + " 的解锁目标不存在：" + unlockId, AssetDatabase.GetAssetPath(database), database);
                }
            }
        }

        return regionIds;
    }

    private void ValidateHero(HeroArchetypeDatabaseAsset database)
    {
        if (database == null)
        {
            AddIssue(IssueSeverity.Error, "主角流派表", "缺少 HeroArchetypeDatabase.asset。", "Assets/Resources/Data/HeroArchetypeDatabase.asset", null);
            return;
        }

        var ids = new HashSet<string>();
        if (database.archetypes == null || database.archetypes.Length == 0)
        {
            AddIssue(IssueSeverity.Error, "主角流派表", "当前没有任何流派定义。", AssetDatabase.GetAssetPath(database), database);
            return;
        }

        for (var i = 0; i < database.archetypes.Length; i++)
        {
            var archetype = database.archetypes[i];
            if (archetype == null)
            {
                AddIssue(IssueSeverity.Warning, "主角流派表", "第 " + i + " 条流派记录为空。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (string.IsNullOrWhiteSpace(archetype.id))
            {
                AddIssue(IssueSeverity.Error, "主角流派表", "第 " + i + " 条流派缺少 id。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (!ids.Add(archetype.id))
            {
                AddIssue(IssueSeverity.Error, "主角流派表", "流派 id 重复：" + archetype.id, AssetDatabase.GetAssetPath(database), database);
            }

            if (string.IsNullOrWhiteSpace(archetype.displayName))
            {
                AddIssue(IssueSeverity.Warning, "主角流派表", "流派 " + archetype.id + " 缺少 displayName。", AssetDatabase.GetAssetPath(database), database);
            }

            if (archetype.skills == null || archetype.skills.Length == 0)
            {
                AddIssue(IssueSeverity.Error, "主角流派表", "流派 " + archetype.id + " 没有配置任何技能。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            var skillIds = new HashSet<string>();
            for (var skillIndex = 0; skillIndex < archetype.skills.Length; skillIndex++)
            {
                var skill = archetype.skills[skillIndex];
                if (skill == null || string.IsNullOrWhiteSpace(skill.id))
                {
                    AddIssue(IssueSeverity.Error, "主角流派表", "流派 " + archetype.id + " 存在空技能或缺少技能 id。", AssetDatabase.GetAssetPath(database), database);
                    continue;
                }

                if (!skillIds.Add(skill.id))
                {
                    AddIssue(IssueSeverity.Error, "主角流派表", "流派 " + archetype.id + " 的技能 id 重复：" + skill.id, AssetDatabase.GetAssetPath(database), database);
                }
            }
        }
    }

    private void ValidateEnemies(EnemyArchetypeDatabaseAsset database)
    {
        if (database == null)
        {
            AddIssue(IssueSeverity.Error, "敌人原型表", "缺少 EnemyArchetypeDatabase.asset。", "Assets/Resources/Data/EnemyArchetypeDatabase.asset", null);
            return;
        }

        var ids = new HashSet<string>();
        if (database.archetypes == null || database.archetypes.Length == 0)
        {
            AddIssue(IssueSeverity.Error, "敌人原型表", "当前没有任何敌人原型。", AssetDatabase.GetAssetPath(database), database);
            return;
        }

        for (var i = 0; i < database.archetypes.Length; i++)
        {
            var archetype = database.archetypes[i];
            if (archetype == null)
            {
                AddIssue(IssueSeverity.Warning, "敌人原型表", "第 " + i + " 条敌人原型为空。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (string.IsNullOrWhiteSpace(archetype.id))
            {
                AddIssue(IssueSeverity.Error, "敌人原型表", "第 " + i + " 条敌人原型缺少 id。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (!ids.Add(archetype.id))
            {
                AddIssue(IssueSeverity.Error, "敌人原型表", "敌人原型 id 重复：" + archetype.id, AssetDatabase.GetAssetPath(database), database);
            }

            if (!IsValidFaction(archetype.faction))
            {
                AddIssue(IssueSeverity.Error, "敌人原型表", "敌人原型 " + archetype.id + " 的 faction 超出范围：" + archetype.faction, AssetDatabase.GetAssetPath(database), database);
            }

            if (string.IsNullOrWhiteSpace(archetype.displayName) || string.IsNullOrWhiteSpace(archetype.techniqueName))
            {
                AddIssue(IssueSeverity.Warning, "敌人原型表", "敌人原型 " + archetype.id + " 缺少展示名或招式名。", AssetDatabase.GetAssetPath(database), database);
            }

            if (archetype.allowedRoomKinds != null)
            {
                for (var roomIndex = 0; roomIndex < archetype.allowedRoomKinds.Length; roomIndex++)
                {
                    if (!IsValidRoomKind(archetype.allowedRoomKinds[roomIndex]))
                    {
                        AddIssue(IssueSeverity.Error, "敌人原型表", "敌人原型 " + archetype.id + " 使用了无效房间种类：" + archetype.allowedRoomKinds[roomIndex], AssetDatabase.GetAssetPath(database), database);
                    }
                }
            }
        }
    }

    private void ValidateEncounters(RegionEncounterDatabaseAsset database, HashSet<string> regionIds)
    {
        if (database == null)
        {
            AddIssue(IssueSeverity.Error, "区域敌群表", "缺少 RegionEncounterDatabase.asset。", "Assets/Resources/Data/RegionEncounterDatabase.asset", null);
            return;
        }

        var ids = new HashSet<string>();
        if (database.profiles == null || database.profiles.Length == 0)
        {
            AddIssue(IssueSeverity.Warning, "区域敌群表", "当前没有任何区域敌群配置。", AssetDatabase.GetAssetPath(database), database);
            return;
        }

        for (var i = 0; i < database.profiles.Length; i++)
        {
            var profile = database.profiles[i];
            if (profile == null || string.IsNullOrWhiteSpace(profile.regionId))
            {
                AddIssue(IssueSeverity.Error, "区域敌群表", "第 " + i + " 条敌群配置缺少 regionId。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (!ids.Add(profile.regionId))
            {
                AddIssue(IssueSeverity.Error, "区域敌群表", "区域敌群配置重复：" + profile.regionId, AssetDatabase.GetAssetPath(database), database);
            }

            if (regionIds.Count > 0 && !regionIds.Contains(profile.regionId))
            {
                AddIssue(IssueSeverity.Error, "区域敌群表", "区域敌群引用了不存在的区域：" + profile.regionId, AssetDatabase.GetAssetPath(database), database);
            }

            if (profile.factions == null || profile.factions.Length == 0)
            {
                AddIssue(IssueSeverity.Warning, "区域敌群表", "区域 " + profile.regionId + " 没有配置任何阵营。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            for (var factionIndex = 0; factionIndex < profile.factions.Length; factionIndex++)
            {
                if (!IsValidFaction(profile.factions[factionIndex]))
                {
                    AddIssue(IssueSeverity.Error, "区域敌群表", "区域 " + profile.regionId + " 使用了无效 faction：" + profile.factions[factionIndex], AssetDatabase.GetAssetPath(database), database);
                }
            }
        }
    }

    private void ValidateLoot(LootTableAsset table, HashSet<string> itemIds, HashSet<string> regionIds)
    {
        if (table == null)
        {
            AddIssue(IssueSeverity.Error, "掉落总表", "缺少 LootTable.asset。", "Assets/Resources/Data/LootTable.asset", null);
            return;
        }

        if (table.factionLoots != null)
        {
            for (var i = 0; i < table.factionLoots.Length; i++)
            {
                var record = table.factionLoots[i];
                if (record == null)
                {
                    continue;
                }

                if (!IsValidFaction(record.faction))
                {
                    AddIssue(IssueSeverity.Error, "掉落总表", "阵营掉落记录使用了无效 faction：" + record.faction, AssetDatabase.GetAssetPath(table), table);
                }

                ValidateDropList("掉落总表", "阵营 faction " + record.faction, record.drops, itemIds, table);
            }
        }

        if (table.roomLoots != null)
        {
            for (var i = 0; i < table.roomLoots.Length; i++)
            {
                var record = table.roomLoots[i];
                if (record == null)
                {
                    continue;
                }

                if (!IsValidRoomKind(record.roomKind))
                {
                    AddIssue(IssueSeverity.Error, "掉落总表", "房间掉落记录使用了无效 roomKind：" + record.roomKind, AssetDatabase.GetAssetPath(table), table);
                }

                ValidateDropList("掉落总表", "房间 kind " + record.roomKind, record.drops, itemIds, table);
            }
        }

        if (table.regionLoots != null)
        {
            for (var i = 0; i < table.regionLoots.Length; i++)
            {
                var record = table.regionLoots[i];
                if (record == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(record.regionId))
                {
                    AddIssue(IssueSeverity.Error, "掉落总表", "存在缺少 regionId 的地域掉落记录。", AssetDatabase.GetAssetPath(table), table);
                    continue;
                }

                if (regionIds.Count > 0 && !regionIds.Contains(record.regionId))
                {
                    AddIssue(IssueSeverity.Error, "掉落总表", "地域掉落引用了不存在的区域：" + record.regionId, AssetDatabase.GetAssetPath(table), table);
                }

                ValidateItemReference("掉落总表", "地域 " + record.regionId + " 的 rareItemId", record.rareItemId, itemIds, table);
                ValidateItemReference("掉落总表", "地域 " + record.regionId + " 的 herbItemId", record.herbItemId, itemIds, table);
                ValidateDropList("掉落总表", "地域 " + record.regionId + " 的通关掉落", record.clearDrops, itemIds, table);
            }
        }
    }

    private void ValidateTasks(TaskDatabaseAsset database, HashSet<string> itemIds, HashSet<string> regionIds)
    {
        if (database == null)
        {
            AddIssue(IssueSeverity.Error, "委托总表", "缺少 TaskDatabase.asset。", "Assets/Resources/Data/TaskDatabase.asset", null);
            return;
        }

        var ids = new HashSet<string>();
        if (database.tasks == null || database.tasks.Length == 0)
        {
            AddIssue(IssueSeverity.Warning, "委托总表", "当前没有任何委托。", AssetDatabase.GetAssetPath(database), database);
            return;
        }

        for (var i = 0; i < database.tasks.Length; i++)
        {
            var task = database.tasks[i];
            if (task == null || string.IsNullOrWhiteSpace(task.Id))
            {
                AddIssue(IssueSeverity.Error, "委托总表", "第 " + i + " 条委托缺少 Id。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (!ids.Add(task.Id))
            {
                AddIssue(IssueSeverity.Error, "委托总表", "委托 Id 重复：" + task.Id, AssetDatabase.GetAssetPath(database), database);
            }

            if (!string.IsNullOrWhiteSpace(task.UnlockRegionId) && regionIds.Count > 0 && !regionIds.Contains(task.UnlockRegionId))
            {
                AddIssue(IssueSeverity.Error, "委托总表", "委托 " + task.Id + " 的 UnlockRegionId 不存在：" + task.UnlockRegionId, AssetDatabase.GetAssetPath(database), database);
            }

            if (task.ObjectiveType == TaskObjectiveType.CollectItem)
            {
                ValidateItemReference("委托总表", "委托 " + task.Id + " 的 TargetItemId", task.TargetItemId, itemIds, database);
            }
            else if (task.ObjectiveType == TaskObjectiveType.ClearRegion && regionIds.Count > 0 && !string.IsNullOrWhiteSpace(task.TargetRegionId) && !regionIds.Contains(task.TargetRegionId))
            {
                AddIssue(IssueSeverity.Error, "委托总表", "委托 " + task.Id + " 的 TargetRegionId 不存在：" + task.TargetRegionId, AssetDatabase.GetAssetPath(database), database);
            }

            ValidateItemStacks("委托总表", "委托 " + task.Id + " 的奖励物品", task.RewardItems, itemIds, database);
        }
    }

    private void ValidateWorkshop(WorkshopRecipeDatabaseAsset database, HashSet<string> itemIds)
    {
        if (database == null)
        {
            AddIssue(IssueSeverity.Error, "洞府配方表", "缺少 WorkshopRecipeDatabase.asset。", "Assets/Resources/Data/WorkshopRecipeDatabase.asset", null);
            return;
        }

        var ids = new HashSet<string>();
        if (database.recipes == null || database.recipes.Length == 0)
        {
            AddIssue(IssueSeverity.Warning, "洞府配方表", "当前没有任何配方。", AssetDatabase.GetAssetPath(database), database);
            return;
        }

        for (var i = 0; i < database.recipes.Length; i++)
        {
            var recipe = database.recipes[i];
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.Id))
            {
                AddIssue(IssueSeverity.Error, "洞府配方表", "第 " + i + " 条配方缺少 Id。", AssetDatabase.GetAssetPath(database), database);
                continue;
            }

            if (!ids.Add(recipe.Id))
            {
                AddIssue(IssueSeverity.Error, "洞府配方表", "配方 Id 重复：" + recipe.Id, AssetDatabase.GetAssetPath(database), database);
            }

            ValidateItemStacks("洞府配方表", "配方 " + recipe.Id + " 的成本物品", recipe.CostItems, itemIds, database);
        }
    }

    private void ValidateRoomEvents(RoomEventTableAsset table)
    {
        if (table == null)
        {
            AddIssue(IssueSeverity.Error, "房间事件表", "缺少 RoomEventTable.asset。", "Assets/Resources/Data/RoomEventTable.asset", null);
            return;
        }

        var roomKinds = new HashSet<int>();
        if (table.roomCopies == null)
        {
            return;
        }

        for (var i = 0; i < table.roomCopies.Length; i++)
        {
            var copy = table.roomCopies[i];
            if (copy == null)
            {
                continue;
            }

            if (!IsValidRoomKind(copy.roomKind))
            {
                AddIssue(IssueSeverity.Error, "房间事件表", "存在无效 roomKind：" + copy.roomKind, AssetDatabase.GetAssetPath(table), table);
            }

            if (!roomKinds.Add(copy.roomKind))
            {
                AddIssue(IssueSeverity.Warning, "房间事件表", "roomKind 重复：" + copy.roomKind, AssetDatabase.GetAssetPath(table), table);
            }
        }
    }

    private void ValidateDropList(string scope, string context, LootDropRecord[] drops, HashSet<string> itemIds, UnityEngine.Object asset)
    {
        if (drops == null)
        {
            return;
        }

        for (var i = 0; i < drops.Length; i++)
        {
            var drop = drops[i];
            if (drop == null)
            {
                AddIssue(IssueSeverity.Warning, scope, context + " 存在空掉落记录。", AssetDatabase.GetAssetPath(asset), asset);
                continue;
            }

            ValidateItemReference(scope, context + " 的掉落物品", drop.itemId, itemIds, asset);
        }
    }

    private void ValidateItemStacks(string scope, string context, SaveItemStack[] stacks, HashSet<string> itemIds, UnityEngine.Object asset)
    {
        if (stacks == null)
        {
            return;
        }

        for (var i = 0; i < stacks.Length; i++)
        {
            var stack = stacks[i];
            if (stack == null)
            {
                AddIssue(IssueSeverity.Warning, scope, context + " 存在空物品条目。", AssetDatabase.GetAssetPath(asset), asset);
                continue;
            }

            ValidateItemReference(scope, context, stack.itemId, itemIds, asset);
        }
    }

    private void ValidateItemReference(string scope, string context, string itemId, HashSet<string> itemIds, UnityEngine.Object asset)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            AddIssue(IssueSeverity.Error, scope, context + " 为空。", AssetDatabase.GetAssetPath(asset), asset);
            return;
        }

        if (itemIds.Count > 0 && !itemIds.Contains(itemId))
        {
            AddIssue(IssueSeverity.Error, scope, context + " 引用了不存在的物品 id：" + itemId, AssetDatabase.GetAssetPath(asset), asset);
        }
    }

    private void AddIssue(IssueSeverity severity, string scope, string message, string path, UnityEngine.Object asset)
    {
        validationIssues.Add(new ValidationIssue
        {
            Severity = severity,
            Scope = scope,
            Message = message,
            Path = path,
            Asset = asset
        });
    }

    private static bool IsValidFaction(int faction)
    {
        return faction >= 0 && faction <= (int)ExpeditionEnemyFaction.CorpsePuppet;
    }

    private static bool IsValidRoomKind(int roomKind)
    {
        return roomKind >= 0 && roomKind <= (int)ExpeditionRoomKind.Boss;
    }
}
