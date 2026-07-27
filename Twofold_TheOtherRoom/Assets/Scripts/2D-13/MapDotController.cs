using UnityEngine;

public class MapDotController : MonoBehaviour
{
    [Header("오브젝트 연결")]
    [SerializeField] private RectTransform playerDot;
    [SerializeField] private RectTransform mapRect;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 200f;

    [Header("지도 가장자리 여백")]
    [SerializeField] private float horizontalPadding = 10f;

    private void Start()
    {
        if (playerDot == null)
        {
            playerDot = transform as RectTransform;
        }


        if (mapRect == null && playerDot != null)
        {
            mapRect = playerDot.parent as RectTransform;
        }
    }

    private void Update()
    {
        MoveDot();
    }

    private void MoveDot()
    {
        if (playerDot == null || mapRect == null)
        {
            return;
        }

        float horizontalInput = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput = -1f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput = 1f;
        }

        Vector2 position = playerDot.anchoredPosition;

        position.x += horizontalInput * moveSpeed * Time.deltaTime;

        // 지도 너비의 절반
        float mapHalfWidth = mapRect.rect.width * 0.5f;

        // 점 너비의 절반
        float dotHalfWidth = playerDot.rect.width * 0.5f;

        // 점이 지도 밖으로 빠져나가지 않는 실제 범위
        float minimumX =
            -mapHalfWidth + dotHalfWidth + horizontalPadding;

        float maximumX =
            mapHalfWidth - dotHalfWidth - horizontalPadding;

        position.x = Mathf.Clamp(
            position.x,
            minimumX,
            maximumX
        );

        playerDot.anchoredPosition = position;
    }
}