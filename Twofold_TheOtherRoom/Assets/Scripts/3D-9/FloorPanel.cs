using System;
using System.Collections;
using UnityEngine;

public enum PanelState { Off, On, Solved }
public class FloorPanel : MonoBehaviour
{
    #region Inspector
    [Header("Index")]
    [SerializeField] private Vector2Int panelIndex; 

    [Header("Materials")]
    [SerializeField] private Material offMat;
    [SerializeField] private Material onMat;
    [SerializeField] private Material solvedMat;

    [Header("Debug")]
    [SerializeField] private bool debugMode;
    [SerializeField] private PanelState debugState;

    #endregion

    private Renderer _renderer;
    
    private bool isPlayerOn = false;
    private PanelState currentState = PanelState.Off;

    // Awake - 렌더러 참조
    void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    // 충돌 처리
    void OnTriggerEnter(Collider other)
    {   
        if(!other.CompareTag("Player") || isPlayerOn || currentState == PanelState.Solved) return;
        currentState = (currentState == PanelState.Off) ? PanelState.On : PanelState.Off;
        isPlayerOn = true;
        ChangeMaterial();
        // 컨트롤러에게 state 메세지
        // transform 올리거나 내리기
    }

    void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player") || currentState == PanelState.Solved) return;
        isPlayerOn = false;
    }

    // 컨트롤러한테 Solved 받음
    void ChangeStateToSolved()
    {
        currentState = PanelState.Solved;
        ChangeMaterial();
    }

    // State에 따른 Material 처리
    void ChangeMaterial()
    {
        if(currentState == PanelState.Solved) _renderer.sharedMaterial = solvedMat;
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
        // 컨트롤러에게 state 메세지
    }
}
