
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

    public void BlinkRed()
    {
        Blink(Color.red);
    }

    public void BlinkGreen()
    {
        Blink(Color.green);
    }

    public void BlinkBlue()
    {
        Blink(Color.blue);
    }

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
