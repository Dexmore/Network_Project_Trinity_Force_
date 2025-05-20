using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class TexturePainter : MonoBehaviour
{
    [Header("Texture Settings")]
    public int textureWidth = 1000, textureHeight = 700;

    [Header("Brush Settings")]
    public Color brushColor = Color.black;
    [Range(1, 15)] public int brushSize = 5;
    public bool isEraser = false;
    public bool isFillMode = false;

    [Header("UI References")]
    public Slider brushSizeSlider;
    public Button PencilButton;
    public Button EraseButton;
    public Button FillButton;

    private Texture2D texture;
    private Color32[] fullColorBuffer, displayBuffer;
    private readonly Dictionary<Vector2Int, Color32> pixelBuffer = new();
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D drawAreaCollider;
    private Vector2Int? lastDrawPixelPos;
    private static readonly Color32 White = new(255, 255, 255, 255);

    private struct ChangeSet
    {
        public Dictionary<Vector2Int, Color32> before, after;
    }

    private readonly Stack<ChangeSet> undoStack = new();
    private readonly Stack<ChangeSet> redoStack = new();

    void Start()
    {
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        fullColorBuffer = new Color32[textureWidth * textureHeight];
        displayBuffer = new Color32[textureWidth * textureHeight];
        for (int i = 0; i < fullColorBuffer.Length; i++) fullColorBuffer[i] = White;
        ApplyTexture(fullColorBuffer);

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f));

        drawAreaCollider = GetComponent<BoxCollider2D>();
        drawAreaCollider.size = spriteRenderer.bounds.size;

        brushSizeSlider.onValueChanged.AddListener(val => brushSize = Mathf.Clamp(Mathf.RoundToInt(val), 1, 15));
        FillButton.onClick.AddListener(() =>
        {
            isFillMode = !isFillMode;
            if (!isFillMode)
                SetPencil(); // 채우기 모드 해제 시 자동으로 브러시 모드로
        });

        SetPencil();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        if (Input.GetMouseButton(0) && !isFillMode) { Draw(); UpdateLiveTexture(); }
        if (Input.GetMouseButtonUp(0)) { CommitDraw(); }
    }


    void OnMouseDown()
    {
        Vector2Int pixelPos = GetMousePixel();
        if (!IsValidPixel(pixelPos.x, pixelPos.y)) return;

        if (isFillMode)
        {
            int idx = pixelPos.y * textureWidth + pixelPos.x;
            Color32 target = fullColorBuffer[idx];
            Color32 fill = isEraser ? White : brushColor;
            FloodFill(pixelPos, target, fill);
            return;
        }
        else
        {
            lastDrawPixelPos = null;
            pixelBuffer.Clear();
        }
    }

    Vector2Int GetMousePixel()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!drawAreaCollider.OverlapPoint(worldPos)) return new(-1, -1);
        Vector2 local = worldPos - (Vector2)spriteRenderer.bounds.min;
        Vector2 normalized = new(local.x / spriteRenderer.bounds.size.x, local.y / spriteRenderer.bounds.size.y);
        return new(Mathf.FloorToInt(normalized.x * textureWidth), Mathf.FloorToInt(normalized.y * textureHeight));
    }

    void Draw()
    {
        Vector2Int pixelPos = GetMousePixel();
        if (!IsValidPixel(pixelPos.x, pixelPos.y)) return;
        if (lastDrawPixelPos.HasValue) DrawLine(lastDrawPixelPos.Value, pixelPos);
        else DrawCircle(pixelPos);
        lastDrawPixelPos = pixelPos;
    }

    void DrawCircle(Vector2Int center)
    {
        Color32 color = isEraser ? White : brushColor;
        int rSquared = brushSize * brushSize;
        for (int dx = -brushSize; dx <= brushSize; dx++)
        {
            for (int dy = -brushSize; dy <= brushSize; dy++)
            {
                if (dx * dx + dy * dy > rSquared) continue;
                int x = center.x + dx, y = center.y + dy;
                if (!IsValidPixel(x, y)) continue;
                pixelBuffer[new(x, y)] = color;
            }
        }
    }

    void DrawLine(Vector2Int from, Vector2Int to)
    {
        int steps = Mathf.CeilToInt(Vector2Int.Distance(from, to) * 1.5f);
        for (int i = 0; i <= steps; i++)
            DrawCircle(Vector2Int.RoundToInt(Vector2.Lerp(from, to, (float)i / steps)));
    }

    void UpdateLiveTexture()
    {
        fullColorBuffer.CopyTo(displayBuffer, 0);
        foreach (var kvp in pixelBuffer)
            displayBuffer[kvp.Key.y * textureWidth + kvp.Key.x] = kvp.Value;
        ApplyTexture(displayBuffer);
    }

    void CommitDraw()
    {
        if (pixelBuffer.Count == 0) return;
        var before = new Dictionary<Vector2Int, Color32>();
        var after = new Dictionary<Vector2Int, Color32>();

        foreach (var kvp in pixelBuffer)
        {
            int idx = kvp.Key.y * textureWidth + kvp.Key.x;
            before[kvp.Key] = fullColorBuffer[idx];
            fullColorBuffer[idx] = after[kvp.Key] = kvp.Value;
        }

        ApplyTexture(fullColorBuffer);
        undoStack.Push(new ChangeSet { before = before, after = after });
        redoStack.Clear();
        pixelBuffer.Clear();
    }

    void ApplyTexture(Color32[] buffer)
    {
        texture.SetPixels32(buffer);
        texture.Apply();
    }

    public void Undo() => ApplyChange(undoStack, redoStack);
    public void Redo() => ApplyChange(redoStack, undoStack);

    void ApplyChange(Stack<ChangeSet> from, Stack<ChangeSet> to)
    {
        if (from.Count == 0) return;
        ChangeSet set = from.Pop();
        var opposite = new Dictionary<Vector2Int, Color32>();

        foreach (var kvp in set.before)
        {
            int idx = kvp.Key.y * textureWidth + kvp.Key.x;
            opposite[kvp.Key] = fullColorBuffer[idx];
            fullColorBuffer[idx] = kvp.Value;
        }

        ApplyTexture(fullColorBuffer);
        to.Push(new ChangeSet { before = opposite, after = set.before });
    }

    void FloodFill(Vector2Int start, Color32 target, Color32 fill)
    {
        if (target.Equals(fill)) return;
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        var before = new Dictionary<Vector2Int, Color32>();
        var after = new Dictionary<Vector2Int, Color32>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            int idx = pos.y * textureWidth + pos.x;
            if (!fullColorBuffer[idx].Equals(target)) continue;

            fullColorBuffer[idx] = fill;
            before[pos] = target;
            after[pos] = fill;

            foreach (Vector2Int dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int next = pos + dir;
                if (!IsValidPixel(next.x, next.y) || visited.Contains(next)) continue;
                int nextIdx = next.y * textureWidth + next.x;
                if (fullColorBuffer[nextIdx].Equals(target))
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }

        ApplyTexture(fullColorBuffer);
        undoStack.Push(new ChangeSet { before = before, after = after });
        redoStack.Clear();
    }

    bool IsValidPixel(int x, int y) => x >= 0 && x < textureWidth && y >= 0 && y < textureHeight;
    public void SetBrushColor(Color color) => brushColor = color;
    public void SetEraser()
    {
        isEraser = true;
        PencilButton.gameObject.SetActive(true);
        EraseButton.gameObject.SetActive(false);
    }
    public void SetPencil()
    {
        isEraser = false;
        PencilButton.gameObject.SetActive(false);
        EraseButton.gameObject.SetActive(true);
    }

    public void ClearTexture()
    {
        var beforeBuffer = new Color32[fullColorBuffer.Length];
        fullColorBuffer.CopyTo(beforeBuffer, 0);
        for (int i = 0; i < fullColorBuffer.Length; i++) fullColorBuffer[i] = White;
        ApplyTexture(fullColorBuffer);

        var before = new Dictionary<Vector2Int, Color32>();
        var after = new Dictionary<Vector2Int, Color32>();
        for (int i = 0; i < fullColorBuffer.Length; i++)
        {
            if (!beforeBuffer[i].Equals(fullColorBuffer[i]))
            {
                int x = i % textureWidth, y = i / textureWidth;
                var pos = new Vector2Int(x, y);
                before[pos] = beforeBuffer[i];
                after[pos] = fullColorBuffer[i];
            }
        }

        undoStack.Push(new ChangeSet { before = before, after = after });
        redoStack.Clear();
    }
}