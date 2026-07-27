using UnityEngine;
using System.Collections;

public class LAY_Button : MonoBehaviour, IInteractable
{
    public LAY_Lamp[] lamps;
    public int number;

    public enum ButtonColor
    {
        Red,
        Green,
        Blue
    }

    public ButtonColor buttonColor;

    [Header("Button Animation")]
    public float pressDistance = 0.05f;   // 얼마나 내려갈지
    public float pressSpeed = 8f;         // 내려가는 속도
    public float returnSpeed = 6f;        // 올라오는 속도

    private Vector3 originalPosition;
    private bool isAnimating = false;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void Interact()
    {    
        if (isAnimating)
        {    
            return;
        }

        StartCoroutine(PressButton());
    }
    
    private IEnumerator PressButton()
    {
        isAnimating = true;
        SoundManager.Instance.PlaySFX(SFXType.TestSe);

        Vector3 pressedPosition = originalPosition + Vector3.down * pressDistance;

        // 아래로 누르기
        while (Vector3.Distance(transform.localPosition, pressedPosition) > 0.001f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                pressedPosition,
                pressSpeed * Time.deltaTime);

            yield return null;
        }

        // 버튼 기능 실행
        switch (buttonColor)
        {
            case ButtonColor.Red:
                for (int i = 0; i < number; i++)
                    lamps[i].BlinkRed();
                break;

            case ButtonColor.Green:
                for (int i = 0; i < number; i++)
                    lamps[i].BlinkGreen();
                break;

            case ButtonColor.Blue:
                for (int i = 0; i < number; i++)
                    lamps[i].BlinkBlue();
                break;
        }

        yield return new WaitForSeconds(0.1f);

        // 원래 위치로 복귀
        while (Vector3.Distance(transform.localPosition, originalPosition) > 0.001f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                originalPosition,
                returnSpeed * Time.deltaTime);

            yield return null;
        }

        transform.localPosition = originalPosition;
        isAnimating = false;
    }
}