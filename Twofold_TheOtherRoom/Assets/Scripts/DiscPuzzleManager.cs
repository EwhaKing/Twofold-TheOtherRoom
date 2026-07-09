using UnityEngine;
using TMPro;

public class DiscPuzzleManager : MonoBehaviour
{
    public Disc[] discs; // 3개
    public TMP_Text resultText;
    public Camera puzzleCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryRotate(-1); // 왼쪽 클릭 = 왼쪽 회전
        if (Input.GetMouseButtonDown(1)) TryRotate(1);  // 오른쪽 클릭 = 오른쪽 회전
    }

    void TryRotate(int dir)
    {
        Ray ray = puzzleCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Disc disc = hit.collider.GetComponentInParent<Disc>();
            if (disc != null)
            {
                disc.Rotate(dir);
                CheckSolved();
            }
        }
    }

    void CheckSolved()
    {
        bool solved = true;
        foreach (var d in discs)
            if (!d.IsCorrect()) solved = false;

        resultText.text = solved ? "정답" : "";
    }
}