using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면. 시작하기 / 설정 / 종료
/// </summary>
public class TitleView : ScreenView
{
    [Header("위젯")]
    [SerializeField] Button btnStart;
    [SerializeField] Button btnSetting;
    [SerializeField] Button btnQuit;

    [Header("시작 연출")]
    [SerializeField] Image background;
    [Tooltip("처음 보이는 사진")]
    [SerializeField] Sprite spriteBefore;
    [Tooltip("흔들린 뒤 바뀌는 사진")]
    [SerializeField] Sprite spriteAfter;
    [Tooltip("Overlay 캔버스라 카메라 대신 이걸 흔듦. Screen_Title 지정")]
    [SerializeField] Transform shakeTarget;
    [SerializeField] float shakeDuration = 0.4f;
    [Tooltip("배경 Scale 1.03 기준 16 이하면 가장자리 틈이 안 보임")]
    [SerializeField] float shakeStrength = 12f;
    [Tooltip("사진이 바뀐 뒤 다음 화면으로 넘어가기 전까지 대기")]
    [SerializeField] float holdAfterSwap = 0.6f;

    // 연출 중 연타와 다른 버튼 입력 방지
    bool _starting;

    public override ScreenId Id => ScreenId.Title;

    void Start()
    {
        btnStart.onClick.AddListener(OnClickStart);
        btnSetting.onClick.AddListener(() => { if (!_starting) GameFlow.Instance.OpenSetting(ScreenId.Title); });
        btnQuit.onClick.AddListener(() => { if (!_starting) GameFlow.Instance.QuitGame(); });
    }

    // 메뉴나 게임에서 돌아왔을 때 처음 사진으로 복구
    protected override void OnVisibilityChanged(bool on)
    {
        if (on && background != null) background.sprite = spriteBefore;
    }

    void OnClickStart()
    {
        if (_starting) return;
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        _starting = true;
        
        background.sprite = spriteAfter;
        
        if(SoundManager.Instance != null){
        SoundManager.Instance.PlaySFX(SFXType.GlassBreaking); }

        Vector3 origin = shakeTarget.localPosition;
        for (float t = 0f; t < shakeDuration; t += Time.unscaledDeltaTime)
        {
            // 끝으로 갈수록 약해짐
            float strength = shakeStrength * (1f - t / shakeDuration);
            shakeTarget.localPosition = origin + (Vector3)(Random.insideUnitCircle * strength);
            yield return null;
        }
        shakeTarget.localPosition = origin;

      
        yield return new WaitForSecondsRealtime(holdAfterSwap);

        _starting = false;
        GameFlow.Instance.Show(ScreenId.Menu);
    }
}
