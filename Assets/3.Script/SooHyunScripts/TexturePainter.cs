using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class TexturePainter : MonoBehaviour
{
    [Header("Texture Settings")]
    public int textureWidth = 1024, textureHeight = 1024;

    [Header("Brush Settings")]
    public Color brushColor = Color.black;
<<<<<<< Updated upstream
    [Range(1, 50)] public int brushSize = 5;

    [Header("Slider UI")]
    public Slider brushSizeSlider;

    public float applyInterval = 0.02f;

    private Texture2D texture;
    private Color32[] fullColorBuffer;
    private Dictionary<Vector2Int, Color32> pixelBuffer = new();

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D drawAreaCollider;

    private Vector2Int? lastDrawPixelPos;
    private float lastApplyTime;

    private int minX, minY, maxX, maxY;
=======
    public Color fillColor = Color.red;
    public int brushSize = 5;

    private Texture2D texture;
    private bool isDrawing = false;
    private bool isFillMode = false;
    private BoxCollider2D drawAreaCollider;
    private Vector2? lastMouseDrawPos = null;
    private List<Vector2Int> pixelBuffer = new List<Vector2Int>();
>>>>>>> Stashed changes

    void Start()
    {
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        fullColorBuffer = new Color32[textureWidth * textureHeight];
        ClearTextureInternal(false);
        texture.SetPixels32(fullColorBuffer);
        texture.Apply(false);

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));

        drawAreaCollider = GetComponent<BoxCollider2D>();
        drawAreaCollider.size = spriteRenderer.bounds.size;

        if (brushSizeSlider != null)
        {
            brushSizeSlider.onValueChanged.AddListener(val => SetBrushSize(Mathf.RoundToInt(val)));
            brushSizeSlider.value = brushSize;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
<<<<<<< Updated upstream
            lastDrawPixelPos = null;
            ResetMinMax();
        }

        if (Input.GetMouseButton(0))
        {
            TryDrawAtMousePosition();
=======
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (drawAreaCollider.OverlapPoint(mouseWorldPos))
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                Vector2 local = mouseWorldPos - (Vector2)sr.bounds.min;

                float normalizedX = local.x / sr.bounds.size.x;
                float normalizedY = local.y / sr.bounds.size.y;

                int px = Mathf.FloorToInt(normalizedX * textureWidth);
                int py = Mathf.FloorToInt(normalizedY * textureHeight);

                if (isFillMode)
                {
                    Color targetColor = texture.GetPixel(px, py);
                    if (targetColor != fillColor)
                    {
                        FloodFill(px, py, targetColor, fillColor);
                    }
                }
                else
                {
                    isDrawing = true;
                    lastMouseDrawPos = null;
                    DrawCircleBuffered(px, py);
                    ApplyBufferedPixels();
                    lastMouseDrawPos = new Vector2(px, py);
                }
            }
>>>>>>> Stashed changes
        }

        if (Input.GetMouseButtonUp(0))
        {
<<<<<<< Updated upstream
            lastDrawPixelPos = null;
            ApplyBufferedPixels();
        }

        if (Time.time - lastApplyTime > applyInterval && pixelBuffer.Count > 0)
        {
            ApplyBufferedPixels();
            lastApplyTime = Time.time;
=======
            if (!isFillMode)
            {
                isDrawing = false;
                lastMouseDrawPos = null;
                ApplyBufferedPixels();
            }
        }

        if (isDrawing && !isFillMode)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (drawAreaCollider.OverlapPoint(mouseWorldPos))
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                Vector2 local = mouseWorldPos - (Vector2)sr.bounds.min;

                float normalizedX = local.x / sr.bounds.size.x;
                float normalizedY = local.y / sr.bounds.size.y;

                int px = Mathf.FloorToInt(normalizedX * textureWidth);
                int py = Mathf.FloorToInt(normalizedY * textureHeight);

                if (lastMouseDrawPos.HasValue)
                {
                    Vector2 lastPos = lastMouseDrawPos.Value;
                    int lastPx = Mathf.FloorToInt(lastPos.x);
                    int lastPy = Mathf.FloorToInt(lastPos.y);

                    DrawLineBuffered(lastPx, lastPy, px, py);
                }
                else
                {
                    DrawCircleBuffered(px, py);
                }

                lastMouseDrawPos = new Vector2(px, py);

                if (Time.frameCount % 2 == 0)
                    ApplyBufferedPixels();
            }
