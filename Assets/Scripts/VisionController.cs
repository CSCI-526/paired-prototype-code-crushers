using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionController : MonoBehaviour
{
    public Transform player;
    public float baseScale = 12.5f;

    void Update()
    {
        if (!player) return;

        
        transform.position = player.position;

       
        transform.localScale = new Vector3(baseScale, baseScale, 1f);
    }
}
