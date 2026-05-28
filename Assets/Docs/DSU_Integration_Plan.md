# Dialogue System for Unity 集成计划

## 概述

将 DSU (Dialogue System for Unity) 集成到当前修仙游戏项目中，以 Dialog System 作为对话执行引擎，嵌入 QFramework 分层架构，替换当前硬编码的 NPC 对话系统。

## 当前代码状态

- 已有 `CultivationDialogueSystem`，并在 `CultivationApp` 中完成注册
- 已有 `LuaBridge`，可向 DSU 注册 `GameState_RealmTier / GameState_SpiritCrystals / GameState_HasFlag / GameState_AddCrystals` 等函数
- `NpcChoiceDefinition` 已增加 `ConversationTitle`
- `CultivationNpcSystem.ExecuteChoice()` 已支持“有 `ConversationTitle` 就走 DSU”
- `清岚长老 -> 请教修行关窍` 已作为首个 DSU 入口接线，当前配置名为 `清岚长老`
- `CultivationDialogueSystem` 会自动尝试从 `Resources/Dialogue/CultivationDialogueDatabase` 加载默认数据库
- 已添加 `CultivationDialogueDatabaseBootstrap` 作为手动生成默认数据库的辅助入口，但不会自动创建资源
- 首个 DSU 对话已完成运行打通：数据库加载、Conversation 命中、Lua Script 执行、存档写回链路均已验证
- `LuaBridge` 已同时支持 `GameState_RecordStory(...)` 与 `Story_RecordSignal(...)`
- 已补充 `StartEventConversationCommand` / `CultivationApp.StartEventConversation(...)`，为剧情事件对话预留正式入口

## 实施步骤

### Phase 1 — 桥梁搭建

- [x] 导入 DSU 插件到工程
- [x] 创建 `CultivationDialogueSystem`（继承 `AbstractSystem`），初始化 DSU Controller
- [x] 创建 `LuaBridge`，注册读写函数（`RealmTier`, `HasFlag`, `SetFlag` 等）
- [x] 验证：DSU Lua 对 C# 桥接已生效（`GameState_AddQi(2)`、`GameState_RecordStory(...)` 已在运行时执行）

### Phase 2 — 对话触发

- [x] 给 `NpcChoiceDefinition` 增加 `ConversationTitle` 字段（对应 DSU 对话资产名）
- [x] 修改 `CultivationNpcSystem.ExecuteChoice()`：有 ConversationTitle 就走 DSU
- [x] 选择首个接入 NPC：`清岚长老 / 请教修行关窍`
- [x] 提供默认 Dialogue Database 的手动生成入口脚本
- [x] 手动创建并配置 `Assets/Resources/Dialogue/CultivationDialogueDatabase.asset`
- [x] 验证：游戏中点 NPC 选项 → 弹出 DSU 对话 UI → 推进节点 → 正确退出

## 当前 Unity 验证要点

如果点击 `清岚长老 -> 请教修行关窍` 仍然没有弹出对话，优先检查下面 3 项：

1. 场景里是否存在 `DialogueSystemController`
2. `DialogueSystemController.initialDatabase` 是否已经指定到你的 Dialogue Database 资源，或运行时是否已自动加载 `Resources/Dialogue/CultivationDialogueDatabase`
3. 数据库里是否真的有一个 Conversation，标题严格等于 `清岚长老`

当前代码已经加了运行时日志：

- 没有 `DialogueSystemController`：会创建临时 `DSURuntime`，并提示你补 `Initial Database`
- 没有 `Initial Database`：会报 `DSU Initial Database 未配置或未加载`
- 找不到 Conversation：会报 `找不到 Conversation: 清岚长老`

### Phase 3 — 剧情整合

