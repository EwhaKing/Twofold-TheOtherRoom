using TMPro;
using UnityEngine;

/// <summary>
/// 인트로 나레이션 더빙과 자막을 재생.
///
/// 그리기만 담당하고 진행 시점은 밖에서 결정. 구동은 IntroSequencer 담당.
/// Begin → ApplyTime(경과, 일시정지) → Finish 순서로 호출.
///
/// 자막은 매 프레임 시각으로 다시 뽑고, 오디오는 시계로 평가할 수 없어 어긋난 만큼만 되감음.
/// </summary>
public class IntroNarration : MonoBehaviour
{
    /// <summary>자막 한 줄. 시각은 나레이션 시작 기준.</summary>
    [System.Serializable]
    public struct Line
    {
        [Tooltip("표시 시작 시각(초). 다음 줄이 나올 때까지 유지")]
        public float start;

        [TextArea(1, 3)] public string text;
    }

    [Header("References")]
    private AudioSource source;

    [Tooltip("더빙 클립. Preload Audio Data 켤 것")]
    [SerializeField] private AudioClip clip;

    [SerializeField] private TMP_Text subtitle;

    [Header("Content")]
    // start 는 파형의 무음 구간을 재서 앞쪽 쉼 안에 들어가도록 잡음. 클립을 다시 뽑으면 다시 잴 것
    [Tooltip("자막. start 오름차순으로 넣을 것. 마지막 줄은 클립 끝까지 유지")]
    [SerializeField]
    private Line[] lines =
    {
        new Line { start =  0.00f, text = "Twofold에 오신 것을 환영합니다." },
        new Line { start =  2.90f, text = "두 사람은 서로 다른 차원에서 게임을 시작합니다." },
        new Line { start =  6.70f, text = "2D에서는 마우스로 조작하고," },
        new Line { start =  9.20f, text = "3D에서는 WASD로 이동하며 E키 또는 마우스로 상호작용합니다." },
        new Line { start = 16.10f, text = "각 차원의 퍼즐을 풀고, 숨겨진 거울 조각을 찾아 각자의 거울을 완성해야 합니다." },
        new Line { start = 22.70f, text = "퍼즐은 총 10개. 각 차원에는 5개의 거울 조각이 존재합니다." },
        new Line { start = 28.60f, text = "제한 시간은 15분입니다." },
        new Line { start = 31.30f, text = "두 차원의 협력만이 이곳을 탈출할 수 있는 유일한 방법입니다." },
    };

    /// 시계와 오디오가 이만큼 벌어지면 되감음. 매 프레임 맞추면 소리가 끊김
    private const float MaxDrift = 0.25f;

    /// 이 구간에 들어오면 다시 재생하지 않음. 시계가 조금 뒤처졌을 때 끝말이 두 번 나오는 것 방지
    private const float NoRestartTail = 1f;

    private bool _ready;

    /// <summary>더빙 길이(초). 인트로 예산 검사용.</summary>
    public float Length => clip != null ? clip.length : 0f;

    // ---------- 외부 구동 (IntroSequencer) ----------

    /// <summary>참조를 확인하고 시작 상태로 전환. 아직 소리는 안 냄.</summary>
    public void Begin()
    {
        if (SoundManager.Instance != null)
        {
            source = SoundManager.Instance.NarrationSource;
        }
        // Awake 아님 — 자막이 인트로 전용이라 씬에 꺼진 채 저장돼 있을 수 있음
        _ready = source != null && clip != null && subtitle != null;
        if (!_ready)
        {
            Debug.LogError("[Intro] source / clip / subtitle 인스펙터 참조 연결할 것", this);
            return;
        }

        source.clip         = clip;
        source.playOnAwake  = false;
        source.loop         = false;
        source.spatialBlend = 0f;   // 나레이션은 위치 없음. 임포터의 3D 기본값 무시

        source.Stop();
        subtitle.text = string.Empty;
    }

    /// <summary>나레이션 시작 기준 경과 시각을 넣으면 자막과 오디오에 반영. 음수면 아직 시작 전.</summary>
    public void ApplyTime(float seconds, bool paused)
    {
        if (!_ready) return;

        ApplySubtitle(seconds);
        ApplyAudio(seconds, paused);
    }

    /// <summary>소리와 자막을 끔.</summary>
    public void Finish()
    {
        if (!_ready) return;

        source.Stop();
        subtitle.text = string.Empty;
    }

    // ---------- 내부 ----------

    private void ApplySubtitle(float seconds)
    {
        string text = string.Empty;

        foreach (var line in lines)
        {
            if (seconds < line.start) break;
            text = line.text;
        }

        if (subtitle.text != text) subtitle.text = text;   // 같은 값을 넣어도 TMP 가 매번 다시 그림
    }

    private void ApplyAudio(float seconds, bool paused)
    {
        // 시작 전 · 재생 끝난 뒤
        if (seconds < 0f || seconds >= clip.length)
        {
            if (source.isPlaying) source.Stop();
            return;
        }

        if (paused)
        {
            if (source.isPlaying) source.Pause();
            return;
        }

        // 첫 재생 · 일시정지 해제 · 방금 멈춘 상태. 어느 쪽이든 시계 위치에서 다시 시작
        if (!source.isPlaying)
        {
            if (seconds > clip.length - NoRestartTail) return;

            source.time = seconds;
            source.Play();
            return;
        }

        // 프레임 스톨로 시계가 앞서 나간 만큼만 따라잡음
        if (Mathf.Abs(source.time - seconds) > MaxDrift)
            source.time = seconds;
    }
}
