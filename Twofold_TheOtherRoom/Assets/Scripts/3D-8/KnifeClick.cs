using UnityEngine;

public class KnifeClick : MonoBehaviour, IInteractable
{
    public CutCake controller;

    public void Interact()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Knife);
        }
        controller.CakePiece(this);
    }
}