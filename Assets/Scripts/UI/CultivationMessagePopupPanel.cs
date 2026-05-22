using TMPro;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public sealed class CultivationMessagePopupPanel : CultivationUIPanel
{
    private const float DefaultDuration = 2.4f;
    private const float FadeInSpeed = 12f;
    private const float FadeOutSpeed = 10f;
    private const float WindowShownY = -112f;
    private const float WindowHiddenY = -72f;
    private const float MinTextWidth = 260f;
    private const float MaxTextWidth = 520f;
    private const float HorizontalPadding = 28f;
    private const float TopPadding = 18f;
    private const float BottomPadding = 20f;
    private const float VerticalSpacing = 8f;

    public RectTransform windowRect;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public Image backgroundImage;
    public Image accentImage;

    private float hideAtRealtime;
    private bool isVisible;
    private bool isHiding;

    protected override void OnOpen(IUIData uiData = null)
    {
        ApplyData(uiData as CultivationMessagePopupPanelData);
        UiPanelOrderUtility.BringToFront(this, 250);
    }

    protected override void OnClose()
    {
        hideAtRealtime = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (windowRect != null)
        {
            SetWindowY(WindowHiddenY);
        }

        isVisible = false;
        isHiding = false;
    }

    public void Present(string title, string message, float duration = DefaultDuration, CultivationMessagePopupStyle style = CultivationMessagePopupStyle.Info)
    {
        ApplyData(new CultivationMessagePopupPanelData(title, message, duration, style));
        UiPanelOrderUtility.BringToFront(this, 250);
    }

    private void Update()
    {
        UpdateTransition();

        if (hideAtRealtime > 0f && Time.unscaledTime >= hideAtRealtime && !isHiding)
        {
            hideAtRealtime = 0f;
            isVisible = false;
            isHiding = true;
        }
    }

    private void ApplyData(CultivationMessagePopupPanelData data)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        var hasTitle = data != null && !string.IsNullOrWhiteSpace(data.Title);
        if (titleText != null)
        {
            titleText.gameObject.SetActive(hasTitle);
            titleText.text = hasTitle ? data.Title.Trim() : string.Empty;
        }

        if (bodyText != null)
        {
            bodyText.text = data != null ? data.Message.Trim() : string.Empty;
        }

        ApplyVisualStyle(data != null ? data.Style : CultivationMessagePopupStyle.Info);
        RefreshLayout(hasTitle);
        isVisible = true;
        isHiding = false;
        hideAtRealtime = Time.unscaledTime + Mathf.Max(0.5f, data != null ? data.Duration : DefaultDuration);
    }

    private void ApplyVisualStyle(CultivationMessagePopupStyle style)
    {
        var backgroundColor = new Color(0.93f, 0.88f, 0.75f, 0.98f);
        var accentColor = new Color(0.71f, 0.56f, 0.23f, 1f);
        var titleColor = new Color(0.23f, 0.15f, 0.08f, 1f);
        var bodyColor = new Color(0.2f, 0.16f, 0.12f, 1f);

        switch (style)
        {
            case CultivationMessagePopupStyle.Warning:
                backgroundColor = new Color(0.98f, 0.90f, 0.69f, 0.985f);
                accentColor = new Color(0.83f, 0.47f, 0.10f, 1f);
                titleColor = new Color(0.37f, 0.18f, 0.02f, 1f);
                bodyColor = new Color(0.31f, 0.18f, 0.05f, 1f);
                break;
            case CultivationMessagePopupStyle.Error:
                backgroundColor = new Color(0.88f, 0.76f, 0.73f, 0.985f);
                accentColor = new Color(0.62f, 0.18f, 0.14f, 1f);
                titleColor = new Color(0.39f, 0.10f, 0.08f, 1f);
                bodyColor = new Color(0.30f, 0.11f, 0.10f, 1f);
                break;
            case CultivationMessagePopupStyle.Success:
                backgroundColor = new Color(0.84f, 0.91f, 0.79f, 0.985f);
                accentColor = new Color(0.21f, 0.48f, 0.22f, 1f);
                titleColor = new Color(0.10f, 0.26f, 0.11f, 1f);
                bodyColor = new Color(0.14f, 0.22f, 0.12f, 1f);
                break;
            case CultivationMessagePopupStyle.Neutral:
                backgroundColor = new Color(0.90f, 0.88f, 0.84f, 0.98f);
                accentColor = new Color(0.43f, 0.40f, 0.35f, 1f);
                titleColor = new Color(0.20f, 0.17f, 0.13f, 1f);
                bodyColor = new Color(0.19f, 0.17f, 0.14f, 1f);
                break;
            case CultivationMessagePopupStyle.Info:
            default:
                break;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }

        if (accentImage != null)
        {
            accentImage.color = accentColor;
        }

        if (titleText != null)
        {
            titleText.color = titleColor;
        }

        if (bodyText != null)
        {
            bodyText.color = bodyColor;
        }
    }

    private void RefreshLayout(bool hasTitle)
    {
        if (windowRect == null || bodyText == null)
        {
            return;
        }

        var titlePreferred = hasTitle && titleText != null
            ? titleText.GetPreferredValues(titleText.text, MaxTextWidth, 0f)
            : Vector2.zero;
        var bodyPreferred = bodyText.GetPreferredValues(bodyText.text, MaxTextWidth, 0f);
        var contentWidth = Mathf.Clamp(Mathf.Max(hasTitle ? titlePreferred.x : 0f, bodyPreferred.x), MinTextWidth, MaxTextWidth);
        var titleHeight = hasTitle && titleText != null ? titleText.GetPreferredValues(titleText.text, contentWidth, 0f).y : 0f;
        var bodyHeight = bodyText.GetPreferredValues(bodyText.text, contentWidth, 0f).y;
        var gap = hasTitle && bodyHeight > 0.1f ? VerticalSpacing : 0f;
        var windowWidth = contentWidth + HorizontalPadding * 2f;
        var windowHeight = TopPadding + titleHeight + gap + bodyHeight + BottomPadding;

        windowRect.sizeDelta = new Vector2(windowWidth, windowHeight);
        SetWindowY(isVisible ? WindowShownY : WindowHiddenY);

        if (titleText != null && hasTitle)
        {
            var titleRect = titleText.rectTransform;
            titleRect.anchoredPosition = new Vector2(HorizontalPadding, -TopPadding);
            titleRect.sizeDelta = new Vector2(contentWidth, titleHeight);
        }

        if (bodyText != null)
        {
            var bodyRect = bodyText.rectTransform;
            bodyRect.anchoredPosition = new Vector2(HorizontalPadding, -(TopPadding + titleHeight + gap));
            bodyRect.sizeDelta = new Vector2(contentWidth, bodyHeight);
        }
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

        if (windowRect != null)
        {
            var currentY = windowRect.anchoredPosition.y;
            var targetY = isVisible ? WindowShownY : WindowHiddenY;
            var nextY = Mathf.MoveTowards(currentY, targetY, Time.unscaledDeltaTime * speed * 180f);
            SetWindowY(nextY);
        }

        if (isHiding && canvasGroup.alpha <= 0.001f)
        {
            isHiding = false;
            HideGameUiPanel(GameUiPanelId.MessagePopup);
        }
    }

    private void SetWindowY(float y)
    {
        if (windowRect == null)
        {
            return;
        }

        var position = windowRect.anchoredPosition;
        position.y = y;
        windowRect.anchoredPosition = position;
    }
}
