using UnityEngine;

public sealed class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 shoulderOffset = new(0.4f, 1.55f, 0f);
    [SerializeField] private float distance = 4.2f;
    [SerializeField] private float minPitch = 10f;
    [SerializeField] private float maxPitch = 45f;
    [SerializeField] private float followSmoothing = 12f;
    [SerializeField] private float orbitSensitivity = 180f;

    private float yaw;
    private float pitch = 20f;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * orbitSensitivity * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * orbitSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else
        {
            yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, followSmoothing * Time.deltaTime);
        }

        var rotation = Quaternion.Euler(pitch, yaw, 0f);
        var pivot = target.position + shoulderOffset;
        var desiredPosition = pivot - rotation * Vector3.forward * distance;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmoothing * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(pivot - transform.position, Vector3.up),
            followSmoothing * Time.deltaTime);
    }
}
