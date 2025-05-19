using UnityEngine;

public class BrushController : MonoBehaviour
{
    public TexturePainter paint;
    public FlexibleColorPicker colorPicker; // ���ӽ����̽� ���ʿ�

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
        currentMode = Mode.Brush; // �귯�� ��� ���� ��ȯ
        Color eraserColor = Color.white;
        paint.SetBrushColor(eraserColor);

        // (�ɼ�) Color Picker ���� ������� �ٲٱ�
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
