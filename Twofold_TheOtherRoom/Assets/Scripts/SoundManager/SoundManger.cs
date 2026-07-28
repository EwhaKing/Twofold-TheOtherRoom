using UnityEngine;

/// <summary>
/// 게임에서 사용할 모든 효과음 종류(효과음 추가할때마다 하나씩 추가해야함)
/// </summary>
public enum SFXType
{
    TestSe,
    ButtonClick,
    DoorOpen,
    PuzzleClear,
    WrongAnswer,
    CorrectAnswer,
    ItemPickup,
    UIClick
}

/// <summary>
/// 게임 전체의 BGM과 효과음을 관리하는 싱글톤 SoundManager
///
/// 사용 예시)
/// SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
/// SoundManager.Instance.PlaySFX(SFXType.PuzzleClear);
/// </summary>
public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// 싱글톤 인스턴스
    /// </summary>
    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public class SFXData
    {
        public SFXType type;
        public AudioClip clip;
    }

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM")]
    [SerializeField] private AudioClip defaultBGM;

    [Header("Sound Effects")]
    [SerializeField] private SFXData[] sfxList;

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
        if (defaultBGM != null)
            PlayBGM(defaultBGM);
    }

    /// <summary>
    /// Inspector에 등록된 효과음을 Dictionary에 저장
    /// </summary>
    private void InitializeDictionary()
    {
        sfxDictionary.Clear();

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
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
            return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }
}