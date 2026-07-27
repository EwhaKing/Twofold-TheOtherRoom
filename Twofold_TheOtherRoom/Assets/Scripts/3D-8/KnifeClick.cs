using UnityEngine;

public class KnifeClick : MonoBehaviour, IInteractable
{
    public GameObject section;
    public Renderer cake;          
    public bool click;
    void Start()
    {
        click = false;
    }
    public void Interact()
    {
        section.SetActive(!section.activeSelf);
        if (click == false)
        {
            Color color = cake.material.color;
            color.a = 0.5f;      // 30% 정도만 보이게
            cake.material.color = color;
            click = true;
        }
        else
        {
            Color color = cake.material.color;
            color.a = 1f;      // 30% 정도만 보이게
            cake.material.color = color;
            click = false;
        }
    }
}
