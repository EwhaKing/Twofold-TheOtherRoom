
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class stick3d18 : MonoBehaviour, IInteractable
{
    [Header("Stick Settings")]
    private int stickIndex;
    private gamemanager3D18 manager;

    public int StickIndex => stickIndex;

    public void Setup(gamemanager3D18 puzzleManager, int index)
    {
        manager = puzzleManager;
        stickIndex = index;
    }
    [Header("Light")]
    [SerializeField] private Light pointLight;

    private Renderer lightRenderer;
    private Material mat;
    private Coroutine blinkCoroutine;

    private bool isLit = false;

    private void Awake()
    {
        // 자기 자신의 Renderer 자동 연결
        lightRenderer = GetComponent<Renderer>();

        if (lightRenderer != null)
        {
            mat = lightRenderer.material;
            mat.EnableKeyword("_EMISSION");
        }

        TurnOff();
    }

    // E키 상호작용
    public void Interact()
    {
        if (manager == null) return;

        manager.TryPressStick(stickIndex);
    }

    // 정답 막대의 민트색 불빛 유지
    public void SetLit(bool lit)
    {
        if (isLit == lit)
            return;

        isLit = lit;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (lit)
        {
            ColorUtility.TryParseHtmlString(
                "#0EECC0", out Color color
            );

            SetLight(color);
        }
        else
        {
            TurnOff();
        }
    }

    // 오답 막대의 코랄핑크 점멸
    public void BlinkWrong()
    {
        // 이미 정답인 막대는 점멸하지 않음
        if (isLit) return;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        ColorUtility.TryParseHtmlString(
            "#FF7B8A", out Color color
        );

        SetLight(color);

        yield return new WaitForSeconds(0.5f);

        blinkCoroutine = null;

        if (!isLit)
            TurnOff();
    }

    // 실제 발광 처리
    private void SetLight(Color color)
    {
        if (pointLight != null)
        {
            pointLight.enabled = true;
            pointLight.color = color;
        }

        if (mat != null)
        {
            mat.SetColor(
                "_EmissionColor", color * 3f
            );
        }
    }

    // 불빛 끄기
    public void TurnOff()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (pointLight != null)
            pointLight.enabled = false;

        if (mat != null)
            mat.SetColor("_EmissionColor", Color.black);
    }

    private void OnDestroy()
    {
        if (mat != null)
            Destroy(mat);
    }
}
