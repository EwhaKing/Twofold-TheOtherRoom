using UnityEngine;

public class MirrorObtainNoticeTrigger : MonoBehaviour
{
    [Header("자막 설정")]
    [TextArea]
    [SerializeField] private string noticeText = "거울을 얻었다. 거울 프레임에 맞춰보자";

    private bool isQuitting = false;

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    // MirrorPiece가 거울을 끄는(SetActive false) 순간 자동 발동
    private void OnDisable()
    {
        if (isQuitting) return;
        if (!gameObject.scene.isLoaded) return;   // 씬 언로드
        if (transform.parent == null || !transform.parent.gameObject.activeInHierarchy) return;   // 패널째 꺼짐

        if (NoticeSubtitle.Main == null)
        {
            Debug.LogWarning("[Notice] 씬에 NoticeSubtitle Main 없음 (CommonCanvas 확인)", this);
            return;
        }

        NoticeSubtitle.Main.Show(noticeText);
    }
}
