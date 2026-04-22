using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float baseSpeed = 10f;
    public float speedIncreaseRate = 0.5f;
    public float speedIncreaseInterval = 10f;

    [Header("Carriles")]
    public float laneSwitchSpeed = 8f;

    [Header("Inclinación al cambiar de carril")]
    [Tooltip("Ángulo máximo de inclinación en grados")]
    public float tiltAngle = 20f;
    [Tooltip("Velocidad de la inclinación")]
    public float tiltSpeed = 10f;

    public static readonly float[] LanePositions = { -2.5f, 0f, 2.5f };
    private static readonly string[] LaneNames = { "Izquierda", "Centro", "Derecha" };

    private Rigidbody _rb;
    private int _currentLane = 1;
    private float _targetX;
    private float _currentSpeed;
    private float _speedTimer;
    private bool _gameOver;

    // Para la inclinación
    private Transform _visual;      // hijo visual de la cápsula
    private float _targetTilt = 0f; // ángulo objetivo

    public float CurrentSpeed => _currentSpeed;
    public int CurrentLane => _currentLane;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Start()
    {
        _currentSpeed = baseSpeed;
        _targetX = LanePositions[_currentLane];
        Debug.Log("[Jugador] Partida iniciada. Carril inicial: Centro");

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += () => _gameOver = true;
    }

    private void Update()
    {
        if (_gameOver) return;
        HandleInput();
        UpdateSpeed();
        UpdateTilt();
    }

    private void FixedUpdate()
    {
        if (_gameOver) return;
        float newX = Mathf.Lerp(transform.position.x, _targetX, laneSwitchSpeed * Time.fixedDeltaTime);
        float newZ = transform.position.z + _currentSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(new Vector3(newX, 1f, newZ));
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            SwitchLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            SwitchLane(1);
    }

    private void SwitchLane(int dir)
    {
        int newLane = Mathf.Clamp(_currentLane + dir, 0, 2);
        if (newLane == _currentLane) return;
        _currentLane = newLane;
        _targetX = LanePositions[_currentLane];

        // Inclinar hacia el lado del movimiento
        _targetTilt = -dir * tiltAngle;

        Debug.Log($"[Jugador] Cambio de carril → {LaneNames[_currentLane]}");
    }

    private void UpdateTilt()
    {
        float distToTarget = Mathf.Abs(transform.position.x - _targetX);
        if (distToTarget < 0.05f)
            _targetTilt = 0f;

        float currentZ = transform.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;

        float newZ = Mathf.Lerp(currentZ, _targetTilt, tiltSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(0f, 0f, newZ);
    }

    private void UpdateSpeed()
    {
        _speedTimer += Time.deltaTime;
        if (_speedTimer >= speedIncreaseInterval)
        {
            _speedTimer = 0f;
            _currentSpeed += speedIncreaseRate;
            Debug.Log($"[Jugador] Velocidad aumentada → {_currentSpeed:F1}");
        }
    }
}