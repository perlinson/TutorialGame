using PixelCrushers.DialogueSystem;
using QFramework;
using System;
using System.Text;
using UnityEngine;

/// <summary>
/// DSU 对话系统桥梁 — ISystem 封装 DSU 的初始化、对话启动/结束、数据同步。
/// 不继承 MonoBehaviour，通过 QFramework ISystem 模式提供对话服务。
/// </summary>
public sealed class CultivationDialogueSystem : AbstractSystem
{
    private const string DefaultDatabaseResourcePath = "Dialogue/CultivationDialogueDatabase";

    private DialogueSystemController dsuController;
    private LuaBridge luaBridge;

    private CultivationStorySystem storySystem;
    private CultivationDialogueBindingSystem dialogueBindingSystem;

    private Action pendingOnEnd;

    protected override void OnInit()
    {
        EnsureRuntimeController();

        // 2. 注册 Lua 函数桥
        luaBridge = new LuaBridge();
        luaBridge.RegisterAll(this);
    }

    private void EnsureRuntimeController()
    {
        dsuController = UnityEngine.Object.FindObjectOfType<DialogueSystemController>();
        if (dsuController == null)
        {
            if (RuntimeShutdownTracker.IsShuttingDown)
            {
                return;
            }

            var go = new GameObject("DSURuntime");
            dsuController = go.AddComponent<DialogueSystemController>();
            UnityEngine.Object.DontDestroyOnLoad(go);
            Debug.LogWarning("[CultivationDialogueSystem] 未在场景中找到 DialogueSystemController，已创建临时 DSURuntime。请在 Inspector 中补上 Initial Database 与 Dialogue UI。");
        }

        if (dsuController == null)
        {
            return;
        }

        if (dsuController.initialDatabase == null)
        {
            var database = Resources.Load<DialogueDatabase>(DefaultDatabaseResourcePath);
            if (database != null)
            {
                dsuController.initialDatabase = database;
                Debug.Log("[CultivationDialogueSystem] 已自动加载默认 Dialogue Database: " + DefaultDatabaseResourcePath);
            }
        }

        EnsureDatabaseLoaded();
    }

    private void EnsureDatabaseLoaded()
    {
        if (dsuController == null || dsuController.initialDatabase == null)
        {
            return;
        }

        var masterDatabase = DialogueManager.masterDatabase;
        if (masterDatabase != null && masterDatabase.conversations != null && masterDatabase.conversations.Count > 0)
        {
            return;
        }

        if (dsuController.databaseManager == null)
        {
            return;
        }

        dsuController.databaseManager.defaultDatabase = dsuController.initialDatabase;
        dsuController.ResetDatabase();
        TryRegisterInitialDatabase();
    }

    /// <summary>
    /// 供 LuaBridge 在注册时获取依赖系统。
    /// </summary>
    internal CultivationStorySystem GetStorySystem()
    {
        if (storySystem == null)
            storySystem = this.GetSystem<CultivationStorySystem>();
        return storySystem;
    }

    internal CultivationDialogueBindingSystem GetDialogueBindingSystem()
    {
        dialogueBindingSystem ??= this.GetSystem<CultivationDialogueBindingSystem>();
        return dialogueBindingSystem;
    }

    /// <summary>
    /// DSU 运行时是否已就绪。
    /// </summary>
    public bool IsReady => dsuController != null && dsuController.isActiveAndEnabled && dsuController.isInitialized;

    public bool HasDatabase =>
        dsuController != null &&
        dsuController.initialDatabase != null &&
        DialogueManager.masterDatabase != null &&
        DialogueManager.masterDatabase.conversations != null;

    public bool HasConversation(string conversationTitle)
    {
        EnsureRuntimeController();
        return FindConversation(conversationTitle) != null;
    }

    /// <summary>
    /// 当前是否有对话正在进行。
    /// </summary>
    public bool IsConversationActive => DialogueManager.isConversationActive;

    // ═══════════════════════════════════════════
    //  对话入口
    // ═══════════════════════════════════════════

    /// <summary>
    /// 启动 NPC 对话。conversationTitle 对应 DSU Conversation 资产名。
    /// 对话结束后自动触发 onEnd 回调。
    /// </summary>
    public void StartNpcConversation(string conversationTitle, CultivationSaveData saveData, Action onEnd = null)
    {
        TryStartConversationInternal(conversationTitle, saveData, onEnd, "NPC", "对话");
    }

    public bool TryStartEventConversation(string conversationTitle, CultivationSaveData saveData, Action onEnd = null)
    {
        return TryStartConversationInternal(conversationTitle, saveData, onEnd, "Event", "事件对话");
    }

    /// <summary>
    /// 启动剧情事件对话。语义上区分用途，实现与 NPC 对话一致。
    /// </summary>
    public void StartEventConversation(string conversationTitle, CultivationSaveData saveData, Action onEnd = null)
    {
        TryStartEventConversation(conversationTitle, saveData, onEnd);
    }

    /// <summary>
    /// 停止当前对话。
    /// </summary>
    public void StopConversation()
    {
        if (DialogueManager.isConversationActive)
            DialogueManager.StopConversation();
    }

