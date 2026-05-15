using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class WorldRegionNodeView : MonoBehaviour
{
    public string regionId;
    public Image background;
    public Image border;
    public Text titleText;
    public Text subtitleText;
    public Button button;

    public string RegionId => regionId;

    public void Bind(WorldRegionDefinition region, bool selected, bool unlocked, bool accessible, bool cleared, UnityAction onClick)
    {
        regionId = region.Id;
        titleText.text = region.DisplayName;

        if (!unlocked)
        {
            subtitleText.text = "未探明";
        }
        else if (!accessible)
        {
            subtitleText.text = "需 " + WorldRegionLibrary.GetRealmName(region.RequiredRealmTier);
        }
        else if (cleared)
        {
            subtitleText.text = "已肃清";
        }
        else
        {
            subtitleText.text = "可历练";
        }

        background.color = !unlocked
            ? new Color(0.16f, 0.17f, 0.2f, 0.82f)
            : selected
                ? new Color(0.55f, 0.41f, 0.18f, 0.92f)
                : cleared
                    ? new Color(0.18f, 0.36f, 0.31f, 0.92f)
                    : accessible
                        ? new Color(0.24f, 0.26f, 0.22f, 0.92f)
                        : new Color(0.23f, 0.2f, 0.17f, 0.88f);

        border.color = selected
            ? new Color(0.92f, 0.82f, 0.46f, 1f)
            : unlocked
                ? new Color(0.62f, 0.55f, 0.34f, 0.9f)
                : new Color(0.32f, 0.33f, 0.39f, 0.9f);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }
}
