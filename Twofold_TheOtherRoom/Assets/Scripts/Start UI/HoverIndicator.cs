using UnityEngine;

/// <summary>
/// 선택 버튼에 hover할 때 옆에 뜨는 아이콘 하나를 공유, 옮겨 씀
/// 화면 캔버스 루트에 붙이면 자식인 HoverTarget들이 찾아 씀
/// </summary>
public class HoverIndicator : MonoBehaviour
{
    [Header("Select Icon")]
    [Tooltip("Raycast Target은 꺼둘 것")]
    [SerializeField] RectTransform icon;

    [Header("Offset")]
    [Tooltip("x가 음수면 버튼 왼쪽에 배치됨")]
    [SerializeField] Vector2 offset = new Vector2(-40f, 0f);

    /// 지금 아이콘이 붙어 있는 버튼
    RectTransform current;

    void Awake()
    {
        if (icon != null)
            icon.gameObject.SetActive(false);
    }

    /// scale은 HoverTarget이 지정. 작은 버튼용
    public void Show(RectTransform target, float scale)
    {
        if (icon == null || target == null) return;

        current = target;
        icon.localScale = Vector3.one * scale;
        icon.position = target.position;
        // 아이콘이 작아진 만큼 버튼에 붙게
        icon.anchoredPosition += offset * scale;
        icon.gameObject.SetActive(true);
    }

    public void Hide(RectTransform target)
    {
        // 이미 다른 버튼으로 옮겨갔으면 무시 (Exit가 Enter보다 늦게 오는 경우)
        if (icon == null || current != target) return;

        current = null;
        icon.gameObject.SetActive(false);
    }
}
