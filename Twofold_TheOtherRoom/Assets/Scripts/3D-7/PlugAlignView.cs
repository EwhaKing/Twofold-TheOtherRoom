using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// outlet 확대뷰. plug에 직접 우클릭하는 대신 뷰에서 회전시키는 경로
public class PlugAlignView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Camera renderCamera;
    [SerializeField] RawImage view;
    [SerializeField] float xOffset = 0f;

    // 지금 뷰에 비치는 plug. 우클릭 대상
    PlugController currentPlug;

    void Awake()
    {
        Hide();
    }

    public void Show(PlugController plug)
    {
        currentPlug = plug;
        MoveToOutlet(plug.CurrentOutlet);

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
        currentPlug = null;

        renderCamera.enabled = false;
        view.enabled = false;
    }

    // 좌클릭 삽입, 우클릭 회전. 가능 여부 판단은 plug가 함
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentPlug == null) return;

        if (eventData.button == PointerEventData.InputButton.Left) currentPlug.TryInsert();
        else if (eventData.button == PointerEventData.InputButton.Right) currentPlug.Rotate();
    }
}
