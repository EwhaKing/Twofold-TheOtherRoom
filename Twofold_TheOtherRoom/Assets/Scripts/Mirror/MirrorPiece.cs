using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>월드에 놓인 거울 조각을 수집합니다.</summary>
public class MirrorPiece : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string puzzleId;
    [SerializeField] private GameObject objectToHide;

    private int lastClickFrame = -1;

    private void Start()
    {
        if (MirrorManager.Instance != null &&
            MirrorManager.Instance.HasMirrorPiece(puzzleId))
        {
            HidePickup();
        }
    }

    public void GetMirrorPiece()
    {
        if (lastClickFrame == Time.frameCount) return;
        lastClickFrame = Time.frameCount;

        if (MirrorManager.Instance == null) return;
        Debug.Log("퍼즐매니저한테"+ puzzleId + "보낼꺼임!");
        MirrorManager.Instance.GetMirrorPiece(puzzleId);
        HidePickup();
    }

    public void OnPointerClick(PointerEventData eventData) => GetMirrorPiece();

    private void OnMouseDown() => GetMirrorPiece();

    private void HidePickup()
    {
        (objectToHide != null ? objectToHide : gameObject).SetActive(false);
    }
}

