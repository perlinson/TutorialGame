using UnityEngine;

[CreateAssetMenu(fileName = "ExpeditionEventDatabase", menuName = "TutorialGame/Data/Expedition Event Database")]
public sealed class ExpeditionEventDatabaseAsset : ScriptableObject
{
    public ExpeditionEventDefinition[] events;
}
