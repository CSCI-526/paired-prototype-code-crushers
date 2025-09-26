using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject platformPrefab;

    [Header("Collectible Size (world units)")]
[SerializeField, Tooltip("Circle diameter in WORLD units (independent of parent/platform scale)")]
float collectibleDiameterWorld = 0.65f;   // try 0.65–0.80

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

    [Header("Decor Prefabs (auto-loaded from Resources/Prefabs)")]
    public GameObject spikesPrefab;        // Resources/Prefabs/Spikes
    public GameObject collectiblePrefab;   // Resources/Prefabs/Collectible

    [Header("Decor Frequency")]
    [Range(0f,1f)] public float overallDecorChance = 0.35f; // chance a platform gets ANY decor
    [Range(0f,1f)] public float spikeWeight = 0.55f;        // relative weight vs collectible
    [Range(0f,1f)] public float collectibleWeight = 0.45f;  // should sum ~1.0
    public int minEmptyPlatformsBetweenDecor = 2;           // force gaps of empty platforms

    [Header("Decor Placement")]
    public float spikeYOffset = 0.20f;
    public float spikeWidthFactor = 0.90f;
    public float collectibleYOffset = 0.60f;
    public int renderOrderBoost = 10;

    private float nextSpawnX;
    private float lastY;
    private float maxStepY;
    private bool firstPlatformSpawned = false;

    // frequency state
    private int emptyStreak = 999;   // start high so first few can decorate if roll passes

    void Start()
    {
        nextSpawnX = player.position.x + 2f;
        lastY = 0f;

        float g = Mathf.Abs(Physics2D.gravity.y * gravityScale);
        float jumpVel = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, desiredJumpHeight));
        float airtime = (2f * jumpVel) / g;
        maxStepY = desiredJumpHeight * 0.9f;

        if (spikesPrefab == null)
        {
            spikesPrefab = Resources.Load<GameObject>("Prefabs/Spikes");
            if (spikesPrefab == null) Debug.LogWarning("[Spawner] Missing Resources/Prefabs/Spikes.prefab");
        }
        if (collectiblePrefab == null)
        {
            collectiblePrefab = Resources.Load<GameObject>("Prefabs/Collectible");
            if (collectiblePrefab == null) Debug.LogWarning("[Spawner] Missing Resources/Prefabs/Collectible.prefab");
        }
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

        float gapX = (!firstPlatformSpawned) ? 2f : Random.Range(minGapX, maxGapX);
        firstPlatformSpawned = true;

        float stepY = Random.Range(-maxStepY, maxStepY);
        lastY = Mathf.Clamp(lastY + stepY, minY, maxY);

        float xPos = nextSpawnX + gapX + (width / 2f);
        Vector3 pos = new Vector3(xPos, lastY, 0f);

        GameObject platform = Instantiate(platformPrefab, pos, Quaternion.identity);

        // scale width
        Vector3 s = platform.transform.localScale;
        s.x = width;
        platform.transform.localScale = s;

        // maybe decorate
        MaybeDecorate(platform);

        nextSpawnX = xPos + (width / 2f);
    }

    void MaybeDecorate(GameObject platform)
    {
        // enforce empty streak
        if (emptyStreak < minEmptyPlatformsBetweenDecor)
        {
            emptyStreak++;
            return; // must remain empty
        }

        // roll for any decor at all
        if (Random.value > overallDecorChance)
        {
            emptyStreak++; // stayed empty
            return;
        }

        // choose type by weight
        float total = Mathf.Max(0.0001f, spikeWeight + collectibleWeight);
        float roll = Random.value * total;

        Bounds b = GetWorldBounds(platform);

        if (roll < spikeWeight && spikesPrefab != null)
        {
            var spikes = Instantiate(spikesPrefab, platform.transform);
            PositionSpikes(spikes, b);
        }
        else if (collectiblePrefab != null)
        {
            var c = Instantiate(collectiblePrefab, platform.transform);
            PositionCollectible(c, b);
        }
        else
        {
            // if nothing available, treat as empty
            emptyStreak++;
            return;
        }

        // reset streak since we placed something
        emptyStreak = 0;
    }

void PositionSpikes(GameObject spikesGO, Bounds pb)
{
    // Sit the strip base on the platform top (no vertical gap)
    spikesGO.transform.position = new Vector3(pb.center.x, pb.max.y, 0f);

    // neutralize parent scale (keep this if you added earlier)
    var parent = spikesGO.transform.parent;
    if (parent != null)
    {
        Vector3 p = parent.lossyScale;
        spikesGO.transform.localScale = new Vector3(
            (p.x != 0f) ? 1f / p.x : 1f,
            (p.y != 0f) ? 1f / p.y : 1f,
            1f
        );
    }

    var strip = spikesGO.GetComponent<SpikeStrip>();
    if (strip != null)
    {
        float targetWidth = Mathf.Max(0.1f, pb.size.x * spikeWidthFactor);
        strip.Build(targetWidth);
    }
    else
    {
        Debug.LogWarning("[Spawner] SpikeStrip missing on spikes prefab. Add it to avoid stretching.");
    }
}



// keep this field
void PositionCollectible(GameObject c, Bounds pb)
{
    float x = Random.Range(pb.min.x + 0.25f, pb.max.x - 0.25f);
    float y = pb.max.y + collectibleYOffset;
    c.transform.position = new Vector3(x, y, 0f);

    // --- Force a fixed WORLD diameter, independent of parent/platform scale ---
    var sr = c.GetComponent<SpriteRenderer>();
    if (sr != null && sr.sprite != null)
    {
        // native sprite size in WORLD units
        float spriteW = sr.sprite.rect.width  / sr.sprite.pixelsPerUnit;
        float spriteH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
        float spriteMax = Mathf.Max(0.0001f, Mathf.Max(spriteW, spriteH));

        // scale needed in WORLD units
        float desiredWorldScale = collectibleDiameterWorld / spriteMax;

        // convert to LOCAL scale (cancel parent lossy scale so the world size stays exact)
        Vector3 parentLossy = c.transform.parent ? c.transform.parent.lossyScale : Vector3.one;
        c.transform.localScale = new Vector3(
            desiredWorldScale / Mathf.Max(0.0001f, parentLossy.x),
            desiredWorldScale / Mathf.Max(0.0001f, parentLossy.y),
            1f
        );
    }
    else
    {
        // fallback if no sprite: use a simple multiplier
        float s = collectibleDiameterWorld;
        Vector3 parentLossy = c.transform.parent ? c.transform.parent.lossyScale : Vector3.one;
        c.transform.localScale = new Vector3(
            s / Mathf.Max(0.0001f, parentLossy.x),
            s / Mathf.Max(0.0001f, parentLossy.y),
            1f
        );
    }

    // keep rendering above platform
    var platSR = c.transform.parent.GetComponent<SpriteRenderer>();
    var cs = c.GetComponent<SpriteRenderer>();
    if (cs && platSR) cs.sortingOrder = platSR.sortingOrder + renderOrderBoost;
}




    Bounds GetWorldBounds(GameObject go)
    {
        var col = go.GetComponent<Collider2D>();
        if (col) return col.bounds;

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr) return sr.bounds;

        var childCol = go.GetComponentInChildren<Collider2D>();
        if (childCol) return childCol.bounds;

        var childSR = go.GetComponentInChildren<SpriteRenderer>();
        if (childSR) return childSR.bounds;

        return new Bounds(go.transform.position, new Vector3(1f, 0.25f, 0f));
    }
}
