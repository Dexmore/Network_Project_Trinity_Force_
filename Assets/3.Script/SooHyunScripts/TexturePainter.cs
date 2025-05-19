using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class TexturePainter : MonoBehaviour
{
    public int textureWidth = 1024;
    public int textureHeight = 1024;
    public Color brushColor = Color.black;
    public int brushSize = 5;

    private Texture2D texture;
    private bool isDrawing = false;
    private BoxCollider2D drawAreaCollider;

    private Vector2? lastMouseDrawPos = null;

    // 픽셀 캐시 리스트 (Apply 성능 최적화용)
    private List<Vector2Int> pixelBuffer = new List<Vector2Int>();

    void Start()
    {
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        ClearTexture();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        drawAreaCollider = GetComponent<BoxCollider2D>();
        drawAreaCollider.size = sr.bounds.size;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            lastMouseDrawPos = null;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
            lastMouseDrawPos = null;
            ApplyBufferedPixels(); // 마우스 뗄 때만 apply
        }

        if (isDrawing)
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

                // Apply 텍스처는 2~3 프레임마다만 호출
                if (Time.frameCount % 2 == 0)
                    ApplyBufferedPixels();
            }
        }
    }

    void DrawCircleBuffered(int x, int y)
    {
        for (int dx = -brushSize; dx <= brushSize; dx++)
        {
            for (int dy = -brushSize; dy <= brushSize; dy++)
            {
                if (dx * dx + dy * dy <= brushSize * brushSize)
                {
                    int px = x + dx;
                    int py = y + dy;

                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    {
                        pixelBuffer.Add(new Vector2Int(px, py));
                    }
                }
            }
        }
    }

    void DrawLineBuffered(int x0, int y0, int x1, int y1)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawCircleBuffered(x0, y0);

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    void ApplyBufferedPixels()
    {
        foreach (Vector2Int p in pixelBuffer)
        {
            texture.SetPixel(p.x, p.y, brushColor);
        }

        if (pixelBuffer.Count > 0)
        {
            texture.Apply();
            pixelBuffer.Clear();
        }
    }

    public void ClearTexture()
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }
        texture.Apply();
    }

    public void SaveToPNG()
    {
        byte[] bytes = texture.EncodeToPNG();
        string folderPath = Application.persistentDataPath + "/Drawings";

        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        string fileName = "drawing_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string fullPath = System.IO.Path.Combine(folderPath, fileName);

        System.IO.File.WriteAllBytes(fullPath, bytes);
        Debug.Log("Saved drawing to: " + fullPath);
    }

    public void SetBrushColor(Color color)
    {
        brushColor = color;
    }

    public void SetBrushSize(int size)
    {
        Debug.Log("Requested brush size: " + size);
        brushSize = Mathf.Max(1, size);
    }

    public void UseEraser()
    {
        brushColor = Color.white;
    }
}