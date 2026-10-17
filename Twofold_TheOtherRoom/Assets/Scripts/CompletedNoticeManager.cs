using UnityEngine;

public class CompletedNoticeManager : MonoBehaviour
{
    public static CompletedNoticeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 이미 완료된 퍼즐 클릭 시 호출
    public void ShowNotice(string message = "이미 푼 퍼즐이다")
    {
        if (NoticeSubtitle.Main == null)
        {
            Debug.LogWarning("[Notice] 씬에 NoticeSubtitle Main 없음 (CommonCanvas 확인)", this);
            return;
        }

        NoticeSubtitle.Main.Show(message);
    }
}
