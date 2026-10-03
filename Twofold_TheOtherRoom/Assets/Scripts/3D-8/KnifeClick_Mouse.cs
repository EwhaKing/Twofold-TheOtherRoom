using System.Collections;
using UnityEngine;

public class KnifeClick_Mouse : MonoBehaviour
{
    [SerializeField] private CutCake_Mouse controller;

    private Renderer[] renderers;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void Click()
    {
        if (controller == null)
        {
            Debug.LogWarning("[KnifeClick_Mouse] CutCake_Mouse가 연결되지 않았습니다.", this);
            return;
        }

        SoundManager.Instance?.PlaySFX(SFXType.Knife);

        // 칼 클릭하면 깜빡임
        StopAllCoroutines();
        StartCoroutine(Blink());

        controller.CakePiece(this);
    }

    private IEnumerator Blink()
    {
        for (int i = 0; i < 2; i++)
        {
            SetVisible(false);
            yield return new WaitForSeconds(0.1f);

            SetVisible(true);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }
}