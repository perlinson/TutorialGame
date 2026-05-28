using System.Collections.Generic;
using System.IO;
using PixelCrushers.DialogueSystem;
using UnityEditor;
using UnityEngine;

public static class CultivationDialogueDatabaseBootstrap
{
    private const string DatabaseDirectory = "Assets/Resources/Dialogue";
    private const string DatabaseAssetPath = DatabaseDirectory + "/CultivationDialogueDatabase.asset";
    private const int PlayerActorId = 1;
    private const int ElderActorId = 2;
    private const int ConversationId = 1;

    [MenuItem("Tools/Cultivation/DSU/Create Or Update Default Dialogue Database")]
    public static void CreateOrUpdateDefaultDialogueDatabase()
    {
        EnsureDirectory(DatabaseDirectory);

        var database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(DatabaseAssetPath);
        if (database == null)
        {
            database = DialogueSystemMenuItems.CreateDialogueDatabaseInstance();
            database.name = "CultivationDialogueDatabase";
            AssetDatabase.CreateAsset(database, DatabaseAssetPath);
        }

        EnsureDatabaseDefaults(database);
        EnsureActor(database, PlayerActorId, "Player", true);
        EnsureActor(database, ElderActorId, "清岚长老", false);
        EnsureConversation(database);
        database.ResetCache();

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = database;
        Debug.Log("[CultivationDialogueDatabaseBootstrap] 已生成或更新默认 DSU 数据库: " + DatabaseAssetPath);
    }

    private static void EnsureConversation(DialogueDatabase database)
    {
        var conversation = database.GetConversation("清岚长老");
        if (conversation == null)
        {
            conversation = new Conversation
            {
                id = ConversationId,
                fields = new List<Field>()
            };
            conversation.Title = "清岚长老";
            conversation.ActorID = ElderActorId;
            conversation.ConversantID = PlayerActorId;
            database.conversations.Add(conversation);
        }
        else
        {
            conversation.fields ??= new List<Field>();
        }

        conversation.Title = "清岚长老";
        conversation.ActorID = ElderActorId;
        conversation.ConversantID = PlayerActorId;
        conversation.dialogueEntries = BuildQingLanEntries();
        conversation.entryGroups = new List<EntryGroup>();
    }

    private static List<DialogueEntry> BuildQingLanEntries()
    {
        var entries = new List<DialogueEntry>();

        var start = CreateEntry(0, "START", string.Empty, string.Empty, PlayerActorId, ElderActorId, true);
        start.outgoingLinks.Add(new Link(ConversationId, 0, ConversationId, 1));
        entries.Add(start);

        var opening = CreateEntry(1, "Opening", "你来得正好。经阁静，正适合把心念捋顺。你想问哪一处修行关窍？", string.Empty, ElderActorId, PlayerActorId);
        opening.outgoingLinks.Add(new Link(ConversationId, 1, ConversationId, 2));
        opening.outgoingLinks.Add(new Link(ConversationId, 1, ConversationId, 3));
        entries.Add(opening);

        var askQi = CreateEntry(2, "AskQi", "弟子近来气机浮动，想请长老点拨吐纳转运之法。", "请教吐纳运气", PlayerActorId, ElderActorId);
        askQi.userScript = "GameState_AddQi(2); GameState_RecordStory(\"sect_mentor\", \"qi_guidance\", \"清岚长老指点了吐纳运气的关窍。\");";
        askQi.outgoingLinks.Add(new Link(ConversationId, 2, ConversationId, 4));
        entries.Add(askQi);

        var askHeart = CreateEntry(3, "AskHeart", "弟子担心杂念滋长，想请长老指点如何定心守神。", "请教守心定神", PlayerActorId, ElderActorId);
        askHeart.userScript = "GameState_SetFlag(\"met_qinglan\"); GameState_RecordStory(\"sect_mentor\", \"heart_guidance\", \"清岚长老提醒你先守心，再谈破境。\");";
        askHeart.outgoingLinks.Add(new Link(ConversationId, 3, ConversationId, 5));
        entries.Add(askHeart);

        var replyQi = CreateEntry(4, "ReplyQi", "气先顺，意才稳。你回去后先把周天走慢半拍，宁缓勿乱，自会多出两分火候。", string.Empty, ElderActorId, PlayerActorId);
        entries.Add(replyQi);

        var replyHeart = CreateEntry(5, "ReplyHeart", "心若先急，法便先乱。记住今日这句话，往后每临关口，先问自己是否还守得住本心。", string.Empty, ElderActorId, PlayerActorId);
        entries.Add(replyHeart);

        return entries;
    }

    private static DialogueEntry CreateEntry(int id, string title, string dialogueText, string menuText, int actorId, int conversantId, bool isRoot = false)
    {
        var entry = new DialogueEntry
        {
            id = id,
            conversationID = ConversationId,
            isRoot = isRoot,
            fields = new List<Field>(),
            outgoingLinks = new List<Link>(),
            conditionPriority = ConditionPriority.Normal,
            falseConditionAction = string.Empty,
            userScript = string.Empty
        };

        entry.Title = title;
        entry.ActorID = actorId;
        entry.ConversantID = conversantId;
        entry.DialogueText = dialogueText;
        entry.MenuText = string.IsNullOrWhiteSpace(menuText) ? dialogueText : menuText;
        entry.Sequence = string.Empty;
        entry.canvasRect = new Rect(40f + id * 190f, 40f + (id % 2) * 120f, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
        return entry;
    }

    private static void EnsureActor(DialogueDatabase database, int actorId, string actorName, bool isPlayer)
    {
        var actor = database.GetActor(actorId);
        if (actor == null)
        {
            actor = new Actor
            {
                id = actorId,
                fields = new List<Field>()
            };
            database.actors.Add(actor);
        }

        actor.Name = actorName;
        actor.IsPlayer = isPlayer;
    }

    private static void EnsureDatabaseDefaults(DialogueDatabase database)
    {
        if (database == null)
        {
            return;
        }

        database.actors ??= new List<Actor>();
        database.conversations ??= new List<Conversation>();
        database.variables ??= new List<Variable>();
        database.items ??= new List<Item>();
        database.locations ??= new List<Location>();
        database.globalUserScript = string.IsNullOrWhiteSpace(database.globalUserScript)
            ? "-- Generated by CultivationDialogueDatabaseBootstrap"
            : database.globalUserScript;
        if (database.emphasisSettings == null || database.emphasisSettings.Length < DialogueDatabase.NumEmphasisSettings)
        {
            database.ResetEmphasisSettings();
        }
    }

    private static void EnsureDirectory(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return;
        }

        var parent = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
        var folder = Path.GetFileName(assetPath);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureDirectory(parent);
        }

        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folder))
        {
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
