using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 물체를 클릭하면 해당 물체의 확대 프리팹을 DetailRoot에 생성합니다.
///
/// 사용법
/// 1. Canvas에 이 컴포넌트를 붙이고 Detail View와 Detail Root를 연결합니다.
/// 2. 각 물체에도 이 컴포넌트를 붙이고, Canvas의 DetailView 컴포넌트와
///    해당 물체의 Detail Prefab을 연결합니다.
/// 3. BackButton의 OnClick에는 Canvas에 붙인 컴포넌트의 CloseDetail을 연결합니다.
/// </summary>
public class DetailView : MonoBehaviour , IPointerClickHandler
{
    [Header("Canvas에서 설정")]
    [SerializeField] private GameObject detailView;
    [SerializeField] private Transform detailRoot;

    [Header("각 물체에서 설정")]
    [SerializeField] private DetailView manager;
    [SerializeField] private GameObject detailPrefab;



    private GameObject currentDetail;
    private int lastOpenedFrame = -1;

    private bool IsManager => detailView != null && detailRoot != null;

    private void Awake()
    {
        // Canvas에 붙인 관리자 컴포넌트만 시작 시 확대 화면을 닫습니다.
        if (IsManager)
        {
            detailView.SetActive(false);
        }
    }

    /// UI Image/Button에서 이 함수를 직접 연결할 수도 있습니다.
    public void OpenDetail()
    {
        if (manager == null)
        {
            Debug.LogWarning("Canvas에 있는 DetailView 관리자가 연결되지 않았습니다.", this);
            return;
        }

        manager.Show(detailPrefab);
    }

    private void Show(GameObject prefab)
    {
        if (!IsManager)
        {
            Debug.LogError("관리자의 Detail View 또는 Detail Root가 연결되지 않았습니다.", this);
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning("이 물체의 Detail Prefab이 연결되지 않았습니다.", this);
            return;
        }

        // 같은 클릭이 UI 이벤트와 물리 이벤트 양쪽에서 들어오는 경우를 방지합니다.
        if (lastOpenedFrame == Time.frameCount)
        {
            return;
        }

        lastOpenedFrame = Time.frameCount;

        if (currentDetail != null)
        {
            Destroy(currentDetail);
        }

        SoundManager.Instance.PlaySFX(SFXType.DefaultClick);

        detailView.SetActive(true);
        detailView.transform.SetAsLastSibling();
        currentDetail = Instantiate(prefab, detailRoot, false);
    }

    /// BackButton의 OnClick에 Canvas 관리자 컴포넌트의 이 함수를 연결합니다.
    public void CloseDetail()
    {
        if (!IsManager)
        {
            Debug.LogWarning("CloseDetail은 Canvas의 관리자 컴포넌트에서 호출해야 합니다.", this);
            return;
        }

        SoundManager.Instance.PlaySFX(SFXType.DefaultClick);

        if (currentDetail != null)
        {
            Destroy(currentDetail);
            currentDetail = null;
        }

        detailView.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Canvas의 관리자 자신은 클릭 대상이 아닙니다.
        if (!IsManager)
        {
            OpenDetail();
        }
    }

    private void OnMouseDown()
    {
        // Collider/Collider2D가 붙은 일반 Sprite 클릭을 지원합니다.
        if (!IsManager)
        {
            OpenDetail();
        }
    }
}
