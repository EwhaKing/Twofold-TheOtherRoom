using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 종료 패널. 제한시간 종료는 스스로 판정하고, 거울 완성 엔딩은 ShowEnding 을 불러 같은 패널을 쓴다.
/// 게임플레이 씬마다 하나. 패널이 아니라 항상 켜져 있는 오브젝트에 붙일 것 — 꺼진 패널 위에서는 Update 가 안 돈다.
/// </summary>
public class TimeoutPresenter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("시간 종료 시 켤 패널. 씬에는 꺼둔 채로 저장할 것")]
    [SerializeField] GameObject gameoverPanel;

    [Tooltip("나가기 버튼. 일시정지의 나가기와 같은 경로")]
    [SerializeField] Button exitButton;

    [Tooltip("시간 종료 문구. 씬에는 꺼둔 채로 저장할 것")]
    [SerializeField] GameObject timeoutText;

    [Tooltip("거울 완성 엔딩 문구. 씬에는 꺼둔 채로 저장할 것")]
    [SerializeField] GameObject endingText;

    [Tooltip("클리어 타임이 들어갈 칸. 00:00 형식으로 채움. 비워두면 표시 안 함")]
    [SerializeField] TMP_Text clearTimeText;

    [Tooltip("시간 종료와 함께 끌 UI. 패널이 풀스크린이면 비워둬도 됨")]
    [SerializeField] GameObject[] hideOnTimeout;

    [Tooltip("시간 종료 시 끌 입력 컴포넌트. 씬에는 켜둔 채로 저장할 것.\n" +
             "비워두면 PlayerController · PlayerLocomotionInput · PlayerInteractor 를 자동으로 찾음")]
    [SerializeField] Behaviour[] inputToLock;

    /// 이미 띄웠는지. 씬이 통째로 새로 로드되므로 리셋 안 함
    bool _fired;

    /// 클리어 타임(초). 음수면 아직. 방장이 나가면 GameSession 이 사라지므로 미리 들고 있음
    float _clearSeconds = -1f;

    /// 3D 커서 잠금 해제. 안 풀면 나가기 버튼을 못 누름
    readonly PlayerControlLock playerControlLock = new PlayerControlLock();

    void Awake()
    {
        if (gameoverPanel == null || exitButton == null)
        {
            Debug.LogError("[Timeout] gameoverPanel · exitButton 인스펙터 참조 연결할 것", this);
            enabled = false;
            return;
        }

        gameoverPanel.SetActive(false);
        exitButton.onClick.AddListener(OnExit);
    }

    void Update()
    {
        if (_fired) return;

        var gs = GameSession.Instance;
        if (gs == null) return;             // 단독 실행
        if (gs.StartedTick == 0) return;    // 상대 로드 대기

        // 클리어. 시계는 여기서 멈춰 있고, 그 값이 곧 클리어 타임
        if (gs.ClearedTick != 0)
        {
            if (_clearSeconds < 0f) _clearSeconds = gs.ElapsedSeconds;
            return;
        }

        if (GameSession.TotalSeconds - gs.ElapsedSeconds > 0f) return;

        Show(timeoutText);
    }

    /// <summary>거울 완성 엔딩. 시간 종료와 같은 패널, 문구만 다름</summary>
    public void ShowEnding() => Show(endingText);

    /// 패널을 띄우고 조작을 잠근다. 먼저 뜬 쪽이 이기고 이후 호출은 무시
    void Show(GameObject text)
    {
        if (_fired || gameoverPanel == null) return;
        _fired = true;

        if (timeoutText != null) timeoutText.SetActive(false);
        if (endingText != null) endingText.SetActive(false);
        if (text != null) text.SetActive(true);

        ApplyClearTime();

        foreach (GameObject go in hideOnTimeout)
            if (go != null) go.SetActive(false);

        gameoverPanel.SetActive(true);
        playerControlLock.Lock(this, inputToLock);

        var pause = FindAnyObjectByType<PauseController>();
        if (pause != null) pause.BlockPause = true;
    }

    /// 클리어 타임 채우기. 시간 종료거나 값을 못 잡았으면 칸을 끔
    void ApplyClearTime()
    {
        if (clearTimeText == null) return;

        if (_clearSeconds < 0f)
        {
            clearTimeText.gameObject.SetActive(false);
            return;
        }

        int t = Mathf.FloorToInt(_clearSeconds);
        clearTimeText.text = $"{t / 60:00}:{t % 60:00}";
    }

    /// 일시정지의 나가기와 같음. 씬 언로드 · 타이틀 복귀는 GameFlow 담당
    void OnExit()
    {
        Time.timeScale = 1f;   // 퍼즐이 멈춰둔 채였을 수 있음
        RoomService.Instance?.Leave();
    }
}
