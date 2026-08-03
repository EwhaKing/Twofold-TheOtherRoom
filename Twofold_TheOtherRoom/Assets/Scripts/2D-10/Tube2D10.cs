using UnityEngine;
using System.Collections;

public class Tube2D10 : MonoBehaviour
{
    public GameManager2D10 gameManager;
    public Ball2D10[] ball;
    
    private float ballSize=1.6f;
    private float noneballPosY=-1.85f;

    public void OnMouseDown()
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
                if (gameManager.nextTube.GetTopBall()==null)
                {
                    gameManager.isSelected=false;
                    StartCoroutine(gameManager.GoMove());
                }
                else
                {
                    if(gameManager.currentTube.GetTopBall().GetColor()!=gameManager.nextTube.GetTopBall().GetColor())
                    {
                        gameManager.nextTube=gameManager.currentTube;
                        gameManager.isSelected=false;
                        StartCoroutine(gameManager.GoMove());
                    }
                    else
                    {
                        gameManager.isSelected=false;
                        StartCoroutine(gameManager.GoMove());
                    }
                }

            }
            else
            {
                gameManager.currentTube=this;
                gameManager.isSelected=true;
                StartCoroutine(gameManager.GoUp());            
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
