using UnityEngine;

/// <summary>상호작용 프롬프트 색을 대상별로 지정. false-기본색.</summary>
public interface IInteractPromptColor
{
    bool TryGetPromptColor(out Color color);
}
