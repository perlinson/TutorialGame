using UnityEngine;
using UnityEngine.UI;

public sealed class ExpeditionRoomNodeView : MonoBehaviour
{
    public Image background;
    public Text iconText;
    public Text labelText;
    public Text stateText;

    public void Bind(ExpeditionRoomState room, bool current)
    {
        iconText.text = room.Symbol;
        labelText.text = "第" + (room.Index + 1) + "室";

        if (room.Resolved)
        {
            stateText.text = "已过";
            background.color = new Color(0.21f, 0.36f, 0.29f, 0.95f);
        }
        else if (current)
        {
            stateText.text = "当前";
            background.color = new Color(0.56f, 0.42f, 0.18f, 0.96f);
        }
        else if (room.Visited)
        {
            stateText.text = "已抵达";
            background.color = new Color(0.28f, 0.25f, 0.18f, 0.94f);
        }
        else
        {
            stateText.text = "未至";
            background.color = new Color(0.18f, 0.17f, 0.15f, 0.94f);
        }
    }
}