>>>>>>> Stashed changes
        }
    }

    void TryDrawAtMousePosition()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!drawAreaCollider.OverlapPoint(worldPos)) return;

        Vector2 local = worldPos - (Vector2)spriteRenderer.bounds.min;
        Vector2 normalized = new(local.x / spriteRenderer.bounds.size.x, local.y / spriteRenderer.bounds.size.y);
        Vector2Int pixelPos = new(Mathf.FloorToInt(normalized.x * textureWidth), Mathf.FloorToInt(normalized.y * textureHeight));

        if (lastDrawPixelPos.HasValue)
            DrawLineBuffered(lastDrawPixelPos.Value, pixelPos);
        else
            DrawCircleBuffered(pixelPos);

        lastDrawPixelPos = pixelPos;
    }

    void DrawCircleBuffered(Vector2Int center)
    {
        int rSquared = brushSize * brushSize;

        for (int dx = -brushSize; dx <= brushSize; dx++)
        {
            for (int dy = -brushSize; dy <= brushSize; dy++)
            {
                if (dx * dx + dy * dy > rSquared) continue;

                int x = center.x + dx;
                int y = center.y + dy;
                if (!IsValidPixel(x, y)) continue;

                Vector2Int pos = new(x, y);
                int idx = y * textureWidth + x;

                if (fullColorBuffer[idx].Equals(brushColor)) continue;

                pixelBuffer[pos] = brushColor;

                // Î∏åÎü¨Ïãú ÏòÅÏó≠ Í∞±Ïã†
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
        }
    }

    void DrawLineBuffered(Vector2Int from, Vector2Int to)
    {
        float distance = Vector2Int.Distance(from, to);
        int steps = Mathf.CeilToInt(distance * 1.5f);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 lerp = Vector2.Lerp(from, to, t);
            DrawCircleBuffered(Vector2Int.RoundToInt(lerp));
        }
    }

    void ApplyBufferedPixels()
    {
        if (pixelBuffer.Count == 0) return;

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        Color32[] partialBuffer = new Color32[width * height];

<<<<<<< Updated upstream
        for (int y = minY; y <= maxY; y++)
=======
    void FloodFill(int x, int y, Color targetColor, Color replacementColor)
    {
        if (targetColor == replacementColor)
            return;

        Queue<Vector2Int> pixels = new Queue<Vector2Int>();
        pixels.Enqueue(new Vector2Int(x, y));

        while (pixels.Count > 0)
        {
            Vector2Int p = pixels.Dequeue();
            if (p.x < 0 || p.x >= textureWidth || p.y < 0 || p.y >= textureHeight)
                continue;

            if (texture.GetPixel(p.x, p.y) != targetColor)
                continue;

            texture.SetPixel(p.x, p.y, replacementColor);

            pixels.Enqueue(new Vector2Int(p.x + 1, p.y));
            pixels.Enqueue(new Vector2Int(p.x - 1, p.y));
            pixels.Enqueue(new Vector2Int(p.x, p.y + 1));
            pixels.Enqueue(new Vector2Int(p.x, p.y - 1));
        }

        texture.Apply();
    }

    public void ClearTexture()
    {
        for (int x = 0; x < texture.width; x++)
>>>>>>> Stashed changes
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2Int pos = new(x, y);
                int localIdx = (y - minY) * width + (x - minX);
                int fullIdx = y * textureWidth + x;

                if (pixelBuffer.TryGetValue(pos, out var col))
                {
                    partialBuffer[localIdx] = col;
                    fullColorBuffer[fullIdx] = col;
                }
                else
                {
                    partialBuffer[localIdx] = fullColorBuffer[fullIdx];
                }
            }
        }

        texture.SetPixels32(minX, minY, width, height, partialBuffer);
        texture.Apply(false);
        pixelBuffer.Clear();
        ResetMinMax();
    }

    void ResetMinMax()
    {
        minX = textureWidth;
        minY = textureHeight;
        maxX = 0;
        maxY = 0;
    }

    public void ClearTexture() => ClearTextureInternal(true);

    void ClearTextureInternal(bool apply)
    {
        Color32 white = Color.white;
        for (int i = 0; i < fullColorBuffer.Length; i++) fullColorBuffer[i] = white;

        pixelBuffer.Clear();
        ResetMinMax();

        if (apply)
        {
            texture.SetPixels32(fullColorBuffer);
            texture.Apply();
        }
    }

    public void SaveToPNG()
    {
        string folder = Application.persistentDataPath + "/Drawings";
        if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);

        string path = System.IO.Path.Combine(folder, $"drawing_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Debug.Log("Saved to: " + path);
    }

<<<<<<< Updated upstream
    public void SetBrushColor(Color color) => brushColor = color;
    public void SetBrushSize(int size) => brushSize = Mathf.Clamp(size, 1, 50);
    public void UseEraser() => brushColor = Color.white;
    bool IsValidPixel(int x, int y) => x >= 0 && x < textureWidth && y >= 0 && y < textureHeight;
=======
    public void SetBrushColor(Color color)
    {
        brushColor = color;
        isFillMode = false; // ∫Í∑ØΩ√ ªÁøÎ Ω√ √§øÏ±‚ ∏µÂ OFF
    }

    public void SetFillColor(Color color)
    {
        fillColor = color;
        EnableFillMode(); // Fill ªˆ º±≈√ Ω√ ¿⁄µø Fill ∏µÂ ON
    }

    public void EnableFillMode()
    {
        isFillMode = true;
    }

    public void DisableFillMode()
    {
        isFillMode = false;
    }
>>>>>>> Stashed changes
}
