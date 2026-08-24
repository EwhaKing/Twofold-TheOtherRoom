using UnityEngine;

public class KnifeClick : MonoBehaviour, IInteractable
{
    public CutCake controller;

    public void Interact()
    {
        SoundManager.Instance.PlaySFX(SFXType.Knife);
        controller.CakePiece(this);
    }
}