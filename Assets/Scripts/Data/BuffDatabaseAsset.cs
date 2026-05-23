using UnityEngine;

[CreateAssetMenu(fileName = "BuffDatabase", menuName = "Cultivation/Data/Buff Database")]
public sealed class BuffDatabaseAsset : ScriptableObject
{
    public BuffDefinition[] buffs;
}
