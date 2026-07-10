using UnityEngine;

public class Button : MonoBehaviour
{
    public Lamp lamp0;
    public Lamp lamp1;
    public Lamp lamp2;
    public Lamp lamp3;
    public Lamp lamp4;

    

    public enum ButtonColor
    {
        Red,
        Green,
        Blue
    }

    public ButtonColor buttonColor;

    private void OnMouseDown()
    {

        switch (buttonColor)
        {
            case ButtonColor.Red:
                lamp0.BlinkRed();
                lamp1.BlinkRed();
                lamp2.BlinkRed();
                lamp3.BlinkRed();

                Debug.Log("빨강");
                break;

            case ButtonColor.Green:
                lamp0.BlinkGreen();
                lamp1.BlinkGreen();
                lamp2.BlinkGreen();

                Debug.Log("초록");
                break;

            case ButtonColor.Blue:
                lamp0.BlinkBlue();
                lamp1.BlinkBlue();
                lamp2.BlinkBlue();
                lamp3.BlinkBlue();
                lamp4.BlinkBlue();

                Debug.Log("파랑");
                break;
        }
    }
}
