using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("References")]
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

    [Header("Decor Prefabs (auto-loaded from Resources/Prefabs)")]
    public GameObject spikesPrefab;        
    public GameObject collectiblePrefab;   

    [Header("Decor Frequency (new)")]
    [Tooltip("Force a SPIKE after this many empty platforms (inclusive).")]
    [SerializeField] int forceSpikeAfterEmptyMin = 4;
    [SerializeField] int forceSpikeAfterEmptyMax = 5;
    [SerializeField] bool reRollForceAfterEachSpike = true;

    [Tooltip("Base chance a platform gets ANY decor (before streak boosts).")]
    [SerializeField, Range(0f,1f)] float baseDecorChance = 0.55f;

    [Tooltip("Extra chance added per empty platform (for any decor).")]
    [SerializeField, Range(0f,1f)] float streakBoostPerEmpty = 0.10f;

    [Tooltip("Extra chance added per empty since last SPIKE (prefer spikes after long dry runs).")]
    [SerializeField, Range(0f,1f)] float spikeBoostPerEmpty = 0.08f;

    [Tooltip("Minimum number of empty platforms required between any decorations.")]
    public int minEmptyPlatformsBetweenDecor = 0; 

    [Header("Decor Mix")]
    [Range(0f,1f)] public float spikeWeight = 0.6f;      
    [Range(0f,1f)] public float collectibleWeight = 0.4f; 

    [Header("Decor Placement")]
    [Tooltip("Spikes sit on platform top; keep this 0 unless you want a gap.")]
    public float spikeYOffset = 0f;           
    public float spikeWidthFactor = 0.90f;    
    public float collectibleYOffset = 0.60f;  
    public int renderOrderBoost = 10;         

    [Header("Collectible Size (world units)")]
    [SerializeField, Tooltip("Circle diameter in WORLD units (independent of parent/platform scale)")]
    float collectibleDiameterWorld = 0.70f;   

    
    private float nextSpawnX;
    private float lastY;
    private float maxStepY;
    private bool firstPlatformSpawned = false;

    
    int emptyStreak = 0;         
    int emptiesSinceSpike = 0;   
    int currentForceThreshold;   

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

       
        currentForceThreshold = Random.Range(forceSpikeAfterEmptyMin, forceSpikeAfterEmptyMax + 1);
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

        
        Vector3 s = platform.transform.localScale;
        s.x = width;
        platform.transform.localScale = s;

        
        MaybeDecorate(platform);

        nextSpawnX = xPos + (width / 2f);
    }

    void MaybeDecorate(GameObject platform)
    {
       
        if (emptyStreak < minEmptyPlatformsBetweenDecor)
        {
            emptyStreak++;
            emptiesSinceSpike++;
            return;
        }

        Bounds b = GetWorldBounds(platform);

        
        if (emptiesSinceSpike >= currentForceThreshold && spikesPrefab != null)
        {
            var spikes = Instantiate(spikesPrefab, platform.transform);
            PositionSpikes(spikes, b);

            
            emptyStreak = 0;
            emptiesSinceSpike = 0;
            if (reRollForceAfterEachSpike)
                currentForceThreshold = Random.Range(forceSpikeAfterEmptyMin, forceSpikeAfterEmptyMax + 1);
            return;
        }

        
        float boost = streakBoostPerEmpty * emptyStreak + spikeBoostPerEmpty * emptiesSinceSpike;
        float chance = Mathf.Clamp01(baseDecorChance + boost);

        if (Random.value <= chance)
        {
            
            float total = Mathf.Max(0.0001f, spikeWeight + collectibleWeight);
            float roll = Random.value * total;

            if (roll < spikeWeight && spikesPrefab != null)
            {
                var spikes = Instantiate(spikesPrefab, platform.transform);
                PositionSpikes(spikes, b);
                emptiesSinceSpike = 0; 
            }
            else if (collectiblePrefab != null)
            {
                var c = Instantiate(collectiblePrefab, platform.transform);
                PositionCollectible(c, b);
                
                emptiesSinceSpike++;
            }
            else
            {
                
                emptyStreak++;
                emptiesSinceSpike++;
                return;
            }

            
            emptyStreak = 0;
        }
        else
        {
            
            emptyStreak++;
            emptiesSinceSpike++;
        }
    }

   

    void PositionSpikes(GameObject spikesGO, Bounds pb)
{
    
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
    float seat = (strip != null) ? strip.seatSkin : 0.015f;  
    spikesGO.transform.position = new Vector3(pb.center.x, pb.max.y - seat, 0f);

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


    void PositionCollectible(GameObject c, Bounds pb)
    {
        float x = Random.Range(pb.min.x + 0.25f, pb.max.x - 0.25f);
        float y = pb.max.y + collectibleYOffset;
        c.transform.position = new Vector3(x, y, 0f);

        
        var sr = c.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float spriteW = sr.sprite.rect.width  / sr.sprite.pixelsPerUnit;
            float spriteH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
            float spriteMax = Mathf.Max(0.0001f, Mathf.Max(spriteW, spriteH));

            float desiredWorldScale = collectibleDiameterWorld / spriteMax;

            Vector3 parentLossy = c.transform.parent ? c.transform.parent.lossyScale : Vector3.one;
            c.transform.localScale = new Vector3(
                desiredWorldScale / Mathf.Max(0.0001f, parentLossy.x),
                desiredWorldScale / Mathf.Max(0.0001f, parentLossy.y),
                1f
            );
        }
        else
        {
            
            float s = collectibleDiameterWorld;
            Vector3 parentLossy = c.transform.parent ? c.transform.parent.lossyScale : Vector3.one;
            c.transform.localScale = new Vector3(
                s / Mathf.Max(0.0001f, parentLossy.x),
                s / Mathf.Max(0.0001f, parentLossy.y),
                1f
            );
        }

        
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
