using UnityEngine;

[CreateAssetMenu(fileName = "ExpeditionEventDatabase", menuName = "Cultivation/Data/Expedition Event Database")]
public sealed class ExpeditionEventDatabaseAsset : ScriptableObject
{
    public ExpeditionEventDefinition[] events;
}
