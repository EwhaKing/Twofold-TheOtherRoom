using UnityEngine;

public class CutCake : MonoBehaviour
{   
    [System.Serializable]
    public class KnifeData
    {
        public KnifeClick knife;
        public GameObject section;
    }

    public KnifeData[] knifeList;

    public Renderer cake;       

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
        Color color = cake.material.color;
        color.a = alpha;
        cake.material.color = color;
    }
}
