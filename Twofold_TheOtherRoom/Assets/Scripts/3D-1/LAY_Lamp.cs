
using UnityEngine;
using System.Collections;

public class LAY_Lamp : MonoBehaviour
{
    public Light pointLight;
    public Renderer lampRenderer;

    private Material mat;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        mat = lampRenderer.material;

        TurnOff();
    }

    public void BlinkWhite()
    {
        ColorUtility.TryParseHtmlString("#FFFFFF", out Color color);
        Blink(color);    }

    public void BlinkGreen()
    {
        ColorUtility.TryParseHtmlString("#0EECC0", out Color color);
        Blink(color);    }

    public void BlinkPink()
    {
        ColorUtility.TryParseHtmlString("#FF7B8A", out Color color);
        Blink(color);    }

    private void Blink(Color color)
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkRoutine(color));
    }

    private IEnumerator BlinkRoutine(Color color)
    {
        pointLight.enabled = true;
        pointLight.color = color;

        mat.SetColor("_EmissionColor", color * 3);

        yield return new WaitForSeconds(0.5f);

        TurnOff();
        blinkCoroutine = null;
    }

    public void TurnOff()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        pointLight.enabled = false;
        mat.SetColor("_EmissionColor", Color.black);
    }
}
