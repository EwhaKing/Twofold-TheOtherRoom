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
    private Sprite blurredTopSprite;
    private Sprite blurredBottomSprite;

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

        // 2. 깜빡일 때 사용할 블러 스프라이트 사전 제작
        CreateBlurSprites();

        // 3. 시작 시 눈꺼풀 완전히 열어두기
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

        // 4. 영상 끝나자마자 빛 샐 틈 없는 단색 검은색으로 100% 완전 암전
        SetSolidBlackLids();
        SetLidHeight(maxLidHeight);

        // 영상 렌더링 끄기
        if (rawImageObject != null)
            rawImageObject.SetActive(false);

        // 5. 암전 대기 및 눈 깜빡임 연출 실행
        yield return StartCoroutine(EyeBlinkRoutine());

        Debug.Log("컷씬 종료 및 눈 깜빡임 완료!");
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        isVideoFinished = true;
    }

    IEnumerator EyeBlinkRoutine()
    {
        if (topLid == null || bottomLid == null) yield break;

        // [완전 암전]: 칠흑 같은 어둠 속에서 2초간 정적
        yield return new WaitForSeconds(2.0f);

        // 첫 깜빡임 직전에 블러 스프라이트로 교체
        ApplyBlurSprites();
        maxLidHeight += 120f;
        SetLidHeight(maxLidHeight);

        // [1차 깜빡임] 무겁게 겨우 실눈을 떴다가 힘없이 감김
        yield return StartCoroutine(AnimateLids(maxLidHeight, maxLidHeight * 0.75f, 0.9f)); // 0.9초 동안 아주 느릿하게 실눈
        yield return new WaitForSeconds(0.4f);                                             // 멍하니 0.4초 유지
        yield return StartCoroutine(AnimateLids(maxLidHeight * 0.75f, maxLidHeight, 0.5f)); // 힘없이 스르륵 감김
        yield return new WaitForSeconds(0.8f);                                             // 감은 채로 깊은 숨고르기

        // [2차 깜빡임] 의식을 차리려고 조금 더 힘주어 떠봄
        yield return StartCoroutine(AnimateLids(maxLidHeight, maxLidHeight * 0.55f, 0.7f)); // 0.7초 동안 뜸
        yield return new WaitForSeconds(0.35f);                                            // 0.35초 초점 맞추듯 응시
        yield return StartCoroutine(AnimateLids(maxLidHeight * 0.55f, maxLidHeight, 0.4f)); // 다시 질끈 감김
        yield return new WaitForSeconds(0.6f);                                             // 숨고르기

        // [3차 깜빡임] 주변이 보이기 시작함 (거의 다 뜸)
        yield return StartCoroutine(AnimateLids(maxLidHeight, maxLidHeight * 0.25f, 0.6f));
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(AnimateLids(maxLidHeight * 0.25f, maxLidHeight, 0.35f));
        yield return new WaitForSeconds(0.5f);

        // [4차 깜빡임] 천천히 시야가 완전히 트이며 각성
        yield return StartCoroutine(AnimateLids(maxLidHeight, 0f, 2.2f));
    }

    IEnumerator AnimateLids(float fromHeight, float toHeight, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float currentHeight = Mathf.Lerp(fromHeight, toHeight, timer / duration);
            SetLidHeight(currentHeight);
            yield return null;
        }
        SetLidHeight(toHeight);
    }

    void SetLidHeight(float height)
    {
        if (topLid != null)
            topLid.sizeDelta = new Vector2(topLid.sizeDelta.x, height);

        if (bottomLid != null)
            bottomLid.sizeDelta = new Vector2(bottomLid.sizeDelta.x, height);
    }

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

    void ApplyBlurSprites()
    {
        if (topLid != null && topLid.TryGetComponent(out Image topImg))
        {
            topImg.sprite = blurredTopSprite;
            topImg.color = Color.white;
        }

        if (bottomLid != null && bottomLid.TryGetComponent(out Image botImg))
        {
            botImg.sprite = blurredBottomSprite;
            botImg.color = Color.white;
        }
    }

    void CreateBlurSprites()
    {
        int width = 512;
        int height = 512;

        Texture2D topTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        topTex.wrapMode = TextureWrapMode.Clamp;
        Color[] topPixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float v = (float)y / (height - 1);
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / (width - 1);
                float arch = Mathf.Sin(u * Mathf.PI) * 0.15f;
                float edge = 0.02f + arch;

                float blurRange = 0.25f;
                float alpha = Mathf.SmoothStep(0f, 1f, (v - edge) / blurRange);

                topPixels[y * width + x] = new Color(0f, 0f, 0f, alpha);
            }
        }
        topTex.SetPixels(topPixels);
        topTex.Apply();

        blurredTopSprite = Sprite.Create(topTex, new Rect(0, 0, width, height), new Vector2(0.5f, 1f));

        Texture2D botTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        botTex.wrapMode = TextureWrapMode.Clamp;
        Color[] botPixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                botPixels[y * width + x] = topPixels[(height - 1 - y) * width + x];
            }
        }
        botTex.SetPixels(botPixels);
        botTex.Apply();

        blurredBottomSprite = Sprite.Create(botTex, new Rect(0, 0, width, height), new Vector2(0.5f, 0f));
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }
}