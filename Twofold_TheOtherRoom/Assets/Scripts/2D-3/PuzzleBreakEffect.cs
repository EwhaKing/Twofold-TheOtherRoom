using System.Collections;
using UnityEngine;

public class PuzzleBreakEffect : MonoBehaviour
{
    [Header("깨지는 단계별 이미지들 (Frame_0 ~ Frame_3)")]
    public GameObject[] breakFrames;



    // [Header("구멍 속 거울 오브젝트")]
     public GameObject mirrorObject;

    // [Header("방 바닥 요소 제어")]
    // [Tooltip("방 바닥 전개도 자식으로 있는 구멍 이미지 (HoleOnFloor)")]
    // public GameObject roomHoleImage;

    // [Tooltip("깨진 후 방 화면에서 감출 러그 오브젝트")]
    // public GameObject rugObject;

    // [Tooltip("깨진 후 방 화면에서 감출 기존 전개도/바닥 이미지")]
    // public GameObject simplePlaceholder;

    private bool isBroken = false;
    private bool isMirrorCollected = false;

    // 퍼즐 성공 시 자동 호출
    public void PrepareEffect()
    {
        if (isBroken) return;
        isBroken = true;

        SoundManager.Instance.PlaySFX(SFXType.FloorHole);

        for (int i = 0; i < breakFrames.Length; i++)
        {
            if (breakFrames[i] != null)
                breakFrames[i].SetActive(i == 1);
        }

        if (mirrorObject != null)
            mirrorObject.SetActive(false);

        StartCoroutine(PlayBreakAnimation());
    }

    private IEnumerator PlayBreakAnimation()
    {
        //yield return new WaitForSeconds(0.4f); // 퍼즐 완성 후 연출 대기 시간


        // 단계별 깨지는 연출
        for (int i = 0; i < breakFrames.Length - 1; i++)
        {
            if (breakFrames[i] != null) breakFrames[i].SetActive(false);
            if (breakFrames[i + 1] != null) breakFrames[i + 1].SetActive(true);
            
            yield return new WaitForSeconds(0.2f); // 깨지는 프레임 속도
        }

        // 완전히 뚫린 후 거울 등장
        if (!isMirrorCollected && mirrorObject != null)
        {
            mirrorObject.SetActive(true);
        }

        // // ★ 방 화면(배치용 화면) 상태 변경 ★
        // ApplyRoomBreakState();
    }

    // // 방 화면을 깨진 바닥 상태로 전환하는 함수
    // public void ApplyRoomBreakState()
    // {
    //     // 1. 실제 방 바닥 구멍 이미지 활성화 (줌아웃 후 방 바닥에 보일 구멍)
    //     if (roomHoleImage != null)
    //     {
    //         roomHoleImage.SetActive(true);
    //     }

    //     // 2. 더 이상 필요 없는 러그 비활성화
    //     if (rugObject != null)
    //     {
    //         rugObject.SetActive(false);
    //     }
    // }

    // 줌아웃(뒤로가기) 시 연출 껍데기 끄기
    public void CloseBreakEffect()
    {
        // 연출 전체 그룹 끄기
        gameObject.SetActive(false);
    }

    // // 거울 클릭 시 획득 처리
    // public void OnClickCollectMirror()
    // {
    //     if (isMirrorCollected) return;

    //     isMirrorCollected = true;
        
    //     if (mirrorObject != null)
    //     {
    //         mirrorObject.SetActive(false);
    //     }

    //     Debug.Log("거울을 획득했습니다!");
    // }
}