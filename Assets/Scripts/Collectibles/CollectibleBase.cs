using UnityEngine;

public abstract class CollectibleBase : MonoBehaviour
{
    protected bool _collected = false;

    protected virtual void OnEnable()
    {
        _collected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;

        _collected = true;
        PlayerController pc = other.GetComponent<PlayerController>();
        OnCollect(pc);
    }

    protected abstract void OnCollect(PlayerController player);
}
