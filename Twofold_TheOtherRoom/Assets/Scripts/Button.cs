using UnityEngine;

public class Button : MonoBehaviour
{
    public Lamp[] lamps;
    public int number;

    public enum ButtonColor
    {
        Red,
        Green,
        Blue
    }

    public ButtonColor buttonColor;

//    public float pressDistance = 0.1f;  
//    public float returnDelay = 0.5f;    

//    private Vector3 originalPosition;

    private void OnMouseDown()
    {
        switch (buttonColor)
        {
            case ButtonColor.Red:
                for (int i = 0; i < number; i++)
                {
                    lamps[i].BlinkRed();
                }
                break;

            case ButtonColor.Green:
                for (int i = 0; i < number; i++)
                {
                    lamps[i].BlinkGreen();
                }
                break;

            case ButtonColor.Blue:
                for (int i = 0; i < number; i++)
                {
                    lamps[i].BlinkBlue();
                }

                break;

        }
    }
    
}