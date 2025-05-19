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

    [Header("UI")]
    public Slider brushSizeSlider;

    public float applyInterval = 0.02f;

    private Texture2D texture;
    private Color32[] colorBuffer;
    private Dictionary<Vector2Int, Color32> pixelBuffer = new();

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D drawAreaCollider;

    private Vector2Int? lastDrawPixelPos;
    private float lastApplyTime;

    void Start()
    {
        texture = new(textureWidth, textureHeight, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        colorBuffer = new Color32[textureWidth * textureHeight];
        ClearTextureInternal(false);
        texture.SetPixels32(colorBuffer);
        texture.Apply(false);

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));

        drawAreaCollider = GetComponent<BoxCollider2D>();
        drawAreaCollider.size = spriteRenderer.bounds.size;

        brushSizeSlider?.onValueChanged.AddListener(val => SetBrushSize(Mathf.RoundToInt(val)));
        if (brushSizeSlider != null) brushSizeSlider.value = brushSize;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) lastDrawPixelPos = null;

        if (Input.GetMouseButton(0))
            TryDrawAtMousePosition();

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

                int x = center.x + dx, y = center.y + dy;
                if (x < 0 || x >= textureWidth || y < 0 || y >= textureHeight) continue;

                pixelBuffer[new(x, y)] = brushColor;
            }
        }
    }

    void DrawLineBuffered(Vector2Int from, Vector2Int to)
    {
        int x0 = from.x, y0 = from.y, x1 = to.x, y1 = to.y;
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, err = dx - dy;

        while (true)
        {
            DrawCircleBuffered(new(x0, y0));
            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx)  { err += dx; y0 += sy; }
        }
    }

    void ApplyBufferedPixels()
    {
        foreach (var kvp in pixelBuffer)
        {
            int idx = kvp.Key.y * textureWidth + kvp.Key.x;
            if (idx >= 0 && idx < colorBuffer.Length)
                colorBuffer[idx] = kvp.Value;
        }

        texture.SetPixels32(colorBuffer);
        texture.Apply(false);
        pixelBuffer.Clear();
    }

    public void ClearTexture() => ClearTextureInternal(true);

    void ClearTextureInternal(bool apply)
    {
        for (int i = 0; i < colorBuffer.Length; i++) colorBuffer[i] = Color.white;
        pixelBuffer.Clear();
        if (apply)
        {
            texture.SetPixels32(colorBuffer);
            texture.Apply();
        }
    }

    public void SaveToPNG()
    {
        string folder = Application.persistentDataPath + "/Drawings";
        if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);

        string fileName = $"drawing_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder, fileName), texture.EncodeToPNG());
        Debug.Log("Saved to: " + folder);
    }

    public void SetBrushColor(Color color) => brushColor = color;
    public void SetBrushSize(int size) => brushSize = Mathf.Clamp(size, 1, 15);
    public void UseEraser() => brushColor = Color.white;
}