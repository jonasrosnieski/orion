using UnityEngine;

/// <summary>
/// Fixed-angle orthographic camera for isometric gameplay.
/// </summary>
public sealed class IsometricCamera : MonoBehaviour
{
    private static readonly Quaternion IsometricRotation = Quaternion.Euler(35.264f, 45f, 0f);

    [SerializeField] private Transform target;
    [SerializeField] private float lookAtHeight = 1.05f;
    [SerializeField] private float distance = 12f;
    [SerializeField] private float followSmoothing = 14f;
    [SerializeField] private float orthographicSize = 3.2f;

    private Camera cameraComponent;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
        SnapToTarget();
    }

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = orthographicSize;
        cameraComponent.nearClipPlane = 0.1f;
        cameraComponent.farClipPlane = 200f;
    }

    private void Start()
    {
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        var focusPoint = target.position + Vector3.up * lookAtHeight;
        var forward = IsometricRotation * Vector3.forward;
        var desiredPosition = focusPoint - forward * distance;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmoothing * Time.deltaTime);
        transform.rotation = IsometricRotation;
    }

    private void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }

        var focusPoint = target.position + Vector3.up * lookAtHeight;
        var forward = IsometricRotation * Vector3.forward;
        transform.position = focusPoint - forward * distance;
        transform.rotation = IsometricRotation;
    }
}
