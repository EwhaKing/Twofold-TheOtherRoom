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
        if (openHole != null)
            openHole.SetActive(open);

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