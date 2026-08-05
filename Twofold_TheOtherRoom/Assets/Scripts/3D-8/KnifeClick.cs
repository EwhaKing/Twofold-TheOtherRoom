using UnityEngine;

public class KnifeClick : MonoBehaviour, IInteractable
{
    public CutCake controller;

    public void Interact()
    {
        controller.CakePiece(this);
    }
}