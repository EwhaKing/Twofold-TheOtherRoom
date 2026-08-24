using UnityEngine;

public class PaperSound : MonoBehaviour
{
    private void OnEnable()
    { 
        SoundManager.Instance.PlaySFX(SFXType.Paper);
    }
}
