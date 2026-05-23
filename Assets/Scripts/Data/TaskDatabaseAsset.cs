using UnityEngine;

[CreateAssetMenu(fileName = "TaskDatabase", menuName = "Cultivation/Data/Task Database")]
public sealed class TaskDatabaseAsset : ScriptableObject
{
    public Sprite coverImage;
    public TaskDefinition[] tasks;
}
