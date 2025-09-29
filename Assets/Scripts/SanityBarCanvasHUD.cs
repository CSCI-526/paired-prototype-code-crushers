// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// /// Add this to any scene object (e.g., GameManager).
// /// It will auto-create a top-right "Sanity XX%" bar and keep it updated.
// [DefaultExecutionOrder(1000)]
// public class SanityBarHUD : MonoBehaviour
// {
//     [Header("Auto Build")]
//     [Tooltip("If true, this script will create a Canvas and the SanityBar UI at runtime.")]
//     public bool autoBuildUI = true;

//     [Header("Placement (Top-Right)")]
//     public Vector2 anchoredOffset = new Vector2(-40f, -40f); // from top-right corner
//     public Vector2 barSize = new Vector2(260f, 24f);

//     [Header("Colors")]
//     public Color bgColor   = new Color(0f, 0f, 0f, 0.35f);
//     public Color fillColor = new Color(0.20f, 0.80f, 0.35f, 1f); // green
//     public Color textColor = Color.white;

//     [Header("Text")]
//     public int   fontSize = 18;
//     public string labelPrefix = "Sanity ";

//     PlayerController player;
//     RectTransform fillRT;
//     TextMeshProUGUI labelTMP;

//     void Awake()
//     {
//         // Find player
//         var pgo = GameObject.FindGameObjectWithTag("Player");
//         if (pgo) player = pgo.GetComponent<PlayerController>();
//         if (!player) player = FindObjectOfType<PlayerController>();

//         if (autoBuildUI) BuildUIIfNeeded();
//     }

//     void OnEnable()
//     {
//         if (player != null) player.OnSanityChanged += SetValue;
//     }

//     void Start()
//     {
//         if (player != null) SetValue(player.sanity);
//     }

//     void OnDisable()
//     {
//         if (player != null) player.OnSanityChanged -= SetValue;
//     }

//     void BuildUIIfNeeded()
//     {
//         // Try to reuse an overlay canvas; if none, create one.
//         Canvas canvas = null;
//         foreach (var c in FindObjectsOfType<Canvas>())
//         {
//             if (c.renderMode == RenderMode.ScreenSpaceOverlay) { canvas = c; break; }
//         }
//         if (!canvas)
//         {
//             var cg = new GameObject("Canvas_UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
//             canvas = cg.GetComponent<Canvas>();
//             canvas.renderMode = RenderMode.ScreenSpaceOverlay;

//             var scaler = cg.GetComponent<CanvasScaler>();
//             scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//             scaler.referenceResolution = new Vector2(1920, 1080);
//         }

//         // Root bar
//         var barGO = new GameObject("SanityBar", typeof(RectTransform));
//         barGO.transform.SetParent(canvas.transform, false);
//         var barRT = barGO.GetComponent<RectTransform>();
//         barRT.anchorMin = barRT.anchorMax = new Vector2(1f, 1f); // top-right
//         barRT.pivot = new Vector2(1f, 1f);
//         barRT.anchoredPosition = anchoredOffset;
//         barRT.sizeDelta = barSize;

//         // Background
//         var bgGO = new GameObject("BG", typeof(Image));
//         bgGO.transform.SetParent(barRT, false);
//         var bgImg = bgGO.GetComponent<Image>();
//         bgImg.color = bgColor;
//         var bgRT = bgGO.GetComponent<RectTransform>();
//         bgRT.anchorMin = Vector2.zero;
//         bgRT.anchorMax = Vector2.one;
//         bgRT.offsetMin = Vector2.zero;
//         bgRT.offsetMax = Vector2.zero;

//         // Fill
//         var fillGO = new GameObject("Fill", typeof(Image));
//         fillGO.transform.SetParent(barRT, false);
//         var fillImg = fillGO.GetComponent<Image>();
//         fillImg.color = fillColor;
//         fillRT = fillGO.GetComponent<RectTransform>();
//         fillRT.anchorMin = new Vector2(0f, 0f);
//         fillRT.anchorMax = new Vector2(1f, 1f); // we'll adjust x of anchorMax at runtime
//         fillRT.offsetMin = Vector2.zero;
//         fillRT.offsetMax = Vector2.zero;

