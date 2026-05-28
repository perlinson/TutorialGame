using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactDatabase", menuName = "Cultivation/Data/Artifact Database")]
public sealed class ArtifactDatabaseAsset : ScriptableObject
{
    public ArtifactConfigAsset[] entries;
}
