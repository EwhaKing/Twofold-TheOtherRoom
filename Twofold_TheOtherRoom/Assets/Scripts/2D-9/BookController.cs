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

        closedBook.SetActive(false);
        openBook.SetActive(true);
    }
}