using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SpikeStrip : MonoBehaviour
{
    [Header("Visual tooth (triangle)")]
    public GameObject spikeUnitPrefab;   
    public float unitWidth = 0.5f;       
    public float unitHeight = 0.3f;      
    public float seatSkin = 0.015f; 

    [Header("Sorting")]
    public int sortingOrderBoost = 10;

    BoxCollider2D trigger;

    void Awake()
    {
        trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
    }

    
    public void Build(float targetWidth)
{
   
    for (int i = transform.childCount - 1; i >= 0; i--)
        Destroy(transform.GetChild(i).gameObject);

    int count = Mathf.Max(1, Mathf.RoundToInt(targetWidth / Mathf.Max(0.01f, unitWidth)));
    float totalWidth = count * unitWidth;
    float left = -totalWidth * 0.5f + unitWidth * 0.5f;

   
    for (int i = 0; i < count; i++)
    {
        var tooth = Instantiate(spikeUnitPrefab, transform);
        tooth.transform.localRotation = Quaternion.identity;

        var sr = tooth.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float spriteW = sr.sprite.rect.width  / sr.sprite.pixelsPerUnit;
            float spriteH = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
            tooth.transform.localScale = new Vector3(
                unitWidth  / Mathf.Max(0.0001f, spriteW),
                unitHeight / Mathf.Max(0.0001f, spriteH),
                1f
            );

            int baseOrder = 0;
            var platSR = transform.parent ? transform.parent.GetComponent<SpriteRenderer>() : null;
            if (platSR) baseOrder = platSR.sortingOrder;
            sr.sortingOrder = baseOrder + sortingOrderBoost;
        }

        
        tooth.transform.localPosition = new Vector3(left + i * unitWidth,
                                                    unitHeight * 0.5f,
                                                    0f);
    }

   
    trigger.size   = new Vector2(totalWidth, unitHeight);
    trigger.offset = new Vector2(0f, unitHeight * 0.5f);
}

}
