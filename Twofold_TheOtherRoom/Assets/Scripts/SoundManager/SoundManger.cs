using UnityEngine;

/// <summary>
/// 게임에서 사용할 모든 효과음 종류(효과음 추가할때마다 하나씩 추가해야함)
/// </summary>
public enum BGMType
{
    StartBGM,
    WhiteNoise,
    BasementBG
}
public enum SFXType
{
    Paper,
    LockOpen,
    DrawerOpen,
    DrawerClose,
    FloorHole,
    CabinetOpen,
    CabinetClose,
    DefaultClick,
    RoomMove,
    Rug,

    SteppingNormal,
    SteppingCorrect,
    Scrape,
    Knife,
    DoorOpen,
    ButtonPush,
    FootStep,

    TickTok,
    Ding,
    WrongBtn,
    CorrectBtn,
    CompleteCorrectBtn,
    Beep1,
    Beep2,
    HeartBeat,

    UIClick,

    AlphaS,
    AlphaE,
    AlphaL,
    AlphaF
}

/// <summary>
/// 게임 전체의 BGM과 효과음을 관리하는 싱글톤 SoundManager
///
/// 사용 예시)
/// <BGM>
/// SoundManager.Instance.PlayBGM(BGMType.Title);
/// SoundManager.Instance.StopBGM();
/// SoundManager.Instance.PauseBGM();
/// SoundManager.Instance.ResumeBGM();
/// 
/// <SFX>
/// SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
/// SoundManager.Instance.PlaySFX(SFXType.PuzzleClear);
/// 
/// </summary>
/// 
public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// 싱글톤 인스턴스
    /// </summary>

    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public class BGMData
    {
        public BGMType type;
        public AudioClip clip;
    }

    [System.Serializable]
    public class SFXData
    {
        public SFXType type;
        public AudioClip clip;
    }

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Default BGM")]
    [SerializeField] private BGMType defaultBGM;

    [Header("BGM")]        
    [SerializeField] private BGMData[] bgmList;

    [Header("Sound Effects")]
    [SerializeField] private SFXData[] sfxList;


    private readonly System.Collections.Generic.Dictionary<BGMType, AudioClip> bgmDictionary
        = new System.Collections.Generic.Dictionary<BGMType, AudioClip>();

    private readonly System.Collections.Generic.Dictionary<SFXType, AudioClip> sfxDictionary
        = new System.Collections.Generic.Dictionary<SFXType, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBGM(defaultBGM);
    }

    /// <summary>
    /// Inspector에 등록된 효과음을 Dictionary에 저장
    /// </summary>
    private void InitializeDictionary()
    {
        bgmDictionary.Clear();
        sfxDictionary.Clear();

        foreach (BGMData bgm in bgmList)
        {
            if (bgm != null &&
                bgm.clip != null &&
                !bgmDictionary.ContainsKey(bgm.type))
            {
                bgmDictionary.Add(bgm.type, bgm.clip);
            }
        }

        foreach (SFXData sound in sfxList)
        {
            if (sound != null &&
                sound.clip != null &&
                !sfxDictionary.ContainsKey(sound.type))
            {
                sfxDictionary.Add(sound.type, sound.clip);
            }
        }
    }

    public void PlaySFX(SFXType type)
    {
        if (sfxDictionary.TryGetValue(type, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"등록되지 않은 효과음 : {type}");
        }
    }

    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBGM(BGMType type)
    {
        if (bgmDictionary.TryGetValue(type, out AudioClip clip))
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return;

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"등록되지 않은 BGM : {type}");
        }
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }
    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    /// <summary>
    /// 볼륨 설정
    /// </summary>
    private float masterVolume = 1f;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    private void ApplyVolume()
    {
        bgmSource.volume = masterVolume * bgmVolume;
        sfxSource.volume = masterVolume * sfxVolume;
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }
}