using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class Stage1CutsceneDirector : MonoBehaviour
{
    [Header("UI & 비디오 요소")]
    public VideoPlayer videoPlayer;
    public RectTransform topLid;          // 위 눈꺼풀 (TopLid)
    public RectTransform bottomLid;       // 아래 눈꺼풀 (BottomLid)
    public GameObject rawImageObject;     // 비디오 렌더링용 RawImage
    public Image nextSceneBackgroundImage; // 다음 배경 이미지

    private bool isVideoFinished = false;
    private float maxLidHeight;           // 화면 절반 높이

    void Start()
    {
        // 1. 캔버스 기준 화면 절반 높이 계산
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            maxLidHeight = canvasRect.rect.height / 2f;
        }
        else
        {
            maxLidHeight = Screen.height / 2f;
        }

        // 시작 시 눈꺼풀 완전히 열어두기
        SetLidHeight(0f);

        StartCoroutine(CutsceneSequenceRoutine());
    }

    IEnumerator CutsceneSequenceRoutine()
    {
        if (rawImageObject != null)
            rawImageObject.SetActive(true);

        // 비디오 준비 및 재생
        isVideoFinished = false;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        videoPlayer.Play();

        // 비디오 재생 완료 대기
        yield return new WaitUntil(() => isVideoFinished);

        // 영상 끝나자마자 완전 단색 검은색으로 100% 암전 덮기
        SetSolidBlackLids();
        SetLidHeight(maxLidHeight);

        // 영상 렌더링 끄기
        if (rawImageObject != null)
            rawImageObject.SetActive(false);

        Debug.Log("영상 종료 및 암전 완료!");
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        isVideoFinished = true;
    }

    void SetLidHeight(float height)
    {
        if (topLid != null)
            topLid.sizeDelta = new Vector2(topLid.sizeDelta.x, height);

        if (bottomLid != null)
            bottomLid.sizeDelta = new Vector2(bottomLid.sizeDelta.x, height);
    }

    // 틈새 없이 완벽한 암전을 만드는 단색 검은색 세팅
    void SetSolidBlackLids()
    {
        if (topLid != null && topLid.TryGetComponent(out Image topImg))
        {
            topImg.sprite = null;
            topImg.color = Color.black;
        }

        if (bottomLid != null && bottomLid.TryGetComponent(out Image botImg))
        {
            botImg.sprite = null;
            botImg.color = Color.black;
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }
}