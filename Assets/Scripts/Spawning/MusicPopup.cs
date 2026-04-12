using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Texto inferior que aparece con fade al cambiar de música.
/// Agregá este script a un GameObject vacío dentro del Canvas
/// y asignale las referencias desde el Inspector.
/// </summary>
public class MusicPopup : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El texto TMP del popup (hijo de este GameObject)")]
    public TextMeshProUGUI popupText;
    [Tooltip("El fondo semitransparente detrás del texto (opcional)")]
    public Image background;

    [Header("Configuración")]
    [Tooltip("Cuánto tiempo se mantiene visible antes de desaparecer")]
    public float displayDuration = 1.2f;
    [Tooltip("Velocidad del fade in")]
    public float fadeInSpeed = 8f;
    [Tooltip("Velocidad del fade out")]
    public float fadeOutSpeed = 3f;

    private static readonly string[] TrackLabels = { "♪ Música 1", "♪ Música 2", "♪ Música 3" };
    private static readonly Color[] TrackColors =
    {
        new Color(1f, 0.4f, 0.4f),     // rojo suave
        new Color(0.4f, 1f, 0.5f),     // verde suave
        new Color(0.4f, 0.6f, 1f)      // azul suave
    };

    private Coroutine _current;

    private void Start()
    {
        SetAlpha(0f);

        if (MusicLayerManager.Instance != null)
            MusicLayerManager.Instance.OnTrackChanged += ShowPopup;
    }

    private void OnDestroy()
    {
        if (MusicLayerManager.Instance != null)
            MusicLayerManager.Instance.OnTrackChanged -= ShowPopup;
    }

    public void ShowPopup(int trackIndex)
    {
        if (popupText == null) return;
        popupText.text = TrackLabels[trackIndex];
        popupText.color = TrackColors[trackIndex];

        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(PopupRoutine());
    }

    private IEnumerator PopupRoutine()
    {
        // Fade in
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha = Mathf.MoveTowards(alpha, 1f, fadeInSpeed * Time.deltaTime);
            SetAlpha(alpha);
            yield return null;
        }

        // Mantener visible
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        while (alpha > 0f)
        {
            alpha = Mathf.MoveTowards(alpha, 0f, fadeOutSpeed * Time.deltaTime);
            SetAlpha(alpha);
            yield return null;
        }
    }

    private void SetAlpha(float a)
    {
        if (popupText != null)
        {
            Color c = popupText.color;
            c.a = a;
            popupText.color = c;
        }
        if (background != null)
        {
            Color c = background.color;
            c.a = a * 0.55f; // fondo siempre más transparente que el texto
            background.color = c;
        }
    }
}
