using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 3D 쪽에서 지정한 퍼즐이 풀리면 이 Image(와 자식 Image)를 빛 색으로 물들입니다.
/// UI/Emission 셰이더로 검은 선까지 같이 물들이고, 서서히 밝아진 뒤 은은하게 깜빡입니다.
/// GameSession.BasementPuzzleMask를 보므로 확대 프리팹이 나중에 생성돼도 상태가 맞춰집니다.
/// </summary>
[RequireComponent(typeof(Image))]
public class Puzzle3DSolvedGlow : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionIntensityId = Shader.PropertyToID("_EmissionIntensity");

    [Header("3D에서 풀려야 하는 퍼즐 ID")]
    [SerializeField] private string puzzleId = "3D-6";

    [Header("빛 색")]
    [Tooltip("UI/Emission 셰이더 (Assets/Shaders/UIEmission.shader)")]
    [SerializeField] private Shader emissionShader;
    [SerializeField] private Color glowColor = new Color(1f, 0.85f, 0.4f, 1f);
    [Tooltip("빛날 때 최대 세기. 1이면 Glow Color로 완전히 물듦")]
    [SerializeField, Range(0f, 1f)] private float maxIntensity = 0.6f;
    [Tooltip("자식 Image도 같이 물들일지")]
    [SerializeField] private bool includeChildren = true;

    [Header("연출")]
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.6f;
    [SerializeField, Min(0f)] private float pulseSpeed = 1f;
    [SerializeField, Range(0f, 1f)] private float pulseAmount = 0.3f;

    private Material material;
    private Coroutine glowRoutine;

    private void Awake()
    {
        if (emissionShader == null)
            emissionShader = Shader.Find("UI/Emission");

        if (emissionShader == null)
        {
            Debug.LogWarning("[Puzzle3DSolvedGlow] UI/Emission 셰이더가 연결되지 않았습니다.", this);
            return;
        }

        // Image마다 따로 빛나야 하므로 머티리얼을 복사해서 사용
        material = new Material(emissionShader);
        material.SetColor(EmissionColorId, glowColor);
        material.SetFloat(EmissionIntensityId, 0f);

        Image[] images = includeChildren
            ? GetComponentsInChildren<Image>(true)
            : new[] { GetComponent<Image>() };

        // 자식 Image(clock, 3D-11 등)도 같은 머티리얼로 같이 물들임.
        // 이미 다른 머티리얼을 쓰는 Image는 건드리지 않음
        foreach (Image image in images)
        {
            if (image.material == image.defaultMaterial)
                image.material = material;
        }
    }

    private void OnEnable()
    {
        GameSession.On3DSolvedChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        GameSession.On3DSolvedChanged -= Refresh;

        // 비활성화되면 코루틴이 멈추므로 다시 켜질 때 Refresh에서 새로 시작
        glowRoutine = null;
    }

    private void Refresh()
    {
        if (GameSession.Instance != null && GameSession.Instance.Is3DSolved(puzzleId))
            SetLit(true);
    }

    private void SetLit(bool lit)
    {
        if (material == null)
            return;

        if (lit)
        {
            // 이미 빛나는 중이면 무시
            if (glowRoutine == null)
                glowRoutine = StartCoroutine(GlowRoutine());
        }
        else
        {
            if (glowRoutine != null)
                StopCoroutine(glowRoutine);

            glowRoutine = null;
            material.SetFloat(EmissionIntensityId, 0f);
        }
    }

    /// 신호가 온 뒤에만 도는 코루틴. 서서히 밝아진 다음 계속 은은하게 깜빡임
    private IEnumerator GlowRoutine()
    {
        float fade = 0f;

        while (true)
        {
            fade = Mathf.MoveTowards(fade, 1f, Time.deltaTime / fadeInDuration);

            float pulse = 1f - pulseAmount * (0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI));
            material.SetFloat(EmissionIntensityId, maxIntensity * fade * pulse);

            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (material != null)
            Destroy(material);
    }

    [ContextMenu("TEST - Glow On")]
    private void TestGlowOn()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("빛남 테스트는 Play Mode에서 실행해 주세요.", this);
            return;
        }

        SetLit(true);
    }

    [ContextMenu("TEST - Glow Off")]
    private void TestGlowOff()
    {
        if (Application.isPlaying)
            SetLit(false);
    }
}
