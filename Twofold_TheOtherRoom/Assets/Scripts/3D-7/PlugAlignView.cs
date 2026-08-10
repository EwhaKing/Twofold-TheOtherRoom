using UnityEngine;
using UnityEngine.UI;

public class PlugAlignView : MonoBehaviour
{
    [SerializeField] Camera renderCamera;
    [SerializeField] RawImage view;
    [SerializeField] float xOffset = 0f;

    void Awake()
    {
        Hide();
    }

    public void Show(PlugOutlet outlet)
    {
        Vector3 position = renderCamera.transform.position;
        position.x = outlet.DockPosition.x + xOffset;
        renderCamera.transform.position = position;

        renderCamera.enabled = true;
        view.enabled = true;
    }

    public void Hide()
    {
        renderCamera.enabled = false;
        view.enabled = false;
    }
}
