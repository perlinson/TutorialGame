using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;

public sealed class GameHudController : MonoBehaviour
{
    public Text titleText;
    public Text hpText;
    public Text realmText;
    public Text progressText;
    public Text objectiveText;
    public Text hintText;

    public void SetTitle(string message)
    {
        if (titleText != null)
        {
            titleText.text = message;
        }
    }

    public void SetStats(string realm, int qi, int qiRequired, int currentHp, int maxHp, string spiritCrystals, int attackLevel, int vitalityLevel)
    {
        if (hpText != null)
        {
            hpText.text = "气血 " + currentHp + " / " + maxHp;
        }

        if (realmText != null)
        {
            realmText.text = "境界 " + realm + " · 主法器 +" + attackLevel + " / 护身法器 +" + vitalityLevel;
        }

        if (progressText != null)
        {
            progressText.text = qiRequired > 0
                ? "修为 " + qi + " / " + qiRequired + " · 灵石 " + spiritCrystals
                : "修为已臻圆满 · 灵石 " + spiritCrystals;
        }
    }

    public void SetObjective(string message)
    {
        if (objectiveText != null)
        {
            objectiveText.text = message;
        }
    }

    public void SetHint(string message)
    {
        if (hintText != null)
        {
            hintText.text = message;
        }
    }
}
