#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public static class CultivationAudioMixerGenerator
{
    private sealed class ExposedVolumeBinding
    {
        public string GroupName;
        public string ParameterName;
    }

    private const string FolderPath = "Assets/Resources/Audio";
    private const string MixerAssetPath = FolderPath + "/CultivationAudioMixer.mixer";
    private static readonly ExposedVolumeBinding[] ExposedVolumeBindings =
    {
        new ExposedVolumeBinding { GroupName = "Music", ParameterName = "MusicVolume" },
        new ExposedVolumeBinding { GroupName = "Sfx", ParameterName = "SfxVolume" },
        new ExposedVolumeBinding { GroupName = "Voice", ParameterName = "VoiceVolume" }
    };

    [MenuItem("Tools/TutorialGame/Regenerate Audio Mixer")]
    private static void RegenerateMixer()
    {
        EnsureMixerAsset();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void EnsureMixerAsset()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder(FolderPath);

        var mixerObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(MixerAssetPath);
        if (mixerObject == null)
        {
            mixerObject = CreateMixerController();
        }

        if (mixerObject == null)
        {
            Debug.LogError("Failed to create CultivationAudioMixer asset.");
            return;
        }

        EnsureGroups(mixerObject);
        EnsureExposedVolumeParameters(mixerObject);
        EditorUtility.SetDirty(mixerObject);
    }

    private static UnityEngine.Object CreateMixerController()
    {
        var controllerType = GetControllerType();
        var createMethod = controllerType != null
            ? controllerType.GetMethod("CreateMixerControllerAtPath", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            : null;
        if (createMethod == null)
        {
            return null;
        }

        return createMethod.Invoke(null, new object[] { MixerAssetPath }) as UnityEngine.Object;
    }

    private static void EnsureGroups(UnityEngine.Object mixerObject)
    {
        var controllerType = GetControllerType();
        if (controllerType == null || mixerObject == null)
        {
            return;
        }

        var masterGroupProperty = controllerType.GetProperty("masterGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var createGroupMethod = controllerType.GetMethod("CreateNewGroup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var addChildMethod = controllerType.GetMethod("AddChildToParent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var masterGroup = masterGroupProperty != null ? masterGroupProperty.GetValue(mixerObject, null) : null;
        var mixer = mixerObject as AudioMixer;
        if (masterGroup == null || createGroupMethod == null || addChildMethod == null || mixer == null)
        {
            return;
        }

        EnsureChildGroup(mixerObject, mixer, masterGroup, createGroupMethod, addChildMethod, "Music");
        EnsureChildGroup(mixerObject, mixer, masterGroup, createGroupMethod, addChildMethod, "Sfx");
        EnsureChildGroup(mixerObject, mixer, masterGroup, createGroupMethod, addChildMethod, "Voice");
    }

    private static void EnsureChildGroup(object mixerObject, AudioMixer mixer, object masterGroup, MethodInfo createGroupMethod, MethodInfo addChildMethod, string groupName)
    {
        if (mixer.FindMatchingGroups(groupName).Length > 0)
        {
            return;
        }

        var groupObject = createGroupMethod.Invoke(mixerObject, new object[] { groupName, false });
        if (groupObject == null)
        {
            return;
        }

        addChildMethod.Invoke(mixerObject, new[] { groupObject, masterGroup });
    }

    private static void EnsureExposedVolumeParameters(UnityEngine.Object mixerObject)
    {
        var controllerType = GetControllerType();
        var exposedParameterType = Type.GetType("UnityEditor.Audio.ExposedAudioParameter, UnityEditor");
        var exposedParametersProperty = controllerType != null
            ? controllerType.GetProperty("exposedParameters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            : null;
        var onChangedMethod = controllerType != null
            ? controllerType.GetMethod("OnChangedExposedParameter", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            : null;
        var guidField = exposedParameterType != null
            ? exposedParameterType.GetField("guid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            : null;
        var nameField = exposedParameterType != null
            ? exposedParameterType.GetField("name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            : null;
        if (mixerObject == null || exposedParameterType == null || exposedParametersProperty == null || guidField == null || nameField == null)
        {
            return;
        }

        var mixer = mixerObject as AudioMixer;
        if (mixer == null)
        {
            return;
        }

        var existing = exposedParametersProperty.GetValue(mixerObject, null) as Array;
        var nextEntries = new List<object>();
        if (existing != null)
        {
            for (var i = 0; i < existing.Length; i++)
            {
                var entry = existing.GetValue(i);
                if (entry == null)
                {
                    continue;
                }

                var exposedName = nameField.GetValue(entry) as string;
                if (!IsManagedParameterName(exposedName))
                {
                    nextEntries.Add(entry);
                }
            }
        }

        for (var i = 0; i < ExposedVolumeBindings.Length; i++)
        {
            var binding = ExposedVolumeBindings[i];
            var group = FindControllerGroup(mixer, binding.GroupName);
            var volumeGuidHex = ReadVolumeGuidHex(group);
            if (string.IsNullOrWhiteSpace(volumeGuidHex))
            {
                continue;
            }

            var entry = Activator.CreateInstance(exposedParameterType);
            guidField.SetValue(entry, new GUID(volumeGuidHex));
            nameField.SetValue(entry, binding.ParameterName);
            nextEntries.Add(entry);
        }

        var nextArray = Array.CreateInstance(exposedParameterType, nextEntries.Count);
        for (var i = 0; i < nextEntries.Count; i++)
        {
            nextArray.SetValue(nextEntries[i], i);
        }

        if (HasSameExposedParameters(existing, nextArray, nameField, guidField))
        {
            return;
        }

        exposedParametersProperty.SetValue(mixerObject, nextArray, null);
        onChangedMethod?.Invoke(mixerObject, null);
    }

    private static bool HasSameExposedParameters(Array existing, Array next, FieldInfo nameField, FieldInfo guidField)
    {
        if (existing == null || next == null || existing.Length != next.Length)
        {
            return false;
        }

        for (var i = 0; i < existing.Length; i++)
        {
            var existingEntry = existing.GetValue(i);
            var nextEntry = next.GetValue(i);
            if (existingEntry == null || nextEntry == null)
            {
                return false;
            }

            var existingName = nameField.GetValue(existingEntry) as string;
            var nextName = nameField.GetValue(nextEntry) as string;
            if (!string.Equals(existingName, nextName, StringComparison.Ordinal))
            {
                return false;
            }

            var existingGuid = guidField.GetValue(existingEntry);
            var nextGuid = guidField.GetValue(nextEntry);
            if (existingGuid == null || nextGuid == null || !string.Equals(existingGuid.ToString(), nextGuid.ToString(), StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsManagedParameterName(string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            return false;
        }

        for (var i = 0; i < ExposedVolumeBindings.Length; i++)
        {
            if (string.Equals(ExposedVolumeBindings[i].ParameterName, parameterName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static AudioMixerGroup FindControllerGroup(AudioMixer mixer, string groupName)
    {
        if (mixer == null || string.IsNullOrWhiteSpace(groupName))
        {
            return null;
        }

        var groups = mixer.FindMatchingGroups(groupName);
        if (groups == null || groups.Length == 0)
        {
            return null;
        }

        for (var i = 0; i < groups.Length; i++)
        {
            if (groups[i] != null && groups[i].name == groupName)
            {
                return groups[i];
            }
        }

        return groups[0];
    }

    private static string ReadVolumeGuidHex(UnityEngine.Object groupObject)
    {
        if (groupObject == null)
        {
            return null;
        }

        var volumeProperty = new SerializedObject(groupObject).FindProperty("m_Volume");
        if (volumeProperty == null)
        {
            return null;
        }

        var guid = new GUID();
        SetGuidField(ref guid, "m_Value0", unchecked((uint)volumeProperty.FindPropertyRelative("data[0]").intValue));
        SetGuidField(ref guid, "m_Value1", unchecked((uint)volumeProperty.FindPropertyRelative("data[1]").intValue));
        SetGuidField(ref guid, "m_Value2", unchecked((uint)volumeProperty.FindPropertyRelative("data[2]").intValue));
        SetGuidField(ref guid, "m_Value3", unchecked((uint)volumeProperty.FindPropertyRelative("data[3]").intValue));
        return guid.ToString();
    }

    private static void SetGuidField(ref GUID guid, string fieldName, uint value)
    {
        var field = typeof(GUID).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            return;
        }

        field.SetValueDirect(__makeref(guid), value);
    }

    private static Type GetControllerType()
    {
        return Type.GetType("UnityEditor.Audio.AudioMixerController, UnityEditor");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        var segments = path.Split('/');
        var current = segments[0];
        for (var i = 1; i < segments.Length; i++)
        {
            var next = current + "/" + segments[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, segments[i]);
            }

            current = next;
        }
    }
}
#endif
