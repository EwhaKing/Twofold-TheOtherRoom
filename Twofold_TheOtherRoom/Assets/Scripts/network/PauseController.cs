using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PauseController : MonoBehaviour
{
    [SerializeField] Button pauseButton;

    [Header("Pause Panel")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] TMP_Text headerText;
    [SerializeField] Button resumeButton;
    [SerializeField] Button exitButton;

    [Header("Setting Panel")]
    [SerializeField] Button settingButton;
    [SerializeField] SettingsPanel settingPanel;

    [Header("Exit Confirm Panel")]
    [SerializeField] GameObject exitConfirmPanel;
    [SerializeField] Button exitConfirmYes;
    [SerializeField] Button exitConfirmNo;

    [Header("Notice Panel")]
    [SerializeField] GameObject noticePanel;
    [SerializeField] TMP_Text noticeText;

    /// 일시정지 차단. Esc · 일시정지 버튼 둘 다 잠김. TimeoutPresenter 가 켬
    public bool BlockPause { get; set; }

    bool _lastPause;
    bool _lastBlockPause;

    void Start()
    {
        // 버튼 메서드 등록
        pauseButton.onClick.AddListener(OnPause);
        resumeButton.onClick.AddListener(OnResume);
        settingButton.onClick.AddListener(OpenSetting);
        exitButton.onClick.AddListener(OpenExitConfirm);
        exitConfirmYes.onClick.AddListener(OnExit);
        exitConfirmNo.onClick.AddListener(CloseExitConfirm);

        // 이벤트 구독
        RoomService.Instance.PeerLeftDuringGamePlay += OnPeerLeft;
        settingPanel.Closed += CloseSetting;
    }

    void Update()
    {
        var gs = GameSession.Instance;
        bool pause = gs != null && gs.IsPaused;

        ApplyBlockPause();
        HandleEscape(pause);

        if (pause == _lastPause) return;
        _lastPause = pause;

        // 값 바뀔 때마다 적용: 게임 멈춤/진행, 패널 상태 바꾸고, 텍스트 ui 적용, 재개 버튼 클릭 설정
        Time.timeScale = pause ? 0f : 1f;
        pausePanel.SetActive(pause);
        if (pause)
        {
            bool pausedByHost = gs.PausedBy == gs.Object.StateAuthority;
            headerText.text = pausedByHost ? $"{PlayerNames.Host}님이 일시정지했습니다." : $"{PlayerNames.Guest}님이 일시정지했습니다.";
            resumeButton.interactable = gs.PausedBy == RoomService.Instance.Runner.LocalPlayer;
        }
        // 설정창/확인창은 Panel_Pause 형제라 같이 안 꺼짐. 상대가 재개해도 남지 않게
        else
        {
            CloseSetting();
            CloseExitConfirm();
        }
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if(RoomService.Instance != null)
            RoomService.Instance.PeerLeftDuringGamePlay -= OnPeerLeft;

        if(settingPanel != null)
            settingPanel.Closed -= CloseSetting;
    }

    #region Escape
    /// 일시정지 토글. 재개는 일시정지를 건 쪽만
    void HandleEscape(bool pause)
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // 퇴장 안내 · 시간 종료 중엔 Esc 무시.
        if (noticePanel.activeSelf) return;
        if (BlockPause) return;

        if (!pause)
        {
            OnPause();
            return;
        }

        // 재개 버튼과 같은 조건
        if (resumeButton.interactable)
            OnResume();
    }
    #endregion

    /// 일시정지 버튼 잠금
    void ApplyBlockPause()
    {
        if (BlockPause == _lastBlockPause) return;
        _lastBlockPause = BlockPause;

        pauseButton.interactable = !BlockPause;
    }

    #region Button Method
    void OnPause()
    {
        // RPC 호출
        GameSession.Instance?.RpcRequestPause();
    }

    void OnResume()
    {
        // RPC 호출
        GameSession.Instance?.RpcRequestResume();
    }

    void OpenSetting()
    {
        settingPanel.gameObject.SetActive(true);
        settingPanel.Open();
    }

    void CloseSetting()
    {
        settingPanel.Cancel();
        settingPanel.gameObject.SetActive(false);
    }

    void OpenExitConfirm()
    {
        exitConfirmPanel.SetActive(true);
    }

    void CloseExitConfirm()
    {
        exitConfirmPanel.SetActive(false);
    }

    void OnExit()
    {
        Time.timeScale = 1f;
        RoomService.Instance.Leave();
    }
    #endregion

    #region When Other Player Exit
    void OnPeerLeft(string nickname)
    {
        if (BlockPause) return;   // 시간 종료 중. 이미 종료 패널이 떠 있음

        StartCoroutine(LeaveNoticeRoutine(nickname));
    }

    IEnumerator LeaveNoticeRoutine(string nickname)
    {
        noticeText.text = $"{nickname}님이 나갔습니다.\n 5초 뒤 타이틀 화면으로 돌아갑니다.";
        noticePanel.SetActive(true);

        yield return new WaitForSecondsRealtime(5f);

        Time.timeScale = 1f;
        RoomService.Instance.Leave();
    }
    #endregion
}
