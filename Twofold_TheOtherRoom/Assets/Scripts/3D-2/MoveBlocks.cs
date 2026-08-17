using UnityEngine;
using System.Collections;
using System;

public class MoveBlocks : MonoBehaviour , IInteractable
{
    public event Action<int> ZoneChanged;
   
    [Header("Zone Settings")]
    public int currentZone = 1;

    [Header("Move Settings")]
    public float stepDistance = 1f;
    public float moveDuration = 0.35f;


    [Header("Owner Puzzle")]
    public ShadowBlockAnswer shadowblockanswer;



    private float startZ;
    private bool movingForward = true;
    private bool isMoving = false;

    private void Start()
    {
        startZ = transform.position.z;
        currentZone = 1;
    }

    public void Interact()
    {
        if (isMoving) return;

        MoveOneStep();



        shadowblockanswer.CheckClear();

    }

    private void MoveOneStep()
    {
        if (movingForward)
        {
            currentZone++;

            if (currentZone >= 4)
            {
                currentZone = 4;
                movingForward = false;
            }
        }
        else
        {
            currentZone--;

            if (currentZone <= 1)
            {
                currentZone = 1;
                movingForward = true;
            }
        }

        float targetZ = startZ - stepDistance * (currentZone - 1);

        Vector3 targetPosition = transform.position;
        targetPosition.z = targetZ;

        ZoneChanged?.Invoke(currentZone);
        StartCoroutine(MoveToPosition(targetPosition));

        Debug.Log(gameObject.name + " 현재 구역: " + currentZone);
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true;

        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / moveDuration;

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    public int GetZone()
    {
        return currentZone;
    }
}
