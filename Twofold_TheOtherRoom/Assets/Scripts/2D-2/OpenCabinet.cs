using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenCabinet : MonoBehaviour
{
    [Header("Drawer Sprites")]
    public Sprite openDrawerSprite;   // 열린 서랍 스프라이트
    public Sprite closedDrawerSprite; // 닫힌 서랍 스프라이트

    [System.Serializable]
    public class DrawerData
    {
        public Button drawerButton;   // 서랍 버튼
        public GameObject itemInside; // 서랍 안 아이템
        public bool isOpen = false;   // 현재 열림/닫힘 상태

        [System.NonSerialized] public float closedHeight;
        [System.NonSerialized] public bool hasCachedClosedHeight;
        [System.NonSerialized] public int originalSiblingIndex;
        [System.NonSerialized] public bool hasCachedSiblingIndex;
    }

    [Header("Drawer List")]
    public List<DrawerData> drawers = new List<DrawerData>();

    private void Start()
    {
        // 시작할 때 모든 서랍을 닫힌 상태로 초기화
        for (int i = 0; i < drawers.Count; i++)
        {
            DrawerData drawer = drawers[i];

            drawer.isOpen = false;

            if (drawer.drawerButton != null)
            {
                // 닫힌 서랍 이미지로 설정
                drawer.drawerButton.image.sprite = closedDrawerSprite;

                RectTransform drawerRect =
                    drawer.drawerButton.image.rectTransform;

                // 닫힌 상태의 원래 높이 저장
                drawer.closedHeight = drawerRect.sizeDelta.y;
                drawer.hasCachedClosedHeight = true;
            }

            // 서랍 안 아이템은 처음에 숨김
            if (drawer.itemInside != null)
            {
                drawer.itemInside.SetActive(false);
            }
        }
    }

    // 서랍 클릭 시 실행
    public void ToggleDrawer(int index)
    {
        if (index < 0 || index >= drawers.Count)
            return;

        DrawerData drawer = drawers[index];

        // 열림 / 닫힘 상태 반전
        drawer.isOpen = !drawer.isOpen;

        // 서랍 이미지 변경
        if (drawer.drawerButton != null)
        {
            drawer.drawerButton.image.sprite =
                drawer.isOpen
                ? openDrawerSprite
                : closedDrawerSprite;

            RectTransform drawerRect =
                drawer.drawerButton.image.rectTransform;

            if (!drawer.hasCachedClosedHeight)
            {
                drawer.closedHeight = drawerRect.sizeDelta.y;
                drawer.hasCachedClosedHeight = true;
            }

            Vector2 size = drawerRect.sizeDelta;

            size.y =
                drawer.isOpen
                ? drawer.closedHeight + 60f
                : drawer.closedHeight;

            drawerRect.sizeDelta = size;
        }

        // 서랍을 열었을 때 효과음 재생
        if (SoundManager.Instance != null)
        {
            if (drawer.isOpen)
            {
                SoundManager.Instance.PlaySFX(SFXType.DrawerOpen);
            }
            else
            {
                SoundManager.Instance.PlaySFX(SFXType.DrawerClose);
            }
        }

        // 서랍 앞뒤 순서 정리
        ReorderDrawers();

        // 서랍 안 아이템 표시 / 숨김
        if (drawer.itemInside != null)
        {
            drawer.itemInside.SetActive(drawer.isOpen);
        }
    }

    private void ReorderDrawers()
    {
        // 닫힌 서랍은 뒤쪽으로
        for (int i = 0; i < drawers.Count; i++)
        {
            DrawerData drawer = drawers[i];

            if (!drawer.isOpen && drawer.drawerButton != null)
            {
                drawer.drawerButton.image.rectTransform.SetAsLastSibling();
            }
        }

        // 열린 서랍은 앞쪽으로
        for (int i = drawers.Count - 1; i >= 0; i--)
        {
            DrawerData drawer = drawers[i];

            if (drawer.isOpen && drawer.drawerButton != null)
            {
                drawer.drawerButton.image.rectTransform.SetAsLastSibling();
            }
        }
    }
}