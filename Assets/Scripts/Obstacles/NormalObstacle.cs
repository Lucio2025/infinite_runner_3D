using UnityEngine;

public class NormalObstacle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("[Obstáculo] ¡Choque con obstáculo normal!");
        other.GetComponent<PlayerHealth>()?.TakeDamage();
    }
}