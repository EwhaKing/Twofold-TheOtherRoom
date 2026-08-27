using System.Collections;
using UnityEngine;

public class FloorPanelPuzzleResetButton : MonoBehaviour, IInteractable
{
    [SerializeField] private FloorPanelPuzzleController controller;

    [Header("Press Animation")]
    [SerializeField] private float pressDepth = 0.1f;      // 눌림 깊이
    [SerializeField] private float pressDuration = 0.08f;  // 구간 시간

    private Vector3 startPos;       // 원위치
    private Coroutine pressCo;      // 진행 중인 애니 핸들 (연타 겹침 방지)

    void Awake()
    {
        startPos = transform.localPosition;
    }

    // PlayerInteractor가 E키로 호출
    public void Interact()
    {
        if (controller != null) controller.ResetPuzzle();
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.RoomMove);
        }
        if (pressCo != null) StopCoroutine(pressCo); // 위치 튐 방지
        pressCo = StartCoroutine(PressAnim());
    }

    IEnumerator PressAnim()
    {
        Vector3 downPos = startPos + new Vector3(0f, -pressDepth, 0f);

        yield return MoveTo(startPos, downPos);  // 내려가기
        yield return MoveTo(downPos, startPos);  // 올라오기

        transform.localPosition = startPos;      // 오차 보정
        pressCo = null;
    }

    IEnumerator MoveTo(Vector3 a, Vector3 b)
    {
        float t = 0f;
        while (t < pressDuration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(a, b, t / pressDuration);
            yield return null;
        }
        transform.localPosition = b;
    }
}
