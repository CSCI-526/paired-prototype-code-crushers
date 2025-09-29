using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectiblePickup : MonoBehaviour
{
    [Tooltip("Heal amount in sanity units (0..1). 0.10 = 10%")]
    public float healAmount = 0.10f;

    void Awake()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (!pc) return;

        pc.ChangeSanity(healAmount);
        Destroy(gameObject);
    }
}


