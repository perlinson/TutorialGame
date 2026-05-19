using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed class ExpeditionRoomNodeView : MonoBehaviour
{
    public Image background;
    public Text iconText;
    public Text labelText;
    public Text stateText;

    public void Bind(ExpeditionRoomState room, bool current)
    {
        GeneratedUiSkinLibrary.ApplyRoomNodeSkin(background);
        iconText.text = room.Symbol;
        labelText.text = "第" + (room.Index + 1) + "室";

        if (room.Resolved)
        {
            stateText.text = "已过";
            background.color = new Color(0.86f, 0.96f, 0.88f, 0.96f);
        }
        else if (current)
        {
            stateText.text = "当前";
            background.color = new Color(1f, 0.96f, 0.86f, 1f);
        }
        else if (room.Visited)
        {
            stateText.text = "已抵达";
            background.color = new Color(0.92f, 0.88f, 0.8f, 0.96f);
        }
        else
        {
            stateText.text = "未至";
            background.color = new Color(0.8f, 0.78f, 0.72f, 0.88f);
        }

        iconText.color = current
            ? new Color(0.32f, 0.22f, 0.1f, 1f)
            : new Color(0.2f, 0.18f, 0.12f, 1f);
        labelText.color = new Color(0.28f, 0.22f, 0.12f, 1f);
        stateText.color = new Color(0.36f, 0.28f, 0.14f, 0.96f);
    }
}
