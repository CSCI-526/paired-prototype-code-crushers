using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class SpikeHazard : MonoBehaviour
{
    [Tooltip("Damage in sanity units (0..1). 0.10 = 10%")]
    public float damage = 0.10f;

    private HashSet<PlayerController> touched = new HashSet<PlayerController>();

    void Awake()
    {
        var box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (!pc) return;

        // first time entering this spike strip during this contact
        if (touched.Add(pc))
            pc.ChangeSanity(-damage);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc) touched.Remove(pc);
    }
}