    // ═══════════════════════════════════════════
    //  内部
    // ═══════════════════════════════════════════

    private void HandleConversationEnd(Transform actor)
    {
        DialogueManager.instance.conversationEnded -= HandleConversationEnd;
        luaBridge.PullSaveData(null);
        GetDialogueBindingSystem()?.ClearRuntimeConversationContext();
        pendingOnEnd?.Invoke();
        pendingOnEnd = null;
    }

    private bool TryStartConversationInternal(string conversationTitle, CultivationSaveData saveData, Action onEnd, string contextLabel, string displayLabel)
    {
        EnsureRuntimeController();

        if (string.IsNullOrEmpty(conversationTitle))
        {
            Debug.LogWarning("[CultivationDialogueSystem] " + displayLabel + " conversationTitle is empty.");
            onEnd?.Invoke();
            return false;
        }

        if (!IsReady)
        {
            Debug.LogError("[CultivationDialogueSystem] DSU controller is not ready for " + displayLabel + "。");
            onEnd?.Invoke();
            return false;
        }

        if (!HasDatabase)
        {
            Debug.LogError("[CultivationDialogueSystem] DSU Initial Database 未配置或未加载，无法启动" + displayLabel + "：" + conversationTitle);
            onEnd?.Invoke();
            return false;
        }

        var conversation = FindConversation(conversationTitle);
        if (conversation == null)
        {
            Debug.LogError("[CultivationDialogueSystem] 在当前 Dialogue Database 中找不到" + displayLabel + " Conversation: " + conversationTitle + BuildConversationDiagnostics());
            onEnd?.Invoke();
            return false;
        }

        saveData?.EnsureDefaults();
        luaBridge.PushSaveData(saveData);
        pendingOnEnd = onEnd;

        DialogueManager.instance.conversationEnded += HandleConversationEnd;
        Debug.Log("[CultivationDialogueSystem] Start" + contextLabel + "Conversation -> " + conversationTitle);
        DialogueManager.StartConversation(conversationTitle);
        return true;
    }

    private Conversation FindConversation(string conversationTitle)
    {
        var masterDatabase = DialogueManager.masterDatabase;
        var conversation = FindConversationInDatabase(masterDatabase, conversationTitle);
        if (conversation != null)
        {
            return conversation;
        }

        var initialDatabase = dsuController != null ? dsuController.initialDatabase : null;
        if (initialDatabase == null)
        {
            return null;
        }

        initialDatabase.ResetCache();
        if (FindConversationInDatabase(initialDatabase, conversationTitle) == null)
        {
            return null;
        }

        TryRegisterInitialDatabase();

        return FindConversationInDatabase(DialogueManager.masterDatabase, conversationTitle);
    }

    private void TryRegisterInitialDatabase()
    {
        if (dsuController == null || dsuController.initialDatabase == null || dsuController.databaseManager == null)
        {
            return;
        }

        dsuController.databaseManager.defaultDatabase = dsuController.initialDatabase;
        dsuController.databaseManager.Add(dsuController.initialDatabase);
    }

    private static Conversation FindConversationInDatabase(DialogueDatabase database, string conversationTitle)
    {
        if (database == null || string.IsNullOrWhiteSpace(conversationTitle))
        {
            return null;
        }

        database.ResetCache();

        var conversation = database.GetConversation(conversationTitle);
        if (conversation != null)
        {
            return conversation;
        }

        if (database.conversations == null)
        {
            return null;
        }

        var normalizedTarget = NormalizeConversationTitle(conversationTitle);
        for (var i = 0; i < database.conversations.Count; i++)
        {
            var candidate = database.conversations[i];
            if (candidate == null)
            {
                continue;
            }

            if (string.Equals(NormalizeConversationTitle(candidate.Title), normalizedTarget, StringComparison.Ordinal))
            {
                return candidate;
            }
        }

        return null;
    }

    private string BuildConversationDiagnostics()
    {
        var builder = new StringBuilder();
        builder.Append(" | initialDatabase=");
        builder.Append(dsuController != null && dsuController.initialDatabase != null ? dsuController.initialDatabase.name : "<null>");

        var masterDatabase = DialogueManager.masterDatabase;
        builder.Append(" | masterDatabase=");
        builder.Append(masterDatabase != null ? masterDatabase.name : "<null>");

        builder.Append(" | availableConversations=");
        if (masterDatabase == null || masterDatabase.conversations == null || masterDatabase.conversations.Count == 0)
        {
            builder.Append("<empty>");
            return builder.ToString();
        }

        for (var i = 0; i < masterDatabase.conversations.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            var title = masterDatabase.conversations[i] != null ? masterDatabase.conversations[i].Title : null;
            builder.Append(string.IsNullOrWhiteSpace(title) ? "<untitled>" : title);
        }

        return builder.ToString();
    }

    private static string NormalizeConversationTitle(string title)
    {
        return string.IsNullOrWhiteSpace(title) ? string.Empty : title.Trim().Replace('\u3000', ' ');
    }
}
