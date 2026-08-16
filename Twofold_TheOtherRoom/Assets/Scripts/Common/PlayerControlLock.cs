using System.Collections.Generic;
using UnityEngine;


/// Stores and restores player-control Behaviours and cursor state for one inspection owner.
/// This is a regular C# helper, not a MonoBehaviour.
// 함수 내부의 Behaviour를 lock과 unlock이 관리함. 상위 스크립트에게 전달하지 않음
//sealed는 이 클래스를 다른 클래스가 상속할 수 없다는 뜻 
public sealed class PlayerControlLock
{   //PlayerControlLock이 “실제로 끈 대상”을 기록하는 목록
    private readonly List<Behaviour> disabledBehaviours = new List<Behaviour>();

    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;
    private bool isLocked;

    public void Lock(
        Behaviour owner,
        //상위 스크립트에서 전달한 “끌 후보 목록”
        Behaviour[] behavioursToDisable,
        bool alwaysDisablePlayerInteractor = false)
    {
        if (isLocked)
            return;

        originalCursorLockMode = Cursor.lockState;
        originalCursorVisible = Cursor.visible;
        disabledBehaviours.Clear();

        if (alwaysDisablePlayerInteractor)
            TryDisable(Object.FindAnyObjectByType<PlayerInteractor>(), owner);

        if (behavioursToDisable == null || behavioursToDisable.Length == 0)
        {
            TryDisable(Object.FindAnyObjectByType<PlayerController>(), owner);
            TryDisable(Object.FindAnyObjectByType<PlayerLocomotionInput>(), owner);

            if (!alwaysDisablePlayerInteractor)
                TryDisable(Object.FindAnyObjectByType<PlayerInteractor>(), owner);
        }
        else
        {
            foreach (Behaviour behaviour in behavioursToDisable)
                TryDisable(behaviour, owner);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isLocked = true;
    }

    public void Unlock()
    {
        if (!isLocked)
            return;

        foreach (Behaviour behaviour in disabledBehaviours)
        {
            if (behaviour != null)
                behaviour.enabled = true;
        }

        disabledBehaviours.Clear();
        Cursor.lockState = originalCursorLockMode;
        Cursor.visible = originalCursorVisible;
        isLocked = false;
    }

    private void TryDisable(Behaviour behaviour, Behaviour owner)
    {
        if (behaviour == null || behaviour == owner || !behaviour.enabled)
            return;

        behaviour.enabled = false;
        disabledBehaviours.Add(behaviour);
    }
}
