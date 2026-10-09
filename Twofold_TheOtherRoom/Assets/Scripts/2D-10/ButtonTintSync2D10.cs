using UnityEngine;
using UnityEngine.UI;

// Button의 Color Tint(Normal/Highlighted/Pressed/Disabled)를 자식 그래픽에도 똑같이 적용
[RequireComponent(typeof(Button))]
public class ButtonTintSync2D10 : MonoBehaviour
{
    [SerializeField] private Graphic[] extraGraphics;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void LateUpdate()
    {
        if (button.targetGraphic == null)
        {
            return;
        }

        Color tint = button.targetGraphic.canvasRenderer.GetColor();

        foreach (Graphic graphic in extraGraphics)
        {
            if (graphic != null)
            {
                graphic.canvasRenderer.SetColor(tint);
            }
        }
    }
}
