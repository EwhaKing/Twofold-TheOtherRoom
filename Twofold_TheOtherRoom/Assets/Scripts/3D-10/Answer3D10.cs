using UnityEngine;

public class Answer3D10 : MonoBehaviour
{
    public cylinder3D10_local[] input;

    public float[] answer;
    public bool _solved = false;

    public void CheckAnswer()
    {
        

        for (int i = 0; i < input.Length; i++)
        {
            // 미세하게 회전 각도가 달라서, 오차가 심하지 않으면 정답이게 수정
            if (Mathf.Abs(Mathf.DeltaAngle(input[i].GetZ(), answer[i])) > 0.1f)
                return;
        }

        Debug.Log("퍼즐 성공!");

        _solved = true;
        
        PuzzleManager.Instance.ReportSolved(
            "3D-10",
            PuzzleDimension.ThreeD
        );
    }
    public bool GetSolve()
    {
        return _solved;
    }
}
