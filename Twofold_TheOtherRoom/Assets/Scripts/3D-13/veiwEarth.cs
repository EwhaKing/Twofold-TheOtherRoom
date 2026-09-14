using UnityEngine;

public class veiwEarth : MonoBehaviour, IInteractable
{
    [SerializeField] private ObjectCameraManager cameraManager;

    public void Interact()
    {
        if (cameraManager != null)
        {
            cameraManager.StartView();
        }
    }
}
