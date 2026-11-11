
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
    public float pressDistance = 0.05f;
    public float pressSpeed = 8f;
    public float returnSpeed = 6f;

    private Vector3 originalPosition;

    private static bool isAnimating = false;
    private static LAY_Button currentButton;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }
    public void Interact()
    {
        if (isAnimating)
            return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SFXType.ButtonPush);

        StartCoroutine(PressButton());
    }

    private IEnumerator PressButton()
    {
        isAnimating = true;

        if (currentButton != null)
            currentButton.TurnOffLamps();

        currentButton = this;

        Vector3 pressedPosition =
            originalPosition + Vector3.down * pressDistance;

        while (Vector3.Distance(transform.localPosition, pressedPosition) > 0.001f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                pressedPosition,
                pressSpeed * Time.deltaTime);

            yield return null;
        }

        for (int i = 0; i < number && i < lamps.Length; i++)
        {
            if (lamps[i] == null)
                continue;

            switch (buttonColor)
            {
                case ButtonColor.Red:
                    lamps[i].BlinkRed();
                    break;

                case ButtonColor.Green:
                    lamps[i].BlinkGreen();
                    break;

                case ButtonColor.Blue:
                    lamps[i].BlinkBlue();
                    break;
            }
        }

        yield return new WaitForSeconds(0.1f);

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

    public void TurnOffLamps()
    {
        if (lamps == null)
            return;

        foreach (LAY_Lamp lamp in lamps)
        {
            if (lamp != null)
                lamp.TurnOff();
        }
    }
}
