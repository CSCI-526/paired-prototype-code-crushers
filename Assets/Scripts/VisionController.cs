using UnityEngine;

public class VisionController : MonoBehaviour
{
    public Transform player;

    [Header("Scale Mapping")]
    [Tooltip("World scale at sanity = 1.0 (full vision)")]
    public float baseScale = 12.5f;

    [Tooltip("Baseline size near 0 sanity (keeps the circle visible at 0)")]
    public float floorScale = 8.0f;   

    [Tooltip("Gently lifts low sanity values so 0..~0.2 doesn't collapse. 0 = off.")]
    [Range(0f, 0.3f)]
    public float lowEndBias = 0.08f;  

    private PlayerController playerController;

    void Start()
    {
        if (!player)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo) player = pgo.transform;
        }
        if (player) playerController = player.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!player || !playerController) return;

       
        transform.position = player.position;

        
        float s = Mathf.Clamp01(playerController.sanity);
        float t = (s + lowEndBias) / (1f + lowEndBias);   
        float eased = t * t * (3f - 2f * t);              

        
        float newScale = Mathf.Lerp(floorScale, baseScale, eased);
        transform.localScale = new Vector3(newScale, newScale, 1f);
    }
}
