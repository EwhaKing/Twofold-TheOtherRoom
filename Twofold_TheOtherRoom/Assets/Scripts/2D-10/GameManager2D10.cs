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

    [SerializeField] private float liftHeight = 400f;
    [SerializeField] private float ballMoveSpeed = 1900f;
    private Vector2 targetPos;
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
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFXType.CompleteCorrectBtn);
            }
            PuzzleManager.Instance.ReportSolved(
                "2D-10",
                PuzzleDimension.TwoD
            );
        }
    }
    public IEnumerator GoUp()
    {
        isAnimating = true;
        targetPos = currentTube.GetPositionForBall(currentball);
        targetPos.y += liftHeight;
        yield return StartCoroutine(MoveBall(currentball, targetPos));
        currentTube.Pop();
        isAnimating = false;
    }
    public IEnumerator GoMove()
    {
        isAnimating = true;

        targetPos = nextTube.GetPositionForBall(currentball);
        targetPos.y += liftHeight;
        yield return StartCoroutine(MoveBall(currentball, targetPos));

        targetPos = nextTube.GetNextBallPosition(currentball);
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

    private IEnumerator MoveBall(Ball2D10 ball, Vector2 targetPos)
    {
        RectTransform ballRect = ball.GetComponent<RectTransform>();

        while (Vector2.Distance(ballRect.anchoredPosition, targetPos) > 0.1f)
        {
            ballRect.anchoredPosition = Vector2.MoveTowards(
                ballRect.anchoredPosition,
                targetPos,
                ballMoveSpeed * Time.deltaTime);

            yield return null;
        }

        ballRect.anchoredPosition = targetPos;
    }

    public bool CanMove()
    {
        if(beforeball==null)
        {
            return true;
        } 
        // else if(beforeball.GetColor()!=currentball.GetColor() || nextTube.ball[3]!=null)
        else if(nextTube.ball[3]!=null)
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

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
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
