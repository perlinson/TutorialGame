# 重构实施单

- [x] 1. 重构存档层：把 `PlayerPrefs` 档位/设置迁到文件存储，去掉旧兼容
- [x] 2. 为远征运行态建立可持久化快照，支持中途恢复
- [x] 3. 收敛主菜单 UI 的单一来源，减少 prefab 与 runtime builder 双轨漂移
- [x] 4. 拆分超大控制器和系统，先拆 `GameController` / `WorldMapController` / `CultivationBattleSystem`
- [x] 5. 给任务、工坊、战斗、条件判断补第一批 EditMode 测试
- [x] 6. 建立 UI 排他规则：全屏主界面唯一显示，弹窗单独管理，禁止主 UI 重叠
- [x] 7. 建立 `Boot` 常驻入口，统一全局音频 / EventSystem / 存档服务 / 场景路由
- [x] 8. 统一日志出口：把零散 `Debug.Log*` 收敛到 `CultivationApp` utility 门面
- [x] 9. 建立 `prefab-first` UI 入口：主页面优先从 prefab 装载，runtime builder 仅保留临时 fallback
- [x] 10. 把 `WorldMapRoot` / `ExpeditionRoot` 真正落成静态 prefab，并移除运行时对 builder 的依赖
  当前状态：两个 prefab 已落地；旧 builder 仅保留为编辑器导出工具，不再参与正式运行时路径
- [x] 11. 用真正 `ResKit` 替换当前 `Resources` 驱动的资源服务
  当前状态：官方 `Assets/QFramework/Toolkits/ResKit` 已并入，`GameResourceService` 已改为 `ResKit.Init()` + `ResLoader.LoadSync<T>()`
- [x] 12. 接入官方 `AudioKit`，移除自建双 `AudioSource` 全局音频通道
  当前状态：`GameAudioService` 已切到 `AudioKit.PlayMusic/PlaySound`，`Boot` 的 `GlobalAudioManager` 不再手工创建音乐/音效源，音量设置同步到 `AudioKit.Settings`
- [x] 13. 把 `GameController` 的远征主流程切到 `FSMKit`
  当前状态：远征输入驱动和相位切换已由 `FSM<ExpeditionFlowPhase>` 接管；`RoomDecision / CombatPlayerTurn / AfterRoom / Completed / Retreated / Failed` 已接入状态进入与更新逻辑
- [x] 14. 建立统一音频 cue 层，把场景 BGM、UI 按钮音、战斗反馈接到正式运行链路
  当前状态：已新增 `CultivationAudio` 作为总入口；主菜单 / 大地图 / 历练场景会自动播占位 BGM；按钮点击、确认/取消、战斗命中/受击/拾取已统一出声
- [x] 15. 拆分用户音频设置：从单音量升级为音乐 / 音效 / 语音三通道，并把主菜单设置面板静态落地
  当前状态：`MainMenuSaveStore` / `GameSettingsService` / `CultivationApp` 已改成三通道设置；`MainMenuRoot.prefab` 已重生为三组独立加减控件
- [x] 16. 建立 `AudioMixer` 分组路由：把 `Music / Sfx / Voice` 从设置层落到真实播放总线
  当前状态：`Assets/Resources/Audio/CultivationAudioMixer.mixer` 已生成，包含 `Music / Sfx / Voice` 分组；`AudioKit` 创建出的 `AudioSource` 已按 bus 自动分配到对应 `AudioMixerGroup`
- [x] 17. 把主菜单 UI 状态切到 `FSMKit`
  当前状态：`MainMenuController` 已新增 `Home / Settings / Load / CharacterCreate` 状态；按钮和 `Escape` 现在通过状态切换控制面板显示
- [x] 18. 收敛大地图 UI 状态切换逻辑，去掉 `WorldMapRoot` 内嵌多面板切状态
  当前状态：`WorldMap` 主地图只保留地图本体和右侧详情；`Inventory / Workshop / SectResidence` 已切向独立 prefab，并通过 `GameUiPanelId + UIKit` 调度显示/隐藏，避免多个主界面同时堆在同一个 root 下
- [x] 19. 建立全局游戏流程状态机，并补上 `Splash` 入口
  当前状态：`AppRoot` 已新增全局 `GlobalGameFlowManager`；`Boot` 会先进入 `Splash` 再路由到 `Main / WorldMap / Game`；后续场景切换统一通过全局流程状态驱动
- [x] 20. 把音量设置真正落到 `AudioMixer` 暴露参数，并加入基础 ducking
  当前状态：`GameSettingsService` 现在会同步写入 `MusicVolume / SfxVolume / VoiceVolume` 暴露参数；主菜单弹窗 / 大地图弹窗 / 历练 overlay 已接入基础压混；`CultivationAudioMixerGenerator` 会自动补齐这 3 个暴露参数
- [x] 20. 补全 `Paused` 流程：历练内 `Esc` 不再直接撤离，而是进入暂停弹窗
  当前状态：`Gameplay -> Paused -> Gameplay` 已接入全局 `FSMKit`；`ExpeditionRoot.prefab` 已静态落地暂停遮罩；暂停时会冻结实时输入，并提供继续历练 / 提前撤离 / 返回主菜单三条路径
