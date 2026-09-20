using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 임시 전환 연출. 화면을 검게 덮고 Finished 보냄.
/// </summary>
public class StageCutsceneStub : MonoBehaviour, IStageCutscene
{
    [Tooltip("암전용 풀스크린 검정. 씬에는 꺼둔 채로 저장할 것")]
    [SerializeField] private CanvasGroup blackout;

    [Tooltip("연출 동안 끌 것. 타이머 UI, 거울 완성 이펙트 등")]
    [SerializeField] private GameObject[] hideDuringCutscene;

    [Tooltip("연출 동안 끌 입력 컴포넌트. 씬에는 켜둔 채로 저장할 것.\n" +
             "비워두면 PlayerController · PlayerLocomotionInput · PlayerInteractor 를 자동으로 찾음")]
    [SerializeField] private Behaviour[] inputToLock;

    [SerializeField] private float fadeSeconds = 2f;
    [SerializeField] private float holdBlackSeconds = 1f;

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();

    private Coroutine routine;

    private void Awake()
    {
        // 암전 canvas 끔
        if (blackout == null) return;

        blackout.alpha = 0f;
        blackout.gameObject.SetActive(false);
    }

    public event Action Finished;

    public bool IsPlaying => routine != null;

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        playerControlLock.Unlock();
    }

    [ContextMenu("TEST - Play")]
    public void Play()
    {
        if (IsPlaying || !Application.isPlaying) return;

        routine = StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        playerControlLock.Lock(this, inputToLock);

        foreach (GameObject go in hideDuringCutscene)
            if (go != null) go.SetActive(false);

        if (blackout != null)
        {
            blackout.alpha = 0f;
            blackout.gameObject.SetActive(true);

            for (float elapsed = 0f; elapsed < fadeSeconds; elapsed += Time.deltaTime)
            {
                blackout.alpha = elapsed / fadeSeconds;
                yield return null;
            }

            blackout.alpha = 1f;
        }

        yield return new WaitForSeconds(holdBlackSeconds);

        routine = null;
        Finished?.Invoke();
    }
}
