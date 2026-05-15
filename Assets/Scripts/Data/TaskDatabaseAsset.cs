using UnityEngine;

[CreateAssetMenu(fileName = "TaskDatabase", menuName = "TutorialGame/Data/Task Database")]
public sealed class TaskDatabaseAsset : ScriptableObject
{
    public Sprite coverImage;
    public TaskDefinition[] tasks;
}
