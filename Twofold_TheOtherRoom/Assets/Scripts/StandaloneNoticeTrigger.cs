using UnityEngine;
using UnityEngine.EventSystems;

public class StandaloneNoticeTrigger : MonoBehaviour, IPointerClickHandler
{
    [Header("띄울 문구 설정")]
    [TextArea]
    [SerializeField] private string noticeText = "이것은 작동하지 않는 것 같다";

    // UI Image 클릭 시
    public void OnPointerClick(PointerEventData eventData)
    {
        ShowNotice();
    }

    // 2D 콜라이더 클릭 시
    private void OnMouseDown()
    {
        ShowNotice();
    }

    public void ShowNotice()
    {
        if (NoticeSubtitle.Instance == null)
        {
            Debug.LogWarning("[Notice] 씬에 NoticeCanvas 없음", this);
            return;
        }

        NoticeSubtitle.Instance.Show(noticeText);
    }
}
