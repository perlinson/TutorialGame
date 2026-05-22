using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

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
        GeneratedUiSkinLibrary.ApplyRegionNodeSkin(background);

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
            ? new Color(0.52f, 0.54f, 0.58f, 0.78f)
            : selected
                ? new Color(1f, 0.96f, 0.88f, 1f)
                : cleared
                    ? new Color(0.82f, 0.94f, 0.86f, 0.96f)
                    : accessible
                        ? new Color(0.94f, 0.94f, 0.92f, 0.96f)
                        : new Color(0.84f, 0.8f, 0.75f, 0.9f);

        border.color = selected
            ? new Color(0.92f, 0.82f, 0.46f, 1f)
            : unlocked
                ? new Color(0.62f, 0.55f, 0.34f, 0.9f)
                : new Color(0.32f, 0.33f, 0.39f, 0.9f);

        titleText.color = selected
            ? new Color(0.23f, 0.18f, 0.08f, 1f)
            : new Color(0.9f, 0.86f, 0.78f, 1f);
        subtitleText.color = unlocked
            ? new Color(0.84f, 0.8f, 0.72f, 0.96f)
            : new Color(0.7f, 0.72f, 0.76f, 0.94f);

        CultivationUiAudio.BindButton(button, onClick);
        CultivationTooltipBinder.Bind(button, region.DisplayName + " · " + region.Subtitle, BuildTooltipBody(region, unlocked, accessible, cleared));
    }

    private static string BuildTooltipBody(WorldRegionDefinition region, bool unlocked, bool accessible, bool cleared)
    {
        var status = !unlocked
            ? "状态：未探明"
            : !accessible
                ? "状态：需达到 " + WorldRegionLibrary.GetRealmName(region.RequiredRealmTier)
                : cleared
                    ? "状态：已肃清，可重复历练"
                    : "状态：可前往历练";

        return status + "\n危险阶：第 " + region.DangerRank + " 等\n基础奖赏：修为 +" + region.ClearQiReward +
               " / 灵石 +" + region.ClearCrystalReward + "\n" + region.Description;
    }
}
