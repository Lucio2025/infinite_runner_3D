using UnityEngine;

/// <summary>
/// Agregá este script a un GameObject vacío en la escena llamado "DayNightCycle".
/// Asignale la Directional Light desde el Inspector.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastrá el Directional Light de la escena aquí")]
    public Light sun;

    [Header("Velocidad")]
    [Tooltip("Duración de un ciclo completo día→noche→día en segundos")]
    public float cycleDuration = 60f;

    [Header("Luz del sol")]
    [Tooltip("Intensidad máxima (mediodía)")]
    public float maxSunIntensity = 1.3f;
    [Tooltip("Intensidad mínima (medianoche) — nunca llega a 0 para que se vea algo")]
    public float minSunIntensity = 0.08f;

    [Header("Colores del cielo")]
    public Color dayColor = new Color(0.49f, 0.71f, 0.91f);   // celeste
    public Color sunsetColor = new Color(0.95f, 0.45f, 0.15f);   // naranja
    public Color nightColor = new Color(0.03f, 0.03f, 0.10f);   // azul muy oscuro

    [Header("Color de la luz")]
    public Color daySunColor = new Color(0.98f, 0.92f, 0.82f); // blanco cálido
    public Color nightSunColor = new Color(0.20f, 0.20f, 0.40f); // azul frío

    [Header("Ambiente")]
    public Color dayAmbient = new Color(0.20f, 0.20f, 0.25f);
    public Color nightAmbient = new Color(0.04f, 0.04f, 0.10f);

    [Header("Brillo del piso de noche")]
    [Tooltip("El piso emite este color cuando oscurece")]
    public Color groundNightEmission = new Color(0.0f, 0.15f, 0.3f);
    [Tooltip("Referencia al material del piso (se asigna automáticamente si dejás vacío)")]
    public Material groundMaterial;

    private float _time = 0f; // 0 = mediodía, 0.5 = medianoche

    private void Start()
    {
        // Intenta obtener el material del piso desde SpawnPool si no fue asignado
        if (groundMaterial == null && SpawnPool.Instance != null)
            groundMaterial = SpawnPool.Instance.GroundMaterial;
    }

    private void Update()
    {
        _time += Time.deltaTime / cycleDuration;
        if (_time > 1f) _time -= 1f;

        // t va de 0 (mediodía) a 1 (medianoche) y vuelve, usando una onda senoidal
        float t = (1f - Mathf.Cos(_time * 2f * Mathf.PI)) * 0.5f;

        UpdateSun(t);
        UpdateSky(t);
        UpdateGround(t);
    }

    private void UpdateSun(float t)
    {
        if (sun == null) return;
        sun.intensity = Mathf.Lerp(maxSunIntensity, minSunIntensity, t);
        sun.color = Color.Lerp(daySunColor, nightSunColor, t);

        // Rotación: de 50° (día) a -30° (noche)
        float pitch = Mathf.Lerp(50f, -30f, t);
        sun.transform.rotation = Quaternion.Euler(pitch, -30f, 0f);
    }

    private void UpdateSky(float t)
    {
        Color skyColor;
        if (t < 0.5f)
            skyColor = Color.Lerp(dayColor, sunsetColor, t * 2f);
        else
            skyColor = Color.Lerp(sunsetColor, nightColor, (t - 0.5f) * 2f);

        Camera.main.backgroundColor = skyColor;
        RenderSettings.ambientLight = Color.Lerp(dayAmbient, nightAmbient, t);
    }

    private void UpdateGround(float t)
    {
        if (groundMaterial == null) return;

        // De noche el piso emite un leve brillo azulado
        groundMaterial.EnableKeyword("_EMISSION");
        Color emission = Color.Lerp(Color.black, groundNightEmission, t);
        groundMaterial.SetColor("_EmissionColor", emission);
    }
}
