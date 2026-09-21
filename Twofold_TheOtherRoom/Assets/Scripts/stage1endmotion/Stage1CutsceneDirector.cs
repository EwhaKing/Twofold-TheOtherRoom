using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using UnityEngine.UI;

public class Stage1CutsceneDirector : MonoBehaviour
{
    [Header("UI & 비디오 요소")]
    public VideoPlayer videoPlayer;
    public CanvasGroup fadeCanvasGroup;   // BlackFadeOverlay의 Canvas Group
    public GameObject rawImageObject;     // 비디오 출력용 RawImage
    public Image nextSceneBackgroundImage; // 다음 배경 이미지 (필요시)

    [Header("연출 완료 이벤트")]
    [Tooltip("인스펙터에서 스테이지 전환 매니저 등의 완료 함수를 연결할 수 있습니다.")]
    public UnityEvent onCutsceneFinished;

    // Action 이벤트
    public event Action OnFinished;

    private bool isVideoFinished = false;

    void Start()
    {
        // 0. 시작 시 검은 암전 화면 투명화 
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        if (rawImageObject != null)
            rawImageObject.SetActive(true);

        StartCoroutine(CutsceneSequenceRoutine());
    }

    IEnumerator CutsceneSequenceRoutine()
    {
        // 1. 비디오 종료 이벤트 등록 및 준비
        isVideoFinished = false;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        videoPlayer.Play();

        // 2. 영상이 끝날 때까지 대기
        yield return new WaitUntil(() => isVideoFinished);

        // 3. 영상 끝나자마자 암전(검은 화면 100%) 처리
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true; // 암전 중 클릭 방지
        }

        // 4. 비디오 렌더링 끄기
        if (rawImageObject != null)
            rawImageObject.SetActive(false);

        Debug.Log("컷씬 종료 및 완전 암전 완료 -> Finished 이벤트");

        // Finished 이벤트 호출
        OnFinished?.Invoke();
        onCutsceneFinished?.Invoke();
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