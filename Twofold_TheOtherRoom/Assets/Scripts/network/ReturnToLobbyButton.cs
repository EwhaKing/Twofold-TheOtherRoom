using UnityEngine;
using UnityEngine.UI;

/// <summary>종료 패널의 로비 버튼. 내 화면만 먼저 로비로 복귀</summary>
public class ReturnToLobbyButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null)
        {
            Debug.LogError("[ReturnLobby] button 인스펙터 참조 연결할 것", this);
            enabled = false;
            return;
        }

        button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(OnClick);
    }

    // 로비를 이어갈 수 없어 비활성
    private void Update()
    {
        if (IsAloneGuest() && button.interactable) button.interactable = false;
    }

    private void OnClick()
    {
        if (IsAloneGuest()) return;   // 비활성 직전 프레임의 클릭

        if (GameFlow.Instance != null) GameFlow.Instance.ReturnToLobby();
    }

    // 방장이 나가 혼자 남은 게스트. IsHost 는 방장 퇴장 시 게스트로 넘어가므로 권한으로 판정
    private static bool IsAloneGuest()
    {
        var room = RoomService.Instance;
        if (room == null || !room.IsInRoom || room.PlayerCount >= 2) return false;

        var session = GameSession.Instance;
        return session == null || session.Object == null || !session.Object.HasStateAuthority;
    }
}
