using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RugToggle : MonoBehaviour, IPointerClickHandler
{
    [Header("러그 이미지 설정")]
    public Sprite unfoldedRugSprite; // 펼쳐진 러그 (배치용_2)
    public Sprite foldedRugSprite;   // 접힌 러그 (배치용_3)

    [Header("바닥 전개도 (러그 밑 요소)")]
    public GameObject simplePlaceholder; // Simple_Placeholder 연결

    [Header("줌 컨트롤러 연결")]
    public NetPuzzleZoomController zoomController;

    private Image rugImage;
    public bool isFolded = false; // 현재 러그 상태
    public bool isCleared = false; // 퍼즐 클리어 여부 플래그

    private void Awake()
    {
        rugImage = GetComponent<Image>();
        if (rugImage != null)
        {
            rugImage.raycastTarget = true;

            // 이미지의 투명 영역(알파값 0.1 미만) 클릭 판정 제외
            rugImage.alphaHitTestMinimumThreshold = 0.1f;
        }
    }

    private void Start()
    {
        UpdateRugSprite();
    }

    // 러그 클릭 시 토글(접기 ↔ 펼치기) 실행
    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭할 때마다 isFolded 상태 반전 (true ↔ false)
        isFolded = !isFolded;
        
        if (isFolded)
        {
            Debug.Log("[RugToggle] 러그를 접었습니다.");
        }
        else
        {
            Debug.Log("[RugToggle] 러그를 다시 펼쳤습니다.");
        }

        UpdateRugSprite();
    }

    // ZoomController 등 다른 스크립트에서 호출하는 메서드 이름 유지를 위해 UpdateRugSprite로 명명
    public void UpdateRugSprite()
    {
        if (rugImage == null) rugImage = GetComponent<Image>();

        // 1. 러그 상태에 맞춰 이미지 교체 (크기 변경 로직 제외)
        if (rugImage != null)
        {
            rugImage.sprite = isFolded ? foldedRugSprite : unfoldedRugSprite;
        }

        // 2. 이미 퍼즐이 풀렸다면(isCleared == true) 무조건 비활성화 상태 유지
        if (isCleared)
        {
            if (simplePlaceholder != null) simplePlaceholder.SetActive(false);
            return;
        }

        // 3. 퍼즐이 안 풀렸을 때만 러그 접힘 여부에 따라 Placeholder 활성화
        if (simplePlaceholder != null)
        {
            simplePlaceholder.SetActive(isFolded);
        }
    }

    // 퍼즐 클리어 시 호출해 주는 함수
    public void SetCleared()
    {
        isCleared = true;
        if (simplePlaceholder != null)
        {
            simplePlaceholder.SetActive(false);
        }
    }
}