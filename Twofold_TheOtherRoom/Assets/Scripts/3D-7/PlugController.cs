using System;
using UnityEngine;

// 각 plug 회전, 꽂힌 것 다시 뽑는 담당
public class PlugController : MonoBehaviour
{
    // Free: 케이블에 매달려 흔들림 / Dragging: 마우스로 끄는 중
    // Docked: outlet 앞에 붙어서 우클릭으로 회전 가능 / Inserted: 방향이 맞아 꽂힘
    public enum PlugState { Free, Dragging, Docked, Inserted }

    [SerializeField][Range(1, 3)] int dockRotationIndex = 2;

    public PlugState State { get; private set; } = PlugState.Free;
    public PlugOutlet CurrentOutlet => currentOutlet;
    public event Action<PlugController> OnInserted;

    int rotationIndex;
    DragAndDrop3D currentDrag;
    PlugOutlet currentOutlet;
    Rigidbody plugRigid;

    void Awake()
    {
        currentDrag = GetComponent<DragAndDrop3D>();
        plugRigid = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        currentDrag.OnPickUp += HandlePickUp;
        currentDrag.OnRelease += HandleRelease;
    }

    void OnDisable()
    {
        currentDrag.OnPickUp -= HandlePickUp;
        currentDrag.OnRelease -= HandleRelease;
    }

    void OnMouseOver()
    {
        // 마우스가 올라간 plug만, 도킹 상태일 때만 우클릭으로 회전
        if (State == PlugState.Docked && Input.GetMouseButtonDown(1)) Rotate();
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
    #endregion

    #region Plug Rotate/Insert
    private void Rotate()
    {
        if (currentOutlet == null) return;

        rotationIndex = (rotationIndex + 1) % 4;
        ApplyDockRotation();

        // 0번 칸이 outlet과 정렬된 방향 -> 그대로 꽂힘
        if (rotationIndex == 0) SetState(PlugState.Inserted);
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
                rotationIndex = dockRotationIndex;
                transform.position = currentOutlet.DockPosition;
                ApplyDockRotation();
                break;

            case PlugState.Inserted:
                // 회전은 이미 정렬돼 있으므로 위치만 이동. 추후 애니메이션 및 효과음
                transform.position = currentOutlet.InsertPosition;
                OnInserted?.Invoke(this);
                break;
        }
    }
    #endregion
}
