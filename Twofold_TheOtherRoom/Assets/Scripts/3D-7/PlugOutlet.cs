using UnityEngine;

// Plug가 Outlet collider에 들어오고 나갈 때 Plug Controller에게 알림
// Outlet에 Plug 꽂혀있는지 관리
public class PlugOutlet : MonoBehaviour
{
    [SerializeField] Transform dockPoint;
    [SerializeField] float insertDepth = 0.05f; // dockPoint에서 들어가는 깊이

    public Vector3 DockPosition => dockPoint.position;
    public Quaternion DockRotation => dockPoint.rotation;
    public Vector3 InsertPosition => dockPoint.position + dockPoint.up * insertDepth;

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
