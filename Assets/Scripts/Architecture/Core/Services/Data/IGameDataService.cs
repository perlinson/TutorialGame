using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 统一配置表加载入口。
/// 约定：
///   - ScriptableObject 数据库（功法 / 技能 / Buff / 怪物 / 物品 / 区域 等核心定义）通过 <see cref="LoadAsset{T}"/> 读取。
///   - Json/Csv 数值表（伤害曲线 / 商店行情 / 概率表 等高频调参内容）通过 <see cref="LoadJson{T}"/> 读取。
///   - 加载结果按"路径 + 类型"缓存；编辑器或开发模式可通过 <see cref="ClearCache"/> / <see cref="Reload"/> 手动刷新。
/// </summary>
public interface IGameDataService : IUtility
{
    /// <summary>加载 ScriptableObject 配置数据库。</summary>
    /// <param name="resourcePath">相对 Resources 的路径，例如 "Data/TaskDatabase"。</param>
    T LoadAsset<T>(string resourcePath) where T : ScriptableObject;

    /// <summary>加载 Json/Csv 文本并反序列化（默认走 <see cref="JsonUtility"/>）。</summary>
    /// <param name="resourcePath">相对 Resources 的路径（不含扩展名），例如 "Config/DamageCurves"。</param>
    T LoadJson<T>(string resourcePath) where T : class, new();

    /// <summary>查询缓存里是否已经加载过该路径（任意类型）。</summary>
    bool IsCached(string resourcePath);

    /// <summary>清除缓存，下次读取时会重新走 <see cref="GameResource"/>。</summary>
    void ClearCache();

    /// <summary>等价于 <see cref="ClearCache"/>，更明确的"重新载入"语义。</summary>
    void Reload();
}

public sealed class GameDataService : IGameDataService
{
    private readonly Dictionary<string, ScriptableObject> assetCache = new Dictionary<string, ScriptableObject>();
    private readonly Dictionary<string, object> jsonCache = new Dictionary<string, object>();

    public T LoadAsset<T>(string resourcePath) where T : ScriptableObject
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return null;
        }

        var key = BuildKey(typeof(T), resourcePath);
        if (assetCache.TryGetValue(key, out var cached) && cached is T typed)
        {
            return typed;
        }

        var asset = GameResource.Load<T>(resourcePath);
        if (asset == null)
        {
            return null;
        }

        assetCache[key] = asset;
        return asset;
    }

    public T LoadJson<T>(string resourcePath) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return null;
        }

        var key = BuildKey(typeof(T), resourcePath);
        if (jsonCache.TryGetValue(key, out var cached) && cached is T typed)
        {
            return typed;
        }

        var textAsset = GameResource.Load<TextAsset>(resourcePath);
        if (textAsset == null || string.IsNullOrEmpty(textAsset.text))
        {
            return null;
        }

        T parsed;
        try
        {
            parsed = JsonUtility.FromJson<T>(textAsset.text);
        }
        catch (Exception ex)
        {
            GameLog.Warning("GameDataService failed to parse json `" + resourcePath + "`: " + ex.Message);
            return null;
        }

        if (parsed == null)
        {
            return null;
        }

        jsonCache[key] = parsed;
        return parsed;
    }

    public bool IsCached(string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return false;
        }

        foreach (var key in assetCache.Keys)
        {
            if (key.EndsWith("|" + resourcePath, StringComparison.Ordinal))
            {
                return true;
            }
        }

        foreach (var key in jsonCache.Keys)
        {
            if (key.EndsWith("|" + resourcePath, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public void ClearCache()
    {
        assetCache.Clear();
        jsonCache.Clear();
    }

    public void Reload()
    {
        ClearCache();
    }

    private static string BuildKey(Type type, string resourcePath)
    {
        return type.FullName + "|" + resourcePath;
    }
}
