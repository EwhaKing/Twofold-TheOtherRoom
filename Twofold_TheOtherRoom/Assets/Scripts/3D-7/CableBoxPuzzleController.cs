using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
   케이블 박스 퍼즐 (3D-7)
   E키로 진입 -> 액자 클릭해서 열기 -> plug 5개를 정답 outlet에 꽂기 -> CommonCanvas 뒤로가기로 나감

   [블록 콜라이더]
   콜라이더 하나가 세 역할 담당
   - E 상호작용 대상 / 밖에서 케이블 드래그 차단 / 액자 클릭 대상
   꺼지는 건 "진입 + 액자 열림 + 미해결" 한 경우뿐. (SetInteractable)
   그래서 정답 이후엔 자동으로 다시 켜지고 plug가 잠김
   조작 안내 UI(guideUI)도 같은 조건으로 켜고 끔

   [progress]
   Closed -> Opened -> Solved 단방향. 
   액자는 한번 열면 열린 상태 유지하므로 Exit()에서 리셋X

   [frameTransform]
   회전할 액자 메시
*/

public class CableBoxPuzzleController : MonoBehaviour, IInteractable, ICloseInspection
{
    [Header("Puzzle ID")]
    [SerializeField] string puzzleId = "3D-7";
    [SerializeField] PuzzleDimension dimension = PuzzleDimension.ThreeD;

    [Header("Cameras")]
    [SerializeField] Camera playerCamera;
    [SerializeField] Camera puzzleCamera;

    [Header("Align View")]
    [SerializeField] PlugAlignView alignView;

    [Header("Guide UI")]
    [Tooltip("드래그/클릭 조작 안내. 액자가 열려 plug를 만질 수 있는 동안만 켜짐")]
    [SerializeField] GameObject guideUI;

