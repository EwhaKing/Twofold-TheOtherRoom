using UnityEngine;

public class BookController : MonoBehaviour
{
    [SerializeField] private GameObject closedBook;
    [SerializeField] private GameObject openBook;

    private bool isOpened = false;

    public void OpenBook()
    {
        if (isOpened)
        {
            return;
        }

        isOpened = true;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.Paper);
        }

        closedBook.SetActive(false);
        openBook.SetActive(true);
    }
}