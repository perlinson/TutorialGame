using UnityEngine;

[CreateAssetMenu(fileName = "SpiritRootConfig", menuName = "Cultivation/Spirit Root Config")]
public sealed class SpiritRootConfigAsset : ScriptableObject
{
    [Header("灵根信息")]
    public SpiritRootType type;
    public string displayName;
    [TextArea(2, 4)] public string description;

    [Header("修炼系数")]
    [Tooltip("修炼速度倍率")]
    public float cultivationSpeedMultiplier = 1.0f;

    [Header("五行亲和")]
    [Tooltip("金")]
    public int metalAffinity;
    [Tooltip("木")]
    public int woodAffinity;
    [Tooltip("水")]
    public int waterAffinity;
    [Tooltip("火")]
    public int fireAffinity;
    [Tooltip("土")]
    public int earthAffinity;

    [Header("特殊效果")]
    public bool hasSpecialEffect;
    [TextArea(2, 3)] public string specialEffectDescription;
}
