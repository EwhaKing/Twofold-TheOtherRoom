
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
    private Material[][] cakeMaterials;

    private void Awake()
    {
        cakeRenderers = cake != null
            ? cake.GetComponentsInChildren<Renderer>(true)
            : new Renderer[0];

        cakeMaterials = new Material[cakeRenderers.Length][];

        for (int i = 0; i < cakeRenderers.Length; i++)
        {
            cakeMaterials[i] = cakeRenderers[i].materials;
        }

        if (cake == null)
            Debug.LogWarning("[CutCake_Mouse] Cake가 연결되지 않았습니다.", this);
    }

    public void CakePiece(KnifeClick_Mouse knife)
    {
        KnifeData target = FindKnifeData(knife);

        if (target == null || target.section == null)
        {
            Debug.LogWarning("[CutCake_Mouse] Knife 또는 Section 연결을 확인하세요.", this);
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
        for (int i = 0; i < cakeMaterials.Length; i++)
        {
            foreach (Material material in cakeMaterials[i])
            {
                if (material == null)
                    continue;

                Color color = material.HasProperty("_BaseColor")
                    ? material.GetColor("_BaseColor")
                    : material.color;

                color.a = alpha;

                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", color);

                if (material.HasProperty("_Color"))
                    material.SetColor("_Color", color);
            }
        }
    }
}
