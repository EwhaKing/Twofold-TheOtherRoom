using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RugSlotController : MonoBehaviour, IPointerClickHandler
{
    [Header("Rug Sprites")]
    public Sprite unfoldedRugSprite; // 펼쳐진 러그 (배치용_0)
    public Sprite foldedRugSprite;   // 접힌 러그

    [Header("Zoom Controller (자동 찾기 적용)")]
    // ★ 타입을 PuzzleZoomController에서 NetPuzzleZoomController로 변경!
    public NetPuzzleZoomController zoomController; 

    private Image rugImage;
    private bool isFolded = false;

    private void Awake()
    {
        rugImage = GetComponent<Image>();
        if (rugImage != null && unfoldedRugSprite != null)
        {
            rugImage.sprite = unfoldedRugSprite;
        }

        // 씬에서 자동 검색
        if (zoomController == null)
        {
            zoomController = FindFirstObjectByType<NetPuzzleZoomController>();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFolded)
        {
            // 1차 클릭: 러그 접기
            isFolded = true;
            if (rugImage != null && foldedRugSprite != null)
            {
                rugImage.sprite = foldedRugSprite;
            }
        }
        else
        {
            // 2차 클릭: 퍼즐 확대 실행
            if (zoomController != null)
            {
                // NetPuzzleZoomController 안의 확대 메소드 호출 (ZoomIn 또는 해당 스크립트의 확대 함수)
                zoomController.ZoomIn(); 
            }
        }
    }

    public void ResetRugState()
    {
        isFolded = false;
        if (rugImage != null && unfoldedRugSprite != null)
        {
            rugImage.sprite = unfoldedRugSprite;
        }
    }
}