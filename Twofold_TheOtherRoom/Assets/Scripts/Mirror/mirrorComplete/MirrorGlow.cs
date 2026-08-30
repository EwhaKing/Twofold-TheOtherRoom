using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 완성 거울의 발광. UI Image(2D)와 모델 Renderer(3D) 양쪽을 다룬다.
///
/// 2D는 M_MirrorGlow 의 _GlowIntensity, 3D는 URP Lit 의 _EmissionColor 로 쓰는 것을 전제.
/// 어느 쪽이든 머티리얼 복사본을 만들어 쓰므로 에셋은 안 더러워진다.
/// </summary>
public class MirrorGlow : MonoBehaviour
{
    [Header("Material")]
    [Tooltip("완성 시 갈아끼울 머티리얼. 비워두면 지금 물린 머티리얼 그대로 값만 올림")]
    [SerializeField] private Material glowMaterial;

    [Tooltip("올릴 셰이더 프로퍼티. 2D는 _Glow_Intensity, 3D는 _EmissionColor")]
    [SerializeField] private string property = "_Glow_Intensity";

    [Tooltip("_EmissionColor 처럼 Color 프로퍼티일 때 켤 것. 색 × 세기를 넣고 _EMISSION 도 켬")]
    [SerializeField] private bool isColorProperty;

    [SerializeField] private Color glowColor = Color.white;

    [Header("Timing")]
    [SerializeField] private float targetIntensity = 3f;
    [SerializeField] private float duration = 0.4f;

    /// 에셋을 그대로 물고 값을 바꾸면 에디터 플레이 중 .mat 이 더러워짐
    private Material instance;

    private int propertyId;
    private Coroutine routine;

    private void OnDestroy()
    {
        if (instance != null) Destroy(instance);
    }

    /// <summary>발광 적용. instant면 연출 없이 최종 상태로 (이미 완성된 채 씬에 들어온 경우).</summary>
    public void Apply(bool instant)
    {
        if (!EnsureMaterial()) return;

        if (routine != null) StopCoroutine(routine);

        if (instant || duration <= 0f)
        {
            SetIntensity(targetIntensity);
            return;
        }

        routine = StartCoroutine(GlowRoutine());
    }

    private bool EnsureMaterial()
    {
        if (instance != null) return true;

        // Graphic.material 은 자동 인스턴스화를 안 해서 직접 복사해야 함. 3D도 같은 방식으로 통일
        Graphic graphic = GetComponent<Graphic>();
        Renderer targetRenderer = graphic == null ? GetComponent<Renderer>() : null;

        Material source = glowMaterial != null ? glowMaterial
            : graphic != null ? graphic.material
            : targetRenderer != null ? targetRenderer.sharedMaterial
            : null;

        if (source == null)
        {
            Debug.LogError("[MirrorGlow] Graphic 도 Renderer 도 머티리얼도 없음", this);
            return false;
        }

        // 갈아끼우기 전에 검사. 잘못된 설정으로 머티리얼만 바꿔놓고 실패하지 않도록
        if (!source.HasProperty(property))
        {
            Debug.LogError($"[MirrorGlow] {source.name} 에 {property} 가 없음 " +
                           "— Shader Graph 프로퍼티의 Reference 이름 확인할 것", this);
            return false;
        }

        instance = new Material(source);

        if (graphic != null) graphic.material = instance;
        else targetRenderer.material = instance;

        propertyId = Shader.PropertyToID(property);
        if (isColorProperty) instance.EnableKeyword("_EMISSION");

        return true;
    }

    private IEnumerator GlowRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetIntensity(Mathf.Lerp(0f, targetIntensity, elapsed / duration));
            yield return null;
        }

        SetIntensity(targetIntensity);
        routine = null;
    }

    private void SetIntensity(float value)
    {
        if (isColorProperty) instance.SetColor(propertyId, glowColor * value);
        else instance.SetFloat(propertyId, value);
    }
}
