using UnityEngine;
using UnityEngine.UI;
using TMPro;


[DefaultExecutionOrder(1000)]
public class SanityBarCanvasHUD : MonoBehaviour
{
    [Header("Find Player")]
    [Tooltip("If left empty, we'll find a PlayerController by tag 'Player' or first in scene.")]
    public PlayerController player;

    [Header("Canvas & Anchoring")] 
    public Vector2 anchoredOffset = new Vector2(-24f, -24f); 

    [Header("Bar Size (fixed px)")] 
    public Vector2 barSize = new Vector2(320f, 28f); 

    [Header("Colors")] 
    public Color bgColor   = new Color(0f, 0f, 0f, 0.40f);
    public Color fillColor = new Color(0.20f, 0.85f, 0.35f, 1f);
    public Color textColor = Color.white;

    [Header("Text")]
    public string labelPrefix = "SANITY ";
    public int fontSize = 24; 

    
    RectTransform barRT, fillRT;
    TextMeshProUGUI labelTMP;

    void Awake()
    {
        if (!player)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo) player = pgo.GetComponent<PlayerController>();
            if (!player) player = FindObjectOfType<PlayerController>();
        }
        BuildCanvasUI();
    }

    void OnEnable()
    {
        if (player) player.OnSanityChanged += SetValue;
    }

    void Start()
    {
        if (player) SetValue(player.sanity);
    }

    void OnDisable()
    {
        if (player) player.OnSanityChanged -= SetValue;
    }

    void BuildCanvasUI()
    {
        
        Canvas canvas = null;
        foreach (var c in FindObjectsOfType<Canvas>())
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = c; break;
            }
        }

        if (!canvas)
        {
            var go = new GameObject("Canvas_UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; 
            scaler.scaleFactor = 1.0f;                                     
        }
        else
        {
           
            canvas.pixelPerfect = true;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (!scaler) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; 
            scaler.scaleFactor = 1.0f;
        }

        
        var barGO = new GameObject("SanityBar", typeof(RectTransform));
        barGO.transform.SetParent(canvas.transform, false);
        barRT = barGO.GetComponent<RectTransform>();
        barRT.anchorMin = barRT.anchorMax = new Vector2(1f, 1f);
        barRT.pivot = new Vector2(1f, 1f);
        barRT.anchoredPosition = anchoredOffset;
        barRT.sizeDelta = barSize; 

        
        var bgGO = new GameObject("BG", typeof(Image));
        bgGO.transform.SetParent(barRT, false);
        var bgImg = bgGO.GetComponent<Image>();
        bgImg.color = bgColor;
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

        
        var fillGO = new GameObject("Fill", typeof(Image));
        fillGO.transform.SetParent(barRT, false);
        var fillImg = fillGO.GetComponent<Image>();
        fillImg.color = fillColor;
        fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.pivot     = new Vector2(0f, 0.5f);
        fillRT.anchoredPosition = Vector2.zero;
        fillRT.sizeDelta = new Vector2(1f, 0f); 

        
        var labelGO = new GameObject("Label", typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(barRT, false);
        labelTMP = labelGO.GetComponent<TextMeshProUGUI>();
        labelTMP.color = textColor;
        labelTMP.fontSize = fontSize;
        labelTMP.fontStyle = FontStyles.Bold;
        labelTMP.alignment = TextAlignmentOptions.Midline;
        labelTMP.enableWordWrapping = false;
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(10f, 0f); labelRT.offsetMax = new Vector2(-10f, 0f);
    }

    void SetValue(float v)
    {
        v = Mathf.Clamp01(v);

       
        float w = Mathf.Round(barRT.rect.width * v);
        fillRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);

        int pct = Mathf.RoundToInt(v * 100f);
        if (labelTMP) labelTMP.text = $"{labelPrefix}{pct}%";
    }
}
