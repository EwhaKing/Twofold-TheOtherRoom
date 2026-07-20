using System;
using UnityEngine;

public enum PanelState { Off, On, Answer }
public class FloorPanel : MonoBehaviour
{
    #region Inspector
    [Header("Index")]
    [SerializeField] private Vector2Int panelIndex; 

    [Header("Materials")]
    [SerializeField] private Material offMat;
    [SerializeField] private Material onMat;
    [SerializeField] private Material answerMat;

    [Header("Debug")]
    [SerializeField] private bool debugMode;
    [SerializeField] private PanelState debugState;

    #endregion

    private Renderer _renderer;
    private bool isPlayerOn = false;
    private bool frozen = false;
    private PanelState currentState = PanelState.Off;

    public event Action<Vector2Int, bool> OnToggled; // 이벤트 - 인덱스, on/off 상태 알림
    public Vector2Int Index => panelIndex;

    // Awake - 렌더러 참조
    void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    // 충돌 처리
    void OnTriggerEnter(Collider other)
    {   
        if(!other.CompareTag("Player") || isPlayerOn || frozen) return;
        currentState = (currentState == PanelState.Off) ? PanelState.On : PanelState.Off;
        isPlayerOn = true;
        ChangeMaterial();
        OnToggled?.Invoke(panelIndex, currentState == PanelState.On);
    }

    void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player") || frozen) return;
        isPlayerOn = false;
    }

    // 정답색으로 변경 - 컨트롤러가 호출
    public void ChangeStateToAnswer()
    {
        Freeze();
        currentState = PanelState.Answer;
        ChangeMaterial();
    }

    public void Freeze()
    {
        frozen = true;
    }

    // State에 따른 Material 처리
    void ChangeMaterial()
    {
        if(currentState == PanelState.Answer) _renderer.sharedMaterial = answerMat;
        else if(currentState == PanelState.Off) _renderer.sharedMaterial = offMat;
        else _renderer.sharedMaterial = onMat;
    }

    // 디버그용 - 인스펙터로 조절. renderer 필요하므로 런타임에 사용
    void OnValidate()
    {
        if (!debugMode) return;
        if (_renderer == null)
        {
            Debug.Log("renderer 필요하므로 런타임에 사용");
            return;
        }
        currentState = debugState;
        ChangeMaterial();
    }
}
