using UnityEngine;
using UnityEngine.UI;

public class FloorHole2D : MonoBehaviour
{
    public static FloorHole2D Instance { get; private set; }

    [SerializeField] private Image holeImage;

    [Header("Hole")]
    [SerializeField] private Sprite closedSprite; // 닫힌 개구멍
    [SerializeField] private Sprite openSprite;   // 열린 개구멍

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameSession.Instance != null)
        {
            SetOpen(GameSession.Instance.BasementOpen);
        }
        else
        {
            SetOpen(false);
        }
    }

    public void SetOpen(bool open)
    {
        if (holeImage == null)
            return;

        holeImage.sprite = open ? openSprite : closedSprite;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
