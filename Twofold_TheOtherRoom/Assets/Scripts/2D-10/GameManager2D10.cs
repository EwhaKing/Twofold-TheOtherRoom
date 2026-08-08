using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager2D10 : MonoBehaviour
{    
    public Tube2D10[] tube;
    
    public Tube2D10 currentTube;
    public Tube2D10 nextTube;
    public Ball2D10 currentball;
    public Ball2D10 beforeball;
    private Image LightImage;
    
    private bool _solved;
    public bool isSelected;
    public bool isAnimating;

    private float tubeTopPos=5.5f;
    private Vector3 targetPos;
    void Start()
    {
        _solved=false;
        isSelected=false;
        isAnimating=false;
    }

    public void isSolved()
    {
        if(tube)
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
        nextTube.Answer();
        nextTube=null;
        currentball=null;
        beforeball=null;
        isAnimating = false;
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

}
