using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class Stage1CutsceneDirector : MonoBehaviour
{
    [Header("UI & 비디오 요소")]
    public VideoPlayer videoPlayer;
    public CanvasGroup fadeCanvasGroup;    // BlackFadeOverlay의 Canvas Group
    public GameObject rawImageObject;      // 비디오 출력용 RawImage
    public Image nextSceneBackgroundImage; // 다음 배경 이미지

    //이벤트
    public event Action Finished;

    private bool isVideoFinished = false;

    void Start()
    {
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