using UnityEngine;

[CreateAssetMenu(fileName = "SpiritRootDatabase", menuName = "Cultivation/Data/Spirit Root Database")]
public sealed class SpiritRootDatabaseAsset : ScriptableObject
{
    public SpiritRootConfigAsset[] entries;
}
