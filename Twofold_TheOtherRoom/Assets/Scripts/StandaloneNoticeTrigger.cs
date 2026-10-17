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
        if (NoticeSubtitle.Main == null)
        {
            Debug.LogWarning("[Notice] 씬에 NoticeSubtitle Main 없음 (CommonCanvas 확인)", this);
            return;
        }

        NoticeSubtitle.Main.Show(noticeText);
    }
}
