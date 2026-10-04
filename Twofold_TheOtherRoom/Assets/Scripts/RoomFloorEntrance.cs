using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Attach to the entrance UI Image with Raycast Target enabled.</summary>
public class RoomFloorEntrance : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TwoFloorRoomNavigator navigator;
    [Tooltip("True: go downstairs. False: return upstairs.")]
    [SerializeField] private bool targetLowerFloor = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 누르면 내려가는 거임
        if (eventData.button == PointerEventData.InputButton.Left)
            Enter();
    }

    // Can also be invoked by a Button OnClick instead of the pointer handler.
    public void Enter()
    {
        if (!isActiveAndEnabled)
            return;
        if (navigator == null)
        {
            Debug.LogWarning("RoomFloorEntrance: Assign Navigator.", this);
            return;
        }

        navigator.SetFloor(targetLowerFloor);
    }
}