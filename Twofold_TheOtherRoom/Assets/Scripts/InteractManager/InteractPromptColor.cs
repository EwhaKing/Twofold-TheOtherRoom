using UnityEngine;

/// <summary>인스펙터에서 지정하는 UI 색.</summary>
public class InteractPromptColor : MonoBehaviour, IInteractPromptColor
{
    [SerializeField] private Color color = Color.black;

    public bool TryGetPromptColor(out Color color)
    {
        color = this.color;
        return enabled;
    }
}
