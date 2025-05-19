using UnityEngine;

public class CustomBrushController : MonoBehaviour
{
    public CustomTexturePainter paint;

    public void SetRed()
    {
        paint.SetBrushColor(Color.red);
    }

    public void SetBlue()
    {
        paint.SetBrushColor(Color.blue);
    }

    public void SetBlack()
    {
        paint.SetBrushColor(Color.black);
    }

    public void SetEraser()
    {
        paint.SetBrushColor(Color.white);
        paint.SetBrushMode(); // Fill ����� ��� ������ Brush�� ��ȯ
    }


    public void UseBrush()
    {
        paint.SetBrushMode();
    }

    public void UseFill()
    {
        paint.SetFillMode();
    }
}