- [x] DSU 对话节点 Script 中调用 `GameState_SetFlag`、`GameState_RecordStory`
- [x] 增加 `Story_RecordSignal(...)` 作为剧情脚本别名
- [x] `CultivationStorySystem` 已可接收 DSU 事件驱动写入 `storyFlags / storyLog`
- [x] 提供事件对话入口：`CultivationDialogueSystem.StartEventConversation()` / `StartEventConversationCommand`
- [x] 接入首个事件型触发钩子：首次前往地区时可自动尝试启动 DSU 事件对话
- [ ] 创建剧情事件对话资产（如 `region_intro_<regionId>`）
- [ ] 验证：剧情 flag 在 DSU 对话中被正确设置，StorySystem 能读取

### Phase 4 — 迁移与完善

- [ ] 陆续将硬编码 NPC 对话迁移为 DSU 对话资产
- [ ] 配置 DSU 的 Ink 或 JSON 导入（如果已有外部剧情稿）
- [ ] 添加 DSU 的对话序列（Sequence）：人像动画、音效触发
- [ ] 整合 DSU Quest System（可选，替代 CultivationTaskSystem 的部分功能）
- [ ] 水墨风 DSU UI 皮肤定制

## 架构设计

```
                    ┌──────────────────────────┐
                    │  CultivationApp           │
                    │  (Architecture<CultApp>)  │
                    └────┬──────────┬───────────┘
                         │          │
              ┌──────────▼──┐   ┌──▼────────────┐
              │ Existing:   │   │ NEW:           │
              │ StorySystem │   │ DialogueSystem │
              │ NpcSystem   │   │ (ISystem)      │
              │ TaskSystem  │   │                │
              │ CondSystem  │   │ ├ Controller   │
              └──────────┬──┘   │ ├ LuaBridge    │
                         │      │ ├ VariableSync │
                         │      └──┬─────────────┘
                         │         │
              ┌──────────▼─────────▼────────────┐
              │  DSU Runtime                     │
              │  (DialogueSystemController)      │
              │                                  │
              │  ┌─────────┐  ┌───────────────┐  │
              │  │ Dialogue│  │ Quest System  │  │
              │  │ Database│  │ (可选替代Task) │  │
              │  └─────────┘  └───────────────┘  │
              └──────────────────────────────────┘
```

## 关键集成点

### 1. CultivationDialogueSystem（新 ISystem — 桥梁层）

```csharp
public sealed class CultivationDialogueSystem : AbstractSystem
{
    private DialogueSystemController dsuController;
    private LuaBridge luaBridge;
    private CultivationStorySystem storySystem;
    private Action pendingOnEnd;

    protected override void OnInit()
    {
        dsuController = GameObject.FindObjectOfType<DialogueSystemController>();
        if (dsuController == null)
        {
            var go = new GameObject("DSURuntime");
            dsuController = go.AddComponent<DialogueSystemController>();
            GameObject.DontDestroyOnLoad(go);
        }

        luaBridge = new LuaBridge();
        luaBridge.RegisterAll(this);
    }

    internal CultivationStorySystem GetStorySystem()
    {
        if (storySystem == null)
            storySystem = this.GetSystem<CultivationStorySystem>();
        return storySystem;
    }

    public void StartNpcConversation(string conversationTitle,
        CultivationSaveData saveData, Action onEnd = null)
    {
        saveData?.EnsureDefaults();
        luaBridge.PushSaveData(saveData);
        pendingOnEnd = onEnd;
        DialogueManager.instance.conversationEnded += HandleConversationEnd;
        DialogueManager.StartConversation(conversationTitle);
    }

    private void HandleConversationEnd(Transform actor)
    {
        DialogueManager.instance.conversationEnded -= HandleConversationEnd;
        luaBridge.PullSaveData(null);
        pendingOnEnd?.Invoke();
        pendingOnEnd = null;
    }
}
```

### 2. LuaBridge — DSU ↔ 游戏状态同步

DSU 用 Lua 表达式判断条件、执行效果。通过注册 Custom Lua Function 来桥接 C#。

