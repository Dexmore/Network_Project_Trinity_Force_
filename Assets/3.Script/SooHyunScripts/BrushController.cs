using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushController : MonoBehaviour
{
    public TexturePainter paint;
    public void SetBlack() => paint.SetBrushColor(Color.black);
    public void SetRed() => paint.SetBrushColor(Color.red);
    public void SetBlue() => paint.SetBrushColor(Color.blue);
}
