using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomBrushController : MonoBehaviour
{
    public CustomTexturePainter paint;

    [Header("UI 표시용")]
    public Image colorPreview;               // 선택한 색상 미리보기용 이미지
    public TextMeshProUGUI modeText;         // 현재 모드 표시 텍스트

    public void SetRed()
    {
        paint.SetBrushColor(Color.red);
        UpdateColor(Color.red);
    }

    public void SetBlue()
    {
        paint.SetBrushColor(Color.blue);
        UpdateColor(Color.blue);
    }

    public void SetBlack()
    {
        paint.SetBrushColor(Color.black);
        UpdateColor(Color.black);
    }

    public void SetEraser()
    {
        paint.SetBrushColor(Color.white);
        paint.SetBrushMode();                // 지우개는 Brush 모드로 강제 전환
        UpdateColor(Color.white);
        UpdateMode("Brush");
    }

    public void UseBrush()
    {
        paint.SetBrushMode();
        UpdateMode("Brush");
    }

    public void UseFill()
    {
        paint.SetFillMode();
        UpdateMode("Fill");
    }

    void UpdateColor(Color color)
    {
        if (colorPreview != null)
            colorPreview.color = color;
    }

    void UpdateMode(string mode)
    {
        if (modeText != null)
            modeText.text = "Mode: " + mode;
    }
}
