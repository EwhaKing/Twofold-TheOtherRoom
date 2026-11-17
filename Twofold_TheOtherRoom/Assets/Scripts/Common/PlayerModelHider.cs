using System.Collections.Generic;
using UnityEngine;

/// <summary>조사 카메라에 걸리는 플레이어 모델 숨김. PlayerControlLock 과 같은 일반 C# 헬퍼.</summary>
public sealed class PlayerModelHider
{
    /// 끈 렌더러와 원래 enabled
    private readonly Dictionary<Renderer, bool> hiddenRenderers = new Dictionary<Renderer, bool>();

    public void Hide()
    {
        Restore();

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            foreach (Renderer playerRenderer in player.GetComponentsInChildren<Renderer>(true))
            {
                // 부모·자식 둘 다 Player 태그일 때 중복 저장 방지
                if (hiddenRenderers.TryAdd(playerRenderer, playerRenderer.enabled))
                    playerRenderer.enabled = false;
            }
        }
    }

    public void Restore()
    {
        foreach (KeyValuePair<Renderer, bool> state in hiddenRenderers)
        {
            if (state.Key != null)
                state.Key.enabled = state.Value;
        }

        hiddenRenderers.Clear();
    }
}
