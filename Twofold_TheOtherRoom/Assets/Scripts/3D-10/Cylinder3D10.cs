using UnityEngine;
using System.Collections;

public class Cylinder3D10 : MonoBehaviour
{
    private bool isRotating = false;
    private float currentY = 80f;
    public Answer3D10 controller;


    void OnMouseDown()
    {
        if (isRotating)
            return;
        currentY += 120f;

        if (currentY >= 360f)
            currentY -= 360f;

        StartCoroutine(RotateSmooth());
        controller.CheckAnswer();

    }

    public float GetY()
    {
        return currentY;
    }

     IEnumerator RotateSmooth()
    {
        isRotating = true;

        Quaternion start = transform.rotation;
        Quaternion end = Quaternion.Euler(0, currentY, 0);

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 5f;   // 숫자가 클수록 빨라짐
            transform.rotation = Quaternion.Lerp(start, end, t);
            yield return null;
        }

        transform.rotation = end;
        isRotating = false;
    }

}