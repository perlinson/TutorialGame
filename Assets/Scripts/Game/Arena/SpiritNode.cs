using UnityEngine;

public sealed class SpiritNode : MonoBehaviour
{
    [SerializeField] private int qiAmount = 1;

    private GameController controller;

    public void Configure(GameController gameController, int amount)
    {
        controller = gameController;
        qiAmount = Mathf.Max(1, amount);
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, 40f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (controller == null || other.GetComponent<PlayerCultivator>() == null)
        {
            return;
        }

        controller.OnSpiritCollected(this, qiAmount);
        Destroy(gameObject);
    }
}
