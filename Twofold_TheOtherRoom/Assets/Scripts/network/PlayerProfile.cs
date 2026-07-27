using UnityEngine;

/// <summary>
/// 이 PC의 플레이어 이름. 네트워크와 무관한 로컬 저장소.
///
/// PlayerProfile.Nickname = 입력값; 로 연결 가능
/// 네트워크로 나가는 건 GameSession(HostName / GuestName)이 담당
/// </summary>
public static class PlayerProfile
{
    const string PrefsKey = "twofold.nickname";

    /// GameSession의 NetworkString 크기에 맞춤. 넘기면 안됨.
    public const int MaxLength = 12;

    public const string DefaultNickname = "플레이어";

    // null = 아직 PlayerPrefs를 안 읽음
    static string _cached;

    public static string Nickname
    {
        get
        {
            if (_cached == null)
                _cached = Sanitize(PlayerPrefs.GetString(PrefsKey, string.Empty));
            return string.IsNullOrEmpty(_cached) ? DefaultNickname : _cached;
        }
        set
        {
            _cached = Sanitize(value);
            PlayerPrefs.SetString(PrefsKey, _cached);
            PlayerPrefs.Save();
        }
    }

    /// 빈 이름을 화면에 그대로 띄우지 않기 위한 대체값
    public static string OrDefault(string name)
        => string.IsNullOrWhiteSpace(name) ? DefaultNickname : name.Trim();

    /// <summary>
    /// 로그 문장에 넣을 형태로 만듦. "님"
    /// </summary>
    public static string Honorific(string name) => OrDefault(name) + " 님";

    static string Sanitize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        raw = raw.Trim();
        return raw.Length > MaxLength ? raw.Substring(0, MaxLength) : raw;
    }
}
