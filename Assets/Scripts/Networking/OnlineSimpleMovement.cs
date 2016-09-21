using UnityEngine;
using UnityEngine.Networking;

public class OnlineSimpleMovement : NetworkBehaviour
{
    [SerializeField]
    private float speed = 6f;

    private Vector3 moveDirection = Vector3.zero;

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        moveDirection = transform.forward * Input.GetAxis("Vertical");
        moveDirection += transform.right * Input.GetAxis("Horizontal");
        moveDirection *= speed;

        transform.position += moveDirection * Time.deltaTime;
    }
}
