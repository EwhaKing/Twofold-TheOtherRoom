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

    private void Awake()
    {
        rugImage = GetComponent<Image>();
        if (rugImage != null)
        {
            rugImage.raycastTarget = true;
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

        // 1. 러그 상태에 맞춰 이미지 교체
        if (rugImage != null)
        {
            rugImage.sprite = isFolded ? foldedRugSprite : unfoldedRugSprite;
        }

        // 2. 러그가 접혔을 때만 바닥 전개도(Simple_Placeholder)를 활성화
        if (simplePlaceholder != null)
        {
            simplePlaceholder.SetActive(isFolded);
        }
    }
}