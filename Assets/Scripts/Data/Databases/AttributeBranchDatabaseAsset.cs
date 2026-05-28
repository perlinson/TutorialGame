using UnityEngine;

[CreateAssetMenu(fileName = "AttributeBranchDatabase", menuName = "Cultivation/Data/Attribute Branch Database")]
public sealed class AttributeBranchDatabaseAsset : ScriptableObject
{
    public AttributeBranchConfigAsset[] entries;
}
