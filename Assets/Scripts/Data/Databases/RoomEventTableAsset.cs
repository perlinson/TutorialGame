using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomEventTable", menuName = "Cultivation/Data/Room Event Table")]
public sealed class RoomEventTableAsset : ScriptableObject
{
    public Sprite coverImage;
    public string startScoutTitle;
    public Sprite startScoutImage;
    [TextArea(2, 5)] public string startScoutDescription;
    public string eliteTitle;
    public Sprite eliteImage;
    [TextArea(2, 5)] public string eliteDescription;
    public string bossTitle;
    public Sprite bossImage;
    [TextArea(2, 5)] public string bossDescription;
    public RoomCopyRecord[] roomCopies;
}

[Serializable]
public sealed class RoomCopyRecord
{
    public int roomKind;
    public string title;
    public Sprite illustrationImage;
    [TextArea(2, 5)] public string description;
}
