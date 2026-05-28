using UnityEngine;

public sealed class SpiritHerb : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    [SerializeField] private int qiAmount = 1;

    private GameController controller;

    public void Configure(GameController gameController, int heal, int qi)
    {
        controller = gameController;
        healAmount = Mathf.Max(1, heal);
        qiAmount = Mathf.Max(1, qi);
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, 55f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (controller == null || other.GetComponent<PlayerCultivator>() == null)
        {
            return;
        }

        controller.OnHerbCollected(this, healAmount, qiAmount);
        Destroy(gameObject);
    }
}
