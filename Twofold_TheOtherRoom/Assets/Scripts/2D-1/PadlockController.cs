using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PadlockController : MonoBehaviour
{
    [Header("UI Panels & Boxes")]
    public GameObject lockZoomPanel;    // 자물쇠 확대 패널
    public GameObject closedBoxUI;      // 메인 화면 닫힌 상자 (Box_Closed)
    public GameObject openBoxUI;        // 메인 화면 열린 상자 (Box_Open)

    [Header("Zoom Panel - Shackle Swap")]
    public Image zoomShackleImage;      // 확대창 자물쇠 고리 UI Image Component
    public Sprite zoomOpenShackleSprite;// 확대창 자물쇠 '열린 고리(Shackle_Open)' Sprite

    [Header("Keypad - Sprite Swap")]
    public List<Image> buttonUIImages;       // 자판 UI의 Image 컴포넌트 10개 (0 ~ 9)
    public List<Sprite> normalButtonSprites; // 평상시(밝은) 버튼 Sprite 10개 (0 ~ 9)
    public List<Sprite> pressedButtonSprites;// 눌렸을 때(어두운) 버튼 Sprite 10개 (0 ~ 9)

    [Header("Password Settings")]
    public List<int> correctPassword = new List<int> { 5, 3, 2 }; // 정답 비밀번호 532

    private List<int> currentInputs = new List<int>();
    private bool isUnlocked = false;

    // 1. 메인 닫힌 상자 클릭 시 자물쇠 확대창 켜기
    public void OpenLockZoom()
    {
        if (isUnlocked) return;
        if (lockZoomPanel != null) lockZoomPanel.SetActive(true);
    }

    // 2. 자물쇠 확대창 닫기
    public void CloseLockZoom()
    {
        if (lockZoomPanel != null) lockZoomPanel.SetActive(false);
    }

    // 3. 자판 버튼 눌렀을 때 (0 ~ 9)
    public void OnKeypadClicked(int digit)
    {
        if (isUnlocked || currentInputs.Count >= 3) return;

        currentInputs.Add(digit);

        // [연출 1] 눌린 버튼의 Sprite를 어두운 버전 Sprite로 즉시 교체!
        if (digit >= 0 && digit < buttonUIImages.Count && buttonUIImages[digit] != null)
        {
            if (digit < pressedButtonSprites.Count && pressedButtonSprites[digit] != null)
            {
                buttonUIImages[digit].sprite = pressedButtonSprites[digit];
            }
        }

        // 3개 입력 완료 시 정답 검사
        if (currentInputs.Count == 3)
        {
            StartCoroutine(CheckPasswordRoutine());
        }
    }

    private IEnumerator CheckPasswordRoutine()
    {
        yield return new WaitForSeconds(0.25f); // 눌림 연출을 잠시 보여주기 위한 대기

        if (IsCorrect())
        {
            StartCoroutine(UnlockSequence());
        }
        else
        {
            // [연출 2] 3개 틀리면 리셋 및 다시 밝은 버전 Sprite로 원복
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

    // 4. 해제 성공 시 연출 시퀀스
    private IEnumerator UnlockSequence()
    {
        isUnlocked = true;

        // [확대창 연출] 큰 자물쇠 고리 이미지를 '열린 고리(Shackle_Open)' Sprite로 교체
        if (zoomShackleImage != null && zoomOpenShackleSprite != null)
        {
            zoomShackleImage.sprite = zoomOpenShackleSprite;
        }

        yield return new WaitForSeconds(0.6f);

        // [축소 연출] 자물쇠 확대창 닫기
        CloseLockZoom();

        // [메인 화면 연출] 메인 닫힌 상자 Off -> 상자열린거 On
        if (closedBoxUI != null) closedBoxUI.SetActive(false);
        if (openBoxUI != null) openBoxUI.SetActive(true);

        // [시스템 연동] PuzzleManager에 2D-1 퍼즐 해결 보고!
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.ReportSolved("2D-1", PuzzleDimension.TwoD);
        }
        else
        {
            Debug.LogWarning("[PadlockController] PuzzleManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    // [연출 2] 틀렸을 때 리셋 함수 (모든 자판을 밝은 Sprite로 복구)
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