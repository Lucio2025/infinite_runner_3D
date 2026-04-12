using UnityEngine;
 
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;
 
    [Header("Posición")]
    public Vector3 offset = new Vector3(0f, 5f, -8f);
 
    [Header("Suavizado")]
    [Tooltip("Qué tan rápido sigue la cámara al jugador. Más alto = más pegada")]
    public float smoothSpeed = 12f;
 
    private Vector3 _smoothVelocity = Vector3.zero;
 
    private void LateUpdate()
    {
        if (target == null) return;
 
        Vector3 desired = target.position + offset;
 
        // SmoothDamp elimina el temblor causado por la diferencia entre
        // FixedUpdate (física) y LateUpdate (cámara)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref _smoothVelocity,
            1f / smoothSpeed
        );
 
        transform.LookAt(target.position + Vector3.forward * 3f);
    }
}
