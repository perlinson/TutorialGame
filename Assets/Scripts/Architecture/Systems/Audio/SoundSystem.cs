using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public enum SoundType
{
    Button_Low,
    Button_Confirm,
    Close,
    BattleStart,
    CombatHitLight,
    CombatHitHeavy,
    CombatMiss,
    HeroDamaged,
    HeroHealed,
    Pickup
}

public interface ISoundSystem : ISystem
{
    void PlayBackgroundMusic();
    void PlayMainMenuMusic();
    void PlayWorldMapMusic();
    void PlayExpeditionMusic(WorldRegionDefinition region);
    void PlaySound(SoundType type);
    void PlaySound(string filename);
    void StopAllSound();
    void SetSound(float volume);
    void SetBackgroundMusic(float volume);
}

public sealed class SoundSystem : AbstractSystem, ISoundSystem
{
    private const int SampleRate = 44100;

    private readonly Dictionary<string, AudioClip> cachedClips = new Dictionary<string, AudioClip>();

    private IGameAudioService audioService;
    private IGameSettingsService settingsService;

    protected override void OnInit()
    {
        audioService = this.GetUtility<IGameAudioService>();
        settingsService = this.GetUtility<IGameSettingsService>();
        audioService.Initialize();
    }

    public void PlayBackgroundMusic()
    {
        PlayMainMenuMusic();
    }

    public void PlayMainMenuMusic()
    {
        audioService.PlayMusic(
            LoadOrCreateMusic("Audio/Bgm/MainMenu", "fallback_bgm_main_menu", () => CreateAmbientMusicClip("MainMenuBgm", 196f, 247f, 311f)),
            true,
            0.72f);
    }

    public void PlayWorldMapMusic()
    {
        audioService.PlayMusic(
            LoadOrCreateMusic("Audio/Bgm/WorldMap", "fallback_bgm_world_map", () => CreateAmbientMusicClip("WorldMapBgm", 174f, 220f, 261.63f)),
            true,
            0.7f);
    }

    public void PlayExpeditionMusic(WorldRegionDefinition region)
    {
        var clip = TryLoadAudioClipSilently("Audio/Bgm/Expedition");
        if (clip == null)
        {
            var root = Mathf.Lerp(130f, 190f, region != null ? Mathf.Clamp01(region.RequiredRealmTier / 6f) : 0.35f);
            clip = GetOrCreateClip(
                "fallback_bgm_expedition_" + Mathf.RoundToInt(root),
                () => CreateAmbientMusicClip("ExpeditionBgm", root, root * 1.26f, root * 1.52f));
        }

        audioService.PlayMusic(clip, true, 0.68f);
    }

    public void PlaySound(SoundType type)
    {
        switch (type)
        {
            case SoundType.Button_Confirm:
                audioService.PlaySfx(LoadOrCreateSfx("Audio/Sfx/UiConfirm", "fallback_sfx_ui_confirm", () => CreateUiConfirmClip("UiConfirm")), 0.92f);
                break;
            case SoundType.Close:
                audioService.PlaySfx(LoadOrCreateSfx("Audio/Sfx/UiCancel", "fallback_sfx_ui_cancel", () => CreateUiCancelClip("UiCancel")), 0.86f);
                break;
            case SoundType.BattleStart:
                audioService.PlaySfx(LoadOrCreateSfx("Audio/Sfx/BattleStart", "fallback_sfx_battle_start", () => CreateBattleStartClip("BattleStart")), 0.96f);
                break;
            case SoundType.CombatHitLight:
                audioService.PlaySfx(LoadOrCreateSfx("Audio/Sfx/CombatHitLight", "fallback_sfx_combat_hit_light", () => CreateCombatHitClip("CombatHitLight", 0.12f, 220f)), 0.82f);
                break;
            case SoundType.CombatHitHeavy:
                audioService.PlaySfx(LoadOrCreateSfx("Audio/Sfx/CombatHitHeavy", "fallback_sfx_combat_hit_heavy", () => CreateCombatHitClip("CombatHitHeavy", 0.17f, 156f)), 1f);
                break;
            case SoundType.CombatMiss:
                audioService.PlaySfx(LoadOrCreateSfx("Audio/Sfx/CombatMiss", "fallback_sfx_combat_miss", () => CreateCombatMissClip("CombatMiss")), 0.76f);
                break;
            case SoundType.HeroDamaged:
                audioService.PlaySfx(LoadOrCreateSfx("Audio/Sfx/HeroDamaged", "fallback_sfx_hero_damaged", () => CreateCombatHitClip("HeroDamaged", 0.15f, 110f)), 0.94f);
                break;
            case SoundType.HeroHealed:
                audioService.PlaySfx(LoadOrCreateSfx("Audio/Sfx/HeroHealed", "fallback_sfx_hero_healed", () => CreateHeroHealedClip("HeroHealed")), 0.88f);
                break;
            case SoundType.Pickup:
                audioService.PlaySfx(LoadOrCreateSfx("Audio/Sfx/Pickup", "fallback_sfx_pickup", () => CreatePickupClip("Pickup")), 0.74f);
                break;
            case SoundType.Button_Low:
            default:
                audioService.PlaySfx(LoadOrCreateSfx("Audio/Sfx/UiClick", "fallback_sfx_ui_click", () => CreateUiClickClip("UiClick")), 0.8f);
                break;
        }
    }

