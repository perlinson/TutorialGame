using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    public Text titleText;
    public Text subtitleText;
    public Text descriptionText;
    public Text statusText;
    public Text infoFlavorText;
    public Text recentSaveText;
    public Text volumeValueText;
    public Text fullscreenValueText;
    public Text loadDetailTitleText;
    public Text loadDetailBodyText;
    public Text loadActionText;
    public Text characterSummaryTitleText;
    public Text characterSummaryBodyText;

    public Button newGameButton;
    public Button continueButton;
    public Button loadButton;
    public Button settingsButton;
    public Button quitButton;

    public Button volumeDownButton;
    public Button volumeUpButton;
    public Button fullscreenToggleButton;
    public Button resetSettingsButton;
    public Button closeSettingsButton;

    public Button loadSelectedButton;
    public Button deleteSelectedButton;
    public Button closeLoadPanelButton;

    public Button startNewGameButton;
    public Button closeCharacterPanelButton;

    public InputField heroNameInput;

    public GameObject settingsPanel;
    public GameObject loadPanel;
    public GameObject characterPanel;

    public Transform loadSlotsParent;
    public Transform characterSlotsParent;
    public Transform archetypeCardsParent;

    public SaveSlotView loadSlotPrefab;
    public SaveSlotView characterSlotPrefab;
    public ArchetypeCardView archetypeCardPrefab;

    private readonly List<SaveSlotView> loadSlotViews = new List<SaveSlotView>();
    private readonly List<SaveSlotView> characterSlotViews = new List<SaveSlotView>();
    private readonly List<ArchetypeCardView> archetypeViews = new List<ArchetypeCardView>();
    private readonly List<MainMenuArchetype> archetypes = new List<MainMenuArchetype>();

    private MainMenuConfig config;
    private bool isInitialized;
    private int selectedLoadSlotIndex = -1;
    private int selectedCharacterSlotIndex = -1;
    private int selectedArchetypeIndex;
    private float masterVolume;
    private bool isFullscreen;

    public void Initialize(MainMenuConfig newConfig)
    {
        config = newConfig;
        EnsureArchetypes();
        EnsureDynamicViews();
        WireButtons();
        LoadPreferences();
        ApplyCopy();
        RefreshAll();
        SetStatus("主界面已就绪");
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || !Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (characterPanel.activeSelf)
        {
            CloseCharacterPanel();
            return;
        }

        if (loadPanel.activeSelf)
        {
            CloseLoadPanel();
            return;
        }

        if (settingsPanel.activeSelf)
        {
            CloseSettings();
        }
    }

    private void EnsureArchetypes()
    {
        if (archetypes.Count > 0)
        {
            return;
        }

        var database = Resources.Load<HeroArchetypeDatabaseAsset>("Data/HeroArchetypeDatabase");
        if (database != null && database.archetypes != null && database.archetypes.Length > 0)
        {
            for (var i = 0; i < database.archetypes.Length; i++)
            {
                var record = database.archetypes[i];
                if (record == null || string.IsNullOrWhiteSpace(record.id))
                {
                    continue;
                }

                archetypes.Add(new MainMenuArchetype
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
            archetypes.Add(new MainMenuArchetype
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

            archetypes.Add(new MainMenuArchetype
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

            archetypes.Add(new MainMenuArchetype
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

    private static Sprite LoadArchetypePortrait(string archetypeId)
    {
        var database = Resources.Load<HeroArchetypeDatabaseAsset>("Data/HeroArchetypeDatabase");
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

    private void EnsureDynamicViews()
    {
        if (loadSlotViews.Count == 0)
        {
            for (var i = 0; i < MainMenuSaveStore.SaveSlotCount; i++)
            {
                loadSlotViews.Add(Instantiate(loadSlotPrefab, loadSlotsParent));
            }
        }

        if (characterSlotViews.Count == 0)
        {
            for (var i = 0; i < MainMenuSaveStore.SaveSlotCount; i++)
            {
                characterSlotViews.Add(Instantiate(characterSlotPrefab, characterSlotsParent));
            }
        }

        while (archetypeViews.Count < archetypes.Count)
        {
            archetypeViews.Add(Instantiate(archetypeCardPrefab, archetypeCardsParent));
        }
    }

    private void WireButtons()
    {
        BindButton(newGameButton, OpenCharacterPanel);
        BindButton(continueButton, ContinueGame);
        BindButton(loadButton, OpenLoadPanel);
        BindButton(settingsButton, OpenSettings);
        BindButton(quitButton, QuitGame);

        BindButton(volumeDownButton, () => ChangeVolume(-0.1f));
        BindButton(volumeUpButton, () => ChangeVolume(0.1f));
        BindButton(fullscreenToggleButton, ToggleFullscreenMode);
        BindButton(resetSettingsButton, ResetSettings);
        BindButton(closeSettingsButton, CloseSettings);

        BindButton(loadSelectedButton, LoadSelectedSave);
        BindButton(deleteSelectedButton, DeleteSelectedSave);
        BindButton(closeLoadPanelButton, CloseLoadPanel);

        BindButton(startNewGameButton, StartNewGame);
        BindButton(closeCharacterPanelButton, CloseCharacterPanel);
    }

    private void ApplyCopy()
    {
        titleText.text = config.Title;
        subtitleText.text = config.Subtitle;
        descriptionText.text = config.Description;
    }

    private void LoadPreferences()
    {
        masterVolume = MainMenuSaveStore.LoadVolume();
        isFullscreen = MainMenuSaveStore.LoadFullscreen();
        selectedArchetypeIndex = Mathf.Clamp(MainMenuSaveStore.LoadSelectedArchetype(), 0, archetypes.Count - 1);
        selectedLoadSlotIndex = MainMenuSaveStore.GetPreferredLoadSlot();
        selectedCharacterSlotIndex = MainMenuSaveStore.GetPreferredNewGameSlot();

        AudioListener.volume = masterVolume;
        Screen.fullScreen = isFullscreen;
    }

    private void RefreshAll()
    {
        RefreshSettingsLabels();
        RefreshMainButtons();
        RefreshInfoPanel();
        RefreshLoadSlots();
        RefreshLoadDetails();
        RefreshCharacterSlots();
        RefreshArchetypes();
        RefreshCharacterSummary();
    }

    private void RefreshMainButtons()
    {
        continueButton.interactable = MainMenuSaveStore.HasAnySave();
    }

    private void RefreshSettingsLabels()
    {
        volumeValueText.text = Mathf.RoundToInt(masterVolume * 100f) + "%";
        fullscreenValueText.text = isFullscreen ? "全屏" : "窗口";
    }

    private void RefreshInfoPanel()
    {
        infoFlavorText.text = "主界面改成 prefab 驱动后，结构会更稳定：重复的档位项和角色卡都由独立预制体生成，后续你可以直接在 prefab 上迭代样式。";

        if (MainMenuSaveStore.TryGetCurrentSave(out var slotIndex, out var data))
        {
            recentSaveText.text = "当前活跃档位：第 " + (slotIndex + 1) + " 档\n" +
                                  data.heroName + " / " + data.archetypeName + "\n" +
                                  data.realm + " · " + data.location + " · " + data.sectName + "\n" +
                                  "灵石：" + data.spiritCrystals + " · " + CultivationLoadoutLibrary.BuildCompactProgressSummary(data) + "\n" +
                                  "上次游历：" + data.lastPlayed;
        }
        else
        {
            recentSaveText.text = "尚未立下任何修士档案。\n点击“新游戏”后，可先在角色选择里定道脉与档位。";
        }
    }

    private void RefreshLoadSlots()
    {
        for (var i = 0; i < loadSlotViews.Count; i++)
        {
            var slotIndex = i;
            var occupied = MainMenuSaveStore.TryLoadSlot(i, out var data);
            var detail = occupied ? data.heroName + " / " + data.archetypeName : "暂无存档";
            var footer = occupied ? data.realm + " · " + data.location : "等待开辟";

            loadSlotViews[i].Bind(
                i,
                "第 " + (i + 1) + " 档",
                detail,
                footer,
                i == selectedLoadSlotIndex,
                occupied,
                () => SelectLoadSlot(slotIndex));
        }
    }

    private void RefreshLoadDetails()
    {
        if (MainMenuSaveStore.TryLoadSlot(selectedLoadSlotIndex, out var data))
        {
            loadDetailTitleText.text = data.heroName + " / " + data.archetypeName;
            loadDetailBodyText.text = "出身：" + data.origin + "\n" +
                                      "路线：" + data.specialty + "\n" +
                                      "门派：" + data.sectName + "\n" +
                                      "境界：" + data.realm + "\n" +
                                      "位置：" + data.location + "\n" +
                                      "灵石：" + data.spiritCrystals + " · " + CultivationLoadoutLibrary.BuildCompactProgressSummary(data) + "\n" +
                                      "上次游历：" + data.lastPlayed + "\n\n" +
                                      CultivationLoadoutLibrary.BuildEquipmentOverview(data) + "\n\n" +
                                      data.description;
            loadActionText.text = "可直接载入，或先删除后重新开局。";
        }
        else
        {
            loadDetailTitleText.text = "空白档位";
            loadDetailBodyText.text = "此档位尚未记录任何修士。\n\n可以先返回主菜单，点击“新游戏”后在角色选择中将新的修士写入这个档位。";
            loadActionText.text = "当前没有内容可载入。";
        }
    }

    private void RefreshCharacterSlots()
    {
        for (var i = 0; i < characterSlotViews.Count; i++)
        {
            var slotIndex = i;
            var occupied = MainMenuSaveStore.TryLoadSlot(i, out var data);
            characterSlotViews[i].Bind(
                i,
                "档 " + (i + 1),
                occupied ? data.heroName : "空",
                occupied ? "将覆盖" : "新建",
                i == selectedCharacterSlotIndex,
                occupied,
                () => SelectCharacterSlot(slotIndex));
        }
    }

    private void RefreshArchetypes()
    {
        for (var i = 0; i < archetypeViews.Count; i++)
        {
            var index = i;
            archetypeViews[i].Bind(i, archetypes[i], i == selectedArchetypeIndex, () => SelectArchetype(index));
        }

        var currentName = heroNameInput.text.Trim();
        if (string.IsNullOrEmpty(currentName) || MatchesAnyDefaultName(currentName))
        {
            heroNameInput.text = archetypes[selectedArchetypeIndex].defaultHeroName;
        }
    }

    private void RefreshCharacterSummary()
    {
        var archetype = archetypes[selectedArchetypeIndex];
        var slotIndex = selectedCharacterSlotIndex;
        if (slotIndex < 0)
        {
            slotIndex = 0;
        }

        var slotSummary = MainMenuSaveStore.TryLoadSlot(slotIndex, out var existingSave)
            ? "档位状态：第 " + (slotIndex + 1) + " 档将覆盖现有存档 " + existingSave.heroName
            : "档位状态：第 " + (slotIndex + 1) + " 档为空，将新建修士";

        characterSummaryTitleText.text = archetype.name;
        characterSummaryBodyText.text = "出身：" + archetype.origin + "\n" +
                                        "路线：" + archetype.specialty + "\n\n" +
                                        "开局位置：" + (archetype.id == "wanderer" ? "散修无门派，直接出现在山海大地图" : "拜入青玄山门，先进入门派驻地") + "\n\n" +
                                        archetype.description + "\n" +
                                        archetype.trait + "\n\n" +
                                        archetype.recommendation + "\n\n" +
                                        slotSummary;
    }

    public void OpenCharacterPanel()
    {
        CloseAllPanels();
        characterPanel.SetActive(true);
        selectedCharacterSlotIndex = MainMenuSaveStore.GetPreferredNewGameSlot();
        selectedArchetypeIndex = Mathf.Clamp(MainMenuSaveStore.LoadSelectedArchetype(), 0, archetypes.Count - 1);
        RefreshAll();
        SetStatus("选择一条修途与存档档位");
    }

    public void CloseCharacterPanel()
    {
        characterPanel.SetActive(false);
    }

    public void OpenLoadPanel()
    {
        CloseAllPanels();
        loadPanel.SetActive(true);
        selectedLoadSlotIndex = MainMenuSaveStore.GetPreferredLoadSlot();
        RefreshAll();
        SetStatus("已展开存档卷轴");
    }

    public void CloseLoadPanel()
    {
        loadPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        CloseAllPanels();
        settingsPanel.SetActive(true);
        SetStatus("已打开洞府设置");
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void ContinueGame()
    {
        var snapshot = CultivationApp.BootstrapCurrentArchive();
        if (snapshot == null || snapshot.SaveData == null)
        {
            SetStatus("当前没有可继续的存档");
            return;
        }

        EnterGameplay(snapshot.SlotIndex, snapshot.SaveData, "继续游戏");
    }

    public void StartNewGame()
    {
        var archetype = archetypes[selectedArchetypeIndex];
        var heroName = heroNameInput.text.Trim();
        if (string.IsNullOrEmpty(heroName))
        {
            heroName = archetype.defaultHeroName;
        }

        var isSectDisciple = archetype.id != "wanderer";
        var sectId = isSectDisciple ? "qingxuan_sect" : "rogue";
        var sectName = isSectDisciple ? "青玄山门" : "散修";
        var saveData = new MainMenuSaveData
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
        saveData.spiritCrystals = 0;
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
        saveData.EnsureDefaults();

        CultivationApp.SaveArchive(selectedCharacterSlotIndex, saveData);
        MainMenuSaveStore.SaveSelectedArchetype(selectedArchetypeIndex);
        CloseCharacterPanel();
        RefreshAll();
        SetStatus("已立下新档案：" + heroName + " / " + archetype.name);
        EnterGameplay(selectedCharacterSlotIndex, saveData, "新游戏");
    }

    public void LoadSelectedSave()
    {
        if (!MainMenuSaveStore.TryLoadSlot(selectedLoadSlotIndex, out var saveData))
        {
            SetStatus("所选档位暂无存档");
            return;
        }

        EnterGameplay(selectedLoadSlotIndex, saveData, "加载存档");
    }

    public void DeleteSelectedSave()
    {
        if (!MainMenuSaveStore.TryLoadSlot(selectedLoadSlotIndex, out var saveData))
        {
            SetStatus("空档位无需删除");
            return;
        }

        CultivationApp.DeleteArchive(selectedLoadSlotIndex);
        selectedLoadSlotIndex = MainMenuSaveStore.GetPreferredLoadSlot();
        RefreshAll();
        SetStatus("已删除存档：" + saveData.heroName);
    }

    public void ChangeVolume(float delta)
    {
        masterVolume = Mathf.Clamp01(masterVolume + delta);
        AudioListener.volume = masterVolume;
        MainMenuSaveStore.SaveVolume(masterVolume);
        RefreshSettingsLabels();
        SetStatus("主音量已调整为 " + Mathf.RoundToInt(masterVolume * 100f) + "%");
    }

    public void ToggleFullscreenMode()
    {
        isFullscreen = !isFullscreen;
        Screen.fullScreen = isFullscreen;
        MainMenuSaveStore.SaveFullscreen(isFullscreen);
        RefreshSettingsLabels();
        SetStatus(isFullscreen ? "已切换为全屏" : "已切换为窗口模式");
    }

    public void ResetSettings()
    {
        masterVolume = 0.8f;
        isFullscreen = true;
        AudioListener.volume = masterVolume;
        Screen.fullScreen = isFullscreen;
        MainMenuSaveStore.SaveVolume(masterVolume);
        MainMenuSaveStore.SaveFullscreen(isFullscreen);
        RefreshSettingsLabels();
        SetStatus("设置已恢复默认");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        SetStatus("编辑器模式下不会直接退出，停止 Play 模式即可");
#else
        Application.Quit();
#endif
    }

    private void SelectLoadSlot(int slotIndex)
    {
        selectedLoadSlotIndex = Mathf.Clamp(slotIndex, 0, MainMenuSaveStore.SaveSlotCount - 1);
        RefreshLoadSlots();
        RefreshLoadDetails();
    }

    private void SelectCharacterSlot(int slotIndex)
    {
        selectedCharacterSlotIndex = Mathf.Clamp(slotIndex, 0, MainMenuSaveStore.SaveSlotCount - 1);
        RefreshCharacterSlots();
        RefreshCharacterSummary();
    }

    private void SelectArchetype(int archetypeIndex)
    {
        selectedArchetypeIndex = Mathf.Clamp(archetypeIndex, 0, archetypes.Count - 1);
        MainMenuSaveStore.SaveSelectedArchetype(selectedArchetypeIndex);
        RefreshArchetypes();
        RefreshCharacterSummary();
    }

    private void EnterGameplay(int slotIndex, MainMenuSaveData saveData, string source)
    {
        saveData.lastPlayed = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        CultivationApp.SaveArchive(slotIndex, saveData);
        RefreshAll();

        if (string.IsNullOrWhiteSpace(config.GameplaySceneName))
        {
            SetStatus(source + "已就绪，但还未配置 gameplaySceneName");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(config.GameplaySceneName))
        {
            SetStatus("场景未加入 Build Settings: " + config.GameplaySceneName);
            return;
        }

        SceneManager.LoadScene(config.GameplaySceneName);
    }

    private void CloseAllPanels()
    {
        settingsPanel.SetActive(false);
        loadPanel.SetActive(false);
        characterPanel.SetActive(false);
    }

    private void SetStatus(string message)
    {
        statusText.text = "山门告示 / " + message;
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

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }
}
