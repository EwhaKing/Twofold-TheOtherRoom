using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CakeCamera3D_8 : MonoBehaviour, IInteractable, ICloseInspection
{
    [Header("Cameras")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera puzzleCamera;

    [Header("Mouse Click")]
    [SerializeField] private LayerMask knifeLayers = ~0;
    [SerializeField, Min(0.1f)] private float clickDistance = 100f;

    [Header("Player Lock")]
    [Tooltip("비워 두면 PlayerControlLock이 플레이어 이동과 상호작용을 자동으로 잠급니다.")]
    [SerializeField] private Behaviour[] behavioursToDisable;

    [Header("Puzzle Camera Visibility")]
    [Tooltip("검사 화면에 표시하지 않을 오브젝트의 태그입니다.")]
    [SerializeField] private string hiddenTag = "Player";

    [Header("Interaction")]
    [Tooltip("비워 두면 이 오브젝트에 붙은 Collider를 사용합니다.")]
    [SerializeField] private Collider interactionCollider;

    private readonly PlayerControlLock playerControlLock = new PlayerControlLock();
    private readonly List<Renderer> hiddenRenderers = new List<Renderer>();
    private readonly List<bool> rendererEnabledStates = new List<bool>();
    private bool isInspecting;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactionCollider == null)
            interactionCollider = GetComponent<Collider>();

        if (puzzleCamera != null)
            puzzleCamera.enabled = false;
    }

    private void Update()
    {
        if (!isInspecting || puzzleCamera == null)
            return;

        if (Input.GetMouseButtonDown(0))
            TryClickKnife();
    }

    public void Interact()
    {
        if (!isInspecting)
            EnterInspection();
    }

    private void EnterInspection()
    {
        if (puzzleCamera == null)
        {
            Debug.LogWarning("[CakeCamera3D_8] Puzzle Camera가 연결되지 않았습니다.", this);
            return;
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        playerControlLock.Lock(
            this,
            behavioursToDisable,
            alwaysDisablePlayerInteractor: true);

        if (playerCamera != null)
            playerCamera.enabled = false;

        HideTaggedRenderers();
        puzzleCamera.enabled = true;

        // 진입용 대형 Collider가 퍼즐 내부의 Knife Raycast를 가리지 않게 한다.
        if (interactionCollider != null)
            interactionCollider.enabled = false;

        isInspecting = true;
        InspectionUIController.Instance?.Show(this);
    }

    private void TryClickKnife()
    {
        // 공통 Canvas의 뒤로가기 버튼을 누를 때 뒤쪽 Knife까지 클릭되는 것을 막는다.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = puzzleCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                clickDistance,
                knifeLayers,
                QueryTriggerInteraction.Collide))
        {
            return;
        }

        KnifeClick_Mouse knife = hit.collider.GetComponentInParent<KnifeClick_Mouse>();
        if (knife != null)
            knife.Click();
    }

    public void CloseInspection()
    {
        if (!isInspecting)
            return;

        InspectionUIController.Instance?.Hide(this);

        if (puzzleCamera != null)
            puzzleCamera.enabled = false;

        RestoreTaggedRenderers();

        if (playerCamera != null)
            playerCamera.enabled = true;

        if (interactionCollider != null)
            interactionCollider.enabled = true;

        playerControlLock.Unlock();
        isInspecting = false;
    }

    private void HideTaggedRenderers()
    {
        RestoreTaggedRenderers();

        if (string.IsNullOrWhiteSpace(hiddenTag))
            return;

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(hiddenTag);
        foreach (GameObject taggedObject in taggedObjects)
        {
            foreach (Renderer targetRenderer in taggedObject.GetComponentsInChildren<Renderer>(true))
            {
                hiddenRenderers.Add(targetRenderer);
                rendererEnabledStates.Add(targetRenderer.enabled);
                targetRenderer.enabled = false;
            }
        }
    }

    private void RestoreTaggedRenderers()
    {
        for (int i = 0; i < hiddenRenderers.Count; i++)
        {
            if (hiddenRenderers[i] != null)
                hiddenRenderers[i].enabled = rendererEnabledStates[i];
        }

        hiddenRenderers.Clear();
        rendererEnabledStates.Clear();
    }
}
