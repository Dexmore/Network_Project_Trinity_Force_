using UnityEngine;

public class BrushController : MonoBehaviour
{
    public TexturePainter paint;

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
        paint.SetBrushColor(Color.white); // 흰색으로 지우기
    }
}
