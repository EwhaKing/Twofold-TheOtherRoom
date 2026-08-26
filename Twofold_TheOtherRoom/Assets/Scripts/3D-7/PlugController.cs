using System;
using System.Collections;
using UnityEngine;

// 각 plug 회전, 꽂힌 것 다시 뽑는 담당
public class PlugController : MonoBehaviour
{
    // Free: 케이블에 매달려 흔들림 / Dragging: 마우스로 끄는 중
    // Docked: outlet 앞에 붙어서 우클릭으로 회전 가능 / Inserted: 방향이 맞아 꽂힘
    public enum PlugState { Free, Dragging, Docked, Inserted }
    public PlugState State { get; private set; } = PlugState.Free;
    public PlugOutlet CurrentOutlet => currentOutlet;
    public event Action<PlugController> OnPlugStateChanged;

    [SerializeField] float rejectDuration = 0.16f; // 걸렸다가 되돌아오는 데 걸리는 전체 시간

    int rotationIndex;
    DragAndDrop3D currentDrag;
    PlugOutlet currentOutlet;
    Rigidbody plugRigid;
    Coroutine rejectRoutine;

    void Awake()
    {
        currentDrag = GetComponent<DragAndDrop3D>();
        plugRigid = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        currentDrag.OnPickUp += HandlePickUp;
        currentDrag.OnRelease += HandleRelease;
        currentDrag.OnClick += HandleClick;
    }

    void OnDisable()
    {
        currentDrag.OnPickUp -= HandlePickUp;
        currentDrag.OnRelease -= HandleRelease;
        currentDrag.OnClick -= HandleClick;
    }

    void OnMouseOver()
    {
        // 마우스가 올라간 plug만 우클릭으로 회전
        if (Input.GetMouseButtonDown(1)) Rotate();
    }

    #region Outlet Collider In/Out
    public void NotifyNear(PlugOutlet outlet)
    {
        if (State == PlugState.Docked || State == PlugState.Inserted) return;
        currentOutlet = outlet;
    }

    public void NotifyLeave(PlugOutlet outlet)
    {
        if (State == PlugState.Docked || State == PlugState.Inserted) return;
        if (currentOutlet == outlet) currentOutlet = null;
    }
    #endregion

    #region Plug Pick/Release
    private void HandlePickUp()
    {
        // 집으면 무조건 풀림
        currentOutlet?.Release(this);
        SetState(PlugState.Dragging);
    }

    private void HandleRelease()
    {
        // 드래그를 놓았을 때 outlet collider 안이면 도킹, 아니면 다시 매달림
        bool canDock = currentOutlet != null && currentOutlet.TryOccupy(this);
        SetState(canDock ? PlugState.Docked : PlugState.Free);
    }

    private void HandleClick()
    {
        // 드래그X, 클릭O = insert 시도
        TryInsert();
    }

    // 다른 plug를 집었을 때 기존 dock 해제로 항상 docked 최대 1개 유지
    public void Undock()
    {
        if (State != PlugState.Docked) return;

        if (currentOutlet != null) currentOutlet.Release(this);
        SetState(PlugState.Free);
    }
    #endregion

    #region Plug Rotate/Insert
    // plug 직접 우클릭 / 확대뷰 우클릭 공용
    public void Rotate()
    {
        if (State != PlugState.Docked || currentOutlet == null) return;

        rotationIndex = (rotationIndex + 1) % 4;
        ApplyDockRotation();
    }

    // plug 직접 클릭 / 확대뷰 클릭 공용
    public void TryInsert()
    {
        if (State != PlugState.Docked || currentOutlet == null) return;

        // 방향이 안 맞으면 걸려서 도로 나옴. Docked 유지
        if (rotationIndex != 0)
        {
            RejectInsert();
            return;
        }

        SetState(PlugState.Inserted);
    }

    private void RejectInsert()
    {
        StopRejectRoutine();
        rejectRoutine = StartCoroutine(RejectRoutine());
    }

    private IEnumerator RejectRoutine()
    {
        Vector3 dockPosition = currentOutlet.DockPosition;
        Vector3 caughtPosition = currentOutlet.RejectPosition;
        float half = rejectDuration * 0.5f;

        // if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SFXType.???);

        yield return MovePosition(dockPosition, caughtPosition, half);
        yield return MovePosition(caughtPosition, dockPosition, half);

        rejectRoutine = null;
    }

    private IEnumerator MovePosition(Vector3 from, Vector3 to, float duration)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        transform.position = to;
    }

    // 드래그나 삽입으로 상태가 바뀌면 되돌아오는 중이던 연출 취소
    private void StopRejectRoutine()
    {
        if (rejectRoutine == null) return;
        StopCoroutine(rejectRoutine);
        rejectRoutine = null;
    }

    private int IndexFromCurrentRotation()
    {
        Quaternion local = Quaternion.Inverse(currentOutlet.DockRotation) * transform.rotation;
        return Mathf.RoundToInt(local.eulerAngles.y / 90f) % 4;
    }

    // 회전은 항상 outlet 기준으로 새로 계산
    private void ApplyDockRotation()
    {
        transform.rotation = currentOutlet.DockRotation * Quaternion.Euler(0, 90 * rotationIndex, 0);
    }
    #endregion

    #region State
    private void SetState(PlugState next)
    {
        StopRejectRoutine();
        State = next;

        switch (next)
        {
            case PlugState.Free:
                plugRigid.isKinematic = false;
                break;

            case PlugState.Dragging:
                plugRigid.isKinematic = true;
                break;

            case PlugState.Docked:
                plugRigid.isKinematic = true;
                rotationIndex = IndexFromCurrentRotation();
                transform.position = currentOutlet.DockPosition;
                ApplyDockRotation();
                break;

            case PlugState.Inserted:
                // 회전은 이미 정렬돼 있으므로 위치만 이동.
                transform.position = currentOutlet.InsertPosition;
                // if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SFXType.???);
                break;
        }
        
        OnPlugStateChanged?.Invoke(this);
    }
    #endregion
}
