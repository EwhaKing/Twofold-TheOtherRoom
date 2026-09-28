using UnityEngine;
using UnityEngine.UI;

public class FloorHole2D : MonoBehaviour
{
    public static FloorHole2D Instance { get; private set; }

    [SerializeField] private Image holeImage;

    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    [SerializeField] private DetailView detailView;
    [SerializeField] private RoomFloorEntrance floorEntrance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameSession.Instance != null)
            SetOpen(GameSession.Instance.BasementOpen);
        else
            SetOpen(false);
    }

    public void SetOpen(bool open)
    {
        if (holeImage != null)
            holeImage.sprite = open ? openSprite : closedSprite;

        // 닫혀 있을 때:
        if (detailView != null)
            detailView.enabled = !open;

        // 열려 있을 때:
        if (floorEntrance != null)
            floorEntrance.enabled = open;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}