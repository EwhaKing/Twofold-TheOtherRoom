using UnityEngine;

public class CutCake_Mouse : MonoBehaviour
{
    [System.Serializable]
    public class KnifeData
    {
        public KnifeClick_Mouse knife;
        public GameObject section;
    }

    [SerializeField] private KnifeData[] knifeList;
    [SerializeField] private GameObject cake;

    private Renderer[] cakeRenderers;

    private void Awake()
    {
        cakeRenderers = cake != null
            ? cake.GetComponentsInChildren<Renderer>(true)
            : new Renderer[0];

        if (cake == null)
            Debug.LogWarning("[CutCake_Mouse] Cake가 연결되지 않았습니다.", this);
    }

    public void CakePiece(KnifeClick_Mouse knife)
    {
        KnifeData target = FindKnifeData(knife);
        if (target == null || target.section == null)
        {
            Debug.LogWarning("[CutCake_Mouse] 클릭한 Knife 또는 Section 연결을 확인하세요.", this);
            return;
        }

        bool wasOpen = target.section.activeSelf;

        if (knifeList != null)
        {
            foreach (KnifeData data in knifeList)
            {
                if (data?.section != null)
                    data.section.SetActive(false);
            }
        }

        if (wasOpen)
        {
            SetCakeAlpha(1f);
            return;
        }

        target.section.SetActive(true);
        SetCakeAlpha(0.1f);
    }

    private KnifeData FindKnifeData(KnifeClick_Mouse knife)
    {
        if (knifeList == null)
            return null;

        foreach (KnifeData data in knifeList)
        {
            if (data != null && data.knife == knife)
                return data;
        }

        return null;
    }

    private void SetCakeAlpha(float alpha)
    {
        foreach (Renderer cakeRenderer in cakeRenderers)
        {
            if (cakeRenderer == null)
                continue;

            Material material = cakeRenderer.material;
            Color color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }
}
