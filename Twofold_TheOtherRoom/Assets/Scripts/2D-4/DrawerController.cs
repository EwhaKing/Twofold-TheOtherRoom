using UnityEngine;

public class DrawerController : MonoBehaviour
{
    [Header("열린 서랍")]
    [SerializeField] private GameObject leftDrawerOpen;
    [SerializeField] private GameObject rightDrawerOpen;

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

        rightDrawerOpen.SetActive(
            !rightDrawerOpen.activeSelf
        );
    }
}