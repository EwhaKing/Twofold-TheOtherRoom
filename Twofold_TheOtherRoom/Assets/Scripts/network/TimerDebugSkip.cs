using UnityEngine;

/// <summary>
/// 시계를 앞당겨 남은 시간을 줄인다. 종료 연출 테스트용.
/// StartedTick 이 [Networked] 라 양쪽 화면이 같이 점프. 방장만 누를 수 있음.
/// 에디터 · 개발 빌드에서만 동작.
/// </summary>
public class TimerDebugSkip : MonoBehaviour
{
    [Tooltip("점프 키")]
    [SerializeField] KeyCode key = KeyCode.T;

    [Tooltip("점프 후 남길 시간(초). 0 이면 즉시 종료")]
    [SerializeField] float remainSeconds = 6f;

    void Awake()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        enabled = false;   // 릴리스 빌드
#endif
    }

    void Update()
    {
        if (!Input.GetKeyDown(key)) return;

        var gs = GameSession.Instance;
        if (gs == null || gs.StartedTick == 0) return;   // 단독 실행 · 상대 로드 대기

        // StartedTick 은 방장만 쓸 수 있음. 게스트가 눌러도 값이 안 퍼짐
        if (!gs.Object.HasStateAuthority)
        {
            Debug.LogWarning("[TimerDebug] 방장만 시계를 앞당길 수 있음", this);
            return;
        }

        int now = gs.IsPaused ? gs.PausedTick : gs.Runner.Tick;
        float target = GameSession.IntroSeconds + GameSession.TotalSeconds - remainSeconds;
        int shifted = now - gs.TotalPausedTicks - Mathf.CeilToInt(target / gs.Runner.DeltaTime);

        gs.StartedTick = shifted != 0 ? shifted : -1;   // 0 은 "상대 로드 대기" 표식이라 피함

        Debug.Log($"[TimerDebug] 남은 시간 {remainSeconds}초로 점프");
    }
}
