using UnityEngine;

public class CustomBrushController : MonoBehaviour
{
    public CustomTexturePainter paint;

    // 🔴 빨강
    public void SetRed()
    {
        paint.SetBrushColor(Color.red);
    }

    // 🔵 파랑
    public void SetBlue()
    {
        paint.SetBrushColor(Color.blue);
    }

    // ⚫ 검정
    public void SetBlack()
    {
        paint.SetBrushColor(Color.black);
    }

    // 🧽 지우개 (흰색으로 칠함 + 강제 브러시 모드 전환)
    public void SetEraser()
    {
        paint.SetBrushColor(Color.white);
        paint.SetBrushMode(); // Fill 모드에서 강제로 Brush 모드로 전환
    }

    // ✏️ 브러시 모드
    public void UseBrush()
    {
        paint.SetBrushMode();
    }

    // 🪣 채우기 모드
    public void UseFill()
    {
        paint.SetFillMode();
    }
}
