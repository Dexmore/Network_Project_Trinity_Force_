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
            lastDrawPixelPos = null;
            ResetMinMax();
        }

        if (Input.GetMouseButton(0))
        {
            TryDrawAtMousePosition();
        }

        if (Input.GetMouseButtonUp(0))
        {
            lastDrawPixelPos = null;
            ApplyBufferedPixels();
        }

        if (Time.time - lastApplyTime > applyInterval && pixelBuffer.Count > 0)
        {
            ApplyBufferedPixels();
            lastApplyTime = Time.time;
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

        for (int y = minY; y <= maxY; y++)
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

    public void ClearTexture() => ClearTextureInternal(true);

    public void SaveToPNG()
    {
        string folder = Application.persistentDataPath + "/Drawings";
        if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);

        string path = System.IO.Path.Combine(folder, $"drawing_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Debug.Log("Saved to: " + path);
    }

    // 🔴 파란 검정 지우개 공통 사용 함수
    public void SetBrushColor(Color color)
    {
        brushColor = color;
    }

    public void SetBrushSize(int size) => brushSize = Mathf.Clamp(size, 1, 50);

    bool IsValidPixel(int x, int y) => x >= 0 && x < textureWidth && y >= 0 && y < textureHeight;
}