//         // Label
//         var labelGO = new GameObject("Label", typeof(TextMeshProUGUI));
//         labelGO.transform.SetParent(barRT, false);
//         labelTMP = labelGO.GetComponent<TextMeshProUGUI>();
//         labelTMP.fontSize = fontSize;
//         labelTMP.color = textColor;
//         labelTMP.alignment = TextAlignmentOptions.Center;
//         var labelRT = labelGO.GetComponent<RectTransform>();
//         labelRT.anchorMin = Vector2.zero;
//         labelRT.anchorMax = Vector2.one;
//         labelRT.offsetMin = Vector2.zero;
//         labelRT.offsetMax = Vector2.zero;
//     }

//     void SetValue(float v)
//     {
//         v = Mathf.Clamp01(v);

//         if (fillRT != null)
//         {
//             var a = fillRT.anchorMax;
//             a.x = Mathf.Max(0.0001f, v); // visually shrink/grow left->right
//             fillRT.anchorMax = a;
//         }

//         if (labelTMP != null)
//         {
//             int pct = Mathf.RoundToInt(v * 100f);
//             labelTMP.text = $"{labelPrefix}{pct}%";
//         }
//     }
// }


using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Attach this to any scene object (e.g., GameManager). It creates a Canvas-based
/// sanity bar (no Slider) in the top-right with crisp text. It auto-finds PlayerController
/// and updates when sanity changes.
[DefaultExecutionOrder(1000)]
public class SanityBarCanvasHUD : MonoBehaviour
{
    [Header("Find Player")]
    [Tooltip("If left empty, we'll find a PlayerController by tag 'Player' or first in scene.")]
    public PlayerController player;

    [Header("Canvas & Anchoring")] 
    public Vector2 anchoredOffset = new Vector2(-24f, -24f); // from top-right

    [Header("Bar Size (fixed px)")] 
    public Vector2 barSize = new Vector2(320f, 28f); // compact and readable

    [Header("Colors")] 
    public Color bgColor   = new Color(0f, 0f, 0f, 0.40f);
    public Color fillColor = new Color(0.20f, 0.85f, 0.35f, 1f);
    public Color textColor = Color.white;

    [Header("Text")]
    public string labelPrefix = "SANITY ";
    public int fontSize = 24; // crisp size

    // runtime refs
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
        // Reuse existing overlay canvas if present
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
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; // predictable pixels
            scaler.scaleFactor = 1.0f;                                     // crisp
        }
        else
        {
            // Ensure crisp settings even if canvas existed
            canvas.pixelPerfect = true;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (!scaler) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; // fixed px bar
            scaler.scaleFactor = 1.0f;
        }

        // Root (anchors top-right)
        var barGO = new GameObject("SanityBar", typeof(RectTransform));
        barGO.transform.SetParent(canvas.transform, false);
        barRT = barGO.GetComponent<RectTransform>();
        barRT.anchorMin = barRT.anchorMax = new Vector2(1f, 1f);
        barRT.pivot = new Vector2(1f, 1f);
        barRT.anchoredPosition = anchoredOffset;
        barRT.sizeDelta = barSize; // fixed pixels

        // Background
        var bgGO = new GameObject("BG", typeof(Image));
        bgGO.transform.SetParent(barRT, false);
        var bgImg = bgGO.GetComponent<Image>();
        bgImg.color = bgColor;
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

        // Fill (left to right)
        var fillGO = new GameObject("Fill", typeof(Image));
        fillGO.transform.SetParent(barRT, false);
        var fillImg = fillGO.GetComponent<Image>();
        fillImg.color = fillColor;
        fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0f, 0f);
        fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.pivot     = new Vector2(0f, 0.5f);
        fillRT.anchoredPosition = Vector2.zero;
        fillRT.sizeDelta = new Vector2(1f, 0f); // width set in SetValue

        // Label (TextMeshPro)
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

        // width in pixels (crisp edge)
        float w = Mathf.Round(barRT.rect.width * v);
        fillRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);

        int pct = Mathf.RoundToInt(v * 100f);
        if (labelTMP) labelTMP.text = $"{labelPrefix}{pct}%";
    }
}
