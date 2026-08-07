using UnityEngine;
using System.Collections;

public class GameManager2D10 : MonoBehaviour
{    
    public Tube2D10[] tube;
    
    public Tube2D10 currentTube;
    public Tube2D10 nextTube;
    public Ball2D10 currentball;
    public Ball2D10 beforeball;
    
    public bool isSelected=false;
    public bool isAnimating=false;

    private float tubeTopPos=5.5f;
    private Vector3 targetPos;

    public IEnumerator GoUp()
    {
        isAnimating = true;
        if (currentTube == null)
            yield break;
        currentball = currentTube.GetTopBall();

        targetPos = currentTube.GetTubePosition();
        targetPos.y = tubeTopPos;
        yield return StartCoroutine(MoveBall(currentball, targetPos));
        isAnimating = false;
    }
    public IEnumerator GoMove()
    {
        isAnimating = true;
        if (currentTube == null)
            yield break;
        currentball = currentTube.GetTopBall();
            
        targetPos = nextTube.GetTubePosition();
        targetPos.y = tubeTopPos;
        yield return StartCoroutine(MoveBall(currentball, targetPos));

        targetPos.y = nextTube.GetNextBallPositionY();
        yield return StartCoroutine(MoveBall(currentball, targetPos));
        currentTube.Pop();
        nextTube.Push(currentball);
        currentTube=null;
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
                5f * Time.deltaTime); 

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
        else if(beforeball.GetColor()==currentball.GetColor())
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
