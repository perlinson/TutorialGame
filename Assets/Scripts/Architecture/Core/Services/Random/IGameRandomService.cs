using System;
using QFramework;
using UnityEngine;

/// <summary>
/// U4 IGameRandomService：带种子的随机源。
/// - <see cref="Reseed"/> 由战斗 / 远征 开始时调用，以"区域+回合数+存档时间戳"等组合写入种子，便于回放。
/// - 默认未播种时复用 <see cref="UnityEngine.Random"/>，避免破坏现有调用。
/// 通过实现 <see cref="IGameRandomSource"/> 接口，可直接传给 <see cref="CultivationDamageSystem"/>。
/// </summary>
public interface IGameRandomService : IUtility, IGameRandomSource
{
    int CurrentSeed { get; }
    bool IsSeeded { get; }
    void Reseed(int seed);
    void ClearSeed();
}

public sealed class GameRandomService : IGameRandomService
{
    private System.Random rng;
    private int currentSeed;
    private bool isSeeded;

    public int CurrentSeed => currentSeed;
    public bool IsSeeded => isSeeded;

    public void Reseed(int seed)
    {
        currentSeed = seed;
        rng = new System.Random(seed);
        isSeeded = true;
    }

    public void ClearSeed()
    {
        rng = null;
        currentSeed = 0;
        isSeeded = false;
    }

    public int Range(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive)
        {
            return minInclusive;
        }

        if (rng != null)
        {
            return rng.Next(minInclusive, maxExclusive);
        }

        return UnityEngine.Random.Range(minInclusive, maxExclusive);
    }

    public float Range01()
    {
        if (rng != null)
        {
            return (float)rng.NextDouble();
        }

        return UnityEngine.Random.value;
    }
}
