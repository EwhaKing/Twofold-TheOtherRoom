using UnityEngine;

// Plug가 Outlet collider에 들어오고 나갈 때 Plug Controller에게 알림
// Outlet에 Plug 꽂혀있는지 관리
public class PlugOutlet : MonoBehaviour
{
    [SerializeField] Transform dockPoint;
    [SerializeField] Transform insertPoint; // 꽂혔을 때 plug가 놓일 자리
    [SerializeField] Transform rejectPoint; // 방향이 안 맞아 걸렸을 때 자리

    public Vector3 DockPosition => dockPoint.position;
    public Quaternion DockRotation => dockPoint.rotation;
    // 위치만 읽으므로 두 point의 회전은 무의미. 삽입은 순수 평행이동이고 회전은 dockPoint가 소유
    public Vector3 InsertPosition => insertPoint.position;
    public Vector3 RejectPosition => rejectPoint.position;

    PlugController occupant;

    #region Outlet Collider In/Out Notify to Plug Controller
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlugController>(out var plug))
        {
            plug.NotifyNear(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlugController>(out var plug))
        {
            plug.NotifyLeave(this);
        }
    }
    #endregion

    #region Plug Occupation
    public bool TryOccupy(PlugController plug)
    {
        if(occupant != null) return false;
        occupant = plug;
        return true;
    }

    public void Release(PlugController plug)
    {
        if(occupant != plug) return;
        occupant = null;
    }
    #endregion
}
