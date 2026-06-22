using UnityEngine;

/// <summary>
/// Fixed-angle orthographic camera for isometric gameplay.
/// </summary>
public sealed class IsometricCamera : MonoBehaviour
{
    private const float IsometricPitch = 35.264f;
    private const float IsometricYaw = 45f;

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset = new(10f, 10f, -10f);
    [SerializeField] private float followSmoothing = 14f;
    [SerializeField] private float orthographicSize = 2.8f;

    private Camera cameraComponent;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
    }

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = orthographicSize;
        cameraComponent.nearClipPlane = 0.1f;
        cameraComponent.farClipPlane = 200f;
        transform.rotation = Quaternion.Euler(IsometricPitch, IsometricYaw, 0f);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        var desiredPosition = target.position + worldOffset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmoothing * Time.deltaTime);
        transform.rotation = Quaternion.Euler(IsometricPitch, IsometricYaw, 0f);
    }
}
