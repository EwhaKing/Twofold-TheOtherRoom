using TMPro;
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

    [Header("Puzzle Description Images")]
    [Tooltip("HorizontalLayoutGroup이 붙은 설명 이미지 부모입니다.")]
    [SerializeField] private GameObject descriptionLayoutRoot;
    [Tooltip("공통 Canvas가 보유한 모든 설명 Image를 배열 순서대로 연결합니다.")]
    [SerializeField] private Image[] descriptionImages;

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

        HideDescriptionImages();
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

        ShowRequestedDescriptionImages(inspection);
        canvasRoot.SetActive(true);
    }

    public void Hide(ICloseInspection inspection)
    {
        if (!object.ReferenceEquals(currentInspection, inspection))
            return;

        currentInspection = null;
        HideDescriptionImages();
        canvasRoot.SetActive(false);
    }

    public void CloseCurrentInspection()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
        }
        currentInspection?.CloseInspection();
    }

    public void ResetCurrentInspection()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
        }
        if (currentInspection is IResetInspection resettableInspection)
            resettableInspection.ResetInspection();
    }

    private void ShowRequestedDescriptionImages(ICloseInspection inspection)
    {
        HideDescriptionImages();

        if (!(inspection is Component inspectionComponent))
            return;

        PuzzleDescriptionImages request =
            inspectionComponent.GetComponent<PuzzleDescriptionImages>();

        if (request == null || request.ImageIndexes == null)
            return;

        int visibleCount = 0;
        foreach (PuzzleDescriptionImages.DescriptionImage description in request.ImageIndexes)
        {
            
            if (description == null)
                continue;

            int imageIndex = description.imageIndex;
            if (descriptionImages == null || imageIndex < 0 || imageIndex >= descriptionImages.Length)
            {
                Debug.LogWarning($"[InspectionUIController] 설명 이미지 인덱스가 범위를 벗어났습니다: {imageIndex}", request);
                continue;
            }

            Image image = descriptionImages[imageIndex];
            if (image == null)
                continue;
// image안에 지정된 txt로 자식 txt 표시 
            TMP_Text label = image.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = description.text ?? string.Empty;
            else
            {
                Text legacyLabel = image.GetComponentInChildren<Text>(true);
                if (legacyLabel != null)
                    legacyLabel.text = description.text ?? string.Empty;
            }

            image.gameObject.SetActive(true);
            visibleCount++;
        }

        if (descriptionLayoutRoot != null)
            descriptionLayoutRoot.SetActive(visibleCount > 0);
    }

    private void HideDescriptionImages()
    {
        if (descriptionImages != null)
        {
            foreach (Image image in descriptionImages)
            {
                if (image != null)
                    image.gameObject.SetActive(false);
            }
        }

        if (descriptionLayoutRoot != null)
            descriptionLayoutRoot.SetActive(false);
    }
}
