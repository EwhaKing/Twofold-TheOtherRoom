using UnityEngine;
using System.Collections;

public class GameManager2D10 : MonoBehaviour
{    
    public Tube2D10[] tube;
    
    public Tube2D10 currentTube;
    public Tube2D10 nextTube;
    public Ball2D10 topball;
    
    public bool isSelected=false;
    public bool isAnimating=false;

    private float tubeTopPos=5f;          //확인 필요구간 
    private Vector3 targetPos;

    public IEnumerator GoUp()
    {
        isAnimating = true;
        if (currentTube == null)
            yield break;
        topball = currentTube.GetTopBall();

        targetPos = currentTube.GetTubePosition();
        targetPos.y = tubeTopPos;
        yield return StartCoroutine(MoveBall(topball, targetPos));
        isAnimating = false;
    }
    public IEnumerator GoMove()
    {
        isAnimating = true;
        if (currentTube == null)
            yield break;
        topball = currentTube.GetTopBall();
            
        targetPos = nextTube.GetTubePosition();
        targetPos.y = tubeTopPos;
        yield return StartCoroutine(MoveBall(topball, targetPos));

        targetPos.y = nextTube.GetNextBallPositionY();
        yield return StartCoroutine(MoveBall(topball, targetPos));
        currentTube.Pop();
        currentTube=null;
        nextTube.Push(topball);
        nextTube=null;
        isAnimating = false;
    }

    private IEnumerator MoveBall(Ball2D10 ball, Vector3 targetPos)
    {   

        while (Vector3.Distance(ball.transform.position, targetPos) > 0.01f)
        {
            ball.transform.position = Vector3.MoveTowards(
                ball.transform.position,
                targetPos,
                5f * Time.deltaTime);   //확인필요

            yield return null;
        }

        ball.transform.position = targetPos;
    }


}
