using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PadlockController : MonoBehaviour
{
    // [Header("Zoom Panel & Box Swap")]
    // public GameObject closedBoxUI;       // 메인 화면 닫힌 상자 (Box_Closed)
    // public GameObject openBoxUI;         // 메인 화면 열린 상자 (Box_Open)

    [Header("Zoom Panel - Shackle Swap")]
    public Image zoomShackleImage;       // 자물쇠 고리 UI Image Component
    public Sprite zoomOpenShackleSprite; // '열린 고리' Sprite

    [Header("Keypad - Sprite Swap")]
    public List<Image> buttonUIImages;       // Button 1 ~ Button 8 순서대로 등록 (Element 0 = Button 1)
    public List<Sprite> normalButtonSprites; // 평상시 Sprite (8개)
    public List<Sprite> pressedButtonSprites;// 눌렸을 때 Sprite (8개)

    [Header("Password Settings")]
    public List<int> correctPassword = new List<int> { 5, 3, 2 }; // 직관적으로 '5, 3, 2' 입력하면 됨!

    private List<int> currentInputs = new List<int>();
    private bool isUnlocked = false;

    public bool IsUnlocked => isUnlocked;

    // 자판 버튼 클릭시 (1 ~ 8 전달)
    public void OnKeypadClicked(int digit)
    {
        if (isUnlocked || currentInputs.Count >= 3) return;

        currentInputs.Add(digit);

        // 버튼 1이 클릭되었을 때 리스트 0번 인덱스(Element 0)에 접근하도록 (digit - 1) 처리
        int index = digit - 1;

        if (index >= 0 && index < buttonUIImages.Count && buttonUIImages[index] != null)
        {
            if (index < pressedButtonSprites.Count && pressedButtonSprites[index] != null)
            {
                buttonUIImages[index].sprite = pressedButtonSprites[index];
            }
        }

        // 3자리 입력 시 검사
        if (currentInputs.Count == 3)
        {
            StartCoroutine(CheckPasswordRoutine());
        }
    }

    private IEnumerator CheckPasswordRoutine()
    {
        yield return new WaitForSeconds(0.25f);

        if (IsCorrect())
        {
            StartCoroutine(UnlockSequence());
        }
        else
        {
            ResetInputs();
        }
    }

    private bool IsCorrect()
    {
        for (int i = 0; i < 3; i++)
        {
            if (currentInputs[i] != correctPassword[i])
                return false;
        }
        return true;
    }

    private IEnumerator UnlockSequence()
    {
        isUnlocked = true;

        // 1. 자물쇠 고리 열림 연출 (자동으로 줌아웃 안 됨!)
        if (zoomShackleImage != null && zoomOpenShackleSprite != null)
        {
            zoomShackleImage.sprite = zoomOpenShackleSprite;
        }

        yield return new WaitForSeconds(0.3f);

        // 메인 화면 닫힌 상자 -> 열린 상자로 상태 전환
        // if (closedBoxUI != null) closedBoxUI.SetActive(false);
        // if (openBoxUI != null) openBoxUI.SetActive(true);

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved("2D_1", PuzzleDimension.TwoD);
        }
        else
        {
            Debug.LogWarning("[PadlockController] PuzzleManager.Instance를 찾을 수 없습니다.");
        }
    }

    private void ResetInputs()
    {
        currentInputs.Clear();
        for (int i = 0; i < buttonUIImages.Count; i++)
        {
            if (buttonUIImages[i] != null && i < normalButtonSprites.Count)
            {
                buttonUIImages[i].sprite = normalButtonSprites[i];
            }
        }
    }
}