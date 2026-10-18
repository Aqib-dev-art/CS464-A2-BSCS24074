using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;                         // empty: found in Start
    [SerializeField] Vector3 offset = new Vector3(0f, 6f, -8f);
    [SerializeField] float sharpness = 5f;                     // k in 1 - e^(-k dt)
    [SerializeField] float lookAhead = 0.35f;                  // s of flat velocity

    Rigidbody targetRb;

    void Start()
    {
        if (target == null)
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null) target = player.transform;
        }
        if (target != null) targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 vel = targetRb != null ? targetRb.linearVelocity : Vector3.zero;
        vel.y = 0f;                                            // flat velocity only

        Vector3 aim = target.position + vel * lookAhead;       // where the player will be
        Vector3 wanted = aim + offset;

        float blend = 1f - Mathf.Exp(-sharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, wanted, blend);

        transform.LookAt(aim);                                 // look at the aim point
    }
}
