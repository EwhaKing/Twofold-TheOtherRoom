using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

/// <summary>
/// 3D 스테이지 전환 컷신. 연출만 담당하고 진행 판정은 밖에서 결정.
///
/// glow 끄기 → 컷신 카메라와 액터로 전환 → 반사가 선명해지며 액터가 거울 앞으로 걸어옴
/// → 반사된 얼굴의 눈으로 확대 → 암전 → Finished.
/// </summary>
public class StageChange3D : MonoBehaviour, IStageCutscene
{
    [Header("Mirror")]
    [Tooltip("완성 거울의 발광. 연출 시작과 함께 끔")]
    [SerializeField] private MirrorGlow glow;

    [SerializeField] private MirrorReflection mirrorReflection;

    [Header("Player (연출 동안 끔)")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener playerListener;

    [Tooltip("연출 동안 끌 입력 컴포넌트. 씬에는 켜둔 채로 저장할 것.\n" +
             "비워두면 PlayerController · PlayerLocomotionInput · PlayerInteractor 를 자동으로 찾음")]
    [SerializeField] private Behaviour[] inputToLock;

    [Header("Cutscene Rig (씬에는 꺼둔 채로 저장할 것)")]
    [SerializeField] private GameObject cutsceneRig;
    [SerializeField] private Camera cutsceneCamera;
    [SerializeField] private AudioListener cutsceneListener;

    [Tooltip("액터 뒤에서 거울을 보는 샷")]
    [SerializeField] private CinemachineVirtualCamera vcamMirror;

    [Tooltip("거울 면 코앞, 반사된 얼굴의 눈 높이")]
    [SerializeField] private CinemachineVirtualCamera vcamEyeCloseUp;

    [Header("Actor")]
    [Tooltip("실제 플레이어를 그대로 써도 됨. PlayerAnimation 은 연출 동안 자동으로 꺼짐")]
    [SerializeField] private Transform actor;

    [SerializeField] private Animator actorAnimator;
    [SerializeField] private Transform actorStart;
    [SerializeField] private Transform actorMirrorFront;

    [Header("UI")]
    [Tooltip("암전용 풀스크린 검정. 씬에는 alpha 0 으로 저장할 것")]
    [SerializeField] private CanvasGroup blackout;

    [Tooltip("연출 동안 끌 것. 타이머 UI, 거울 완성 이펙트 등.\n" +
             "런타임에 켜진 것도 넣을 수 있음 — 시작 시점 상태로 되돌림")]
    [SerializeField] private GameObject[] hideDuringCutscene;

    [Header("Timing")]
    [SerializeField] private float glowFadeSeconds = 0.6f;

    [Tooltip("컷 전환 후 숨 고르는 시간")]
    [SerializeField] private float settleSeconds = 0.5f;

    [SerializeField] private float reflectionFadeSeconds = 2.5f;
    [SerializeField] private float walkSeconds = 3f;

    [Tooltip("걸음을 멈춤 자세로 되돌리는 시간")]
    [SerializeField] private float walkStopSeconds = 0.35f;

    [Tooltip("멈춘 뒤 Animator 정지. idle 흔들림까지 없애 굳은 채로 서 있음")]
    [SerializeField] private bool freezeAfterWalk = true;

    [Tooltip("얼어붙을 애니메이션 프레임(0~1). 자세가 어색하면 조금씩 옮겨 찾을 것")]
    [Range(0f, 1f)]
    [SerializeField] private float freezeNormalizedTime = 0f;

    [Tooltip("눈 확대 길이. 재생 시 Brain 의 Default Blend 길이로 반영됨.\n" +
             "확대의 모양(천천히 하다가 확)은 Brain 의 Custom 커브에서 조절할 것")]
    [SerializeField] private float eyeZoomSeconds = 3.5f;

    [Tooltip("암전 길이. 눈 확대 끝에 맞물리도록 뒤에서부터 겹쳐 시작")]
    [SerializeField] private float blackoutSeconds = 0.4f;

    [SerializeField] private float holdBlackSeconds = 1f;

    [Header("Look")]
    [Tooltip("연출 시작 시점의 반사 세기. 0 이면 첫 순간에 아무것도 안 비침")]
    [Range(0f, 1f)]
    [SerializeField] private float startReflectivity = 0.12f;

    [SerializeField] private AnimationCurve reflectionEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve walkEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Debug")]
    [Tooltip("연출 확인용. 정식 빌드 전에 None 으로 둘 것")]
    [SerializeField] private KeyCode debugKey = KeyCode.None;

    /// 컷신 vcam 우선순위. CM2 는 vcam 채널이 없어 3D-6 퍼즐 vcam(10)보다 위여야 함
    private const int MirrorShotPriority = 100;
    private const int EyeShotPriority    = 110;

    private static readonly int InputYHash = Animator.StringToHash("InputY");

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();

    private Coroutine routine;

    /// 연출 동안 끈 PlayerAnimation. 액터가 실제 플레이어일 때만 잡힘
    private PlayerAnimation suppressedAnimation;

    /// hideDuringCutscene 의 연출 직전 활성 상태. null 이면 아직 숨긴 적 없음
    private bool[] hiddenPriorState;

    /// Animator 를 얼리기 전 speed. null 이면 안 얼림
    private float? frozenAnimatorSpeed;

    /// <summary>연출이 끝난 시점. Stage 증가 · 씬 로드가 물릴 자리</summary>
    public event Action Finished;

    public bool IsPlaying => routine != null;

    private void Update()
    {
        if (debugKey != KeyCode.None && Input.GetKeyDown(debugKey)) Play();
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        Restore();
    }

    // ---------- 외부 구동 ----------

    /// <summary>연출 재생. 재생 중에 다시 불러도 무시.</summary>
    [ContextMenu("TEST - Play")]
    public void Play()
    {
        if (IsPlaying) return;

        if (!Application.isPlaying)
        {
            Debug.LogWarning("[StageChange3D] 플레이 모드에서만 동작", this);
            return;
        }

        routine = StartCoroutine(Sequence());
    }

    // ---------- 연출 ----------

    private IEnumerator Sequence()
    {
        BeginCutscene();

        // 1. glow 끄기
        if (glow != null) glow.FadeOut(glowFadeSeconds);
        yield return new WaitForSeconds(glowFadeSeconds);

        // 2. 카메라 · 액터 배치 전환
        SwitchToCutsceneView();
        yield return new WaitForSeconds(settleSeconds);

        // 3. 반사 켜기
        if (mirrorReflection != null)
        {
            mirrorReflection.SetSourceCamera(cutsceneCamera);
            mirrorReflection.Enable(true);
            mirrorReflection.Reflectivity = startReflectivity;
        }

        // 4. 반사가 선명해지는 동안 액터가 거울 앞으로 걸어옴
        Coroutine walk = StartCoroutine(WalkRoutine());

        if (mirrorReflection != null)
        {
            yield return Animate(reflectionFadeSeconds, reflectionEase,
                t => mirrorReflection.Reflectivity = Mathf.Lerp(startReflectivity, 1f, t));
        }

        yield return walk;

        // 5. 눈 확대. 실제 카메라 움직임은 Brain 의 블렌드가 만듦
        ApplyZoomBlendTime();
        if (vcamEyeCloseUp != null) vcamEyeCloseUp.Priority = EyeShotPriority;

        yield return new WaitForSeconds(Mathf.Max(0f, eyeZoomSeconds - blackoutSeconds));

        // 6. 암전. 눈동자가 화면을 채우는 순간과 맞물림
        if (blackout != null)
        {
            blackout.gameObject.SetActive(true);
            yield return Animate(blackoutSeconds, AnimationCurve.Linear(0f, 0f, 1f, 1f),
                t => blackout.alpha = t);
        }

        yield return new WaitForSeconds(holdBlackSeconds);

        routine = null;
        Finished?.Invoke();
    }

    /// 시작 지점에서 거울 앞까지 걷고 멈춤
    private IEnumerator WalkRoutine()
    {
        if (actor == null || actorStart == null || actorMirrorFront == null) yield break;

        Vector3 fromPos = actorStart.position;
        Vector3 toPos = actorMirrorFront.position;
        Quaternion fromRot = actorStart.rotation;
        Quaternion toRot = actorMirrorFront.rotation;

        // PlayerAnimation 이 꺼진 상태라 블렌드 트리를 직접 몬다
        if (actorAnimator != null) actorAnimator.SetFloat(InputYHash, 1f);

        yield return Animate(walkSeconds, walkEase, t =>
        {
            actor.SetPositionAndRotation(
                Vector3.Lerp(fromPos, toPos, t),
                Quaternion.Slerp(fromRot, toRot, t));
        });

        if (actorAnimator == null) yield break;

        // 걸음을 뚝 끊으면 발이 미끄러진 것처럼 보임
        yield return Animate(walkStopSeconds, AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
            t => actorAnimator.SetFloat(InputYHash, Mathf.Lerp(1f, 0f, t)));

        if (!freezeAfterWalk) yield break;

        // 지정 프레임으로 되감고 정지. 위상이 프레임률을 타서 눈 위치가 튀는 것 방지
        AnimatorStateInfo state = actorAnimator.GetCurrentAnimatorStateInfo(0);
        actorAnimator.Play(state.fullPathHash, 0, freezeNormalizedTime);
        actorAnimator.Update(0f);

        frozenAnimatorSpeed = actorAnimator.speed;
        actorAnimator.speed = 0f;
    }

    /// 입력 · UI 를 연출 상태로
    private void BeginCutscene()
    {
        playerControlLock.Lock(this, inputToLock);

        // Lock 이 커서를 강제로 보이게 함. 복귀는 Unlock 담당
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        hiddenPriorState = new bool[hideDuringCutscene.Length];

        for (int i = 0; i < hideDuringCutscene.Length; i++)
        {
            GameObject go = hideDuringCutscene[i];
            if (go == null) continue;

            hiddenPriorState[i] = go.activeSelf;
            go.SetActive(false);
        }

        if (blackout != null)
        {
            blackout.alpha = 0f;
            blackout.gameObject.SetActive(false);
        }
    }

    /// 플레이어 시점에서 컷신 리그로
    private void SwitchToCutsceneView()
    {
        if (playerCamera != null) playerCamera.enabled = false;
        if (playerListener != null) playerListener.enabled = false;

        SuppressPlayerAnimation();

        // 리그를 켜기 전에 잡아둬야 첫 프레임부터 거울 샷
        if (vcamMirror != null) vcamMirror.Priority = MirrorShotPriority;
        if (vcamEyeCloseUp != null) vcamEyeCloseUp.Priority = 0;

        if (actor != null && actorStart != null)
            actor.SetPositionAndRotation(actorStart.position, actorStart.rotation);

        if (cutsceneRig != null) cutsceneRig.SetActive(true);
        if (cutsceneCamera != null) cutsceneCamera.enabled = true;
        if (cutsceneListener != null) cutsceneListener.enabled = true;
    }

    /// eyeZoomSeconds 를 Brain 의 블렌드 길이로 반영. 모양(커브)은 Brain 것을 씀
    private void ApplyZoomBlendTime()
    {
        CinemachineBrain brain = cutsceneCamera != null
            ? cutsceneCamera.GetComponent<CinemachineBrain>()
            : null;

        if (brain == null) return;

        if (brain.m_DefaultBlend.m_Style == CinemachineBlendDefinition.Style.Cut)
        {
            Debug.LogWarning("[StageChange3D] Brain 의 Default Blend 가 Cut — 눈 확대가 즉시 끝남. " +
                             "Ease In Out 이나 Custom 으로 바꿀 것", this);
            return;
        }

        CinemachineBlendDefinition blend = brain.m_DefaultBlend;
        blend.m_Time = eyeZoomSeconds;
        brain.m_DefaultBlend = blend;
    }

    /// 액터가 실제 플레이어일 때 InputY 를 매 프레임 덮어쓰는 PlayerAnimation 차단.
    /// PlayerControlLock 은 입력 컴포넌트만 다룸
    private void SuppressPlayerAnimation()
    {
        if (actorAnimator == null) return;

        PlayerAnimation animation = actorAnimator.GetComponent<PlayerAnimation>();
        if (animation == null || !animation.enabled) return;

        animation.enabled = false;
        suppressedAnimation = animation;
    }

    /// 연출 전 상태로. 중간에 꺼져도 씬이 잠기지 않도록
    private void Restore()
    {
        if (frozenAnimatorSpeed.HasValue)
        {
            if (actorAnimator != null) actorAnimator.speed = frozenAnimatorSpeed.Value;
            frozenAnimatorSpeed = null;
        }

        // 남겨두면 PlayerAnimation 이 켜질 때까지 걷는 자세로 서 있음
        if (actorAnimator != null) actorAnimator.SetFloat(InputYHash, 0f);

        if (suppressedAnimation != null)
        {
            suppressedAnimation.enabled = true;
            suppressedAnimation = null;
        }

        if (mirrorReflection != null) mirrorReflection.Enable(false);

        if (cutsceneListener != null) cutsceneListener.enabled = false;
        if (cutsceneCamera != null) cutsceneCamera.enabled = false;
        if (cutsceneRig != null) cutsceneRig.SetActive(false);

        if (vcamMirror != null) vcamMirror.Priority = 0;
        if (vcamEyeCloseUp != null) vcamEyeCloseUp.Priority = 0;

        if (playerCamera != null) playerCamera.enabled = true;
        if (playerListener != null) playerListener.enabled = true;

        // 숨긴 적 없으면 건드리지 않음. 씬이 내려갈 때 엉뚱하게 켜지지 않도록
        if (hiddenPriorState != null)
        {
            int count = Mathf.Min(hideDuringCutscene.Length, hiddenPriorState.Length);

            for (int i = 0; i < count; i++)
            {
                GameObject go = hideDuringCutscene[i];
                if (go != null) go.SetActive(hiddenPriorState[i]);
            }

            hiddenPriorState = null;
        }

        if (blackout != null)
        {
            blackout.alpha = 0f;
            blackout.gameObject.SetActive(false);
        }

        playerControlLock.Unlock();
    }

    /// 이동 공용 보간
    private IEnumerator Animate(float duration, AnimationCurve ease, Action<float> apply)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            apply(ease.Evaluate(elapsed / duration));
            yield return null;
        }

        // 커브 끝값과 무관하게 목표 지점으로 정확히 스냅
        apply(1f);
    }
}
