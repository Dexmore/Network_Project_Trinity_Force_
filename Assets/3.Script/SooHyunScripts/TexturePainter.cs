using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class TexturePainter : MonoBehaviour
{
    [Header("Texture Settings")]
    public int textureWidth = 1000, textureHeight = 700;

    [Header("Brush Settings")]
    public Color brushColor = Color.black;
    [Range(1, 10)] public int brushSize = 5;
    public bool isEraser = false;

    [Header("UI References")]
    public Slider brushSizeSlider;
    public Button PencilButton;
    public Button EraseButton;
    public Button UndoButton;
    public Button RedoButton;

    private Texture2D texture;
    private Color32[] fullColorBuffer;
    private Color32[] displayBuffer; // 추가: 실시간 반영용
    private Dictionary<Vector2Int, Color32> pixelBuffer = new();

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D drawAreaCollider;
    private Vector2Int? lastDrawPixelPos;

    private struct ChangeSet
    {
        public Dictionary<Vector2Int, Color32> before;
        public Dictionary<Vector2Int, Color32> after;
    }

    private Stack<ChangeSet> undoStack = new();
    private Stack<ChangeSet> redoStack = new();

    void Start()
    {
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        fullColorBuffer = new Color32[textureWidth * textureHeight];
        displayBuffer = new Color32[textureWidth * textureHeight];
        for (int i = 0; i < fullColorBuffer.Length; i++)
            fullColorBuffer[i] = Color.white;

        fullColorBuffer.CopyTo(displayBuffer, 0);

        texture.SetPixels32(displayBuffer);
        texture.Apply();

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));

        drawAreaCollider = GetComponent<BoxCollider2D>();
        drawAreaCollider.size = spriteRenderer.bounds.size;

        if (brushSizeSlider != null)
        {
            brushSizeSlider.onValueChanged.AddListener(val => brushSize = Mathf.Clamp(Mathf.RoundToInt(val), 1, 10));
            brushSizeSlider.value = brushSize;
        }

        if (UndoButton != null) UndoButton.onClick.AddListener(Undo);
        if (RedoButton != null) RedoButton.onClick.AddListener(Redo);

        PencilButton?.gameObject.SetActive(false);
        EraseButton?.gameObject.SetActive(true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastDrawPixelPos = null;
            pixelBuffer.Clear();
        }

        if (Input.GetMouseButton(0))
        {
            TryDrawAtMousePosition();
            UpdateLiveTexture();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ApplyBufferedPixels();
            lastDrawPixelPos = null;
        }

        if (UndoButton != null) UndoButton.interactable = undoStack.Count > 0;
        if (RedoButton != null) RedoButton.interactable = redoStack.Count > 0;
    }

    void TryDrawAtMousePosition()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!drawAreaCollider.OverlapPoint(worldPos)) return;

        Vector2 local = worldPos - (Vector2)spriteRenderer.bounds.min;
        Vector2 normalized = new(local.x / spriteRenderer.bounds.size.x, local.y / spriteRenderer.bounds.size.y);
        Vector2Int pixelPos = new(Mathf.FloorToInt(normalized.x * textureWidth), Mathf.FloorToInt(normalized.y * textureHeight));

        if (lastDrawPixelPos.HasValue)
        {
            DrawLineBuffered(lastDrawPixelPos.Value, pixelPos);
        }
        else
        {
            DrawCircleBuffered(pixelPos);
        }

        lastDrawPixelPos = pixelPos;
    }

    void DrawCircleBuffered(Vector2Int center)
    {
        Color32 drawColor = isEraser ? new Color32(255, 255, 255, 255) : brushColor;
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
                pixelBuffer[pos] = drawColor;
            }
        }
    }

    void DrawLineBuffered(Vector2Int from, Vector2Int to)
    {
        int steps = Mathf.CeilToInt(Vector2Int.Distance(from, to) * 1.5f);
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 lerp = Vector2.Lerp(from, to, t);
            DrawCircleBuffered(Vector2Int.RoundToInt(lerp));
        }
    }

    void UpdateLiveTexture()
    {
        fullColorBuffer.CopyTo(displayBuffer, 0);

        foreach (var kvp in pixelBuffer)
        {
            int idx = kvp.Key.y * textureWidth + kvp.Key.x;
            displayBuffer[idx] = kvp.Value;
        }

        texture.SetPixels32(displayBuffer);
        texture.Apply();
    }

    void ApplyBufferedPixels()
    {
        if (pixelBuffer.Count == 0) return;

        Dictionary<Vector2Int, Color32> before = new();
        Dictionary<Vector2Int, Color32> after = new();

        foreach (var kvp in pixelBuffer)
        {
            int idx = kvp.Key.y * textureWidth + kvp.Key.x;
            before[kvp.Key] = fullColorBuffer[idx];
            after[kvp.Key] = kvp.Value;
            fullColorBuffer[idx] = kvp.Value;
        }

        texture.SetPixels32(fullColorBuffer);
        texture.Apply();
        undoStack.Push(new ChangeSet { before = before, after = after });
        redoStack.Clear();
        pixelBuffer.Clear();
    }

    public void Undo()
    {
        if (undoStack.Count == 0) return;
        ChangeSet set = undoStack.Pop();
        ApplyChangeSet(set.before);
        redoStack.Push(set);
    }

    public void Redo()
    {
        if (redoStack.Count == 0) return;
        ChangeSet set = redoStack.Pop();
        ApplyChangeSet(set.after);
        undoStack.Push(set);
    }

    void ApplyChangeSet(Dictionary<Vector2Int, Color32> changes)
    {
        foreach (var kvp in changes)
        {
            int idx = kvp.Key.y * textureWidth + kvp.Key.x;
            fullColorBuffer[idx] = kvp.Value;
        }

        texture.SetPixels32(fullColorBuffer);
        texture.Apply();
    }

    bool IsValidPixel(int x, int y) => x >= 0 && x < textureWidth && y >= 0 && y < textureHeight;

    public void SetBrushColor(Color color) => brushColor = color;

    public void SetEraser()
    {
        isEraser = true;
        PencilButton?.gameObject.SetActive(true);
        EraseButton?.gameObject.SetActive(false);
    }

    public void SetPencil()
    {
        isEraser = false;
        PencilButton?.gameObject.SetActive(false);
        EraseButton?.gameObject.SetActive(true);
    }

    public void ClearTexture()
    {
        Dictionary<Vector2Int, Color32> before = new();
        Dictionary<Vector2Int, Color32> after = new();

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                Vector2Int pos = new(x, y);
                int idx = y * textureWidth + x;
                before[pos] = fullColorBuffer[idx];
                fullColorBuffer[idx] = new Color32(255, 255, 255, 255);
                after[pos] = fullColorBuffer[idx];
            }
        }

        texture.SetPixels32(fullColorBuffer);
        texture.Apply();
        undoStack.Push(new ChangeSet { before = before, after = after });
        redoStack.Clear();
    }
}