using System.Collections;
using UnityEngine;
 
public class MusicLayerManager : MonoBehaviour
{
    public static MusicLayerManager Instance { get; private set; }
 
    [Header("Música (opcional — arrastrá tus AudioClips aquí)")]
    [Tooltip("Si dejás este campo vacío, se genera un tono automáticamente")]
    public AudioClip musicTrack1;
    [Tooltip("Si dejás este campo vacío, se genera un tono automáticamente")]
    public AudioClip musicTrack2;
    [Tooltip("Si dejás este campo vacío, se genera un tono automáticamente")]
    public AudioClip musicTrack3;
 
    [Header("Configuración")]
    [Tooltip("Duración del fade al cambiar de música (segundos)")]
    public float fadeDuration = 0.3f;
 
    private AudioSource[] _sources = new AudioSource[3];
    private int _activeTrack = 0;
    private bool _isFading = false;
 
    private static readonly float[] Frequencies = { 261.63f, 329.63f, 392.00f };
    private static readonly string[] TrackNames = { "Pista 1 (Rojo)", "Pista 2 (Verde)", "Pista 3 (Azul)" };
    private const float ToneDuration = 4f;
    private const int SampleRate = 44100;
 
    public int ActiveTrackIndex => _activeTrack;
    public event System.Action<int> OnTrackChanged;
 
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
 
        AudioClip[] clips = {
            musicTrack1 != null ? musicTrack1 : GenerateTone(Frequencies[0], ToneDuration),
            musicTrack2 != null ? musicTrack2 : GenerateTone(Frequencies[1], ToneDuration),
            musicTrack3 != null ? musicTrack3 : GenerateTone(Frequencies[2], ToneDuration)
        };
 
        for (int i = 0; i < 3; i++)
        {
            _sources[i] = gameObject.AddComponent<AudioSource>();
            _sources[i].loop = true;
            _sources[i].playOnAwake = false;
            _sources[i].volume = (i == 0) ? 1f : 0f;
            _sources[i].clip = clips[i];
 
            string origen = (i == 0 ? musicTrack1 : i == 1 ? musicTrack2 : musicTrack3) != null
                ? "AudioClip personalizado" : "Tono generado";
            Debug.Log($"[Música] Pista {i + 1} cargada: {origen}");
        }
    }
 
    private void Start()
    {
        double startDsp = AudioSettings.dspTime + 0.1;
        for (int i = 0; i < 3; i++)
            _sources[i].PlayScheduled(startDsp);
 
        Debug.Log($"[Música] Todas las pistas iniciadas. Activa: {TrackNames[_activeTrack]}");
    }
 
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
 
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToTrack(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToTrack(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToTrack(2);
    }
 
    public void SwitchToTrack(int index)
    {
        if (index == _activeTrack || _isFading) return;
        Debug.Log($"[Música] Cambiando → {TrackNames[index]}");
        StartCoroutine(FadeTrack(_activeTrack, index));
    }
 
    private IEnumerator FadeTrack(int from, int to)
    {
        _isFading = true;
        float elapsed = 0f;
        float volFrom = _sources[from].volume;
 
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            _sources[from].volume = Mathf.Lerp(volFrom, 0f, t);
            _sources[to].volume = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
 
        _sources[from].volume = 0f;
        _sources[to].volume = 1f;
        _activeTrack = to;
        _isFading = false;
        OnTrackChanged?.Invoke(_activeTrack);
        Debug.Log($"[Música] Pista activa ahora: {TrackNames[_activeTrack]}");
    }
 
    private static AudioClip GenerateTone(float frequency, float duration)
    {
        int totalSamples = Mathf.RoundToInt(duration * SampleRate);
        int samplesPerPeriod = Mathf.RoundToInt(SampleRate / frequency);
        totalSamples = (totalSamples / samplesPerPeriod) * samplesPerPeriod;
 
        float[] data = new float[totalSamples];
        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / SampleRate;
            data[i] = (Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.55f
                     + Mathf.Sin(2f * Mathf.PI * frequency * 2f * t) * 0.28f
                     + Mathf.Sin(2f * Mathf.PI * frequency * 3f * t) * 0.17f) * 0.45f;
        }
 
        AudioClip clip = AudioClip.Create($"Tone_{frequency:F1}Hz", totalSamples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
