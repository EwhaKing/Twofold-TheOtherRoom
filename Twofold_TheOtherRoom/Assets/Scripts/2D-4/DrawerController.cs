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
        bool isOpen = !leftDrawerOpen.activeSelf;
        leftDrawerOpen.SetActive(isOpen);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(
                isOpen ? SFXType.CabinetOpen : SFXType.CabinetClose
            );
        }
    }

    public void ToggleRightDrawer()
    {
        if (rightDrawerOpen == null)
        {
            return;
        }

        bool isOpen = !rightDrawerOpen.activeSelf;

        rightDrawerOpen.SetActive(isOpen);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(
                isOpen ? SFXType.CabinetOpen : SFXType.CabinetClose
            );
        }

        if (book != null)
        {
            book.SetActive(isOpen);

            // RightClickArea overlaps the book and is later in the Canvas hierarchy,
            // so it otherwise receives the UI raycast before the book does.
            if (isOpen)
            {
                book.transform.SetAsLastSibling();
            }
        }
    }
}
