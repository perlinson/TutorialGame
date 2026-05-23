using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed partial class WorldMapController
{
    // 觅长生风格大地图：自由二维拖拽平移 + 人物在地图上移动
    private static readonly Vector2 MapViewportSize = new Vector2(1280f, 820f);
    private static readonly Vector2 MapContentSize = new Vector2(1920f, 2160f);
    private const float NodeHalfWidth = 110f;
    private const float NodeHalfHeight = 110f;
    private const float MapWheelSensitivity = 80f;
    private const float MapEdgePadding = 24f;

    // 城镇在二维地图中的预设布局（归一化坐标 0-1，X 轴左右交替，Y 轴自上而下）
    private static readonly Vector2[] MapSlots = new Vector2[]
    {
        new Vector2(0.30f, 0.06f),
        new Vector2(0.70f, 0.20f),
        new Vector2(0.28f, 0.36f),
        new Vector2(0.66f, 0.52f),
        new Vector2(0.32f, 0.70f),
        new Vector2(0.68f, 0.86f),
        new Vector2(0.50f, 0.96f),
    };

    private bool navigationInitialized;
    private RectMask2D mapViewportMask;
    private float mapContentMinX, mapContentMaxX;
    private float mapContentMinY, mapContentMaxY;
    private int pendingTravelRegionIndex = -1;
    private float characterArriveThreshold = 12f;

    private void EnsureNavigationInitialized()
    {
        if (navigationInitialized || mapFieldRect == null || mapPanelRect == null)
        {
            return;
        }

        navigationInitialized = true;
        ConfigureMapViewport();
        ConfigureMapContent();
        LayoutRegionNodesVertically();
        SetupNodeVisuals();
        EnsureCharacterIcon();
        SnapMapToCurrentRegion();
    }

    private void ConfigureMapViewport()
    {
        // 把 MapPanel 改造成 viewport：放大、居中、添加遮罩
        mapPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        mapPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        mapPanelRect.pivot = new Vector2(0.5f, 0.5f);
        mapPanelRect.anchoredPosition = new Vector2(0f, -20f);
        mapPanelRect.sizeDelta = MapViewportSize;

        var image = mapPanelRect.GetComponent<Image>();
        if (image == null)
        {
            image = mapPanelRect.gameObject.AddComponent<Image>();
        }
        image.color = new Color(0.14f, 0.12f, 0.09f, 0.95f);
        image.raycastTarget = true;

        mapViewportMask = mapPanelRect.GetComponent<RectMask2D>();
        if (mapViewportMask == null)
        {
            mapViewportMask = mapPanelRect.gameObject.AddComponent<RectMask2D>();
        }

        // 让 viewport 可接收拖拽事件（背景拖拽 = 平移地图）
        var dragRelay = mapPanelRect.GetComponent<WorldMapDragRelay>();
        if (dragRelay == null)
        {
            dragRelay = mapPanelRect.gameObject.AddComponent<WorldMapDragRelay>();
        }
        dragRelay.Bind(this);
    }

    private void ConfigureMapContent()
    {
        mapScrollContent = mapFieldRect;

        // MapField 改为顶对齐的"长卷轴"
        mapFieldRect.anchorMin = new Vector2(0.5f, 1f);
        mapFieldRect.anchorMax = new Vector2(0.5f, 1f);
        mapFieldRect.pivot = new Vector2(0.5f, 1f);
        mapFieldRect.sizeDelta = MapContentSize;
        mapFieldRect.anchoredPosition = new Vector2(0f, 0f);

        // 给地图加一张古风背景
        var bg = mapFieldRect.Find("MapBackdrop") as RectTransform;
        if (bg == null)
        {
            var go = new GameObject("MapBackdrop", typeof(RectTransform), typeof(Image));
            go.layer = mapFieldRect.gameObject.layer;
            go.transform.SetParent(mapFieldRect, false);
            go.transform.SetAsFirstSibling();
            bg = go.GetComponent<RectTransform>();
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;
            var bgImage = go.GetComponent<Image>();
            bgImage.color = new Color(0.18f, 0.16f, 0.12f, 1f);
            bgImage.raycastTarget = false;
        }

        // 添加装饰性网格线，让地图看起来不空旷
        EnsureMapGridLines();

        // 计算可滚动范围：以 viewport 为窗口
        mapContentMinX = 0f;
        mapContentMaxX = Mathf.Max(0f, MapContentSize.x - MapViewportSize.x);
        mapContentMinY = 0f;
        mapContentMaxY = Mathf.Max(0f, MapContentSize.y - MapViewportSize.y);
    }

    private void EnsureMapGridLines()
    {
        var grid = mapFieldRect.Find("MapGridLines");
        if (grid != null) return;

        var gridGo = new GameObject("MapGridLines", typeof(RectTransform));
        gridGo.layer = mapFieldRect.gameObject.layer;
        gridGo.transform.SetParent(mapFieldRect, false);
        gridGo.transform.SetSiblingIndex(1);
        var gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.offsetMin = Vector2.zero;
        gridRect.offsetMax = Vector2.zero;

        // 水平线
        var lineColor = new Color(0.25f, 0.22f, 0.16f, 0.5f);
        var hCount = 8;
        for (var i = 0; i < hCount; i++)
        {
            var t = (i + 1f) / (hCount + 1f);
            CreateGridLine(gridRect, "HLine_" + i, lineColor,
                new Vector2(0f, 1f - t), new Vector2(1f, 1f - t), 1f);
        }

        // 竖线
        var vCount = 6;
        for (var i = 0; i < vCount; i++)
        {
            var t = (i + 1f) / (vCount + 1f);
            CreateGridLine(gridRect, "VLine_" + i, lineColor,
                new Vector2(t, 0f), new Vector2(t, 1f), 1f);
        }
    }

    private static void CreateGridLine(RectTransform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    private void LayoutRegionNodesVertically()
    {
        if (regions == null || regions.Count == 0) return;

        for (var i = 0; i < regions.Count; i++)
        {
            var region = regions[i];
            var view = FindNodeView(region.Id);
            if (view == null) continue;

            Vector2 pos;
            if (region.MapPosition != Vector2.zero)
            {
                // 使用 region 自带的地图坐标
                pos = new Vector2(region.MapPosition.x, -region.MapPosition.y);
            }
            else
            {
                // 回退到 slot 系统
                var slot = MapSlots[Mathf.Min(i, MapSlots.Length - 1)];
                pos = new Vector2(
                    Mathf.Lerp(MapEdgePadding + NodeHalfWidth, MapContentSize.x - MapEdgePadding - NodeHalfWidth, slot.x),
                    -Mathf.Lerp(MapEdgePadding + NodeHalfHeight, MapContentSize.y - MapEdgePadding - NodeHalfHeight, slot.y));
            }

            var rect = view.transform as RectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(NodeHalfWidth * 2f, NodeHalfHeight * 2f);
        }

        DrawNodePaths();
    }

    private void DrawNodePaths()
    {
        // 清除旧连线
        var pathsParent = mapFieldRect.Find("NodePaths");
        if (pathsParent != null)
        {
            if (Application.isPlaying)
                Object.Destroy(pathsParent.gameObject);
            else
                Object.DestroyImmediate(pathsParent.gameObject);
        }

        if (regions == null || regions.Count < 2) return;

        var pathsGo = new GameObject("NodePaths", typeof(RectTransform));
        pathsGo.layer = mapFieldRect.gameObject.layer;
        pathsGo.transform.SetParent(mapFieldRect, false);
        pathsGo.transform.SetSiblingIndex(2);
        var pathsRect = pathsGo.GetComponent<RectTransform>();
        pathsRect.anchorMin = Vector2.zero;
        pathsRect.anchorMax = Vector2.one;
        pathsRect.offsetMin = Vector2.zero;
        pathsRect.offsetMax = Vector2.zero;

        var pathColor = new Color(0.55f, 0.45f, 0.28f, 0.6f);

        for (var i = 0; i < regions.Count - 1; i++)
        {
            var viewA = FindNodeView(regions[i].Id);
            var viewB = FindNodeView(regions[i + 1].Id);
            if (viewA == null || viewB == null) continue;

            var rectA = viewA.transform as RectTransform;
            var rectB = viewB.transform as RectTransform;
            var posA = rectA.anchoredPosition;
            var posB = rectB.anchoredPosition;

            CreatePathLine(pathsRect, "Path_" + i, posA, posB, pathColor, 2f);
        }
    }

    private static void CreatePathLine(RectTransform parent, string name, Vector2 from, Vector2 to, Color color, float width)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();

        var dir = to - from;
        var dist = dir.magnitude;
        var mid = (from + to) * 0.5f;

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = mid;
        rect.sizeDelta = new Vector2(dist, width);

        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);

        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    private void SetupNodeVisuals()
    {
        for (var i = 0; i < nodeViews.Count; i++)
        {
            var view = nodeViews[i];
            if (view == null)
            {
                continue;
            }

            WorldRegionDefinition region = null;
            for (var r = 0; r < regions.Count; r++)
            {
                if (regions[r].Id == view.RegionId)
                {
                    region = regions[r];
                    break;
                }
            }
            if (region == null)
            {
                continue;
            }

            // 添加/查找城镇占位图
            var rect = view.transform as RectTransform;
            var preview = rect.Find("TownPreview") as RectTransform;
            if (preview == null)
            {
                var go = new GameObject("TownPreview", typeof(RectTransform), typeof(Image));
                go.layer = rect.gameObject.layer;
                go.transform.SetParent(rect, false);
                preview = go.GetComponent<RectTransform>();
                preview.anchorMin = new Vector2(0.5f, 1f);
                preview.anchorMax = new Vector2(0.5f, 1f);
                preview.pivot = new Vector2(0.5f, 1f);
                preview.anchoredPosition = new Vector2(0f, -10f);
                preview.sizeDelta = new Vector2(170f, 130f);
            }
            var previewImage = preview.GetComponent<Image>();
            var previewLabel = view.titleText; // 暂时复用 title 作为图片下方标签
            GameSpriteLibrary.BindSpriteOrPlaceholder(
                previewImage,
                null,
                region.IllustrationImage != null ? region.IllustrationImage : region.MapIconImage,
                region.DisplayName + "占位图",
                new Color(region.AccentColor.r * 0.6f, region.AccentColor.g * 0.6f, region.AccentColor.b * 0.6f, 1f));

            // 标题置于底部
            var titleRect = view.titleText != null ? view.titleText.transform as RectTransform : null;
            if (titleRect != null)
            {
                titleRect.anchorMin = new Vector2(0.5f, 0f);
                titleRect.anchorMax = new Vector2(0.5f, 0f);
                titleRect.pivot = new Vector2(0.5f, 0f);
                titleRect.anchoredPosition = new Vector2(0f, 30f);
                titleRect.sizeDelta = new Vector2(210f, 28f);
                view.titleText.alignment = TMPro.TextAlignmentOptions.Center;
                view.titleText.fontSize = 22;
            }
            var subRect = view.subtitleText != null ? view.subtitleText.transform as RectTransform : null;
            if (subRect != null)
            {
                subRect.anchorMin = new Vector2(0.5f, 0f);
                subRect.anchorMax = new Vector2(0.5f, 0f);
                subRect.pivot = new Vector2(0.5f, 0f);
                subRect.anchoredPosition = new Vector2(0f, 6f);
                subRect.sizeDelta = new Vector2(210f, 22f);
                view.subtitleText.alignment = TMPro.TextAlignmentOptions.Center;
                view.subtitleText.fontSize = 16;
            }
        }
    }

    private void EnsureCharacterIcon()
    {
        if (mapFieldRect == null)
        {
            return;
        }

        if (characterIcon == null)
        {
            var go = new GameObject("CharacterIcon", typeof(RectTransform), typeof(Image));
            go.layer = mapFieldRect.gameObject.layer;
            go.transform.SetParent(mapFieldRect, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(56f, 56f);

            characterIcon = go.GetComponent<Image>();
            characterIcon.color = new Color(1f, 0.85f, 0.4f, 1f);
            characterIcon.sprite = GameSpriteLibrary.WhiteSquareSprite;
            characterIcon.raycastTarget = false;

            // 加一圈描边
            var outline = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            outline.layer = mapFieldRect.gameObject.layer;
            outline.transform.SetParent(rect, false);
            var outlineRect = outline.GetComponent<RectTransform>();
            outlineRect.anchorMin = Vector2.zero;
            outlineRect.anchorMax = Vector2.one;
            outlineRect.offsetMin = new Vector2(-4f, -4f);
            outlineRect.offsetMax = new Vector2(4f, 4f);
            var outlineImage = outline.GetComponent<Image>();
            outlineImage.color = new Color(0.18f, 0.12f, 0.06f, 1f);
            outlineImage.sprite = GameSpriteLibrary.WhiteSquareSprite;
            outlineImage.raycastTarget = false;
            outline.transform.SetAsFirstSibling();

            var label = new GameObject("Label", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            label.layer = mapFieldRect.gameObject.layer;
            label.transform.SetParent(rect, false);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = new Vector2(0f, -2f);
            labelRect.sizeDelta = new Vector2(120f, 22f);
            var labelText = label.GetComponent<TMPro.TextMeshProUGUI>();
            labelText.text = saveData != null && !string.IsNullOrEmpty(saveData.heroName) ? saveData.heroName : "修士";
            labelText.alignment = TMPro.TextAlignmentOptions.Center;
            labelText.fontSize = 16;
            labelText.color = new Color(0.95f, 0.92f, 0.78f, 1f);
            labelText.raycastTarget = false;
        }

        characterIcon.transform.SetAsLastSibling();
    }

    private void SnapMapToCurrentRegion()
    {
        if (mapFieldRect == null || characterIcon == null || saveData == null) return;

        var view = FindNodeView(saveData.currentRegionId);
        if (view == null && regions.Count > 0) view = FindNodeView(regions[0].Id);
        if (view == null) return;

        var nodeRect = view.transform as RectTransform;
        var iconRect = characterIcon.transform as RectTransform;
        iconRect.anchoredPosition = nodeRect.anchoredPosition + new Vector2(0f, NodeHalfHeight * 0.2f);
        characterTargetPosition = iconRect.anchoredPosition;
        isCharacterMoving = false;

        // 将地图滚动到使该位置在 viewport 中央
        targetMapScrollPosition = ComputeScrollForPoint(iconRect.anchoredPosition);
        mapScrollPosition = targetMapScrollPosition;
        mapFieldRect.anchoredPosition = new Vector2(mapScrollPosition.x, mapScrollPosition.y);
    }

    private Vector2 ComputeScrollForPoint(Vector2 contentPoint)
    {
        // contentPoint 是 MapField 本地坐标（anchor 左上，y 向下为负）
        // 让该点出现在 viewport 中央
        var desiredX = -contentPoint.x + MapViewportSize.x * 0.5f;
        var desiredY = -contentPoint.y - MapViewportSize.y * 0.5f;
        return new Vector2(
            Mathf.Clamp(desiredX, mapContentMinX, mapContentMaxX),
            Mathf.Clamp(desiredY, mapContentMinY, mapContentMaxY));
    }

    private WorldRegionNodeView FindNodeView(string regionId)
    {
        if (string.IsNullOrEmpty(regionId))
        {
            return null;
        }
        for (var i = 0; i < nodeViews.Count; i++)
        {
            if (nodeViews[i] != null && nodeViews[i].RegionId == regionId)
            {
                return nodeViews[i];
            }
        }
        return null;
    }

    // —— 由 Bootstrap.Update 调用 ——
    private void UpdateNavigation(float dt)
    {
        if (!navigationInitialized)
        {
            return;
        }

        HandleScrollInput(dt);
        UpdateCharacterMovement(dt);
        ApplyMapScroll();
    }

    private void HandleScrollInput(float dt)
    {
        if (HasBlockingPanelOpen() || mapPanelRect == null) return;

        var wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) > 0.01f && IsMouseOverViewport())
        {
            targetMapScrollPosition.y = Mathf.Clamp(
                targetMapScrollPosition.y - wheel * MapWheelSensitivity,
                mapContentMinY, mapContentMaxY);
        }

        // 键盘方向键 / WASD 自由平移
        if (IsMouseOverViewport() || isDraggingMap)
        {
            var h = 0f;
            var v = 0f;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) h -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) h += 1f;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) v -= 1f;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) v += 1f;

            if (Mathf.Abs(h) > 0f)
            {
                targetMapScrollPosition.x = Mathf.Clamp(
                    targetMapScrollPosition.x + h * scrollSpeed * dt,
                    mapContentMinX, mapContentMaxX);
            }
            if (Mathf.Abs(v) > 0f)
            {
                targetMapScrollPosition.y = Mathf.Clamp(
                    targetMapScrollPosition.y + v * scrollSpeed * dt,
                    mapContentMinY, mapContentMaxY);
            }
        }
    }

    private void ApplyMapScroll()
    {
        mapScrollPosition.x = Mathf.MoveTowards(mapScrollPosition.x, targetMapScrollPosition.x, scrollSpeed * Time.unscaledDeltaTime * 1.4f);
        mapScrollPosition.y = Mathf.MoveTowards(mapScrollPosition.y, targetMapScrollPosition.y, scrollSpeed * Time.unscaledDeltaTime * 1.4f);
        if (mapFieldRect != null)
        {
            mapFieldRect.anchoredPosition = new Vector2(mapScrollPosition.x, mapScrollPosition.y);
        }
    }

    private bool IsMouseOverViewport()
    {
        if (mapPanelRect == null)
        {
            return false;
        }
        return RectTransformUtility.RectangleContainsScreenPoint(mapPanelRect, Input.mousePosition, null);
    }

    private void UpdateCharacterMovement(float dt)
    {
        if (characterIcon == null)
        {
            return;
        }

        var rect = characterIcon.transform as RectTransform;
        if (!isCharacterMoving)
        {
            return;
        }

        var current = rect.anchoredPosition;
        var next = Vector2.MoveTowards(current, characterTargetPosition, characterMoveSpeed * dt);
        rect.anchoredPosition = next;

        if (Vector2.Distance(next, characterTargetPosition) <= characterArriveThreshold)
        {
            rect.anchoredPosition = characterTargetPosition;
            isCharacterMoving = false;
            OnCharacterArrived();
        }
        else
        {
            // 让目标节点保持在视野内
            targetMapScrollPosition = ComputeScrollForPoint(characterTargetPosition);
        }
    }

    private void OnCharacterArrived()
    {
        if (pendingTravelRegionIndex < 0 || pendingTravelRegionIndex >= regions.Count)
        {
            return;
        }

        var index = pendingTravelRegionIndex;
        pendingTravelRegionIndex = -1;
        OpenRegionPage(index);
    }

    // 替换原本"直接打开页面"的行为：先让人物走到城镇，再打开
    public void BeginTravelToRegion(int regionIndex)
    {
        EnsureNavigationInitialized();
        if (regionIndex < 0 || regionIndex >= regions.Count || characterIcon == null)
        {
            OpenRegionPage(regionIndex);
            return;
        }

        var view = FindNodeView(regions[regionIndex].Id);
        if (view == null)
        {
            OpenRegionPage(regionIndex);
            return;
        }

        var nodeRect = view.transform as RectTransform;
        characterTargetPosition = nodeRect.anchoredPosition + new Vector2(0f, NodeHalfHeight * 0.2f);
        isCharacterMoving = true;
        pendingTravelRegionIndex = regionIndex;
        SetHint("正前往 " + regions[regionIndex].DisplayName + " ...");
    }

    // —— 拖拽（由 WorldMapDragRelay 通过 EventSystem 调用） ——
    internal void OnMapDragBegin(PointerEventData ev)
    {
        isDraggingMap = true;
        lastMousePosition = ev.position;
    }

    internal void OnMapDrag(PointerEventData ev)
    {
        if (!isDraggingMap) return;
        var delta = ev.position - lastMousePosition;
        lastMousePosition = ev.position;
        // 自然拖拽：地图跟随手指方向移动
        targetMapScrollPosition.x = Mathf.Clamp(
            targetMapScrollPosition.x + delta.x,
            mapContentMinX, mapContentMaxX);
        targetMapScrollPosition.y = Mathf.Clamp(
            targetMapScrollPosition.y + delta.y,
            mapContentMinY, mapContentMaxY);
        mapScrollPosition = targetMapScrollPosition;
    }

    internal void OnMapDragEnd(PointerEventData ev)
    {
        isDraggingMap = false;
    }
}
