using UnityEngine;

public class DrawerController : MonoBehaviour
{
    [Header("열린 서랍장")]
    [SerializeField] private GameObject leftDrawerOpen;
    [SerializeField] private GameObject rightDrawerOpen;

    [Header("책")]
    [SerializeField] private GameObject book;

    private void Start()
    {
        if (leftDrawerOpen != null)
        {
            leftDrawerOpen.SetActive(false);
        }

        if (rightDrawerOpen != null)
        {
            rightDrawerOpen.SetActive(false);
        }

        if (book != null)
        {
            book.SetActive(false);
        }
    }

    public void ToggleLeftDrawer()
    {
        if (leftDrawerOpen == null)
        {
            return;
        }

        leftDrawerOpen.SetActive(
            !leftDrawerOpen.activeSelf
        );
    }

    public void ToggleRightDrawer()
    {
        if (rightDrawerOpen == null)
        {
            return;
        }

        bool isOpen = !rightDrawerOpen.activeSelf;

        rightDrawerOpen.SetActive(isOpen);

        if (book != null)
        {
            book.SetActive(isOpen);
        }
    }
}