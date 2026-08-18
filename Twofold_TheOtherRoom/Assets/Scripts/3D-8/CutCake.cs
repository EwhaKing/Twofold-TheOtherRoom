using UnityEngine;
using UnityEngine.Rendering;


public class CutCake : MonoBehaviour
{   
    [System.Serializable]
    public class KnifeData
    {
        public KnifeClick knife;
        public GameObject section;
    }

    public KnifeData[] knifeList;

    [SerializeField] private GameObject cake;

    private Renderer[] cakeRenderers;

    private void Awake()
    {
        cakeRenderers = cake.GetComponentsInChildren<Renderer>();
    }
/**
    public GameObject cake;
    public Renderer[] cakeRenderers;

    private void Awake()
    {
        cakeRenderers = cake.GetComponentsInChildren<Renderer>();
    }
**/
    public void CakePiece(KnifeClick knife)
    {
           KnifeData target = null;

        foreach(var data in knifeList)
        {
            if(data.knife == knife)
            {
                target = data;
                break;
            }
        }

        bool wasOpen = target.section.activeSelf;

        foreach(var data in knifeList)
            data.section.SetActive(false);

        if(wasOpen)
        {
            SetCakeAlpha(1f);
        }
        else
        {
            target.section.SetActive(true);
            SetCakeAlpha(0.5f);
        }
    }
    void SetCakeAlpha(float alpha)
    {        
        foreach (Renderer renderer in cakeRenderers)
        {
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;
        }
    }
    /**
    private void SetCakeAlpha(float alpha)
    {
        foreach (Renderer renderer in cakeRenderers)
        {
            Material mat = renderer.material;

            Color color = mat.color;
            color.a = alpha;
            mat.color = color;

            if (alpha < 1f)
            {
                // URP Lit 계열 Material을 Transparent로 변경
                if (mat.HasProperty("_Surface"))
                    mat.SetFloat("_Surface", 1f);

                if (mat.HasProperty("_Blend"))
                    mat.SetFloat("_Blend", 0f);

                if (mat.HasProperty("_SrcBlend"))
                    mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);

                if (mat.HasProperty("_DstBlend"))
                    mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);

                if (mat.HasProperty("_ZWrite"))
                    mat.SetFloat("_ZWrite", 0f);

                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                if (mat.HasProperty("_Surface"))
                    mat.SetFloat("_Surface", 0f);

                if (mat.HasProperty("_ZWrite"))
                    mat.SetFloat("_ZWrite", 1f);

                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = -1;
            }
        }
    }
    **/
}
