using UnityEngine;

public class SImpleMove : MonoBehaviour
{
       [Header("Move")]
    public float moveSpeed = 5f;

    private void Update()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveDir += transform.forward;

        if (Input.GetKey(KeyCode.S))
            moveDir -= transform.forward;

        if (Input.GetKey(KeyCode.A))
            moveDir -= transform.right;

        if (Input.GetKey(KeyCode.D))
            moveDir += transform.right;

        if (Input.GetKey(KeyCode.Space))
            moveDir += Vector3.up;

        if (Input.GetKey(KeyCode.LeftControl))
            moveDir -= Vector3.up;

        transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
    }
}
