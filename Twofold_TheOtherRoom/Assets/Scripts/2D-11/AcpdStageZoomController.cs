using System.Collections;
using UnityEngine;

public class AcpdStageZoomController : MonoBehaviour
{
    [Header("확대/축소 대상 그룹")]
    public RectTransform roomBackground; 
    public GameObject puzzleContainer;   

    [Header("방 요소들")]
    public GameObject wallPlaceholder;   
    public GameObject backButton;        

    [Header("게임 매니저 연결")]
    public AcpdStageGameManager gameManager; 

    [Header("줌 연출 설정")]
    public float zoomDuration = 0.4f;    
    public float targetScale = 3.5f;     

    private bool isZoomed = false;
    private Coroutine zoomCoroutine;

    private Vector3 originalBgPosition;
    private Vector3 originalBgScale;

    private void Start()
    {
        if (roomBackground != null)
        {
            originalBgPosition = roomBackground.anchoredPosition;
            originalBgScale = roomBackground.localScale;
        }

        if (puzzleContainer != null) puzzleContainer.SetActive(false);
        if (backButton != null) backButton.SetActive(false);
        if (wallPlaceholder != null) wallPlaceholder.SetActive(true);

        if (gameManager == null)
        {
            gameManager = GetComponent<AcpdStageGameManager>();
        }
    }

    public void ZoomIn()
    {
        if (isZoomed || roomBackground == null || wallPlaceholder == null) return;
        isZoomed = true;

        if (wallPlaceholder != null) wallPlaceholder.SetActive(false);

        Vector3 wallPos = wallPlaceholder.GetComponent<RectTransform>().anchoredPosition;
        Vector3 targetPos = -wallPos * targetScale; 

        StartZoomAnimation(targetPos, Vector3.one * targetScale, () => {
            if (puzzleContainer != null) puzzleContainer.SetActive(true);
            if (backButton != null) backButton.SetActive(true);

            // 🌟 퍼즐 확대 완료 직후에 첫 Alphabet(ACP) 표시 및 3초 시작!
            if (gameManager != null)
            {
                gameManager.InitPuzzleState();
            }
        });
    }

    public void ZoomOut()
    {
        if (!isZoomed || roomBackground == null) return;
        isZoomed = false;

        if (puzzleContainer != null) puzzleContainer.SetActive(false);
        if (backButton != null) backButton.SetActive(false);

        StartZoomAnimation(originalBgPosition, originalBgScale, () => {
            if (wallPlaceholder != null) wallPlaceholder.SetActive(true);
        });
    }

    private void StartZoomAnimation(Vector3 targetPos, Vector3 targetScale, System.Action onComplete)
    {
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(AnimateRoomZoom(targetPos, targetScale, onComplete));
    }

    private IEnumerator AnimateRoomZoom(Vector3 targetPos, Vector3 targetScale, System.Action onComplete)
    {
        Vector3 startPos = roomBackground.anchoredPosition;
        Vector3 startScale = roomBackground.localScale;
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration);

            roomBackground.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
            roomBackground.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        roomBackground.anchoredPosition = targetPos;
        roomBackground.localScale = targetScale;
        onComplete?.Invoke();
    }
}
