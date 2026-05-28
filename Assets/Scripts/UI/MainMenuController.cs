using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed partial class MainMenuController : CultivationUIPanel
{
    public Text titleText;
    public Text subtitleText;
    public Text descriptionText;
    public Text statusText;
    public Text infoFlavorText;
    public Text recentSaveText;

    public Button newGameButton;
    public Button continueButton;
    public Button loadButton;
    public Button settingsButton;
    public Button quitButton;

    private readonly List<HeroArchetypeOption> archetypes = new List<HeroArchetypeOption>();

    private MainMenuConfig config;
    private RectTransform rootRect;
    private RectTransform menuPanelRect;
    private RectTransform infoPanelRect;
    private RectTransform footerPanelRect;
    private bool isInitialized;
    private int selectedLoadSlotIndex = -1;
    private int selectedCharacterSlotIndex = -1;
    private int selectedArchetypeIndex;
    private int lastLayoutWidth = -1;
    private int lastLayoutHeight = -1;
    private float musicVolume;
    private float sfxVolume;
    private float voiceVolume;
    private bool isFullscreen;
    private string pendingHeroName = string.Empty;

    private sealed class LoadDetailSnapshot
    {
        public string Title;
        public string Body;
        public string Action;
        public bool CanLoad;
        public bool CanDelete;
    }

    public void Initialize(MainMenuConfig newConfig)
    {
        config = newConfig;
        EnsureArchetypes();

        if (!isInitialized)
        {
            ResolveLayoutReferences();
            WireButtons();
            isInitialized = true;
        }

        LoadPreferences();
        ApplyCopy();
        RefreshResponsiveLayout(true);
        RefreshAll();
        ResetUiStateMachine(MainMenuUiState.Home);
        SetStatus("主界面已就绪");
    }

    private void Update()
    {
        RefreshResponsiveLayout(false);
        if (!isInitialized)
        {
            return;
        }

        uiStateMachine.Update();
    }

    private void EnsureArchetypes()
    {
        if (archetypes.Count > 0)
        {
            return;
        }

        var database = LoadResource<HeroArchetypeDatabaseAsset>("Data/HeroArchetypeDatabase");
        if (database != null && database.archetypes != null && database.archetypes.Length > 0)
        {
            for (var i = 0; i < database.archetypes.Length; i++)
            {
                var record = database.archetypes[i];
                if (record == null || string.IsNullOrWhiteSpace(record.id))
                {
                    continue;
                }

                archetypes.Add(new HeroArchetypeOption
                {
                    id = record.id,
                    name = string.IsNullOrWhiteSpace(record.displayName) ? record.id : record.displayName,
                    origin = record.origin,
                    specialty = record.specialty,
                    description = record.description,
                    trait = record.trait,
                    recommendation = record.recommendation,
                    defaultHeroName = string.IsNullOrWhiteSpace(record.defaultHeroName) ? "无名修士" : record.defaultHeroName,
                    portraitSprite = record.portraitImage != null ? record.portraitImage : GeneratedArtLibrary.GetHeroPortrait(record.id)
                });
            }
        }

        if (archetypes.Count == 0)
        {
            archetypes.Add(new HeroArchetypeOption
            {
                id = "sword",
                name = "流云剑修",
                origin = "寒山剑脉",
                specialty = "快节奏近战",
                description = "身法飘逸，开局偏向剑诀与机动。",
                trait = "先天气脉偏金水，擅长闪避与连击。",
                recommendation = "适合喜欢主动出招和高机动战斗的玩家。",
                defaultHeroName = "顾长风",
                portraitSprite = LoadArchetypePortrait("sword")
            });

            archetypes.Add(new HeroArchetypeOption
            {
                id = "alchemist",
                name = "离火丹修",
                origin = "丹霞旧宗",
                specialty = "炼丹与养成",
                description = "炼丹稳健，适合经营资源与持续成长。",
                trait = "火木相生，丹炉成功率与回复能力更强。",
                recommendation = "适合喜欢资源运营、炼药与持久战的玩家。",
                defaultHeroName = "沈知微",
                portraitSprite = LoadArchetypePortrait("alchemist")
            });

            archetypes.Add(new HeroArchetypeOption
            {
                id = "wanderer",
                name = "幽谷散修",
                origin = "无门散人",
                specialty = "探索与机缘",
                description = "机缘偏多，适合探索支线与随机际遇。",
                trait = "五行平衡，早期容错更高，发展路线更自由。",
                recommendation = "适合喜欢探索、支线与事件分支的玩家。",
                defaultHeroName = "林清玄",
                portraitSprite = LoadArchetypePortrait("wanderer")
            });
        }
    }

    private Sprite LoadArchetypePortrait(string archetypeId)
    {
        var database = LoadResource<HeroArchetypeDatabaseAsset>("Data/HeroArchetypeDatabase");
        if (database == null || database.archetypes == null)
        {
            return null;
        }

        for (var i = 0; i < database.archetypes.Length; i++)
        {
            var record = database.archetypes[i];
            if (record != null && record.id == archetypeId)
            {
                return record.portraitImage != null ? record.portraitImage : GeneratedArtLibrary.GetHeroPortrait(archetypeId);
            }
        }

        return GeneratedArtLibrary.GetHeroPortrait(archetypeId);
    }

    private void WireButtons()
    {
        BindButton(newGameButton, OpenCharacterPanel);
        BindButton(continueButton, ContinueGame);
        BindButton(loadButton, OpenLoadPanel);
        BindButton(settingsButton, OpenSettings);
        BindButton(quitButton, QuitGame);

        CultivationTooltipBinder.Bind(newGameButton, "新游戏", "创建新的修士档案，选择流派、姓名与存档位后进入修行。");
        CultivationTooltipBinder.Bind(continueButton, "继续游戏", "直接载入当前活跃存档，回到上次离开的流程。");
        CultivationTooltipBinder.Bind(loadButton, "加载存档", "打开全部档位，查看并手动载入已有修士。");
        CultivationTooltipBinder.Bind(settingsButton, "设置", "调整音乐、音效、语音和显示模式。");
        CultivationTooltipBinder.Bind(quitButton, "退出游戏", "结束当前游戏进程并返回桌面。");
    }

    private void ApplyCopy()
    {
        if (titleText != null) titleText.text = config.Title;
        if (subtitleText != null) subtitleText.text = config.Subtitle;
        if (descriptionText != null) descriptionText.text = config.Description;
    }

    private void LoadPreferences()
    {
        musicVolume = GetMusicVolume();
        sfxVolume = GetSfxVolume();
        voiceVolume = GetVoiceVolume();
        isFullscreen = IsFullscreen();
        selectedArchetypeIndex = Mathf.Clamp(CultivationLocalSaveStore.LoadSelectedArchetype(), 0, archetypes.Count - 1);
        selectedLoadSlotIndex = CultivationLocalSaveStore.GetPreferredLoadSlot();
        selectedCharacterSlotIndex = CultivationLocalSaveStore.GetPreferredNewGameSlot();
        pendingHeroName = ResolveDefaultHeroName();

        ApplyUserSettings();
    }

    private void RefreshAll()
    {
        RefreshMainButtons();
        RefreshInfoPanel();
    }

    private void RefreshMainButtons()
    {
        if (continueButton != null)
        {
            continueButton.interactable = CultivationLocalSaveStore.HasAnySave();
        }
    }

    public MainMenuSettingsSnapshot BuildSettingsSnapshot()
    {
        return new MainMenuSettingsSnapshot
        {
            MusicVolume = musicVolume,
            SfxVolume = sfxVolume,
            VoiceVolume = voiceVolume,
            IsFullscreen = isFullscreen
        };
    }

    private void RefreshInfoPanel()
    {
        if (infoFlavorText != null)
        {
            infoFlavorText.text = "主界面改成 prefab 驱动后，结构会更稳定：窗口改为独立面板后，后续可以直接按 prefab 维护层级与显示。";
        }

        if (recentSaveText == null)
        {
            return;
        }

        if (CultivationLocalSaveStore.TryGetCurrentSave(out var slotIndex, out var data))
        {
            recentSaveText.text = "当前活跃档位：第 " + (slotIndex + 1) + " 档\n" +
                                  data.heroName + " / " + data.archetypeName + "\n" +
                                  data.realm + " · " + data.location + " · " + data.sectName + "\n" +
                                  "灵石：" + data.wallet.ToDisplayString() + " · " + CultivationLoadoutLibrary.BuildCompactProgressSummary(data) + "\n" +
                                  "上次游历：" + data.lastPlayed;
        }
        else
        {
            recentSaveText.text = "尚未立下任何修士档案。\n点击“新游戏”后，可先在角色选择里定道脉与档位。";
        }
    }

    public MainMenuLoadSnapshot BuildLoadSnapshot()
    {
        var slots = new MainMenuSlotSnapshot[CultivationLocalSaveStore.SaveSlotCount];
        for (var i = 0; i < slots.Length; i++)
        {
            var occupied = CultivationLocalSaveStore.TryLoadSlot(i, out var data);
            slots[i] = new MainMenuSlotSnapshot
            {
                SlotIndex = i,
                Title = "第 " + (i + 1) + " 档",
                Detail = occupied ? data.heroName + " / " + data.archetypeName : "暂无存档",
                Footer = occupied ? data.realm + " · " + data.location : "等待开辟",
                Selected = i == selectedLoadSlotIndex,
                Occupied = occupied
            };
        }

        var detail = BuildLoadDetailSnapshot();
        return new MainMenuLoadSnapshot
        {
            Slots = slots,
            DetailTitle = detail.Title,
            DetailBody = detail.Body,
            ActionText = detail.Action,
            CanLoad = detail.CanLoad,
            CanDelete = detail.CanDelete
        };
    }

    private LoadDetailSnapshot BuildLoadDetailSnapshot()
    {
        if (CultivationLocalSaveStore.TryLoadSlot(selectedLoadSlotIndex, out var data))
        {
            return new LoadDetailSnapshot
            {
                Title = data.heroName + " / " + data.archetypeName,
                Body = "出身：" + data.origin + "\n" +
                       "路线：" + data.specialty + "\n" +
                       "门派：" + data.sectName + "\n" +
                       "境界：" + data.realm + "\n" +
                       "位置：" + data.location + "\n" +
                       "灵石：" + data.wallet.ToDisplayString() + " · " + CultivationLoadoutLibrary.BuildCompactProgressSummary(data) + "\n" +
                       "上次游历：" + data.lastPlayed + "\n\n" +
                       CultivationLoadoutLibrary.BuildEquipmentOverview(data) + "\n\n" +
                       data.description,
                Action = "可直接载入，或先删除后重新开局。",
                CanLoad = true,
                CanDelete = true
            };
        }

        return new LoadDetailSnapshot
        {
            Title = "空白档位",
            Body = "此档位尚未记录任何修士。\n\n可以先返回主菜单，点击“新游戏”后在角色选择中将新的修士写入这个档位。",
            Action = "当前没有内容可载入。",
            CanLoad = false,
            CanDelete = false
        };
    }

    public MainMenuCharacterSnapshot BuildCharacterSnapshot()
    {
        var slots = new MainMenuSlotSnapshot[CultivationLocalSaveStore.SaveSlotCount];
        for (var i = 0; i < slots.Length; i++)
        {
            var occupied = CultivationLocalSaveStore.TryLoadSlot(i, out var data);
            slots[i] = new MainMenuSlotSnapshot
            {
                SlotIndex = i,
                Title = "档 " + (i + 1),
                Detail = occupied ? data.heroName : "空",
                Footer = occupied ? "将覆盖" : "新建",
                Selected = i == selectedCharacterSlotIndex,
                Occupied = occupied
            };
        }

        return new MainMenuCharacterSnapshot
        {
            Slots = slots,
            Archetypes = archetypes.ToArray(),
            SelectedArchetypeIndex = Mathf.Clamp(selectedArchetypeIndex, 0, archetypes.Count - 1),
            SummaryTitle = BuildCharacterSummaryTitle(),
            SummaryBody = BuildCharacterSummaryBody(),
            HeroName = GetPendingHeroName()
        };
    }

    private string BuildCharacterSummaryTitle()
    {
        return archetypes.Count > 0 ? archetypes[selectedArchetypeIndex].name : "未定道脉";
    }

    private string BuildCharacterSummaryBody()
    {
        if (archetypes.Count == 0)
        {
            return "当前没有可用流派数据。";
        }

        var archetype = archetypes[selectedArchetypeIndex];
        var slotIndex = Mathf.Max(0, selectedCharacterSlotIndex);
        var slotSummary = CultivationLocalSaveStore.TryLoadSlot(slotIndex, out var existingSave)
            ? "档位状态：第 " + (slotIndex + 1) + " 档将覆盖现有存档 " + existingSave.heroName
            : "档位状态：第 " + (slotIndex + 1) + " 档为空，将新建修士";

        return "出身：" + archetype.origin + "\n" +
               "路线：" + archetype.specialty + "\n\n" +
               "开局位置：" + (archetype.id == "wanderer" ? "散修无门派，直接出现在山海大地图" : "拜入青玄山门，先进入门派驻地") + "\n\n" +
               archetype.description + "\n" +
               archetype.trait + "\n\n" +
               archetype.recommendation + "\n\n" +
               slotSummary;
    }

    public void OpenCharacterPanel()
    {
        ChangeUiState(MainMenuUiState.CharacterCreate);
    }

    public void CloseCharacterPanel()
    {
        ChangeUiState(MainMenuUiState.Home);
    }

    public void OpenLoadPanel()
    {
        ChangeUiState(MainMenuUiState.Load);
    }

    public void CloseLoadPanel()
    {
        ChangeUiState(MainMenuUiState.Home);
    }

    public void OpenSettings()
    {
        ChangeUiState(MainMenuUiState.Settings);
    }

    public void CloseSettings()
    {
        ChangeUiState(MainMenuUiState.Home);
    }

    public void ContinueGame()
    {
        var snapshot = BootstrapCurrentArchive();
        if (snapshot == null || snapshot.SaveData == null)
        {
            SetStatus("当前没有可继续的存档");
            ShowWarningMessage("当前没有可继续的存档。");
            return;
        }

        EnterGameplay(snapshot.SlotIndex, snapshot.SaveData, "继续游戏");
    }

    public void StartNewGame()
    {
        StartNewGame(pendingHeroName);
    }

    public void StartNewGame(string heroName)
    {
        var archetype = archetypes[selectedArchetypeIndex];
        heroName = (heroName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(heroName))
        {
            heroName = archetype.defaultHeroName;
        }

        pendingHeroName = heroName;
        var isSectDisciple = archetype.id != "wanderer";
        var sectId = isSectDisciple ? "qingxuan_sect" : "rogue";
        var sectName = isSectDisciple ? "青玄山门" : "散修";
        var saveData = new CultivationSaveData
        {
            heroName = heroName,
            archetypeId = archetype.id,
            archetypeName = archetype.name,
            origin = archetype.origin,
            specialty = archetype.specialty,
            description = archetype.description,
            realm = WorldRegionLibrary.GetRealmName(0),
            location = isSectDisciple ? sectName : WorldRegionLibrary.GetRegionDisplayName(WorldRegionLibrary.StartingRegionId),
            sectId = sectId,
            sectName = sectName,
            isSectDisciple = isSectDisciple,
            isInSectResidence = isSectDisciple,
            lastPlayed = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };
        saveData.realmTier = 0;
        saveData.qi = 0;
        saveData.currentRegionId = WorldRegionLibrary.StartingRegionId;
        saveData.unlockedRegionIds = new[] { WorldRegionLibrary.StartingRegionId };
        saveData.clearedRegionIds = System.Array.Empty<string>();
        saveData.wallet = default;
        saveData.attackLevel = 0;
        saveData.vitalityLevel = 0;
        saveData.mainArtifactLevel = 0;
        saveData.protectiveRelicLevel = 0;
        saveData.pillCauldronLevel = 0;
        saveData.talismanCaseLevel = 0;
        saveData.bagCapacity = 12;
        saveData.storageItems = System.Array.Empty<SaveItemStack>();
        saveData.activeTaskId = string.Empty;
        saveData.taskStates = System.Array.Empty<SaveTaskState>();
        saveData.worldSeed = Mathf.Abs((int)(System.DateTime.UtcNow.Ticks % int.MaxValue)) + selectedCharacterSlotIndex + 1;
        saveData.EnsureDefaults();

        SaveArchive(selectedCharacterSlotIndex, saveData);
        CultivationLocalSaveStore.SaveSelectedArchetype(selectedArchetypeIndex);
        CloseCharacterPanel();
        RefreshAll();
        SetStatus("已立下新档案：" + heroName + " / " + archetype.name);
        ShowSuccessMessage("已立下新档案：" + heroName + " / " + archetype.name);
        EnterGameplay(selectedCharacterSlotIndex, saveData, "新游戏");
    }

    public void LoadSelectedSave()
    {
        if (!CultivationLocalSaveStore.TryLoadSlot(selectedLoadSlotIndex, out var saveData))
        {
            SetStatus("所选档位暂无存档");
            ShowWarningMessage("所选档位暂无存档。");
            return;
        }

        EnterGameplay(selectedLoadSlotIndex, saveData, "加载存档");
    }

    public void DeleteSelectedSave()
    {
        if (!CultivationLocalSaveStore.TryLoadSlot(selectedLoadSlotIndex, out var saveData))
        {
            SetStatus("空档位无需删除");
            ShowInfoMessage("空档位无需删除。");
            return;
        }

        DeleteArchive(selectedLoadSlotIndex);
        selectedLoadSlotIndex = CultivationLocalSaveStore.GetPreferredLoadSlot();
        RefreshAll();
        SetStatus("已删除存档：" + saveData.heroName);
        ShowSuccessMessage("已删除存档：" + saveData.heroName);
    }

    public void ChangeMusicVolume(float delta)
    {
        musicVolume = Mathf.Clamp01(musicVolume + delta);
        SetMusicVolume(musicVolume);
        SetStatus("背景音乐音量已调整为 " + Mathf.RoundToInt(musicVolume * 100f) + "%");
    }

    public void ChangeSfxVolume(float delta)
    {
        sfxVolume = Mathf.Clamp01(sfxVolume + delta);
        SetSfxVolume(sfxVolume);
        SetStatus("音效音量已调整为 " + Mathf.RoundToInt(sfxVolume * 100f) + "%");
    }

    public void ChangeVoiceVolume(float delta)
    {
        voiceVolume = Mathf.Clamp01(voiceVolume + delta);
        SetVoiceVolume(voiceVolume);
        SetStatus("语音音量已调整为 " + Mathf.RoundToInt(voiceVolume * 100f) + "%");
    }

    public void ToggleFullscreenMode()
    {
        isFullscreen = !isFullscreen;
        SetFullscreen(isFullscreen);
        SetStatus(isFullscreen ? "已切换为全屏" : "已切换为窗口模式");
    }

    public void ResetSettings()
    {
        musicVolume = 0.8f;
        sfxVolume = 0.8f;
        voiceVolume = 0.8f;
        isFullscreen = true;
        ResetUserSettings();
        SetStatus("设置已恢复默认");
        ShowInfoMessage("设置已恢复默认。");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        SetStatus("编辑器模式下不会直接退出，停止 Play 模式即可");
#else
        Application.Quit();
#endif
    }

    public void SelectLoadSlot(int slotIndex)
    {
        selectedLoadSlotIndex = Mathf.Clamp(slotIndex, 0, CultivationLocalSaveStore.SaveSlotCount - 1);
    }

    public void SelectCharacterSlot(int slotIndex)
    {
        selectedCharacterSlotIndex = Mathf.Clamp(slotIndex, 0, CultivationLocalSaveStore.SaveSlotCount - 1);
    }

    public void SelectArchetype(int archetypeIndex)
    {
        selectedArchetypeIndex = Mathf.Clamp(archetypeIndex, 0, archetypes.Count - 1);
        CultivationLocalSaveStore.SaveSelectedArchetype(selectedArchetypeIndex);
        if (string.IsNullOrWhiteSpace(pendingHeroName) || MatchesAnyDefaultName(pendingHeroName))
        {
            pendingHeroName = ResolveDefaultHeroName();
        }
    }

    public string GetPendingHeroName()
    {
        return string.IsNullOrWhiteSpace(pendingHeroName) ? ResolveDefaultHeroName() : pendingHeroName;
    }

    public void SetPendingHeroName(string heroName)
    {
        pendingHeroName = (heroName ?? string.Empty).Trim();
    }

    private string ResolveDefaultHeroName()
    {
        return archetypes.Count > 0 ? archetypes[Mathf.Clamp(selectedArchetypeIndex, 0, archetypes.Count - 1)].defaultHeroName : "无名修士";
    }

    private void EnterGameplay(int slotIndex, CultivationSaveData saveData, string source)
    {
        saveData.lastPlayed = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        SaveArchive(slotIndex, saveData);
        RefreshAll();

        if (string.IsNullOrWhiteSpace(config.GameplaySceneName))
        {
            SetStatus(source + "已就绪，但还未配置 gameplaySceneName");
            ShowErrorMessage(source + "已就绪，但还未配置 gameplaySceneName。");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(config.GameplaySceneName))
        {
            SetStatus("场景未加入 Build Settings: " + config.GameplaySceneName);
            ShowErrorMessage("场景未加入 Build Settings: " + config.GameplaySceneName);
            return;
        }

        SceneFlow.RequestScene(config.GameplaySceneName);
    }

    private void CloseAllPanels()
    {
        CloseGameUiPanel(GameUiPanelId.MainMenuSettings);
        CloseGameUiPanel(GameUiPanelId.MainMenuLoad);
        CloseGameUiPanel(GameUiPanelId.MainMenuCharacterCreate);
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = "山门告示 / " + message;
        }
    }

    private bool MatchesAnyDefaultName(string value)
    {
        for (var i = 0; i < archetypes.Count; i++)
        {
            if (value == archetypes[i].defaultHeroName)
            {
                return true;
            }
        }

        return false;
    }

    private void ResolveLayoutReferences()
    {
        rootRect = transform as RectTransform;
        menuPanelRect = FindChildRect("MenuPanel");
        infoPanelRect = FindChildRect("InfoPanel");
        footerPanelRect = FindChildRect("FooterPanel");
    }

    private void CloseActiveModal()
    {
        ChangeUiState(MainMenuUiState.Home);
    }

    private void RefreshResponsiveLayout(bool force)
    {
        if (rootRect == null)
        {
            rootRect = transform as RectTransform;
            if (rootRect == null)
            {
                return;
            }
        }

        var rect = rootRect.rect;
        if (rect.width < 1f || rect.height < 1f)
        {
            return;
        }

        var layoutWidth = Mathf.RoundToInt(rect.width);
        var layoutHeight = Mathf.RoundToInt(rect.height);
        if (!force && layoutWidth == lastLayoutWidth && layoutHeight == lastLayoutHeight)
        {
            return;
        }

        lastLayoutWidth = layoutWidth;
        lastLayoutHeight = layoutHeight;
    }

    private RectTransform FindChildRect(string childName)
    {
        var child = transform.Find(childName);
        return child != null ? child as RectTransform : null;
    }
}
