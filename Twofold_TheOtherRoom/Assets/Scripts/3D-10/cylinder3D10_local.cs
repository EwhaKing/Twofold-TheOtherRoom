using System.Collections;
using UnityEngine;

public class cylinder3D10_local : MonoBehaviour, IInteractable
{
    [SerializeField] private Answer3D10 controller;
    [SerializeField] private float rotationStep = 120f;
    [SerializeField] private float rotationSpeed = 5f;

    private bool isRotating;
    private float currentZ;

    private void Start()
    {
        currentZ = transform.localEulerAngles.z;
    }

    public void Interact()
    {
        if (isRotating || (controller != null && controller.GetSolve()))
            return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Scrape);
        }
        currentZ = Mathf.Repeat(currentZ + rotationStep, 360f);
        StartCoroutine(RotateZSmooth());
        controller.CheckAnswer(); 
    }

    public float GetZ()
    {
        return currentZ;
    }

    private IEnumerator RotateZSmooth()
    {
        isRotating = true;

        Quaternion start = transform.localRotation;
        Vector3 targetEuler = transform.localEulerAngles;
        targetEuler.z = currentZ;
        Quaternion end = Quaternion.Euler(targetEuler);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * rotationSpeed;
            transform.localRotation = Quaternion.Slerp(start, end, Mathf.Clamp01(t));
            yield return null;
        }

        transform.localRotation = end;
        isRotating = false;
    }
}
