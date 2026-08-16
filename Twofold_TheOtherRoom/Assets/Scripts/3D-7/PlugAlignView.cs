using UnityEngine;
using UnityEngine.UI;

public class PlugAlignView : MonoBehaviour
{
    [SerializeField] Camera renderCamera;
    [SerializeField] RawImage view;
    [SerializeField] float xOffset = 0f;

    void Awake()
    {
        Hide();
    }

    public void Show(PlugOutlet outlet)
    {
        MoveToOutlet(outlet);

        renderCamera.enabled = true;
        view.enabled = true;
    }

    // 카메라 부모 공간에서 outlet이 늘어선 축(x)만 맞춤
    // 월드 x로 계산하면 부모가 회전/축소된 배치에서 xOffset의 방향과 크기가 어긋남
    private void MoveToOutlet(PlugOutlet outlet)
    {
        Transform space = renderCamera.transform.parent;

        Vector3 local = ToSpace(space, renderCamera.transform.position);
        local.x = ToSpace(space, outlet.DockPosition).x + xOffset;

        renderCamera.transform.position = FromSpace(space, local);
    }

    private static Vector3 ToSpace(Transform space, Vector3 world) => space ? space.InverseTransformPoint(world) : world;
    private static Vector3 FromSpace(Transform space, Vector3 local) => space ? space.TransformPoint(local) : local;

    public void Hide()
    {
        renderCamera.enabled = false;
        view.enabled = false;
    }
}
