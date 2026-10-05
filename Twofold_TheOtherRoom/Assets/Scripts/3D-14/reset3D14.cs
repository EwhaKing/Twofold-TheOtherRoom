using UnityEngine;

public class reset3D14 : MonoBehaviour
{
    
    [Header("Inspection Camera")]
    [SerializeField] private Camera inspectionCamera;

    [Header("Figures")]
    [SerializeField] private figure3D14[] figures;

    private void Update()
    {
        if (!IsInspectionView())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryReset();
        }
    }

    private bool IsInspectionView()
    {
        if (InspectionCameraRig.Instance == null)
            return false;

        return InspectionCameraRig.Instance.IsViewing;
    }

    private void TryReset()
    {
        Camera inspectionCamera =
            InspectionCameraRig.Instance.ViewCamera;

        if (inspectionCamera == null)
            return;

        Ray ray = inspectionCamera.ScreenPointToRay(
            Input.mousePosition
        );

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit))
        {
            return;
        }

        if (hit.transform != transform &&
            !hit.transform.IsChildOf(transform))
        {
            return;
        }

        ResetPuzzle();
    }

    public void ResetPuzzle()
    {
        if (figures == null)
            return;

        foreach (figure3D14 figure in figures)
        {
            if (figure == null)
                continue;

            figure.ResetPos();
        }
    }
}