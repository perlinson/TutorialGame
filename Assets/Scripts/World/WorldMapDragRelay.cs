using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 把 viewport 上的拖拽事件转发给 WorldMapController，
/// 用于实现"鼠标按住空白处拖动地图"。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public sealed class WorldMapDragRelay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private WorldMapController owner;

    public void Bind(WorldMapController controller)
    {
        owner = controller;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (owner != null)
        {
            owner.OnMapDragBegin(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (owner != null)
        {
            owner.OnMapDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (owner != null)
        {
            owner.OnMapDragEnd(eventData);
        }
    }
}
