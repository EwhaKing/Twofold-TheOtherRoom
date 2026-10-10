using UnityEngine;

public class FloorHole2D : MonoBehaviour
{
    public static FloorHole2D Instance { get; private set; }

    [SerializeField] private GameObject openHole;
    [SerializeField] private GameObject closeHole;

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
        // 닫힘 → 열림으로 바뀌는 순간에만 문 열림 효과음
        bool justOpened = open && openHole != null && !openHole.activeSelf;

        if (openHole != null)
            openHole.SetActive(open);

        if (justOpened && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(SFXType.DoorOpen);

        if (closeHole != null)
            closeHole.SetActive(!open);

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