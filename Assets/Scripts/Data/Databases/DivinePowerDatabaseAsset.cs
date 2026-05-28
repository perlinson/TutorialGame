using UnityEngine;

[CreateAssetMenu(fileName = "DivinePowerDatabase", menuName = "Cultivation/Data/Divine Power Database")]
public sealed class DivinePowerDatabaseAsset : ScriptableObject
{
    public DivinePowerConfigAsset[] entries;
}
