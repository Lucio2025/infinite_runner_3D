using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Invencibilidad")]
    [Tooltip("Segundos de invencibilidad tras recibir daño")]
    public float invincibilityDuration = 1.5f;

    [Header("Parpadeo")]
    [Tooltip("Cuántas veces parpadea durante la invencibilidad")]
    public int blinkCount = 6;

    private bool _isInvincible = false;
    public bool IsInvincible => _isInvincible;

    private Renderer[] _renderers;

    private void Awake()
    {
        // Recoge todos los renderers del jugador y sus hijos
        _renderers = GetComponentsInChildren<Renderer>();
    }

    public void TakeDamage()
    {
        if (_isInvincible) return;
        if (GameManager.Instance == null) return;

        int vidasRestantes = Mathf.Max(0, GameManager.Instance.Lives - 1);
        Debug.Log($"[Salud] ¡Daño recibido! Vidas restantes: {vidasRestantes}");

        GameManager.Instance.TakeDamage();
        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        _isInvincible = true;
        Debug.Log($"[Salud] Invencibilidad activada por {invincibilityDuration}s");

        float blinkInterval = invincibilityDuration / (blinkCount * 2f);

        for (int i = 0; i < blinkCount * 2; i++)
        {
            // Alterna visibilidad de todos los renderers
            bool visible = (i % 2 == 0) ? false : true;
            SetRenderersVisible(visible);
            yield return new WaitForSecondsRealtime(blinkInterval);
        }

        SetRenderersVisible(true);
        _isInvincible = false;
        Debug.Log("[Salud] Invencibilidad terminada");
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (var r in _renderers)
            if (r != null) r.enabled = visible;
    }
}