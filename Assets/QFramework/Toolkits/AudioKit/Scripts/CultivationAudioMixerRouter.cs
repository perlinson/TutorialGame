using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;

public static class CultivationAudioMixerRouter
{
    private const string MixerResourcePath = "Audio/CultivationAudioMixer";
    private const string MusicVolumeParameter = "MusicVolume";
    private const string SfxVolumeParameter = "SfxVolume";
    private const string VoiceVolumeParameter = "VoiceVolume";
    private const float MinDb = -80f;
    private const float DuckDbMax = 24f;

    private static AudioMixer mixer;
    private static AudioMixerGroup musicGroup;
    private static AudioMixerGroup sfxGroup;
    private static AudioMixerGroup voiceGroup;
    private static bool groupsResolved;
    private static readonly Dictionary<string, float> MusicDuckRequests = new Dictionary<string, float>(StringComparer.Ordinal);
    private static float musicVolume = 1f;
    private static float sfxVolume = 1f;
    private static float voiceVolume = 1f;
    private static bool warnedMissingMixer;
    private static bool warnedMissingExposedParameters;

    public static void Route(AudioSource source, CultivationAudioBus bus)
    {
        if (source == null)
        {
            return;
        }

        if (!groupsResolved)
        {
            ResolveGroups();
        }

        switch (bus)
        {
            case CultivationAudioBus.Music:
                source.outputAudioMixerGroup = musicGroup;
                break;
            case CultivationAudioBus.Voice:
                source.outputAudioMixerGroup = voiceGroup;
                break;
            default:
                source.outputAudioMixerGroup = sfxGroup;
                break;
        }
    }

    public static void ApplyUserVolumes(float normalizedMusicVolume, float normalizedSfxVolume, float normalizedVoiceVolume)
    {
        musicVolume = Mathf.Clamp01(normalizedMusicVolume);
        sfxVolume = Mathf.Clamp01(normalizedSfxVolume);
        voiceVolume = Mathf.Clamp01(normalizedVoiceVolume);
        ApplyMixerLevels();
    }

    public static void SetMusicDuck(string reason, bool enabled, float duckDb = 8f)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return;
        }

        if (enabled)
        {
            MusicDuckRequests[reason] = Mathf.Clamp(duckDb, 0f, DuckDbMax);
        }
        else
        {
            MusicDuckRequests.Remove(reason);
        }

        ApplyMusicLevel();
    }

    public static void ClearMusicDucks()
    {
        if (MusicDuckRequests.Count == 0)
        {
            return;
        }

        MusicDuckRequests.Clear();
        ApplyMusicLevel();
    }

    private static void ResolveGroups()
    {
        groupsResolved = true;
        mixer = Resources.Load<AudioMixer>(MixerResourcePath);
        if (mixer == null)
        {
            WarnMissingMixer();
            return;
        }

        musicGroup = FindGroup("Music");
        sfxGroup = FindGroup("Sfx");
        voiceGroup = FindGroup("Voice");
        ApplyMixerLevels();
    }

    private static AudioMixerGroup FindGroup(string name)
    {
        if (mixer == null || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var groups = mixer.FindMatchingGroups(name);
        if (groups == null || groups.Length == 0)
        {
            return null;
        }

        for (var i = 0; i < groups.Length; i++)
        {
            if (groups[i] != null && groups[i].name == name)
            {
                return groups[i];
            }
        }

        return groups[0];
    }

    private static bool EnsureMixer()
    {
        if (!groupsResolved)
        {
            ResolveGroups();
        }

        if (mixer != null)
        {
            return true;
        }

        WarnMissingMixer();
        return false;
    }

    private static void ApplyMixerLevels()
    {
        if (!EnsureMixer())
        {
            return;
        }

        ApplyMusicLevel();
        SetParameter(SfxVolumeParameter, NormalizedToDb(sfxVolume));
        SetParameter(VoiceVolumeParameter, NormalizedToDb(voiceVolume));
    }

    private static void ApplyMusicLevel()
    {
        if (!EnsureMixer())
        {
            return;
        }

        SetParameter(MusicVolumeParameter, NormalizedToDb(musicVolume) - GetActiveMusicDuckDb());
    }

    private static float GetActiveMusicDuckDb()
    {
        var maxDuckDb = 0f;
        foreach (var request in MusicDuckRequests)
        {
            if (request.Value > maxDuckDb)
            {
                maxDuckDb = request.Value;
            }
        }

        return maxDuckDb;
    }

    private static void SetParameter(string parameterName, float valueDb)
    {
        if (mixer == null || string.IsNullOrWhiteSpace(parameterName))
        {
            return;
        }

        if (!mixer.SetFloat(parameterName, Mathf.Clamp(valueDb, MinDb, 0f)))
        {
            WarnMissingExposedParameters(parameterName);
        }
    }

    private static float NormalizedToDb(float normalized)
    {
        if (normalized <= 0.0001f)
        {
            return MinDb;
        }

        return Mathf.Clamp(Mathf.Log10(Mathf.Clamp01(normalized)) * 20f, MinDb, 0f);
    }

    private static void WarnMissingMixer()
    {
        if (warnedMissingMixer)
        {
            return;
        }

        warnedMissingMixer = true;
        Debug.LogWarning("CultivationAudioMixerRouter could not load mixer at Resources/" + MixerResourcePath + ".");
    }

    private static void WarnMissingExposedParameters(string parameterName)
    {
        if (warnedMissingExposedParameters)
        {
            return;
        }

        warnedMissingExposedParameters = true;
        Debug.LogWarning("CultivationAudioMixerRouter could not set exposed mixer parameter: " + parameterName + ".");
    }
}
