using UnityEngine;
using UnityEngine.EventSystems;

public class CakeFrame2D8 : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject cakeframe;
    [SerializeField] private GameObject cakeframeOpen;
    [SerializeField] private GameObject display2D8;

    private bool opened = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (opened)
            return;

        opened = true;

        cakeframe.SetActive(false);
        cakeframeOpen.SetActive(true);
        display2D8.SetActive(true);
    }
}