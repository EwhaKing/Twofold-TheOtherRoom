using UnityEngine;
using UnityEngine.EventSystems;

public class CakeFrame2D8 : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject cakeframe;
    [SerializeField] private GameObject cakeframeOpen;
    [SerializeField] private GameObject puzzlePanel;

    private bool opened = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (opened)
            return;

        opened = true;

        cakeframe.SetActive(false);
        cakeframeOpen.SetActive(true);
        puzzlePanel.SetActive(true);
    }
}