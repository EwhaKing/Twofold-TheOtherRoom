using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class FloorPanelPuzzleController : MonoBehaviour
{
    [Header("Solution")]
    [SerializeField] List<Vector2Int> solutionList;
    private string puzzleId = "3D-9";
    private PuzzleDimension dimension = PuzzleDimension.ThreeD;
    private HashSet<Vector2Int> currentOn = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> solution;
    private FloorPanel[] panels;
    private Dictionary<Vector2Int, FloorPanel> panelMap = new();
    private bool isSolved = false;

    void Start()
    {
        solution = new(solutionList);
        panels = GetComponentsInChildren<FloorPanel>();
        foreach (FloorPanel panel in panels)
        {
            panel.OnToggled += OnPanelToggled;
            panelMap[panel.Index] = panel;
        }

    }

    // 패널이 토글 알려줄 때
    void OnPanelToggled(Vector2Int pos, bool isOn)
    {
        if(isSolved) return;
        // on에 추가/제거
        if (isOn) currentOn.Add(pos);
        else currentOn.Remove(pos);

        // 성공 검사
        if (currentOn.SetEquals(solution))
        {
            isSolved = true;
            foreach (var panel in panels) panel.Freeze();
            StartCoroutine(SolvedRoutine()); // 성공 연출
            if (PuzzleManager.Instance != null) // 매니저 보고
                PuzzleManager.Instance.ReportSolved(puzzleId, dimension);
        }
    }

    // 성공 연출
    IEnumerator SolvedRoutine()
    {
        foreach (var pos in solutionList)
        {
            panelMap[pos].ChangeStateToAnswer();
            yield return new WaitForSeconds(0.07f); // 순차적 켜짐
        }
    }

    void OnDestroy()
    {
        if(panels == null) return;
        foreach (FloorPanel panel in panels)
        {
            panel.OnToggled -= OnPanelToggled;
        }
    }

}
