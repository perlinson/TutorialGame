using UnityEngine;
using UnityEngine.UI;

public sealed class UIButtonSoundBinder : MonoBehaviour
{
    [SerializeField] private Button targetButton;
    [SerializeField] private CultivationButtonSound sound = CultivationButtonSound.Click;

    public CultivationButtonSound Sound
    {
        get => sound;
        set => sound = value;
    }

    private void Reset()
    {
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        ApplyBinding();
    }

    private void OnDisable()
    {
        RemoveBinding();
    }

    public void Configure(Button button, CultivationButtonSound newSound)
    {
        targetButton = button;
        sound = newSound;
        ApplyBinding();
    }

    public void ApplyBinding()
    {
        ResolveButton();
        if (targetButton == null)
        {
            return;
        }

        targetButton.onClick.RemoveListener(HandleClick);
        targetButton.onClick.AddListener(HandleClick);
    }

    public void RemoveBinding()
    {
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(HandleClick);
        }
    }

    private void ResolveButton()
    {
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }
    }

    private void HandleClick()
    {
        CultivationUiAudio.PlayButtonSound(sound);
    }
}
