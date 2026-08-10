using UnityEngine;

public class BackButton : MonoBehaviour
{
    [Header("Target Panel to Close")]
    public GameObject targetPanel; // 닫을 패널 오브젝트

    // UI Button OnClick에 연결
    public void ClosePanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }
        else
        {
            // 지정 안 했으면 해당 버튼이 속한 최상위 패널 닫기
            gameObject.SetActive(false);
        }
    }
}