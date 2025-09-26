using UnityEngine;

public class FallDetector : MonoBehaviour
{
    public Transform player;    
    public float fallBuffer = 2f;

    private Camera cam;
    private GameManager gm;

    void Start()
    {
        cam = Camera.main;
        gm  = FindObjectOfType<GameManager>();

        if (player == null)
        {
         
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (!player || !cam || gm == null) return;

        float bottom = cam.transform.position.y - cam.orthographicSize;
        if (player.position.y < bottom - fallBuffer)
        {
            gm.GameOver();
        }
    }
}