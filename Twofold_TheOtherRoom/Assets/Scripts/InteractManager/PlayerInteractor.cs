using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private Mirror3D heldMirror;   // 마지막으로 집은 거울
    private Graphic[] promptGraphics;       // interactText와 자식 그래픽
    private Color[] defaultPromptColors;    // 각 그래픽 원래 색

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

        promptGraphics = interactText.GetComponentsInChildren<Graphic>(true);
        defaultPromptColors = new Color[promptGraphics.Length];
        for (int i = 0; i < promptGraphics.Length; i++)
            defaultPromptColors[i] = promptGraphics[i].color;

        interactText.gameObject.SetActive(false);
    }

    /// 대상 지정색, 없으면 각자 원래 색
    private void ApplyPromptColor(IInteractPromptColor source)
    {
        Color c = default;
        bool overridden = source != null && source.TryGetPromptColor(out c);

        for (int i = 0; i < promptGraphics.Length; i++)
            promptGraphics[i].color = overridden ? c : defaultPromptColors[i];
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


         if (MouseHoldUI != null)
            MouseHoldUI.SetActive(false);
    }

    private void Update()
    {
        DetectInteractable();
        DetectMouseHoldable();

        if (mouseholdInteractable != null && Input.GetMouseButtonDown(0))
        {
            MouseHoldUI.SetActive(false);
            mouseholdInteractable.MouseHoldInteract();
            heldMirror = mouseholdInteractable as Mirror3D;
        }

        if (currentInteractable != null &&
            Input.GetKeyDown(interactKey))
        {
            if (heldMirror != null && heldMirror.IsHolding) return;

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
                    ApplyPromptColor(hit.collider.GetComponentInParent<IInteractPromptColor>());

                    interactText.transform.position = screenPosition;
                    interactText.gameObject.SetActive(true);
                }
            }
        }
    }
}
