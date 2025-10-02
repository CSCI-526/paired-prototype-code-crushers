using UnityEngine;

public class GameOverOnSanity : MonoBehaviour
{
    [Tooltip("Player root that has PlayerController. If left empty, we'll auto-find.")]
    public PlayerController player;

    [Tooltip("Your game manager object that has a public GameOver() method.")]
    public GameManager gm;

    void Awake()
    {
        
        if (!player)
        {
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged) player = tagged.GetComponent<PlayerController>();
            if (!player) player = FindObjectOfType<PlayerController>();
        }
    }

    void OnEnable()
    {
        if (player) player.OnSanityChanged += HandleSanityChanged;
    }

    void Start()
    {
        
        if (player && player.sanity <= 0f) gm?.GameOver();
    }

    void OnDisable()
    {
        if (player) player.OnSanityChanged -= HandleSanityChanged;
    }

    void HandleSanityChanged(float value)
    {
        if (value <= 0f) gm?.GameOver();
    }
}
