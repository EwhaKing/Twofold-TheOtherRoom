using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class GameManager2D10 : MonoBehaviour
{    
    public Tube2D10[] tube;
    
    public Tube2D10 currentTube;
    public Tube2D10 nextTube;
    public Ball2D10 currentball;
    public Ball2D10 beforeball;
    private Image LightImage;
    
    public bool isSelected;
    public bool isAnimating;

    private float tubeTopPos=5.5f;
    private Vector3 targetPos;
    void Start()
    {
        isSelected=false;
        isAnimating=false;
    }

    public void CheckAnswer()
    {

        if (tube[0].solve == true &&
            tube[1].solve == true &&
            tube[2].solve == true){
            PuzzleManager.Instance.ReportSolved(
                "2D-10",
                PuzzleDimension.TwoD
            );
        }
    }
    public IEnumerator GoUp()
    {
        isAnimating = true;
        targetPos = currentTube.GetTubePosition();
        targetPos.y = tubeTopPos;
        yield return StartCoroutine(MoveBall(currentball, targetPos));
        currentTube.Pop();
        isAnimating = false;
    }
    public IEnumerator GoMove()
    {
        isAnimating = true;

        targetPos = nextTube.GetTubePosition();
        targetPos.y = tubeTopPos;
        yield return StartCoroutine(MoveBall(currentball, targetPos));

        targetPos.y = nextTube.GetNextBallPositionY();
        yield return StartCoroutine(MoveBall(currentball, targetPos));
        nextTube.Push(currentball);
        currentTube=null;
        nextTube.Answer(nextTube.GetColor());
        nextTube=null;
        currentball=null;
        beforeball=null;
        isAnimating = false;
        CheckAnswer();
    }

    private IEnumerator MoveBall(Ball2D10 ball, Vector3 targetPos)
    {   
        while (Vector3.Distance(ball.transform.position, targetPos) > 0.01f)
        {
            ball.transform.position = Vector3.MoveTowards(
                ball.transform.position,
                targetPos,
                10f * Time.deltaTime); 

            yield return null;
        }

        ball.transform.position = targetPos;
    }

    public bool CanMove()
    {
        if(beforeball==null)
        {
            return true;
        } 
        else if(beforeball.GetColor()!=currentball.GetColor() || nextTube.ball[3]!=null)
        {   
            return false;
        }
        else
        {
            return true;
        }
    }
    public void RestartGame()
    {
        if (isAnimating)
        {
            return;
        }

        foreach (Tube2D10 currentTube in tube)
        {
            currentTube.ResetTube();
        }

        currentTube = null;
        nextTube = null;
        currentball=null;
        beforeball = null;

        isSelected = false;
        isAnimating = false;
    }
}
