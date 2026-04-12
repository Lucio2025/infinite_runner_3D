using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Agregá este script al Canvas junto a GameUI.
/// Maneja los efectos pop/flash de los textos del HUD.
/// </summary>
public class HUDAnimator : MonoBehaviour
{
    [Header("Referencias HUD")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI musicText;

    [Header("Configuración pop")]
    [Tooltip("Escala máxima del efecto pop (1.3 = 30% más grande)")]
    public float popScale = 1.35f;
    [Tooltip("Velocidad del efecto pop")]
    public float popSpeed = 12f;

    [Header("Colores flash")]
    public Color livesFlashColor = new Color(1f, 0.3f, 0.3f);   // rojo
    public Color coinsFlashColor = new Color(1f, 0.95f, 0.2f);  // amarillo
    public Color musicFlashColor = new Color(0.4f, 0.8f, 1f);   // celeste

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged += _ => Pop(livesText, livesFlashColor);
            GameManager.Instance.OnCoinsChanged += _ => Pop(coinsText, coinsFlashColor);
        }
        if (MusicLayerManager.Instance != null)
            MusicLayerManager.Instance.OnTrackChanged += _ => Pop(musicText, musicFlashColor);
    }

    private void Pop(TextMeshProUGUI label, Color flashColor)
    {
        if (label == null) return;
        StartCoroutine(PopRoutine(label, flashColor));
    }

    private IEnumerator PopRoutine(TextMeshProUGUI label, Color flashColor)
    {
        Color originalColor = Color.white;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * popScale;

        // Flash de color + escala hacia arriba
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * popSpeed;
            label.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            label.color = Color.Lerp(originalColor, flashColor, t);
            yield return null;
        }

        // Volver a normal
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * popSpeed * 0.6f; // vuelve un poco más lento
            label.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            label.color = Color.Lerp(flashColor, originalColor, t);
            yield return null;
        }

        label.transform.localScale = originalScale;
        label.color = originalColor;
    }
}