    public void PlaySound(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return;
        }

        var normalized = filename.Replace('\\', '/');
        var directClip = TryLoadAudioClipSilently(normalized);
        if (directClip != null)
        {
            audioService.PlaySfx(directClip, 1f);
            return;
        }

        var prefixedPath = normalized.Contains("/") ? normalized : "Audio/Sfx/" + normalized;
        var prefixedClip = TryLoadAudioClipSilently(prefixedPath);
        if (prefixedClip != null)
        {
            audioService.PlaySfx(prefixedClip, 1f);
        }
    }

    public void StopAllSound()
    {
        audioService.StopVoice();
        audioService.StopMusic();
        AudioKit.StopAllSound();
    }

    public void SetSound(float volume)
    {
        settingsService.SetSfxVolume(volume);
    }

    public void SetBackgroundMusic(float volume)
    {
        settingsService.SetMusicVolume(volume);
    }

    private AudioClip LoadOrCreateMusic(string resourcePath, string cacheKey, Func<AudioClip> factory)
    {
        var clip = TryLoadAudioClipSilently(resourcePath);
        return clip != null ? clip : GetOrCreateClip(cacheKey, factory);
    }

    private AudioClip LoadOrCreateSfx(string resourcePath, string cacheKey, Func<AudioClip> factory)
    {
        var clip = TryLoadAudioClipSilently(resourcePath);
        return clip != null ? clip : GetOrCreateClip(cacheKey, factory);
    }

    private AudioClip TryLoadAudioClipSilently(string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return null;
        }

        var clip = Resources.Load<AudioClip>(resourcePath);
        if (clip != null)
        {
            return clip;
        }

        var loweredPath = resourcePath.ToLowerInvariant();
        return loweredPath == resourcePath ? null : Resources.Load<AudioClip>(loweredPath);
    }

    private AudioClip GetOrCreateClip(string key, Func<AudioClip> factory)
    {
        if (cachedClips.TryGetValue(key, out var clip) && clip != null)
        {
            return clip;
        }

        clip = factory != null ? factory() : null;
        if (clip != null)
        {
            cachedClips[key] = clip;
        }

        return clip;
    }

    private static AudioClip CreateAmbientMusicClip(string clipName, float rootFrequency, float harmonyFrequency, float accentFrequency)
    {
        return CreateClip(clipName, 4.8f, time =>
        {
            var bed = Mathf.Sin(Mathf.PI * 2f * rootFrequency * time) * 0.28f;
            bed += Mathf.Sin(Mathf.PI * 2f * harmonyFrequency * time) * 0.18f;
            bed += Mathf.Sin(Mathf.PI * 2f * accentFrequency * time) * 0.12f;

            var pulse = 0.78f + 0.22f * Mathf.Sin(Mathf.PI * 2f * 0.5f * time);
            var shimmer = Mathf.Sin(Mathf.PI * 2f * (accentFrequency * 2f) * time + Mathf.Sin(Mathf.PI * 2f * 0.25f * time) * 0.35f) * 0.05f;
            return (bed * pulse + shimmer) * 0.55f;
        }, true);
    }

    private static AudioClip CreateUiClickClip(string clipName)
    {
        return CreateClip(clipName, 0.08f, time =>
        {
            var t = time / 0.08f;
            var frequency = Mathf.Lerp(1160f, 860f, t);
            var envelope = Mathf.Exp(-10f * t);
            return Mathf.Sin(Mathf.PI * 2f * frequency * time) * envelope * 0.45f;
        }, false);
    }

    private static AudioClip CreateUiConfirmClip(string clipName)
    {
        return CreateClip(clipName, 0.18f, time =>
        {
            var noteA = Mathf.Sin(Mathf.PI * 2f * 523.25f * time) * Mathf.Exp(-7f * time);
            var noteB = time > 0.05f ? Mathf.Sin(Mathf.PI * 2f * 659.25f * (time - 0.05f)) * Mathf.Exp(-9f * (time - 0.05f)) : 0f;
            return (noteA + noteB) * 0.34f;
        }, false);
    }

    private static AudioClip CreateUiCancelClip(string clipName)
    {
        return CreateClip(clipName, 0.16f, time =>
        {
            var noteA = Mathf.Sin(Mathf.PI * 2f * 392f * time) * Mathf.Exp(-7f * time);
            var noteB = time > 0.04f ? Mathf.Sin(Mathf.PI * 2f * 311.13f * (time - 0.04f)) * Mathf.Exp(-8f * (time - 0.04f)) : 0f;
            return (noteA + noteB) * 0.3f;
        }, false);
    }

    private static AudioClip CreateBattleStartClip(string clipName)
    {
        return CreateClip(clipName, 0.32f, time =>
        {
            var sweep = Mathf.Lerp(140f, 260f, Mathf.Clamp01(time / 0.2f));
            var strike = Mathf.Sin(Mathf.PI * 2f * sweep * time) * Mathf.Exp(-4f * time);
            var undertone = Mathf.Sin(Mathf.PI * 2f * 82f * time) * Mathf.Exp(-3f * time) * 0.45f;
            return (strike + undertone) * 0.42f;
        }, false);
    }

    private static AudioClip CreateCombatHitClip(string clipName, float duration, float bodyFrequency)
    {
        return CreateClip(clipName, duration, time =>
        {
            var normalized = time / duration;
            var body = Mathf.Sin(Mathf.PI * 2f * bodyFrequency * time) * Mathf.Exp(-13f * normalized);
            var crack = (Mathf.PerlinNoise(time * 82f, bodyFrequency * 0.01f) * 2f - 1f) * Mathf.Exp(-18f * normalized) * 0.62f;
            return body * 0.32f + crack * 0.24f;
        }, false);
    }

    private static AudioClip CreateCombatMissClip(string clipName)
    {
        return CreateClip(clipName, 0.14f, time =>
        {
            var normalized = time / 0.14f;
            var swoosh = (Mathf.PerlinNoise(time * 50f, 0.17f) * 2f - 1f) * Mathf.Exp(-5f * normalized);
            var air = Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(780f, 420f, normalized) * time) * Mathf.Exp(-7f * normalized);
            return swoosh * 0.2f + air * 0.16f;
        }, false);
    }

    private static AudioClip CreateHeroHealedClip(string clipName)
    {
        return CreateClip(clipName, 0.28f, time =>
        {
            var low = Mathf.Sin(Mathf.PI * 2f * 440f * time) * Mathf.Exp(-5.5f * time);
            var high = time > 0.06f ? Mathf.Sin(Mathf.PI * 2f * 659.25f * (time - 0.06f)) * Mathf.Exp(-7f * (time - 0.06f)) : 0f;
            return (low + high) * 0.24f;
        }, false);
    }

    private static AudioClip CreatePickupClip(string clipName)
    {
        return CreateClip(clipName, 0.2f, time =>
        {
            var normalized = time / 0.2f;
            var sparkle = Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(720f, 1080f, normalized) * time) * Mathf.Exp(-8f * normalized);
            var overtone = time > 0.03f ? Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(980f, 1460f, normalized) * (time - 0.03f)) * Mathf.Exp(-12f * normalized) : 0f;
            return (sparkle + overtone) * 0.18f;
        }, false);
    }

    private static AudioClip CreateClip(string clipName, float duration, Func<float, float> sampleGenerator, bool loopSafe)
    {
        var sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
        var samples = new float[sampleCount];
        for (var i = 0; i < sampleCount; i++)
        {
            var time = i / (float) SampleRate;
            samples[i] = Mathf.Clamp(sampleGenerator != null ? sampleGenerator(time) : 0f, -1f, 1f);
        }

        if (!loopSafe)
        {
            ApplyFade(samples, Mathf.Min(sampleCount / 3, Mathf.RoundToInt(SampleRate * 0.012f)));
        }

        var clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static void ApplyFade(float[] samples, int fadeSamples)
    {
        if (samples == null || samples.Length == 0 || fadeSamples <= 0)
        {
            return;
        }

        fadeSamples = Mathf.Min(fadeSamples, samples.Length / 2);
        for (var i = 0; i < fadeSamples; i++)
        {
            var t = i / (float) fadeSamples;
            samples[i] *= t;
            samples[samples.Length - 1 - i] *= t;
        }
    }
}
