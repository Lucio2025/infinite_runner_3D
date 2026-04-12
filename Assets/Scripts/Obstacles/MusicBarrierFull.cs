using UnityEngine;

public class MusicBarrierFull : MonoBehaviour
{
    public int requiredTrack = 0;

    [Header("Transparencia")]
    [Tooltip("Alpha cuando la música es correcta (0=invisible, 1=opaco)")]
    public float safeAlpha = 0.15f;
    [Tooltip("Alpha cuando la música es incorrecta")]
    public float dangerAlpha = 1f;
    [Tooltip("Velocidad del fade de transparencia")]
    public float fadeSpeed = 6f;

    private static readonly Color[] TrackColors =
    {
        new Color(1f, 0.15f, 0.15f),
        new Color(0.15f, 1f, 0.15f),
        new Color(0.15f, 0.4f, 1f)
    };

    private static readonly string[] TrackNames = { "Pista 1 (Rojo)", "Pista 2 (Verde)", "Pista 3 (Azul)" };

    private Material _mat;
    private Color _baseColor;

    private void Awake()
    {
        _mat = GetComponent<Renderer>().material;
        SetMaterialTransparent(_mat);
    }

    public void Configure(int track)
    {
        requiredTrack = track;
        _baseColor = TrackColors[track];
        _mat.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, dangerAlpha);
        _mat.EnableKeyword("_EMISSION");
        _mat.SetColor("_EmissionColor", _baseColor * 1.5f);
    }

    private void Update()
    {
        if (MusicLayerManager.Instance == null) return;
        int active = MusicLayerManager.Instance.ActiveTrackIndex;
        float targetAlpha = (active == requiredTrack) ? safeAlpha : dangerAlpha;
        Color current = _mat.color;
        float newAlpha = Mathf.Lerp(current.a, targetAlpha, fadeSpeed * Time.deltaTime);
        _mat.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, newAlpha);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        int active = MusicLayerManager.Instance?.ActiveTrackIndex ?? 0;
        if (active != requiredTrack)
        {
            Debug.Log($"[Barrera Total] ¡Música incorrecta! Necesitabas: {TrackNames[requiredTrack]} | Tenías: {TrackNames[active]}");
            other.GetComponent<PlayerHealth>()?.TakeDamage();
        }
        else
        {
            Debug.Log($"[Barrera Total] ¡Correcto! Pasaste con {TrackNames[active]}");
        }
    }

    private static void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
    }
}