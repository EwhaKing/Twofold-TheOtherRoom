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
        if (NoticeSubtitle.Instance == null)
        {
            Debug.LogWarning("[Notice] 씬에 NoticeCanvas 없음", this);
            return;
        }

        NoticeSubtitle.Instance.Show(message);
    }
}
