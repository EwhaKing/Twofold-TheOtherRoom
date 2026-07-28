using TMPro;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Layer Settings")]
    public LayerMask interactableLayer;

    [Header("Interaction UI")]
    public TMP_Text interactText;
    public Camera playerCamera;

    private IInteractable currentInteractable;

    public static PlayerInteractor Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

         interactText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        currentInteractable = null;
        HideInteractionPrompt();
    }

    public void HideInteractionPrompt()
    {
        if (interactText != null)
            interactText.gameObject.SetActive(false);
    }

    private void Update()
    {
        DetectInteractable();

        if (currentInteractable != null &&
            Input.GetKeyDown(interactKey))
        {
            interactText.gameObject.SetActive(false);
            currentInteractable.Interact();
        }
    }

    private void DetectInteractable()
    {
        currentInteractable = null;
        interactText.gameObject.SetActive(false);

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactDistance,
                interactableLayer))
        {
            currentInteractable =
                hit.collider.GetComponentInParent<IInteractable>();

            if (currentInteractable != null)
            {
                // Raycast가 닿은 3D 위치를 화면 좌표로 변환
                Vector3 screenPosition =
                    playerCamera.WorldToScreenPoint(hit.point);

                if (interactText != null)
                {
                    interactText.transform.position = screenPosition;
                    interactText.gameObject.SetActive(true);
                }
            }
        }
    }
}
