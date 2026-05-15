using UnityEngine;

[CreateAssetMenu(fileName = "WorkshopRecipeDatabase", menuName = "TutorialGame/Data/Workshop Recipe Database")]
public sealed class WorkshopRecipeDatabaseAsset : ScriptableObject
{
    public Sprite coverImage;
    public WorkshopRecipeDefinition[] recipes;
}
