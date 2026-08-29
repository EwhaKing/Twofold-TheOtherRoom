using TMPro;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Layer Settings")]
    public LayerMask interactableLayer;
    public LayerMask mouseHoldLayer;

    [Header("Interaction UI")]
    public TMP_Text interactText;
    [Header("MouseHold UI")]
    public GameObject MouseHoldUI;
    public Camera playerCamera;

    private IInteractable currentInteractable;
    private IMouseHoldable mouseholdInteractable;

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
        DetectMouseHoldable();

        if (mouseholdInteractable != null && Input.GetMouseButtonDown(0))
        {
            MouseHoldUI.SetActive(false);
            mouseholdInteractable.MouseHoldInteract();
        }

        if (currentInteractable != null &&
            Input.GetKeyDown(interactKey))
        {
            interactText.gameObject.SetActive(false);
            currentInteractable.Interact();
        }
    }

    private void DetectMouseHoldable()
    {
        mouseholdInteractable = null;
        MouseHoldUI.SetActive(false);

        Ray ray = new Ray(transform.position, transform.forward);

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactDistance,
                mouseHoldLayer))
        {
            return;
        }

        mouseholdInteractable =
            hit.collider.GetComponentInParent<IMouseHoldable>();


        if (mouseholdInteractable != null)
            {
                // Raycast가 닿은 3D 위치를 화면 좌표로 변환
                Vector3 screenPosition =
                    playerCamera.WorldToScreenPoint(hit.point);

                if (MouseHoldUI != null)
                {
                    MouseHoldUI.transform.position = screenPosition;
                    MouseHoldUI.SetActive(true);
                }
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
