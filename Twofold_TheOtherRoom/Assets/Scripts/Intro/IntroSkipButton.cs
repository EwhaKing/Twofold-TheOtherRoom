using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인트로 건너뛰기 버튼. 두 사람이 다 눌러야 넘어감.
///
/// 누른 상태를 GameSession 에서 매 프레임 다시 읽어 그림. 상대가 먼저 눌러도 바로 반영됨.
/// IntroCanvas 안(introOnly)에 두면 인트로 동안만 보임.
/// </summary>
public class IntroSkipButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    [Header("Text")]
    [Tooltip("아직 안 눌렀을 때")]
    [SerializeField] private string idleText = "건너뛰기";

    [Tooltip("나만 눌렀을 때. 상대가 누르면 바로 넘어감")]
    [SerializeField] private string waitingText = "상대 대기 중…";

    private void Awake()
    {
        if (button == null || label == null)
        {
            Debug.LogError("[Intro] button / label 인스펙터 참조 연결할 것", this);
            enabled = false;
            return;
        }

        button.onClick.AddListener(Request);
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(Request);
    }

    private void Update()
    {
        var session = GameSession.Instance;
        bool requested = session != null && session.HasRequestedSkip(IsHost);

        // 상대 로드 대기 중에는 못 누름. 아직 인트로가 시작도 안 했음
        button.interactable = session != null
                              && session.Intro == IntroPhase.Running
                              && !requested;

        string text = requested ? waitingText : idleText;
        if (label.text != text) label.text = text;
    }

    private void Request()
    {
        var session = GameSession.Instance;
        if (session != null) session.RpcRequestSkipIntro(IsHost);
    }

    private static bool IsHost => RoomService.Instance != null && RoomService.Instance.IsHost;
}
