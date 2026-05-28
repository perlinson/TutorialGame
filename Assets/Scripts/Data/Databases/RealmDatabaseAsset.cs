using UnityEngine;

[CreateAssetMenu(fileName = "RealmDatabase", menuName = "Cultivation/Data/Realm Database")]
public sealed class RealmDatabaseAsset : ScriptableObject
{
    public RealmConfigAsset[] entries;
}
