using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Full Screen Pass Renderer Feature 에 물린 BlinkIntro 머티리얼을 움직여 눈 깜빡임 연출을 그림.
///
/// 그리기만 담당하고 진행 시점은 밖에서 결정.
/// Begin → ApplyNormalizedTime(0~1) → Finish 순서로 호출.
/// playOnStart 는 이 순서를 코루틴으로 대신 돌려주는 단독 테스트용.
///
/// 재생 중에는 머티리얼 복사본을 Feature 에 끼워 넣고 끝나면 원상 복구.
/// 원본 .mat 과 Renderer Data 에셋을 런타임에 더럽히지 않기 위함.
/// </summary>
public class BlinkIntroPlayer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Universal Renderer Data 에 추가한 Full Screen Pass Renderer Feature")]
    [SerializeField] private FullScreenPassRendererFeature blinkFeature;

    [Tooltip("BlinkIntro 셰이더로 만든 머티리얼. 런타임에는 복사본 사용")]
    [SerializeField] private Material blinkMaterial;

    [Header("Timing")]
    [Tooltip("연출 전체 길이(초)")]
    [SerializeField] private float duration = 7f;

    // 눈은 감을 때가 뜰 때보다 2~3배 빠름. 접선을 비대칭으로 설정해 이 비율을 고정.
    // 골짜기마다 키를 두 개 두어 감은 채 머무는 구간 생성.
    [Tooltip("X = 진행도(0~1), Y = 눈 뜬 정도(0 감김 / 1 뜸 / 2 전체 화면)")]
    [SerializeField]
    private AnimationCurve blinkCurve = new AnimationCurve(
        new Keyframe(0.00f, 0.00f,  0.0f,  0.0f),   // 감긴 채 시작
        new Keyframe(0.05f, 0.00f,  0.0f,  3.6f),
        new Keyframe(0.20f, 0.45f,  0.8f, -7.0f),   // 1회차 45% → 닫힘
        new Keyframe(0.27f, 0.00f, -1.5f,  0.0f),
        new Keyframe(0.35f, 0.00f,  0.0f,  5.5f),   // 감은 채 유지
        new Keyframe(0.50f, 0.70f,  0.8f, -9.5f),   // 2회차 70% → 닫힘
        new Keyframe(0.57f, 0.00f, -2.0f,  0.0f),
        new Keyframe(0.65f, 0.00f,  0.0f,  6.0f),   // 감은 채 유지
        new Keyframe(0.85f, 1.00f,  0.0f,  3.0f),   // 완전히 뜸 (정상 눈 크기)
        new Keyframe(0.97f, 2.00f,  9.0f,  0.0f),   // 눈꺼풀이 화면 밖까지 확 열림
        new Keyframe(1.00f, 2.00f,  0.0f,  0.0f)    // 전체 화면으로 유지
    );

    [Header("Look (씬마다 맞게 설정)")]
    [Tooltip("좌우 끝점 위치. 0.5 = 화면 절반. 연출 내내 고정")]
    [Range(0.05f, 1f)]
    [SerializeField] private float eyeWidth = 0.6f;

    [Tooltip("다 떴을 때 윗눈꺼풀 꼭짓점 높이")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float upperLidPeak = 0.42f;

    [Tooltip("다 떴을 때 아랫눈꺼풀 꼭짓점 깊이. 실제 눈은 위보다 얕음")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float lowerLidPeak = 0.4f;

    [Tooltip("윗눈꺼풀에 움직임을 몰아주는 정도. 0 = 위아래 동일, 1 = 아랫눈꺼풀 거의 고정")]
    [Range(0f, 1f)]
    [SerializeField] private float upperLidBias = 0.1f;

    [Tooltip("마무리에서 좌우 끝점이 벌어지는 시점. 1 = 세로와 동시, 1 미만 = 가로가 먼저, 1 초과 = 가로가 나중")]
    [Range(0.25f, 6f)]
    [SerializeField] private float widenTiming = 0.25f;

    [Tooltip("눈꺼풀 경계가 번지는 정도")]
    [Range(0.001f, 0.3f)]
    [SerializeField] private float softness = 0.12f;

    [Tooltip("감았을 때 과노출 배율")]
    [Range(1f, 8f)]
    [SerializeField] private float exposureWhenClosed = 2f;

    [Tooltip("실눈일 때 하얗게 날아가는 정도")]
    [Range(0f, 1f)]
    [SerializeField] private float lightBleed = 0.6f;

    [Tooltip("감았을 때 블러 반경. 0.02 이상이면 링 보임")]
    [Range(0f, 0.05f)]
    [SerializeField] private float blurRadius = 0.016f;

    [Tooltip("초점이 돌아오는 속도. 1 = 눈 크기와 동일, 1 미만 = 눈은 떠져도 뿌옇게 보임")]
    [Range(0.25f, 3f)]
    [SerializeField] private float focusRecovery = 1f;

    [Header("Playback")]
    [Tooltip("단독 테스트용. 시퀀서가 Begin / ApplyNormalizedTime 을 호출하면 해제")]
    [SerializeField] private bool playOnStart = true;

    /// <summary>연출이 끝났을 때 호출됨.</summary>
    public event Action OnFinished;

    /// 눈꺼풀이 화면 밖까지 벌어져 전체 화면이 되는 _Blink 값.
    private const float FullyOpenBlink = 2f;

    private static readonly int BlinkID       = Shader.PropertyToID("_Blink");
    private static readonly int EyeWidthID    = Shader.PropertyToID("_EyeWidth");
    private static readonly int UpperPeakID   = Shader.PropertyToID("_EyeHeight");
    private static readonly int LowerPeakID   = Shader.PropertyToID("_EyeHeightDown");
    private static readonly int LidBiasID     = Shader.PropertyToID("_LidBias");
    private static readonly int WidenTimingID = Shader.PropertyToID("_WidenTiming");
    private static readonly int SoftnessID    = Shader.PropertyToID("_Softness");
    private static readonly int ExposureID    = Shader.PropertyToID("_Exposure");
    private static readonly int BleedID       = Shader.PropertyToID("_Bleed");
    private static readonly int BlurRadiusID  = Shader.PropertyToID("_BlurRadius");
    private static readonly int BlurFalloffID = Shader.PropertyToID("_BlurFalloff");

    private Material _runtimeMaterial;

    // Hook 전 상태. Unhook 에서 그대로 복원.
    private Material _originalPassMaterial;
    private bool _originalFeatureActive;
    private bool _hooked;

    private Coroutine _routine;

    private void Awake()
    {
        if (blinkFeature == null || blinkMaterial == null)
        {
            Debug.LogError("[BlinkIntro] blinkFeature / blinkMaterial 인스펙터 참조 연결할 것", this);
            enabled = false;
            return;
        }

        _runtimeMaterial = new Material(blinkMaterial) { name = blinkMaterial.name + " (Runtime)" };
        ApplyLook();
    }

    private void Start()
    {
        if (playOnStart) Play();
    }

    private void OnDisable()
    {
        Unhook();
    }

    private void OnDestroy()
    {
        Unhook();

        if (_runtimeMaterial == null) return;

        if (Application.isPlaying) Destroy(_runtimeMaterial);
        else DestroyImmediate(_runtimeMaterial);

        _runtimeMaterial = null;
    }

    // ---------- 자체 재생 (단독 테스트용) ----------

    /// <summary>duration 동안 커브를 따라 한 번 재생.</summary>
    public void Play()
    {
        if (!enabled) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        Begin();
        yield return null;   // Feature 가 켜진 다음 프레임부터 그려짐

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;   // 타임스케일 0 에서도 재생되도록
            ApplyNormalizedTime(elapsed / duration);
            yield return null;
        }

        Finish();
        _routine = null;
    }

    // ---------- 외부 구동 (시퀀서 / 네트워크 시계) ----------

    /// <summary>풀스크린 패스를 켜고 시작 상태로 전환.</summary>
    public void Begin()
    {
        if (!enabled) return;

        Hook();
        ApplyLook();
        ApplyNormalizedTime(0f);
    }

    /// <summary>진행도(0~1)를 넣으면 커브를 평가해 화면에 반영.</summary>
    public void ApplyNormalizedTime(float t01)
    {
        SetBlink(blinkCurve.Evaluate(Mathf.Clamp01(t01)));
    }

    /// <summary>화면 전체가 드러난 상태로 고정한 뒤 풀스크린 패스를 끔.</summary>
    public void Finish()
    {
        SetBlink(FullyOpenBlink);   // 1 로 두면 패스를 끄는 순간 눈 모양이 툭 사라짐

        Unhook();
        OnFinished?.Invoke();
    }

    // ---------- 내부 ----------

    /// 복사본 머티리얼을 Feature 에 끼우고 켬.
    private void Hook()
    {
        if (_hooked || blinkFeature == null) return;

        _originalPassMaterial  = blinkFeature.passMaterial;
        _originalFeatureActive = blinkFeature.isActive;
        _hooked = true;

        blinkFeature.passMaterial = _runtimeMaterial;
        blinkFeature.SetActive(true);
    }

    /// Feature 를 원래 상태로 복원.
    /// 복원하지 않으면 에디터에서 플레이할 때마다 Renderer Data 에셋이 수정된 것으로 잡힘.
    private void Unhook()
    {
        if (!_hooked || blinkFeature == null) return;

        blinkFeature.passMaterial = _originalPassMaterial;
        blinkFeature.SetActive(_originalFeatureActive);
        _hooked = false;
    }

    private void ApplyLook()
    {
        if (_runtimeMaterial == null) return;

        _runtimeMaterial.SetFloat(EyeWidthID,    eyeWidth);
        _runtimeMaterial.SetFloat(UpperPeakID,   upperLidPeak);
        _runtimeMaterial.SetFloat(LowerPeakID,   lowerLidPeak);
        _runtimeMaterial.SetFloat(LidBiasID,     upperLidBias);
        _runtimeMaterial.SetFloat(WidenTimingID, widenTiming);
        _runtimeMaterial.SetFloat(SoftnessID,    softness);
        _runtimeMaterial.SetFloat(ExposureID,    exposureWhenClosed);
        _runtimeMaterial.SetFloat(BleedID,       lightBleed);
        _runtimeMaterial.SetFloat(BlurRadiusID,  blurRadius);
        _runtimeMaterial.SetFloat(BlurFalloffID, focusRecovery);
    }

    private void SetBlink(float blink)
    {
        if (_runtimeMaterial != null)
            _runtimeMaterial.SetFloat(BlinkID, blink);
    }
}
