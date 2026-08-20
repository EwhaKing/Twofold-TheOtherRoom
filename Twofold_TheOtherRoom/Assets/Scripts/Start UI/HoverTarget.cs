using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 선택 버튼에 붙여서 hover 시 공용 아이콘을 자기 옆으로 불러옴
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class HoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Choice Icon")]
    [Tooltip("이 버튼에 뜰 때의 아이콘 배율. 작은 버튼이면 낮출 것")]
    [SerializeField] float iconScale = 1f;

    HoverIndicator indicator;
    RectTransform rect;
    Button button;

    void Awake()
    {
        rect = (RectTransform)transform;
        button = GetComponent<Button>();
        indicator = GetComponentInParent<HoverIndicator>(true);
    }

    // 화면이 꺼지거나 버튼이 잠길 때 아이콘이 남는 것 방지
    void OnDisable()
    {
        if (indicator != null)
            indicator.Hide(rect);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (indicator == null) return;
        if (button != null && !button.interactable) return;   // 접속 중 잠긴 버튼은 무시

        indicator.Show(rect, iconScale);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (indicator != null)
            indicator.Hide(rect);
    }
}
