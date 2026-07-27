using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawerManager : MonoBehaviour
{
    [Header("Drawer Sprites")]
    public Sprite openDrawerSprite;   // 열린 서랍 스프라이트
    public Sprite closedDrawerSprite; // 닫힌 서랍 스프라이트

    [Header("Zoom Panel")]
    public GameObject paperZoomPanel; // 씬의 (비활성화된) Paper_Zoom_Panel

    [System.Serializable]
    public class DrawerData
    {
        public Button drawerButton;   // 서랍 버튼 UI
        public GameObject itemInside; // 서랍 내부의 작은 종이 (없으면 null)
        public bool isOpen = false;   // 현재 열림/닫힘 상태
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
        }

        // 서랍 안 종이가 있다면 서랍 상태에 맞춰 인스펙터 켜고 끄기
        if (drawer.itemInside != null)
        {
            drawer.itemInside.SetActive(drawer.isOpen);
        }
    }

    // 2. 작은 종이 클릭 시 -> Paper_Zoom_Panel 인스펙터 켜기 (On)
    public void OpenPaperZoom()
    {
        if (paperZoomPanel != null)
        {
            paperZoomPanel.SetActive(true);
        }
    }

    // 3. 닫기 버튼 클릭 시 -> Paper_Zoom_Panel 인스펙터 끄기 (Off)
    public void ClosePaperZoom()
    {
        if (paperZoomPanel != null)
        {
            paperZoomPanel.SetActive(false);
        }
    }
}