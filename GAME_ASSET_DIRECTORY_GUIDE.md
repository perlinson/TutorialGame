# 游戏资源目录规范

> 目标：把项目资源分成“可商用正式资源”和“仅供参考的非商用参考包”两条线，避免后期误用、误打包、误提交。

## 1. 当前规则

- `Assets/` 里只放可直接进入游戏工程、允许商用发布、准备继续生产的资源。
- 无授权参考包不再放在 `Assets/` 下，不参与 Unity 导入，不参与打包。
- 外部参考素材统一放在项目根目录 `Reference/` 下，只能用于风格分析、结构拆解、需求提炼。
- 运行时直接加载的内容，尽量只保留 prefab、少量必要贴图、少量必要音频在 `Assets/Resources/`。
- 大量原始美术源文件不要继续全部堆进 `Resources/`。

## 2. 目录分工

### 正式资源目录

- `Assets/GameArt/UI`
  - UI 原始设计图、切片源、按钮、边框、图标、面板底图
- `Assets/GameArt/Characters`
  - 主角立绘、角色战斗姿态、角色头像
- `Assets/GameArt/Enemies`
  - 敌人立绘、敌人战斗图、敌人头像
- `Assets/GameArt/Backgrounds`
  - 主菜单、世界地图、历练、战斗背景
- `Assets/GameArt/VFX`
  - UI 特效贴图、战斗特效贴图、粒子辅助贴图
- `Assets/GameArt/Icons`
  - 通用功能图标、资源图标、状态图标
- `Assets/GameArt/Fonts`
  - 可商用字体和字库资产
- `Assets/GameAudio/BGM`
  - 背景音乐正式资源
- `Assets/GameAudio/SFX`
  - 按钮、战斗、获得奖励等音效
- `Assets/GameAudio/Voice`
  - 配音、播报、人声提示

### 运行时资源目录

- `Assets/Resources/UI/MainMenu`
  - 主菜单 prefab 与主菜单专属少量运行时资源
- `Assets/Resources/UI/WorldMap`
  - 世界地图 prefab 与节点展示资源
- `Assets/Resources/UI/Game`
  - 历练/战斗/结算 prefab
- `Assets/Resources/Audio`
  - 仅放当前代码还在直接读取的少量音频
- `Assets/Resources/Vfx`
  - 当前运行时直接加载的特效 prefab

### 参考目录

- `Reference/UnlicensedPacks`
  - 无授权参考包，只看，不用
- `Assets/Reference/StyleBoards`
  - 自己整理出来的风格版、配色版、构图版
- `Assets/Reference/Notes`
  - 拆解记录、命名规范、替换清单

## 3. 已处理内容

- 无授权参考包已移出 `Assets/`：
  - `Reference/UnlicensedPacks/Q版水墨国风（行侠仗义五千年）`
- 已建立正式生产目录：
  - `Assets/GameArt`
  - `Assets/GameAudio`
  - `Assets/Reference`

## 4. 接入规则

- 要进游戏的图片，先进入 `Assets/GameArt/...`，命名、切片、格式统一后，再决定是否做成 prefab 或图集。
- 要进游戏的音频，先进入 `Assets/GameAudio/...`，统一响度和命名后，再放进当前运行链路需要读取的位置。
- 原则上：
  - 源文件放 `Assets/GameArt` / `Assets/GameAudio`
  - 运行时 prefab 放 `Assets/Resources`
  - 参考包放 `Reference`

## 5. 命名规则

- UI：
  - `ui_panel_mainmenu_root`
  - `ui_btn_primary_gold`
  - `ui_frame_modal_ink`
- 角色：
  - `hero_portrait_alchemist_a`
  - `hero_battle_idle_alchemist`
- 敌人：
  - `enemy_portrait_bloodcultist_a`
  - `enemy_battle_attack_shadowwolf`
- 背景：
  - `bg_mainmenu_mountain_gate`
  - `bg_worldmap_ink_scroll`
  - `bg_expedition_misty_ruins`
- 特效：
  - `vfx_ui_confirm_ring`
  - `vfx_hit_ink_slash`
- 音频：
  - `bgm_main_menu`
  - `bgm_world_map`
  - `sfx_ui_click`
  - `sfx_combat_hit_heavy`

## 6. 生产顺序

1. 先补 UI 基础件：按钮、面板、标题条、页签、弹窗、资源条。
2. 再补主菜单、世界地图、历练界面的背景和主视觉。
3. 再补敌人立绘、敌人攻击动作、受击表现、伤害飘字样式。
4. 最后补角色立绘、角色动作、地图节点图标、稀有度和掉落图标体系。

## 7. 禁止事项

- 禁止把无授权第三方资源重新放回 `Assets/`。
- 禁止把整包参考图直接改名后当正式资源上线。
- 禁止把所有图片都塞进 `Resources/` 图省事。
- 禁止同一种资源同时存在 `Assets/GameArt` 和 `Assets/Resources` 两套长期并行版本但没有说明。

## 8. 现在最值得继续做的接入批次

- 第一批：
  - 主菜单背景
  - 主菜单按钮组
  - 世界地图主面板底图
  - 历练敌人占位立绘
  - 伤害数字样式
  - 战斗命中特效贴图
- 第二批：
  - 门派驻地面板
  - 背包与工坊页签
  - 区域节点图标
  - 奖励弹窗
  - Tooltip 底图
