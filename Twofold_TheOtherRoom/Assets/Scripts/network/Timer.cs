using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] TMP_Text timeText;

    [SerializeField] private AudioClip fiveMinuteNarration;

    private int _lastTime = -1; // 값 다를 때만 UI 반영하도록.
    private bool _fiveMinutePlayed = false;

    void Awake()
    {
        if (timeText == null)
        {
            Debug.LogError("[Timer] timeText 인스펙터 참조 연결할 것", this);
            enabled = false;
        }
    }

    void Update()
    {
        int t;

        // 시작 전이면 전체 시간 그대로
        if (GameSession.Instance == null || GameSession.Instance.StartedTick == 0)
            t = Mathf.CeilToInt(GameSession.TotalSeconds);

        // 아니면 계산. 남은 시간 = 전체 - 지난 시간
        else
        {
            float remainTime = GameSession.TotalSeconds - GameSession.Instance.ElapsedSeconds;
            t = Mathf.CeilToInt(Mathf.Clamp(remainTime, 0f, GameSession.TotalSeconds));
        }

        // 제한시간 5분 남았을 때 나레이션
        if (t <= 300 && !_fiveMinutePlayed)
        {
            _fiveMinutePlayed = true;

            if (SoundManager.Instance != null &&
                SoundManager.Instance.NarrationSource != null &&
                fiveMinuteNarration != null)
            {
                AudioSource source = SoundManager.Instance.NarrationSource;
                source.clip = fiveMinuteNarration;
                source.Play();
            }
        }

        if (t == _lastTime) return;

        _lastTime = t;
        timeText.text = $"{t / 60:00}:{t % 60:00}";
    }
}