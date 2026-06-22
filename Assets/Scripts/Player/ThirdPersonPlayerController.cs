using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class ThirdPersonPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private Transform cameraTransform;

    private CharacterController characterController;
    private Animator animator;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    public void SetCameraTransform(Transform camera)
    {
        cameraTransform = camera;
    }

    private void Update()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        var input = new Vector3(horizontal, 0f, vertical);

        var moveDirection = Vector3.zero;
        if (input.sqrMagnitude > 0.01f)
        {
            input.Normalize();
            moveDirection = GetCameraRelativeDirection(input);
            RotateToward(moveDirection);
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        moveDirection.y = verticalVelocity;
        characterController.Move(moveDirection * Time.deltaTime);

        UpdateAnimator(input.magnitude);
    }

    private Vector3 GetCameraRelativeDirection(Vector3 input)
    {
        if (cameraTransform == null)
        {
            return input * moveSpeed;
        }

        var forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        var right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        var worldDirection = forward * input.z + right * input.x;
        return worldDirection * moveSpeed;
    }

    private void RotateToward(Vector3 moveDirection)
    {
        moveDirection.y = 0f;
        if (moveDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        var targetRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    private void UpdateAnimator(float inputMagnitude)
    {
        if (animator == null)
        {
            return;
        }

        var isMoving = inputMagnitude > 0.1f;
        animator.speed = isMoving ? 1f : 0f;

        if (animator.runtimeAnimatorController != null)
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.name == "Speed" && parameter.type == AnimatorControllerParameterType.Float)
                {
                    animator.SetFloat("Speed", isMoving ? inputMagnitude : 0f);
                }
            }
        }
    }
}
