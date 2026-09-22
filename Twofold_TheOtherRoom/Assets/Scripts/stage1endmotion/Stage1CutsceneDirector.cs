using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class Stage1CutsceneDirector : MonoBehaviour, IStageCutscene
{
    [Header("UI & 비디오 요소")]
    public VideoPlayer videoPlayer;
    public CanvasGroup fadeCanvasGroup;    // BlackFadeOverlay의 Canvas Group
    public GameObject rawImageObject;      // 비디오 출력용 RawImage

    [Header("연출 중 잠금")]
    [Tooltip("연출 동안 끌 것. 일시정지 버튼, 타이머 UI 등")]
    [SerializeField] private GameObject[] hideDuringCutscene;

    [Tooltip("연출 동안 끌 입력 컴포넌트. 씬에는 켜둔 채로 저장할 것")]
    [SerializeField] private Behaviour[] inputToLock;

    [Tooltip("단독 테스트 씬 전용 자동 재생.\n" +
             "StageChangeDriver가 구동하는 씬에서는 꺼둘 것")]
    [SerializeField] private bool playOnStart = false;

    //이벤트
    public event Action Finished;

    private bool isVideoFinished = false;

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();

    private Coroutine routine;

    public bool IsPlaying => routine != null;

    void Start()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        // 재생 전까지 비디오 출력 숨김
        if (rawImageObject != null)
            rawImageObject.SetActive(false);

        if (playOnStart) Play();
    }

    /// 재생 중 재호출은 무시
    [ContextMenu("TEST - Play")]
    public void Play()
    {
        if (IsPlaying || !Application.isPlaying) return;

        routine = StartCoroutine(CutsceneSequenceRoutine());
    }

    void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        playerControlLock.Unlock();
    }

    /// 연출 동안 조작 차단. 다음 씬으로 넘어가므로 되돌리지 않음
    private void LockControls()
    {
        playerControlLock.Lock(this, inputToLock);

        foreach (GameObject go in hideDuringCutscene)
            if (go != null) go.SetActive(false);

        // Esc 와 일시정지 버튼 둘 다 잠김
        PauseController pause = FindAnyObjectByType<PauseController>();
        if (pause != null) pause.BlockPause = true;
    }

    IEnumerator CutsceneSequenceRoutine()
    {
        LockControls();

        if (rawImageObject != null)
            rawImageObject.SetActive(true);

        isVideoFinished = false;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        videoPlayer.Play();

        // 1. 영상 재생 대기
        yield return new WaitUntil(() => isVideoFinished);

        // 2. 영상 끝나자마자 암전(검은 화면 100%)
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
        }

        if (rawImageObject != null)
            rawImageObject.SetActive(false);

        Debug.Log("영상 종료 및 암전 완료 -> Finished 이벤트 호출");

        // 3. 암전 완료 
        routine = null;
        Finished?.Invoke();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        isVideoFinished = true;
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }
}
