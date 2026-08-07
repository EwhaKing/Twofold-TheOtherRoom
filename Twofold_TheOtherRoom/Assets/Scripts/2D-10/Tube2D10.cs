using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tube2D10 : MonoBehaviour, IPointerClickHandler
{
    public GameManager2D10 gameManager;

    public Ball2D10[] ball;
    
    private float ballSize=1.6f;
    private float noneballPosY=-1.15f;

    public void OnPointerClick(PointerEventData eventData)
    {   
        if (gameManager.isAnimating)
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
    public Ball2D10 GetTopBall()
    {
        for (int i = ball.Length - 1; i >= 0; i--)
        {
            if (ball[i] != null)
                return ball[i];
        }
        return null;
    }
    public bool IsEmpty()
    {
        return GetTopBall() == null;
    }

    public bool IsFull()
    {
        return ball[ball.Length - 1] != null;
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
    public Vector3 GetTubePosition()
    {
        return transform.position;
    }

    public float GetNextBallPositionY() 
    {
        int count = 0;

        foreach (Ball2D10 b in ball)
        {
            if (b != null)
                count++;
        }

        return noneballPosY + (count * ballSize);
    }
}
