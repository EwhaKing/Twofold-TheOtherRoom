using UnityEngine;

public class Answer3D10 : MonoBehaviour
{
    public Cylinder3D10[] input;

    public float[] answer = { 80f, 200f, 320f };
    public bool _solved = false;

    public void CheckAnswer()
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i].GetY() != answer[i])
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
