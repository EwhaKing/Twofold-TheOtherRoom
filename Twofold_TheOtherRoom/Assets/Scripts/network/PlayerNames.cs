/// <summary>
/// 화면에 띄울 플레이어 이름.
///
/// 나중에 Steam 닉네임을 받아올 자리. 그때 여기만 고치면 되게 한곳에 모아둠.
/// (사람마다 이름이 달라지면 네트워크로 주고받아야 하므로 GameSession에 필드가 생김)
/// </summary>
public static class PlayerNames
{
    public const string Host  = "Player 1";
    public const string Guest = "Player 2";
}
