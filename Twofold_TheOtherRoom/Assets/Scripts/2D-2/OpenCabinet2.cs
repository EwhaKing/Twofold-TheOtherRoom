using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenCabinet2 : MonoBehaviour
{
    [Header("Drawer Sprites")]
    public Sprite openDrawerSprite;
    public Sprite closedDrawerSprite;

    [System.Serializable]
    public class DrawerData
    {
        public Button drawerButton;
        public GameObject itemInside;
        public bool isOpen;

        [System.NonSerialized] public float closedHeight;
        [System.NonSerialized] public int originalSiblingIndex;
    }

    [Header("Drawer List")]
    public List<DrawerData> drawers = new List<DrawerData>();

    [Header("Open Appearance")]
    [SerializeField] private float openHeightIncrease = 55f;

    private void Awake()
    {
        foreach (DrawerData drawer in drawers)
        {
            if (drawer.drawerButton == null)
                continue;

            RectTransform drawerRect = drawer.drawerButton.image.rectTransform;
            drawer.closedHeight = drawerRect.sizeDelta.y;
            drawer.originalSiblingIndex = drawerRect.GetSiblingIndex();

            SetDrawerState(drawer, false);
        }
    }

    public void ToggleDrawer(int index)
    {
        if (index < 0 || index >= drawers.Count)
            return;

        DrawerData selectedDrawer = drawers[index];
        bool shouldOpen = !selectedDrawer.isOpen;

        // Close every drawer first so their original drawing order is restored.
        for (int i = 0; i < drawers.Count; i++)
        {
            SetDrawerState(drawers[i], false);
        }

        // Open the selected drawer last so it is drawn above the others.
        if (shouldOpen)
            SetDrawerState(selectedDrawer, true);
    }

    private void SetDrawerState(DrawerData drawer, bool isOpen)
    {
        drawer.isOpen = isOpen;

        if (drawer.drawerButton != null)
        {
            Image drawerImage = drawer.drawerButton.image;
            drawerImage.sprite = isOpen ? openDrawerSprite : closedDrawerSprite;

            RectTransform drawerRect = drawerImage.rectTransform;
            Vector2 size = drawerRect.sizeDelta;
            size.y = isOpen ? drawer.closedHeight + openHeightIncrease : drawer.closedHeight;
            drawerRect.sizeDelta = size;

            if (isOpen)
                drawerRect.SetAsLastSibling();
            else
                drawerRect.SetSiblingIndex(drawer.originalSiblingIndex);
        }

        if (drawer.itemInside != null)
            drawer.itemInside.SetActive(isOpen);
    }
}
