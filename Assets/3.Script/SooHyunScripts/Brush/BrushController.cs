using UnityEngine;

public class BrushController : MonoBehaviour
{
    public TexturePainter paint;
    public void SetBlack()
    {
        paint.SetBrushColor(Color.black);
    }
    public void SetGray()
    {
        paint.SetBrushColor(Color.gray);
    }

    public void SetRed()
    {
        paint.SetBrushColor(Color.red);
    }

    public void SetYellow()
    {
        paint.SetBrushColor(Color.yellow);
    }

    public void SetBlue()
    {
        paint.SetBrushColor(Color.blue);
    }

    public void SetGreen()
    {
        paint.SetBrushColor(Color.green);
    }

    public void SetEraser()
    {
        paint.SetBrushColor(Color.white); // 흰색으로 지우기
    }
}
