using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionController : MonoBehaviour
{
    public Transform player;
    public float baseScale = 12.5f;
    private PlayerController playerController;
    void Start()
    {
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!player || !playerController) return;
        
        transform.position = player.position;
        float sanity = playerController.sanity;
        float newScale =  sanity * baseScale;
        
        transform.localScale = new Vector3(newScale, newScale, 1f);
    }
}