**读取类（DSU Condition 框使用）：**
- `GameState_RealmTier()` → `saveData.realmTier`
- `GameState_SpiritCrystals()` → 当前境界对应档位的灵石数量
- `GameState_HasFlag("id")` → 检查 `saveData.storyFlags`
- `GameState_HasItem("id")` → `saveData.GetItemCount("id") > 0`
- `GameState_ItemCount("id")` → 读取物品数量
- `GameState_BagCapacity()` / `GameState_BagUsedSlots()` → 读取背包状态
- `GameState_IsSectDisciple()` / `GameState_SectId()` → 读取身份信息

**写入类（DSU Script 框使用）：**
- `GameState_SetFlag("id")` → 向 `saveData.storyFlags` 追加 flag
- `GameState_AddQi(amount)` → 直接增加 `saveData.qi`
- `GameState_AddCrystals(amount)` → 给当前境界档位灵石加值
- `GameState_GrantItem("id", count)` → `saveData.TryAddItem("id", count)`
- `GameState_RecordStory(storyId, nodeId, resultText)` → 写入 `CultivationStorySystem`

### 3. NPC 系统改造

`NpcChoiceDefinition` 已增加 `ConversationTitle` 字段，`CultivationNpcSystem.ExecuteChoice()` 会优先检测该字段：

- 有 `ConversationTitle`：交给 `CultivationDialogueSystem.StartNpcConversation()`
- 没有 `ConversationTitle`：继续走原先硬编码效果

当前首个接线样例：

- NPC: `清岚长老`
- Choice: `请教修行关窍`
- Conversation Title: `清岚长老`

### 4. 剧情联动

已具备 `GameState_RecordStory(...)` 与 `Story_RecordSignal(...)` Lua 桥，二者都会写入 `CultivationStorySystem`：

- `GameState_SetFlag("met_qinglan")`
- `GameState_RecordStory("sect_mentor", "opening", "清岚长老提点了当前修行关窍。")`
- `Story_RecordSignal("sect_mentor", "opening", "清岚长老提点了当前修行关窍。")`

当前还补上了一条事件触发约定：

- 如果前往地区时存在标题为 `region_intro_<regionId>` 的 DSU Conversation，且 `storyFlags` 中尚未记录 `region_intro:<regionId>`，系统会在切场景前先播放这段事件对话
- 对话结束后会自动记录 `StoryId = "region_intro"`、`NodeId = <regionId>`，用于避免重复触发

### 5. UI 层

推荐使用 DSU 自带 UI（`StandardDialogueUI`），替换素材为水墨风格。DSU 的 Canvas 单独设置 `sortingOrder` 在 UIKit Panel 之上。

## 注意事项

| 风险点 | 应对 |
|---|---|
| DSU 与 QFramework UIKit 的 UI 堆叠 | DSU Canvas 单独一层，设置 sortingOrder 在 UIKit Panel 之上 |
| DSU Lua 无法直接访问 C# 泛型方法 | LuaBridge 提供非泛型包装器 |
| 对话期间游戏状态变更 | 对话启动时 snapshot = saveData.Clone()，结束时写回主存档 |
| DSU 对话是协程驱动 | DialogueSystemController 挂载在 DontDestroyOnLoad 对象上 |
| DSU Quest 系统和现有 TaskSystem 冲突 | 初期只使用 Dialogue 功能，任务仍走现有系统 |

## 一句话总结

> **CultivationDialogueSystem（新 ISystem）做桥梁，CultivationNpcSystem 只负责触发条件判断，DSU 管对话树呈现和 Lua 条件/效果执行，CultivationStorySystem 被动接收 DSU 事件记录剧情进度，CultivationConditionSystem 以 Lua 函数形式暴露给 DSU 做条件判定。**

## 下一步建议

1. 在 DSU 数据库中创建一条 `region_intro_green_stone_gate` 之类的首次地区见闻对话
2. 在对应节点里统一使用 `Story_RecordSignal(...)` 记录更细粒度的剧情推进
3. 再迁移 1-2 个常驻 NPC，把 `ConversationTitle` 模式从单点样例扩成稳定流程
4. 视 UI 需求决定是否开始替换 DSU 默认界面皮肤
