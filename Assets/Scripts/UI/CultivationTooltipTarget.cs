using UnityEngine;
using UnityEngine.EventSystems;

public sealed class CultivationTooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [TextArea(1, 2)]
    public string title;

    [TextArea(2, 6)]
    public string body;

    public void Configure(string newTitle, string newBody)
    {
        title = newTitle ?? string.Empty;
        body = newBody ?? string.Empty;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CultivationTooltip.Show(title, body, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CultivationTooltip.Hide();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        CultivationTooltip.UpdatePosition(eventData.position);
    }

    private void OnDisable()
    {
        CultivationTooltip.Hide();
    }
}
