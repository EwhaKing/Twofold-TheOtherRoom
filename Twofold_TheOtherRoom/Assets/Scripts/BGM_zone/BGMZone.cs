using UnityEngine;

public class BGMZone : MonoBehaviour
{
    [Header("지하실에 들어왔을 때 BGM")]
    [SerializeField] private BGMType insideBGM = BGMType.BasementBG;

    [Header("지하실에서 나갔을 때 BGM")]
    [SerializeField] private BGMType outsideBGM = BGMType.WhiteNoise;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(insideBGM);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(outsideBGM);
        }
    }
}