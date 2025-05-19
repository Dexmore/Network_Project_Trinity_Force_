using UnityEngine;

public class BrushController : MonoBehaviour
{
    public TexturePainter paint;
    public FlexibleColorPicker colorPicker; // 네임스페이스 불필요

    private enum Mode { Brush, Fill }
    private Mode currentMode = Mode.Brush;

    public void SetBrushMode()
    {
        currentMode = Mode.Brush;
        ApplyCurrentColor();
    }

    public void SetFillMode()
    {
        currentMode = Mode.Fill;
        ApplyCurrentColor();
    }

    public void SetEraser()
    {
        currentMode = Mode.Brush; // 브러시 모드 강제 전환
        Color eraserColor = Color.white;
        paint.SetBrushColor(eraserColor);

        // (옵션) Color Picker 색도 흰색으로 바꾸기
        if (colorPicker != null)
            colorPicker.color = eraserColor;
    }


    public void OnColorChanged(Color color)
    {
        ApplyColor(color);
    }

    private void ApplyCurrentColor()
    {
        ApplyColor(colorPicker.color);
    }

    private void ApplyColor(Color color)
    {
        if (currentMode == Mode.Brush)
            paint.SetBrushColor(color);
        else
            paint.SetFillColor(color);
    }
}
