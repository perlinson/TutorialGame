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
- [x] 18. 把大地图 UI 状态切到 `FSMKit`
  当前状态：`WorldMapController` 已拆成 `Map / SectResidence` 主状态与 `None / Inventory / Workshop` 弹窗状态；`Escape` 现在按状态层级回退
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