- [x] 21. 把战斗表现从纯文字/纯代码反馈升级为资产化动画 + 粒子特效
  当前状态：已通过 Unity MCP 新增 `Player/Enemy` 战斗 `AnimationClip` 与 `AnimatorController`，并生成 `HitBurst / HealBurst / EnemyDefeatBurst / SpawnPulse` 粒子 prefab；运行时角色已改为 `VisualPivot -> VisualBody` 结构，攻击/受击/治疗/出生/死亡都会驱动正式表现资源
- [x] 22. 把战斗角色切到 `prefab-first`，并补上基础镜头反馈
  当前状态：`Assets/Resources/Prefabs/Combat/PlayerCultivator.prefab` 与 `SpiritEnemy.prefab` 已落地，`GameArenaBuilder` 会优先实例化 prefab，再按存档/阵营填充立绘与配色；命中时已加入基础 `camera shake + hit stop`
- [x] 23. 建立一批可复用的战斗美术资源，并接回运行时
  当前状态：已新增 `Assets/Resources/Generated/Enemies/*.png` 与 `Assets/Resources/Generated/ArenaBackdrops/*.png`；`GeneratedArtLibrary` 现在会优先读取这些敌人头像与历练背景图，`GameArenaBuilder` 已把区域背景图真正铺到战斗场景里
- [x] 24. 建立物品 / 技能图标资源批量生成链路，并让运行时在空引用时自动回退到生成资源
  当前状态：已新增 `Assets/Editor/GeneratedUiArtBatch.cs`，可批量生成 `Assets/Resources/Generated/Items/*.png` 与 `Assets/Resources/Generated/Skills/*.png`；`InventoryLibrary` / `ExpeditionBuildFactory` 已在数据表未填 `Sprite` 时自动接入生成图标
- [x] 25. 完成核心 UI 的 `TMP + 静态 prefab` 切换，并停止运行时二次套皮
  当前状态：`MainMenuRoot` / `WorldMapRoot` / `ExpeditionRoot` 已重新批量导出；主菜单、世界地图、历练 UI 脚本与生成器已切到 `TMP`；场景 bootstrap 现在只负责实例化 prefab 和绑定数据，不再在运行时覆盖按钮皮肤和背景图；主菜单 `ModalBlocker` 也已静态收进 prefab，非编辑器脚本中已无新增 `RectTransform/Canvas` 的运行时 UI 拼装点
- [x] 26. 清理编辑器 UI 导出链路里的临时反射和隐藏依赖
  当前状态：`UiPrefabGenerationUtility` 导出 `ExpeditionRoot` 时已不再通过反射调用私有方法，而是直接走 `GameSceneBootstrap.BuildPrefabExportView(...)`；大地图导出入口也已收口为 `WorldMapPrefabExportBuilder.BuildPrefabExportController()`；主菜单、大地图、远征 prefab 批量导出均已重新验证通过
- [x] 27. 把战斗 support 对象推进到 `prefab-first`
  当前状态：已新增 `CombatPrefabGenerator`，统一导出 `SpiritNode / SpiritHerb / TrialRelic / FloatingCombatText / CombatSlashEffect / CombatImpactEffect` 到 `Assets/Resources/Prefabs/Combat`；`GameArenaBuilder` 现在会优先实例化这些 prefab，缺失时才回退到代码创建
- [ ] 28. 把 `WorldMap` 内嵌子界面彻底拆成独立 prefab，并切到 `GameUiPanelId + UIKit` 枚举调度
  当前状态：代码层已进一步补上 `GameUiPanelId.WorldMapRegion`，地图节点点击现在会打开独立的 `Bg` 级全屏地点页；`WorldMapRoot` 本身已降级为轻量入口页，不再承担主详情展示；`WorldMapPrefabExportBuilder` 和 `UiPrefabGenerationUtility` 已能导出 `WorldMapRoot / WorldMapRegionPanel / WorldMapInventoryPanel / WorldMapWorkshopPanel / WorldMapSectResidencePanel` 五份 prefab。下一步是继续把城镇/宗门/历练地点这几类页面的视觉结构做统一
- [x] 29. 拆分 UI 生命周期语义：缓存型窗口默认隐藏复用，保留显式销毁入口
  当前状态：`GameUiService.ClosePanel/CloseAllPanels` 已切为软关闭（`Hide`），不再默认销毁；同时新增 `DestroyPanel/DestroyAllPanels` 与 `CultivationApp.DestroyUiPanel/DestroyAllGameUiPanels` 作为硬释放入口
- [ ] 30. 把通用 `Tooltip / MessagePopup` 也切到独立 prefab + `GameUiPanelId` 调度
  当前状态：代码层已补上 `GameUiPanelId.Tooltip / MessagePopup`、非排他 `UILevel.PopUI` 注册和消息弹窗专用入口；`Tooltip` 已拆成独立脚本避免 prefab 丢脚本；`MessagePopup` 已补上 `Info / Warning / Error / Success` 分级皮肤入口；主菜单建档/删档、世界地图强化/炼制/宗门事务等关键反馈已接入弹窗；`OverlayPrefabGenerator` 现已补上自适应尺寸、淡入淡出和消息侧边强调条所需的默认布局。下一步是在 Unity 里执行一次生成，把 `Assets/Resources/UI/Overlay/` 下的 prefab 真正落盘并继续手调样式
