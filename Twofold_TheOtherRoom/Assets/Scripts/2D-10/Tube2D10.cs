using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;


public class Tube2D10 : MonoBehaviour, IPointerClickHandler
{
    public GameManager2D10 gameManager;

    private Ball2D10[] startBalls;
    private Vector2[] startPositions;

    public Ball2D10[] ball;
    public enum TubeColor
    {
        Red,
        Yellow,
        Green
    }  
    public TubeColor tubeColor;
 
    public Image lightImage;
    public bool solve;

    [SerializeField] private float ballSpacing = 145f;
    [SerializeField] private float bottomBallOffset = -270f;

    public void Start()
    {
        solve=false;

        startBalls = new Ball2D10[ball.Length];
        startPositions = new Vector2[ball.Length];

        for (int i = 0; i < ball.Length; i++)
        {
            startBalls[i] = ball[i];

            if (ball[i] != null)
            {
                startPositions[i] = ball[i].GetComponent<RectTransform>().anchoredPosition;
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {   
        if (gameManager.isAnimating || solve==true)
        {
            return;
        }
        else
        {
            if (gameManager.isSelected)
            {  
                gameManager.nextTube=this;
                gameManager.beforeball=this.GetTopBall();
                if(gameManager.CanMove())
                {
                    gameManager.isSelected=false;
                    StartCoroutine(gameManager.GoMove());
                }
                else
                {
                    gameManager.beforeball=gameManager.currentTube.GetTopBall();
                    gameManager.nextTube=gameManager.currentTube;
                    gameManager.isSelected=false;
                    StartCoroutine(gameManager.GoMove());
                }
            }
            else //첫번째 튜브 선택
            {
                gameManager.currentTube=this;
                gameManager.currentball=this.GetTopBall();
                if(gameManager.currentball!=null){
                    gameManager.isSelected=true;
                    StartCoroutine(gameManager.GoUp()); 
                }
                else{
                    gameManager.currentTube=null;
                }
            }
        }
    }
    public TubeColor GetColor()
    {
        return tubeColor;
    }

    public Ball2D10 GetTopBall()
    {
        for (int i = ball.Length - 1; i >= 0; i--)
        {
            if (ball[i] != null)
                return ball[i];
        }
        return null;
    }

    public Ball2D10 Pop()//공 빼낼때, Tube에서 topball 뺀 정보 저장
    {
        for (int i = ball.Length - 1; i >= 0; i--)
        {
            if (ball[i] != null)
            {
                Ball2D10 top = ball[i];
                ball[i] = null;
                return top;
            }
        }
        return null;
    }
    public void Push(Ball2D10 newBall) //공 넣을때, Tube에서 topball 추가한 정보 저장
    {
        for (int i = 0; i < ball.Length; i++)
        {
            if (ball[i] == null)
            {
                ball[i] = newBall;
                return;
            }
        }
    }
    public Vector2 GetPositionForBall(Ball2D10 targetBall)
    {
        // The ball moves with anchoredPosition.
        RectTransform ballRect = targetBall.GetComponent<RectTransform>();

        // Get the ball's coordinate-space parent.
        Transform ballParent = ballRect.parent;

        // Convert the tube position to the ball parent's local space.
        Vector2 localPoint = ballParent.InverseTransformPoint(transform.position);

        RectTransform ballParentRect = ballParent as RectTransform;

        // A normal Transform has no anchor offset.
        if (ballParentRect == null)
        {
            return localPoint;
        }

        // Get the ball's anchor point in the parent rect.
        Vector2 anchorReference = new Vector2(
            Mathf.Lerp(ballParentRect.rect.xMin, ballParentRect.rect.xMax, ballRect.anchorMin.x),
            Mathf.Lerp(ballParentRect.rect.yMin, ballParentRect.rect.yMax, ballRect.anchorMin.y));

        // Return the matching anchoredPosition.
        return localPoint - anchorReference;
    }

    public Vector2 GetNextBallPosition(Ball2D10 targetBall)
    {
        int count = 0;

        foreach (Ball2D10 b in ball)
        {
            if (b != null)
                count++;
        }

        Vector2 position = GetPositionForBall(targetBall);
        position.y += bottomBallOffset + (count * ballSpacing);
        return position;
    }
    public void Answer(TubeColor tubeColor)
    {
        if (ball[3] != null)
        {
            Ball2D10.BallColor color = (Ball2D10.BallColor)tubeColor;

            if (ball.All(ball => ball.GetColor() == color))
            {
                if (lightImage != null)
                {
                    lightImage.color = new Color32(51, 255, 51, 255);

                    if (!solve)
                    {
                        solve = true;
                        bool puzzleCompleted =
                            gameManager.tube[0].solve &&
                            gameManager.tube[1].solve &&
                            gameManager.tube[2].solve;
                        if (!puzzleCompleted && SoundManager.Instance != null)
                        {
                            SoundManager.Instance.PlaySFX(SFXType.CorrectBtn);
                        }
                    }
                }
            }
        }
    }
    public void ResetTube()
    {
        for (int i = 0; i < ball.Length; i++)
        {
            ball[i] = startBalls[i];

            if (ball[i] != null)
            {
                ball[i].GetComponent<RectTransform>().anchoredPosition = startPositions[i];
            }
        }

        solve = false;

        if (lightImage != null)
        {
            lightImage.color = Color.white;
        }
    }
}
