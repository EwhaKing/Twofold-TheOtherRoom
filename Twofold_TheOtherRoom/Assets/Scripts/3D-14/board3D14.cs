using UnityEngine;

public class board3D14 : MonoBehaviour, IInteractable
{
    [Header("Figures")]
    [SerializeField] private figure3D14[] figures;

    public bool isViewing = false;


    public void Interact()
    {
        if (InspectionCameraRig.Instance == null)
        {
            Debug.LogWarning(
                "[board3D14] InspectionCameraRig가 없습니다."
            );

            return;
        }

        InspectionCameraRig.Instance.StartView();
        isViewing = true;
    }

  /*  
    public void CheckPuzzleSolved()
    {
        if (figures == null || figures.Length == 0)
            return;

        foreach (figure3D14 figure in figures)
        {
            if (figure == null)
                return;

            if (!figure.IsCorrectPosition())
                return;
        }

        PuzzleSolved();
    }


    private void PuzzleSolved()
    {
        Debug.Log(
            "[board3D14] ★ 퍼즐 성공 ★"
        );

        // 나중에 PuzzleManager 연결
        /*
        PuzzleManager.Instance.ReportSolved(
            "3D-14",
            PuzzleDimension.ThreeD
        );
        */
}
    