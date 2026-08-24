using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared UI shown while a player is inspecting an object or puzzle.
/// Only the current owner can show or hide the UI.
/// </summary>
public sealed class InspectionUIController : MonoBehaviour
{
    public static InspectionUIController Instance { get; private set; }

    [SerializeField] private GameObject canvasRoot;
    [SerializeField] private Button initializeButton;

    private ICloseInspection currentInspection;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[InspectionUIController] Duplicate controller found.", this);
            enabled = false;
            return;
        }

        Instance = this;

        if (canvasRoot == null)
            canvasRoot = gameObject;


        canvasRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Show(ICloseInspection inspection)
    {
        if (inspection == null)
            return;

        currentInspection = inspection;
        // 초기화 버튼도 있다면
        if (initializeButton != null)
        //초기화 기능까지 추가. 
            initializeButton.gameObject.SetActive(inspection is IResetInspection);
        canvasRoot.SetActive(true);
    }

    public void Hide(ICloseInspection inspection)
    {
        if (!object.ReferenceEquals(currentInspection, inspection))
            return;

        currentInspection = null;
        canvasRoot.SetActive(false);
    }

    public void CloseCurrentInspection()
    {
        SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
        currentInspection?.CloseInspection();
    }

    public void ResetCurrentInspection()
    {
        SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
        if (currentInspection is IResetInspection resettableInspection)
            resettableInspection.ResetInspection();
    }
}
