using UnityEngine;

public class KnifeClick_Mouse : MonoBehaviour
{
    [SerializeField] private CutCake_Mouse controller;

    public void Click()
    {
        if (controller == null)
        {
            Debug.LogWarning("[KnifeClick_Mouse] CutCake_Mouse가 연결되지 않았습니다.", this);
            return;
        }

        SoundManager.Instance?.PlaySFX(SFXType.Knife);
        controller.CakePiece(this);
    }
}
