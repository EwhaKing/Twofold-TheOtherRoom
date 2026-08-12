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
        public Button drawerButton;   // 서랍 버튼 UI
        public GameObject itemInside; // 서랍 내부의 작은 종이 (없으면 null)
        public bool isOpen = false;   // 현재 열림/닫힘 상태
        [System.NonSerialized] public float closedHeight;
        [System.NonSerialized] public bool hasCachedClosedHeight;
        [System.NonSerialized] public int originalSiblingIndex;
        [System.NonSerialized] public bool hasCachedSiblingIndex;
    }

    [Header("Drawer List")]
    public List<DrawerData> drawers = new List<DrawerData>();

    // 1. 서랍 클릭 시 실행 (열림/닫힘 토글)
    public void ToggleDrawer(int index)
    {
        if (index < 0 || index >= drawers.Count) return;

        DrawerData drawer = drawers[index];
        drawer.isOpen = !drawer.isOpen; // 상태 반전 (열림 <-> 닫힘)

        // 서랍 이미지 스프라이트 교체
        if (drawer.drawerButton != null)
        {
            drawer.drawerButton.image.sprite = drawer.isOpen ? openDrawerSprite : closedDrawerSprite;

            RectTransform drawerRect = drawer.drawerButton.image.rectTransform;
            if (!drawer.hasCachedClosedHeight)
            {
                drawer.closedHeight = drawerRect.sizeDelta.y;
                drawer.hasCachedClosedHeight = true;
            }

            Vector2 size = drawerRect.sizeDelta;
            size.y = drawer.isOpen ? drawer.closedHeight + 60f : drawer.closedHeight;
            drawerRect.sizeDelta = size;
        }

        ReorderDrawers();

        // 서랍 안 종이가 있다면 서랍 상태에 맞춰 인스펙터 켜고 끄기
        if (drawer.itemInside != null)
        {
            drawer.itemInside.SetActive(drawer.isOpen);
        }
    }

    private void ReorderDrawers()
    {
        // 닫힌 서랍은 모두 뒤에 둔다.
        for (int i = 0; i < drawers.Count; i++)
        {
            DrawerData drawer = drawers[i];
            if (!drawer.isOpen && drawer.drawerButton != null)
            {
                drawer.drawerButton.image.rectTransform.SetAsLastSibling();
            }
        }

        // 열린 서랍은 index가 큰 것부터 올려서, index가 작은 서ㅁ랍이 가장 앞에 오게 한다.
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
