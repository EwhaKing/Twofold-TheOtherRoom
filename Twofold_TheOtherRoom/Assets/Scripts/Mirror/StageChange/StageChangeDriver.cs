using UnityEngine;

/// <summary>
/// 전환 연출을 구동하고 완료를 서버에 보고. 게임플레이 씬마다 하나.
/// 양쪽이 거울을 클릭해 BothStageReady가 되면 같은 프레임에 재생이 시작.
/// </summary>
public class StageChangeDriver : MonoBehaviour
{
    [Tooltip("이 씬의 전환 연출. IStageCutscene을 구현한 컴포넌트를 넣을 것")]
    [SerializeField] private MonoBehaviour cutsceneSource;

    [Tooltip("Finished가 안 와도 이 시간이 지나면 보고(초).\n" +
             "한쪽이 안 끝나면 양쪽이 검은 화면에서 같이 멈춤")]
    [SerializeField] private float reportTimeoutSeconds = 40f;

    private IStageCutscene cutscene;

    private bool started;

    private bool reported;
    private float startedAt;

    /// 연출 동안 잠근 일시정지. 연출이 중간에 꺼지면 되돌림
    private PauseController blockedPause;

    private void Awake()
    {
        cutscene = cutsceneSource as IStageCutscene;

        if (cutscene == null)
        {
            Debug.LogError("[StageChange] cutsceneSource 에 IStageCutscene 구현 컴포넌트를 연결할 것", this);
            enabled = false;
            return;
        }

        cutscene.Finished += Report;
    }

    private void OnDestroy()
    {
        if (cutscene != null) cutscene.Finished -= Report;
    }

    private void OnDisable()
    {
        if (blockedPause == null) return;

        blockedPause.BlockPause = false;
        blockedPause = null;
    }

    private void Update()
    {
        GameSession session = GameSession.Instance;
        if (session == null) return;   // 단독 실행

        if (!started)
        {
            if (!session.BothStageReady) return;

            // timeScale 0 Play 시 UI만 꺼지고 연출 시작 못함. 재개 후 양쪽이 같이 시작
            if (session.IsPaused) return;

            started = true;
            startedAt = Time.time;

            // 연출 중에는 Esc도 일시정지 버튼도 잠금
            blockedPause = FindAnyObjectByType<PauseController>();
            if (blockedPause != null) blockedPause.BlockPause = true;

            cutscene.Play();
            return;
        }

        if (reported || Time.time - startedAt < reportTimeoutSeconds) return;

        Debug.LogWarning("[StageChange] 연출 완료 신호가 없어 시간 초과로 보고", this);
        Report();
    }

    private void Report()
    {
        if (reported) return;
        reported = true;

        RoomService room = RoomService.Instance;
        if (room == null) return;

        GameSession.Instance?.RpcReportStageDone(room.IsHost);
    }
}
