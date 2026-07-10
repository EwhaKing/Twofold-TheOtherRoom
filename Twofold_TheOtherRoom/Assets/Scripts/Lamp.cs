using UnityEngine;
using System.Collections;

public class Lamp : MonoBehaviour
{
    public Light pointLight;
    public Renderer lampRenderer;

    Material mat;

    void Awake()
    {
        mat = lampRenderer.material;

        pointLight.enabled = false;
        mat.SetColor("_EmissionColor", Color.black);
    }

    public void BlinkRed()
    {   
        Debug.Log("빨강 반짝");
        StopAllCoroutines();
        StartCoroutine(Blink(Color.red));
    }

    public void BlinkGreen()
    {
        StopAllCoroutines();
        StartCoroutine(Blink(Color.green));
    }

    public void BlinkBlue()
    {
        StopAllCoroutines();
        StartCoroutine(Blink(Color.blue));
    }

    IEnumerator Blink(Color color)
    {
        Debug.Log("Blink 시작");
        pointLight.enabled = true;
        pointLight.color = color;

        mat.SetColor("_EmissionColor", color * 3);

        yield return new WaitForSeconds(0.5f);

        pointLight.enabled = false;
        mat.SetColor("_EmissionColor", Color.black);
    }
}