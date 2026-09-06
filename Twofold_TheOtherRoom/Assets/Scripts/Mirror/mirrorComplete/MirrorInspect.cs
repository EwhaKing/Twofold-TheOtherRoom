using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 완성 거울을 보는 진입점. 2D는 클릭, 3D는 E키.
///
/// 2D는 합본 Image 에 직접 붙일 것 — UGUI 는 가장 가까운 핸들러 하나에만 이벤트를 주므로
/// 여기서 막지 않으면 부모 display_mirror 의 DetailView 가 확대본을 다시 연다.
/// </summary>
public class MirrorInspect : MonoBehaviour, IPointerClickHandler, IInteractable
{
    [SerializeField] private MirrorCompletionPresenter presenter;

    public void OnPointerClick(PointerEventData eventData) => presenter?.RequestInspect();

    public void Interact() => presenter?.RequestInspect();
}