    [Header("Frame")]
    [Tooltip("Frame Mesh")]
    [SerializeField] Transform frameTransform;
    [Tooltip("액자가 열린 상태의 로컬 Y 회전량")]
    [SerializeField] float openAngleY = 90f;
    [SerializeField] float openDuration = 0.6f;
    [SerializeField] AnimationCurve openEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Door")]
    [Tooltip("정답 후 열릴 금고 아래 문")]
    [SerializeField] Transform doorTransform;
    [Tooltip("열린 위치까지의 로컬 이동량")]
    [SerializeField] Vector3 doorOpenLocalOffset = new Vector3(-0.3f, 0f, 0f);
    [SerializeField] float doorDuration = 0.8f;
    [SerializeField] AnimationCurve doorEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Serializable]
    struct PlugAnswer
    {
        public PlugController plug;
        public PlugOutlet outlet;
    }
    [Header("Answer")]
    [SerializeField] PlugAnswer[] answers;

    [Header("Plugs")]
    [Tooltip("퍼즐이 관리하는 plug 전체. 비워두면 answers에서 자동으로 채움")]
    [SerializeField] PlugController[] plugs;

    [Header("Player Lock")]
    [Tooltip("PlayerController, PlayerLocomotionInput, PlayerInteractor를 자동으로 찾음")]
    [SerializeField] Behaviour[] behavioursToDisable;

    readonly PlayerControlLock playerControlLock = new PlayerControlLock();

    bool isEntered;

    Collider blockCollider;

    enum Progress {Closed, Opened, Solved}
    Progress progress = Progress.Closed;

    // frame 회전 관련
    Quaternion frameClosedRotation;
    Quaternion frameOpenedRotation;
    Coroutine frameRoutine;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        blockCollider = GetComponent<Collider>();

        if (frameTransform != null)
        {
            // 닫힘 자세를 기준으로 로컬 Y축 기준 회전
            frameClosedRotation = frameTransform.localRotation;
            frameOpenedRotation = frameClosedRotation * Quaternion.Euler(0f, openAngleY, 0f);
        }

        if (puzzleCamera != null)
            puzzleCamera.enabled = false;

        BuildPlugList();
    }

    private void OnEnable()
    {
        foreach (var plug in plugs) plug.OnPlugStateChanged += HandlePlugStateChanged;
    }

    private void OnDisable()
    {
        foreach (var plug in plugs) plug.OnPlugStateChanged -= HandlePlugStateChanged;
    }

    // plugs를 인스펙터에서 비워두면 answers에 등록된 plug로 채움
    private void BuildPlugList()
    {
        if (plugs != null && plugs.Length > 0) return;

        List<PlugController> found = new();
        foreach (var a in answers)
        {
            if (a.plug != null && !found.Contains(a.plug)) found.Add(a.plug);
        }

        plugs = found.ToArray();
    }

    #region Puzzle Enter/Exit
    public void Interact()
    {
        if (progress != Progress.Solved && !isEntered) // 안 풀렸고, 들어간 상태가 아닐 때만 E키로 진입 가능
            Enter();
    }

    private void Enter()
    {
        // 커서 상태 저장·해제까지 Lock이 담당
        playerControlLock.Lock(this, behavioursToDisable);

        if (playerCamera != null)
            playerCamera.enabled = false;
        if (puzzleCamera != null)
            puzzleCamera.enabled = true;

        isEntered = true;
        SetInteractable();

        // 공용 뒤로가기 UI
        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Show(this);
    }

    private void Exit()
    {
        if (!isEntered) return;

        // Frame 여는 도중 나간 경우, 코루틴 중단/열린 상태로 스냅
        if (frameRoutine != null)
        {
            StopFrameRoutine();
            frameTransform.localRotation = frameOpenedRotation;
        }

        UndockAll(); // 나갈 때 Docked 풂, Inserted는 유지

        if (alignView != null)
            alignView.Hide();

        if (puzzleCamera != null)
            puzzleCamera.enabled = false;
        if (playerCamera != null)
            playerCamera.enabled = true;

        playerControlLock.Unlock();

        isEntered = false;
        SetInteractable();

        if (InspectionUIController.Instance != null)
            InspectionUIController.Instance.Hide(this);
    }

    /// CommonCanvas 뒤로가기 버튼이 부름
    public void CloseInspection()
    {
        Exit();
    }
    #endregion

    #region Plug State
    private void HandlePlugStateChanged(PlugController changed)
    {
        EnforceSingleDock(changed);
        UpdateAlignView(changed);
        CheckAnswer();
    }

    // Docked인 plug가 있을 때 정렬 뷰 활성화
    private void UpdateAlignView(PlugController changed)
    {
        if (alignView == null) return;

        if (changed.State == PlugController.PlugState.Docked && changed.CurrentOutlet != null)
            alignView.Show(changed);
        else
            alignView.Hide();
    }
    private void UndockAll()
    {
        foreach (var plug in plugs) plug.Undock();
    }

    // Docked는 항상 최대 하나. 다른 plug를 집는 순간 도킹돼 있던 건 풀려서 다시 매달림
    // Undock()이 상태 변경 이벤트를 다시 쏘긴하는데 Free라 여기서 바로 빠져나감
    private void EnforceSingleDock(PlugController changed)
    {
        if (changed.State != PlugController.PlugState.Dragging) return;

        foreach (var plug in plugs)
        {
            if (plug == changed) continue;
            if (plug.State == PlugController.PlugState.Docked) plug.Undock();
        }
    }
    #endregion

    #region Check Answer
    bool IsAllCorrect()
    {
        foreach (var a in answers)
            if (a.plug.State != PlugController.PlugState.Inserted || a.plug.CurrentOutlet != a.outlet)
                return false;
        return true;
    }

    private void CheckAnswer()
    {
        if (!IsAllCorrect()) return;
        progress = Progress.Solved;
        SoundManager.Instance.PlaySFX(SFXType.SteppingCorrect);
        SetInteractable();

        if (doorTransform != null) StartCoroutine(OpenDoorRoutine());

        if (PuzzleManager.Instance != null) PuzzleManager.Instance.ReportSolved(puzzleId, dimension);
    }
    #endregion


    private void SetInteractable()
    {
        bool interactable = isEntered && progress == Progress.Opened;

        if (blockCollider != null) blockCollider.enabled = !interactable;
        if (guideUI != null) guideUI.SetActive(interactable);
    }

    #region Frame Open
    void OnMouseDown()
    {
        if (!isEntered || progress != Progress.Closed) return;
        OpenFrame();
    }

    private void OpenFrame()
    {
        progress = Progress.Opened;

        StopFrameRoutine();
        frameRoutine = StartCoroutine(OpenFrameRoutine());
    }

    private IEnumerator OpenFrameRoutine()
    {
        Quaternion from = frameTransform.localRotation;

        yield return Animate(openDuration, openEase,
            t => frameTransform.localRotation = Quaternion.Slerp(from, frameOpenedRotation, t));

        frameRoutine = null;
        OnFrameOpened();
    }

    private void OnFrameOpened()
    {
        // 액자가 다 열리고 나서 케이블 클릭/드래그를 허용
        SetInteractable();
    }

    private void StopFrameRoutine()
    {
        if (frameRoutine == null) return;

        StopCoroutine(frameRoutine);
        frameRoutine = null;
    }
    #endregion

    #region Door Open
    private IEnumerator OpenDoorRoutine()
    {
        Vector3 from = doorTransform.localPosition;
        Vector3 to = from + doorOpenLocalOffset;

        yield return Animate(doorDuration, doorEase,
            t => doorTransform.localPosition = Vector3.Lerp(from, to, t));
    }
    #endregion

    #region Animation
    // 이동/회전 공용 보간
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
    #endregion
}