- [x] 31. 把 `GameHub / PlayerCompendium` 从世界地图专属 UI 提升为游戏级展示面板，并切到 `Model + Command + BindableProperty/Event` 驱动
  当前状态：`CultivationPlayerModel / CultivationGameModel` 已补上玩家摘要、世界时序、Hub 可见性、总览页签与分栏选择；`GameHubPanel` 与 `PlayerCompendiumPanel` 已改为 `IController`，显示数据直接从 Model 读取，交互改为发送 Command，刷新改由 `BindableProperty` 和事件驱动；`WorldMapController` 只保留导航职责，不再负责给这两个面板喂显示数据
- [x] 32. 收口 QFramework 架构边界：表现层改走 `CultivationController`，系统层禁止反调 `CultivationApp`
  当前状态：`MainMenuBootstrap / WorldMapSceneBootstrap / GameSceneBootstrap / GameController / ExpeditionView` 已切到 `CultivationController / CultivationUIPanel` 基类，主流程交互统一改走 `Command + Utility`；`CultivationBattleSystem / CultivationExpeditionSystem / CultivationExpeditionEventSystem` 已改为 `GetSystem / GetUtility` 直接协作，不再在系统层通过 `CultivationApp` 静态门面回调自身架构
- [x] 33. 把静态资源读取从 `CultivationApp` 全局门面抽到专用资源桥接层
  当前状态：已新增 `GameResource` 作为只负责 `Load / Instantiate / ClearCache` 的窄桥接；`WorldRegionLibrary / TaskLibrary / InventoryLibrary / GameArenaBuilder / GeneratedArtLibrary / ExpeditionBuildFactory / ExpeditionEnemyFactory / ExpeditionRoomFactory / ExpeditionLootFactory / WorkshopLibrary` 已切到 `GameResource`，不再直接依赖 `CultivationApp` 全量静态接口
- [x] 34. 把音效 / 音乐入口提升为正式 `SoundSystem`
  当前状态：已新增 `Assets/Scripts/Architecture/Systems/Audio/SoundSystem.cs`，通过 `RegisterSystem<ISoundSystem>(new SoundSystem())` 接入架构；主菜单 / 大地图 / 历练 BGM 与战斗/拾取反馈音已切到 `ISoundSystem`；`CultivationAudio` 现降级为兼容包装层，旧按钮绑定代码仍可继续工作，但新代码已经可以直接使用 `this.GetSystem<ISoundSystem>().PlaySound(SoundType.Button_Low);`
- [x] 35. 把 `Boot/AppRoot` 全局管理器和核心 UI 按钮绑定切回 `IController + Utility/System`
  当前状态：`AppRoot` 内的 `GlobalAudioManager / GlobalUiManager` 已改为继承 `CultivationController`，音频初始化、运行时设置应用、UIRoot 初始化与错误日志不再通过 `CultivationApp` 大门面静态转发；同时补上 `GameLog` 窄桥接供 `SceneFlow` 等静态流程使用。`MainMenu / WorldMap / Hub / Compendium` 及其主要子面板的按钮绑定现已直接走 `CultivationUIPanel.BindButton(...) -> ISoundSystem`，`CultivationAudio.BindButton` 只保留给 `SaveSlotView / ArchetypeCardView / WorldRegionNodeView` 这类纯视图组件
- [x] 36. 删除 `CultivationAudio` 兼容层，拆分为 `GameSound + UI helper`
  当前状态：`Assets/Scripts/Audio/CultivationAudio.cs` 已移除；静态播放入口收敛到 `GameSound`，底层统一走 `ISoundSystem`；`SaveSlotView / ArchetypeCardView / WorldRegionNodeView / ExpeditionView` 等剩余按钮绑定点已改为走 `CultivationUiAudio` 表现层 helper。音频系统只负责播放与音量/ducking，不再承担 `Button` 绑定职责
- [x] 37. 给 prefab-first UI 补 `ButtonSoundBinder`，并把修仙技艺页升级为可视节点
  当前状态：已新增 `UIButtonSoundBinder`，`CultivationUiAudio.BindButton(...)` 现在会优先复用 prefab 上的声音绑定组件，避免音效逻辑继续散落在纯视图类里；`PlayerCompendiumSnapshot` 已补 `VisualTitle + VisualNodes`，`PlayerCompendiumPanel` 与 `WorldMapPrefabExportBuilder` 已新增节点区块和 `PlayerCompendiumNodeView`，`主修功法 / 战斗术法 / 丹道 / 符道` 四个子页都能产出可视节点数据。下一步需要在 Unity 里重新生成 `GameHubPanel` / `PlayerCompendiumPanel` prefab 并实跑世界地图流
