using UnityEngine;


public class Ball2D10 : MonoBehaviour
{    
    public enum BallColor
    {
        Red,
        Yellow,
        Green
    }

    public BallColor ballColor;

    public BallColor GetColor()
    {
        return ballColor;
    }
}