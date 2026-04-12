using UnityEngine;

public class Coin : CollectibleBase
{
    [Tooltip("Velocidad de rotación en grados por segundo")]
    public float spinSpeed = 180f;

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    protected override void OnCollect(PlayerController player)
    {
        int totalCoins = (GameManager.Instance?.Coins ?? 0) + 1;
        Debug.Log($"[Moneda] ¡Recogida! Total: {totalCoins}");
        GameManager.Instance?.AddCoins(1);
        SpawnPool.Instance?.ReturnCoin(gameObject);
    }
}