using UnityEngine;

public sealed class TrialRelic : MonoBehaviour
{
    [SerializeField] private int crystalAmount = 1;

    private GameController controller;

    public void Configure(GameController gameController, int amount)
    {
        controller = gameController;
        crystalAmount = Mathf.Max(1, amount);
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, 32f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (controller == null || other.GetComponent<PlayerCultivator>() == null)
        {
            return;
        }

        controller.OnRelicRecovered(this, crystalAmount);
        Destroy(gameObject);
    }
}
