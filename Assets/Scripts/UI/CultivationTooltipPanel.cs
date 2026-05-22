using TMPro;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public sealed class CultivationTooltipPanel : CultivationUIPanel
{
    private const float HorizontalOffset = 18f;
    private const float VerticalOffset = 18f;
    private const float MinTextWidth = 180f;
    private const float MaxTextWidth = 420f;
    private const float HorizontalPadding = 22f;
    private const float TopPadding = 18f;
    private const float BottomPadding = 18f;
    private const float VerticalSpacing = 8f;
    private const float FadeInSpeed = 18f;
    private const float FadeOutSpeed = 14f;
    private const float VisibleScale = 1f;
    private const float HiddenScale = 0.96f;
    private const float ScreenEdgePadding = 12f;

    public RectTransform panelRect;
    public RectTransform titleRect;
    public RectTransform bodyRect;
    public CanvasGroup canvasGroup;
    public Image backgroundImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;

    private RectTransform rootRect;
    private bool isVisible;
    private bool isHiding;

    protected override void OnOpen(IUIData uiData = null)
    {
        rootRect = transform as RectTransform;
        UiPanelOrderUtility.BringToFront(this, 260);
        ApplyVisualStyle();
        HideImmediate();
    }

    protected override void OnClose()
    {
        HideImmediate();
    }

    private void LateUpdate()
    {
        UpdateTransition();

        if (isVisible)
        {
            MoveTo(Input.mousePosition);
        }
    }

    public void Present(string title, string body, Vector2 screenPosition)
    {
        if (panelRect == null || canvasGroup == null || titleText == null || bodyText == null)
        {
            return;
        }

        var hasTitle = !string.IsNullOrWhiteSpace(title);
        titleText.gameObject.SetActive(hasTitle);
        titleText.text = hasTitle ? title.Trim() : string.Empty;
        bodyText.text = string.IsNullOrWhiteSpace(body) ? string.Empty : body.Trim();

        RefreshLayout(hasTitle);
        ApplyVisualStyle();
        isHiding = false;
        isVisible = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        transform.SetAsLastSibling();
        MoveTo(screenPosition);
    }

    public void MoveTo(Vector2 screenPosition)
    {
        if (!isVisible || rootRect == null || panelRect == null)
        {
            return;
        }

        var screenWidth = Screen.width;
        var screenHeight = Screen.height;
        var panelWidth = panelRect.rect.width * Mathf.Max(0.001f, panelRect.localScale.x);
        var panelHeight = panelRect.rect.height * Mathf.Max(0.001f, panelRect.localScale.y);

        var preferLeft = screenPosition.x + HorizontalOffset + panelWidth > screenWidth - ScreenEdgePadding;
        var preferBelow = screenPosition.y - VerticalOffset - panelHeight < ScreenEdgePadding;

        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.zero;
        panelRect.pivot = new Vector2(preferLeft ? 1f : 0f, preferBelow ? 0f : 1f);

        var targetX = preferLeft ? screenPosition.x - HorizontalOffset : screenPosition.x + HorizontalOffset;
        var targetY = preferBelow ? screenPosition.y + VerticalOffset : screenPosition.y - VerticalOffset;

        targetX = Mathf.Clamp(
            targetX,
            ScreenEdgePadding + (preferLeft ? panelWidth : 0f),
            screenWidth - ScreenEdgePadding - (preferLeft ? 0f : panelWidth));
        targetY = Mathf.Clamp(
            targetY,
            ScreenEdgePadding + (preferBelow ? 0f : panelHeight),
            screenHeight - ScreenEdgePadding - (preferBelow ? panelHeight : 0f));

        panelRect.anchoredPosition = new Vector2(targetX, targetY);
    }

    public void HideImmediate()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        isVisible = false;
        isHiding = false;

        if (panelRect != null)
        {
            panelRect.localScale = new Vector3(HiddenScale, HiddenScale, 1f);
        }
    }

    public void BeginHide()
    {
        if (canvasGroup == null)
        {
            HideGameUiPanel(GameUiPanelId.Tooltip);
            return;
        }

        isVisible = false;
        isHiding = true;
    }

    private void RefreshLayout(bool hasTitle)
    {
        if (panelRect == null || bodyRect == null || titleRect == null)
        {
            return;
        }

        var titlePreferred = hasTitle ? titleText.GetPreferredValues(titleText.text, MaxTextWidth, 0f) : Vector2.zero;
        var bodyPreferred = bodyText.GetPreferredValues(bodyText.text, MaxTextWidth, 0f);
        var contentWidth = Mathf.Clamp(Mathf.Max(hasTitle ? titlePreferred.x : 0f, bodyPreferred.x), MinTextWidth, MaxTextWidth);
        var titleHeight = hasTitle ? titleText.GetPreferredValues(titleText.text, contentWidth, 0f).y : 0f;
        var bodyHeight = bodyText.GetPreferredValues(bodyText.text, contentWidth, 0f).y;
        var gap = hasTitle && bodyHeight > 0.1f ? VerticalSpacing : 0f;
        var panelWidth = contentWidth + HorizontalPadding * 2f;
        var panelHeight = TopPadding + titleHeight + gap + bodyHeight + BottomPadding;

        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.zero;

        if (hasTitle)
        {
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(0f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.anchoredPosition = new Vector2(HorizontalPadding, -TopPadding);
            titleRect.sizeDelta = new Vector2(contentWidth, titleHeight);
        }

        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(0f, 1f);
        bodyRect.pivot = new Vector2(0f, 1f);
        bodyRect.anchoredPosition = new Vector2(HorizontalPadding, -(TopPadding + titleHeight + gap));
        bodyRect.sizeDelta = new Vector2(contentWidth, bodyHeight);
    }

    private void UpdateTransition()
    {
        if (canvasGroup == null)
        {
            return;
        }

        var targetAlpha = isVisible ? 1f : 0f;
        var speed = isVisible ? FadeInSpeed : FadeOutSpeed;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * speed);

        if (panelRect != null)
        {
            var targetScale = isVisible ? VisibleScale : HiddenScale;
            var scale = Mathf.MoveTowards(panelRect.localScale.x, targetScale, Time.unscaledDeltaTime * speed * 0.4f);
            panelRect.localScale = new Vector3(scale, scale, 1f);
        }

        if (isHiding && canvasGroup.alpha <= 0.001f)
        {
            isHiding = false;
            HideGameUiPanel(GameUiPanelId.Tooltip);
        }
    }

    private void ApplyVisualStyle()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.98f, 0.94f, 0.86f, 0.98f);
        }

        if (titleText != null)
        {
            titleText.color = new Color(0.22f, 0.15f, 0.08f, 1f);
        }

        if (bodyText != null)
        {
            bodyText.color = new Color(0.16f, 0.13f, 0.10f, 1f);
        }
    }
}
