/*using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class TexturePainter : MonoBehaviour
{
    [Header("Texture Settings")]
    public int textureWidth = 1024, textureHeight = 1024;

    [Header("Brush Settings")]
    public Color brushColor = Color.black;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    [Range(1, 50)] public int brushSize = 5;
=======
    [Range(1, 10)] public int brushSize = 10;
>>>>>>> Stashed changes

    [Header("Slider UI")]
    public Slider brushSizeSlider;

    public float applyInterval = 0.02f;

    Texture2D texture;
    Color32[] fullBuffer;
    Dictionary<Vector2Int, Color32> pixelBuffer = new();

    SpriteRenderer sr;
    BoxCollider2D drawArea;

    Vector2Int? lastPos;
    float lastApplyTime;

<<<<<<< Updated upstream
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
=======
    int minX, minY, maxX, maxY;
>>>>>>> Stashed changes

    void Start()
    {
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        fullBuffer = new Color32[textureWidth * textureHeight];
        ClearTextureInternal(false);
        texture.SetPixels32(fullBuffer);
        texture.Apply();

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));

        drawArea = GetComponent<BoxCollider2D>();
        drawArea.size = sr.bounds.size;

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
=======
            lastPos = null;
            minX = textureWidth; minY = textureHeight; maxX = 0; maxY = 0;
        }

        if (Input.GetMouseButton(0)) TryDraw();

        if (Input.GetMouseButtonUp(0))
        {
            lastPos = null;
            ApplyPixels();
>>>>>>> Stashed changes
        }

        if (Time.time - lastApplyTime > applyInterval && pixelBuffer.Count > 0)
        {
            ApplyPixels();
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

    void TryDraw()
    {
        Vector2 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!drawArea.OverlapPoint(world)) return;

        Vector2 local = world - (Vector2)sr.bounds.min;
        Vector2 norm = new(local.x / sr.bounds.size.x, local.y / sr.bounds.size.y);
        Vector2Int pixel = new(Mathf.FloorToInt(norm.x * textureWidth), Mathf.FloorToInt(norm.y * textureHeight));

        if (lastPos.HasValue) DrawLine(lastPos.Value, pixel);
        else DrawBrush(pixel);

        lastPos = pixel;
    }

    void DrawBrush(Vector2Int center)
    {
        int rSqr = brushSize * brushSize;

        for (int dx = -brushSize; dx <= brushSize; dx++)
        for (int dy = -brushSize; dy <= brushSize; dy++)
        {
            if (dx * dx + dy * dy > rSqr) continue;

            int x = center.x + dx, y = center.y + dy;
            if (x < 0 || x >= textureWidth || y < 0 || y >= textureHeight) continue;

            Vector2Int pos = new(x, y);
            int idx = y * textureWidth + x;

            if (fullBuffer[idx].Equals(brushColor)) continue;

            pixelBuffer[pos] = brushColor;

            minX = Mathf.Min(minX, x);
            minY = Mathf.Min(minY, y);
            maxX = Mathf.Max(maxX, x);
            maxY = Mathf.Max(maxY, y);
        }
    }

    void DrawLine(Vector2Int from, Vector2Int to)
    {
        float dist = Vector2Int.Distance(from, to);
        int steps = Mathf.CeilToInt(dist * 1.5f);
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2Int p = Vector2Int.RoundToInt(Vector2.Lerp(from, to, t));
            DrawBrush(p);
        }
    }

    void ApplyPixels()
    {
        if (pixelBuffer.Count == 0) return;

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        Color32[] temp = new Color32[width * height];

<<<<<<< Updated upstream
        for (int y = minY; y <= maxY; y++)
<<<<<<< Updated upstream
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
=======
        for (int x = minX; x <= maxX; x++)
>>>>>>> Stashed changes
        {
            Vector2Int pos = new(x, y);
            int localIdx = (y - minY) * width + (x - minX);
            int fullIdx = y * textureWidth + x;

            if (pixelBuffer.TryGetValue(pos, out var col))
            {
                temp[localIdx] = col;
                fullBuffer[fullIdx] = col;
            }
            else
            {
                temp[localIdx] = fullBuffer[fullIdx];
            }
        }

        texture.SetPixels32(minX, minY, width, height, temp);
        texture.Apply(false);
        pixelBuffer.Clear();
    }

    public void ClearTexture() => ClearTextureInternal(true);

    void ClearTextureInternal(bool apply)
    {
        for (int i = 0; i < fullBuffer.Length; i++) fullBuffer[i] = Color.white;
        pixelBuffer.Clear();
        if (apply)
        {
            texture.SetPixels32(fullBuffer);
            texture.Apply();
        }
    }

    public void SaveToPNG()
    {
        string folder = Path.Combine(Application.persistentDataPath, "Drawings");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, $"drawing_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Debug.Log("Saved to: " + path);
    }

<<<<<<< Updated upstream
    public void SetBrushColor(Color color) => brushColor = color;
    public void SetBrushSize(int size) => brushSize = Mathf.Clamp(size, 1, 10);
    public void UseEraser() => brushColor = Color.white;
<<<<<<< Updated upstream
    bool IsValidPixel(int x, int y) => x >= 0 && x < textureWidth && y >= 0 && y < textureHeight;
=======
    public void SetBrushColor(Color color)
    {
        brushColor = color;
        isFillMode = false; // �귯�� ��� �� ä��� ��� OFF
    }

    public void SetFillColor(Color color)
    {
        fillColor = color;
        EnableFillMode(); // Fill �� ���� �� �ڵ� Fill ��� ON
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
=======
>>>>>>> Stashed changes
}
*/