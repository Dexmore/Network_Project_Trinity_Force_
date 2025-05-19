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
        paint.SetBrushMode(); // Fill 모드일 경우 강제로 Brush로 전환
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
