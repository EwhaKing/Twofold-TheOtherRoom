using UnityEngine;

// puzzle needs Layer : "Interactable"
// puzzle script needs:  IInteractable


// This code first watchs the layer
// and apply the script that has IInteractable -> and applies Interact()



public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Layer Settings")]
    public LayerMask interactableLayer;

    private IInteractable currentInteractable;

    private void Update()
    {
        DetectInteractable();

        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            Debug.Log("Interact 실행!");
            currentInteractable.Interact();
            Debug.Log("Interact 실행완료!");

        }
    }

    private void DetectInteractable()
    {
        currentInteractable = null;

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            Debug.Log("Raycast 발견: " + hit.collider.gameObject.name);
            currentInteractable = hit.collider.GetComponentInParent<IInteractable>();
        }
    }
}