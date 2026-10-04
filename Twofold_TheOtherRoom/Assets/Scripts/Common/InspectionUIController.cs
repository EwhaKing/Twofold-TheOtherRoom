using System.Collections.Generic;
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
    [Tooltip("설명 항목 안의 아이콘 Image를 배열 순서대로 연결합니다.\n" +
             "Image의 부모(아이콘 + 텍스트 묶음)째로 켜고 끄며, 부모 바로 아래 TMP 텍스트에 설명 문구가 들어갑니다.")]
    [SerializeField] private Image[] descriptionImages;

    private ICloseInspection currentInspection;

    /// 같은 인덱스를 두 번 이상 쓸 때 만든 항목 복제본. 숨길 때 지움
    private readonly List<GameObject> spawnedItems = new List<GameObject>();

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

    public void RefreshDescriptionImages(ICloseInspection inspection)
    {
        if (!object.ReferenceEquals(currentInspection, inspection) || inspection == null)
            return;

        ShowRequestedDescriptionImages(inspection);
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

        var descriptions = new List<PuzzleDescriptionImages.DescriptionImage>();
        if (request != null && request.ImageIndexes != null)
            descriptions.AddRange(request.ImageIndexes);

        var provider = inspectionComponent.GetComponent<ThreeDCommunicationDescriptionImages>();

        if (provider != null && provider.isActiveAndEnabled)
        {
            descriptions.AddRange(provider.GetAdditionalDescriptionImages());
        }

        int visibleCount = 0;
        // 인덱스별로 마지막에 켠 항목. 같은 인덱스가 또 나오면 이 뒤에 복제본을 붙임
        var lastShown = new Dictionary<int, GameObject>();

        foreach (PuzzleDescriptionImages.DescriptionImage description in descriptions)
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

            GameObject item = GetItem(image);

            // 이미 이 항목을 썼으면 같은 아이콘으로 한 줄 더
            if (lastShown.TryGetValue(imageIndex, out GameObject previous))
            {
                item = Instantiate(item, item.transform.parent);
                item.transform.SetSiblingIndex(previous.transform.GetSiblingIndex() + 1);
                spawnedItems.Add(item);
            }

            lastShown[imageIndex] = item;
            SetLabel(item, description.text ?? string.Empty);

            item.SetActive(true);
            visibleCount++;
        }

        if (descriptionLayoutRoot != null)
            descriptionLayoutRoot.SetActive(visibleCount > 0);
    }

    private void HideDescriptionImages()
    {
        foreach (GameObject spawned in spawnedItems)
        {
            if (spawned != null)
                Destroy(spawned);
        }
        spawnedItems.Clear();

        if (descriptionImages != null)
        {
            foreach (Image image in descriptionImages)
            {
                if (image != null)
                    GetItem(image).SetActive(false);
            }
        }

        if (descriptionLayoutRoot != null)
            descriptionLayoutRoot.SetActive(false);
    }

    /// 아이콘 Image 의 부모 = 아이콘 + 텍스트 묶음
    private static GameObject GetItem(Image image) => image.transform.parent.gameObject;

    /// 항목 바로 아래 자식 텍스트에 문구를 넣는다. 아이콘 Image 안의 글자("Enter")는 손자라 건드리지 않음
    private static void SetLabel(GameObject item, string text)
    {
        foreach (Transform child in item.transform)
        {
            if (child.TryGetComponent(out TMP_Text label))
            {
                label.text = text;
                return;
            }
        }

        Debug.LogWarning($"[InspectionUIController] {item.name} 바로 아래에 TMP 텍스트가 없음", item);
    }
}
