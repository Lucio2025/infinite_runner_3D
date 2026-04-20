using UnityEngine;

public class Coin : CollectibleBase
{
    [Header("Rotación")]
    public float spinSpeed = 180f;

    [Header("Imán")]
    [Tooltip("Radio en el que la moneda empieza a moverse hacia el jugador")]
    public float magnetRadius = 2.5f;
    [Tooltip("Velocidad a la que la moneda se mueve hacia el jugador")]
    public float magnetSpeed = 19f;
    [Tooltip("Cuántas unidades detrás del jugador antes de reciclarse")]
    public float despawnBehind = 5f;

    public static float MagnetMultiplier = 1f;

    private Transform _player;
    private bool _attracted = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        _attracted = false;

        if (_player == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null) _player = playerGO.transform;
        }
    }

    private void Update()
    {
        // Rotación siempre activa
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

        if (_collected || _player == null) return;

        // Reciclar solo cuando queda bien atrás del jugador
        if (transform.position.z < _player.position.z - despawnBehind)
        {
            _collected = true;
            SpawnPool.Instance?.ReturnCoin(gameObject);
            return;
        }

        // Lógica del imán
        float dist = Vector3.Distance(transform.position, _player.position);
        float effectiveRadius = magnetRadius * MagnetMultiplier;

        if (dist <= effectiveRadius)
            _attracted = true;

        if (_attracted)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _player.position,
                magnetSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, _player.position) < 0.4f)
            {
                _collected = true;
                OnCollect(null);
            }
        }
    }

    protected override void OnCollect(PlayerController player)
    {
        int totalCoins = (GameManager.Instance?.Coins ?? 0) + 1;
        Debug.Log($"[Moneda] ¡Recogida! Total: {totalCoins}");
        GameManager.Instance?.AddCoins(1);
        SpawnPool.Instance?.ReturnCoin(gameObject);
    }
}
