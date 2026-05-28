using UnityEngine;

[CreateAssetMenu(fileName = "SchoolDatabase", menuName = "Cultivation/Data/School Database")]
public sealed class SchoolDatabaseAsset : ScriptableObject
{
    public SchoolConfigAsset[] entries;
}
