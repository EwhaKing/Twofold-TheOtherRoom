using UnityEngine;

public class PaperSound : MonoBehaviour
{
    private void OnEnable()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Paper);
        }
    }
}
