using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class DetailView : MonoBehaviour , IPointerClickHandler
{
    [Header("Canvas에서 설정")]
    [SerializeField] private GameObject detailView;
    [SerializeField] private Transform detailRoot;

    [Header("각 물체에서 설정")]
    [SerializeField] private DetailView manager;
    [SerializeField] private GameObject detailPrefab;
    [Tooltip("체크하면 닫았다 열어도 이전 상태를 유지합니다. 끄면 열 때마다 새로 생성합니다.")]
    [SerializeField] private bool keepState = true;


    private GameObject currentDetail;
    private bool currentDetailIsCached;
    private readonly Dictionary<GameObject, GameObject> detailCache = new Dictionary<GameObject, GameObject>();
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

        // 각 물체별 DefaultClick 재생 여부 확인
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
        }

        manager.Show(detailPrefab, keepState);
    }

    private void Show(GameObject prefab, bool keep)
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


        GameObject detail = null;

        // 상태 유지 대상이면 만들어 둔 인스턴스를 찾아 재사용합니다.
        if (keep)
        {
            detailCache.TryGetValue(prefab, out detail);
        }

        //확대본이 두번 연속 있을시 가장 이전 것은 지워주는 역할을 함. 
        if (currentDetail != detail)
        {
            HideCurrentDetail();
        }

        // 상태 유지 대상의 첫 생성이거나, 상태 유지 안 하는 프리팹이면 새로 생성합니다.
        if (detail == null)
        {
            detail = Instantiate(prefab, detailRoot, false);

            if (keep)
            {
                detailCache[prefab] = detail;
            }
        }

        detailView.SetActive(true);
        detailView.transform.SetAsLastSibling();

        detail.SetActive(true);
        currentDetail = detail;
        currentDetailIsCached = keep;
    }

    /// 상태 유지 대상은 숨기고, 그렇지 않은 것은 파괴합니다.
    private void HideCurrentDetail()
    {
        if (currentDetail == null)
        {
            return;
        }

        if (currentDetailIsCached)
        {
            currentDetail.SetActive(false);
        }
        else
        {
            Destroy(currentDetail);
        }

        currentDetail = null;
    }

    /// BackButton의 OnClick에 Canvas 관리자 컴포넌트의 이 함수를 연결합니다.
    public void CloseDetail()
    {
        if (!IsManager)
        {
            Debug.LogWarning("CloseDetail은 Canvas의 관리자 컴포넌트에서 호출해야 합니다.", this);
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFXType.DefaultClick);
        }

        HideCurrentDetail();

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
