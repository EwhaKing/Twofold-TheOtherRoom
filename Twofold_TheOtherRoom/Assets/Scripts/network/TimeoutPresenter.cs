using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 제한시간 종료 연출. 게임플레이 씬마다 하나.
/// 패널이 아니라 항상 켜져 있는 오브젝트에 붙일 것 — 꺼진 패널 위에서는 Update 가 안 돈다.
/// </summary>
public class TimeoutPresenter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("시간 종료 시 켤 패널. 씬에는 꺼둔 채로 저장할 것")]
    [SerializeField] GameObject gameoverPanel;

    [Tooltip("나가기 버튼. 일시정지의 나가기와 같은 경로")]
    [SerializeField] Button exitButton;

    [Tooltip("시간 종료와 함께 끌 UI. 패널이 풀스크린이면 비워둬도 됨")]
    [SerializeField] GameObject[] hideOnTimeout;

    [Tooltip("시간 종료 시 끌 입력 컴포넌트. 씬에는 켜둔 채로 저장할 것.\n" +
             "비워두면 PlayerController · PlayerLocomotionInput · PlayerInteractor 를 자동으로 찾음")]
    [SerializeField] Behaviour[] inputToLock;

    /// 이미 띄웠는지. 씬이 통째로 새로 로드되므로 리셋 안 함
    bool _fired;

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
        if (gs.ClearedTick != 0) return;    // 클리어. 시계는 멈췄지만 복제 지연 대비

        if (GameSession.TotalSeconds - gs.ElapsedSeconds > 0f) return;

        Fire();
    }

    /// <summary>시간 종료. 패널을 띄우고 조작을 잠근다.</summary>
    void Fire()
    {
        _fired = true;

        foreach (GameObject go in hideOnTimeout)
            if (go != null) go.SetActive(false);

        gameoverPanel.SetActive(true);
        playerControlLock.Lock(this, inputToLock);

        var pause = FindAnyObjectByType<PauseController>();
        if (pause != null) pause.BlockPause = true;
    }

    /// 일시정지의 나가기와 같음. 씬 언로드 · 타이틀 복귀는 GameFlow 담당
    void OnExit()
    {
        Time.timeScale = 1f;   // 퍼즐이 멈춰둔 채였을 수 있음
        RoomService.Instance?.Leave();
    }
}
