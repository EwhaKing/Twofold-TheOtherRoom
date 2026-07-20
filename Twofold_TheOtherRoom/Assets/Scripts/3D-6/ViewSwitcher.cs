using UnityEngine;
using Cinemachine;

public class ViewSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera sideCam;
    public CinemachineVirtualCamera frontCam;

    public enum ViewMode { Side, Front }
    public static ViewMode CurrentView { get; private set; } = ViewMode.Side;

    public void SwitchToFront()
    {
        sideCam.Priority = 0;
        frontCam.Priority = 10;
        CurrentView = ViewMode.Front;
    }

    public void SwitchToSide()
    {
        sideCam.Priority = 10;
        frontCam.Priority = 0;
        CurrentView = ViewMode.Side;
    }

    public void ToggleView()
    {
        if (CurrentView == ViewMode.Side) SwitchToFront();
        else SwitchToSide();
    }
}