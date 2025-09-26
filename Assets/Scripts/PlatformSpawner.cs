using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject platformPrefab;

    [Header("Spawn Settings")]
    public float spawnAheadDistance = 25f;

    [Header("Platform Shape")]
    public float minWidth = 2f;
    public float maxWidth = 3.5f;

    [Header("Gap Settings")]
    public float minGapX = 1.2f;
    public float maxGapX = 3f;

    [Header("Height Settings")]
    public float minY = -2.5f;
    public float maxY = 3f;

    [Header("Player Jump Sync")]
    public float desiredJumpHeight = 2.5f;
    public float gravityScale = 2.5f;

    private float nextSpawnX;
    private float lastY;
    private float maxStepY;
    private bool firstPlatformSpawned = false;

    void Start()
    {
        nextSpawnX = player.position.x + 2f; 
        lastY = 0f;

        float g = Mathf.Abs(Physics2D.gravity.y * gravityScale);
        float jumpVel = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, desiredJumpHeight));
        float airtime = (2f * jumpVel) / g;

        maxStepY = desiredJumpHeight * 0.9f;
    }

    void Update()
    {
        while (nextSpawnX < player.position.x + spawnAheadDistance)
        {
            SpawnPlatform();
        }
    }

    void SpawnPlatform()
    {
        float width = Random.Range(minWidth, maxWidth);

        float gapX;
        if (!firstPlatformSpawned)
        {
            
            gapX = 2f;
            firstPlatformSpawned = true;
        }
        else
        {
            gapX = Random.Range(minGapX, maxGapX);
        }

        float stepY = Random.Range(-maxStepY, maxStepY);
        lastY = Mathf.Clamp(lastY + stepY, minY, maxY);

        float xPos = nextSpawnX + gapX + (width / 2f);
        Vector3 pos = new Vector3(xPos, lastY, 0f);

        GameObject platform = Instantiate(platformPrefab, pos, Quaternion.identity);

        Vector3 s = platform.transform.localScale;
        s.x = width;
        platform.transform.localScale = s;

        nextSpawnX = xPos + (width / 2f);
    }
}
