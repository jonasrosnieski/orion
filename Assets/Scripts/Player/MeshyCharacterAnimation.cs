using UnityEngine;

/// <summary>
/// Drives Meshy GLB walk clips (glTFast Legacy Animation or Mecanim fallback).
/// </summary>
public sealed class MeshyCharacterAnimation : MonoBehaviour
{
    private Animation legacyAnimation;
    private Animator animator;
    private string legacyClipName;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        legacyAnimation = GetComponentInChildren<Animation>();
        animator = GetComponentInChildren<Animator>();

        if (legacyAnimation != null)
        {
            foreach (AnimationState state in legacyAnimation)
            {
                legacyClipName = state.name;
                state.wrapMode = WrapMode.Loop;
                break;
            }
        }
    }

    public void SetLocomotion(float inputMagnitude)
    {
        var isMoving = inputMagnitude > 0.1f;

        if (legacyAnimation != null && !string.IsNullOrEmpty(legacyClipName))
        {
            if (isMoving)
            {
                if (!legacyAnimation.IsPlaying(legacyClipName))
                {
                    legacyAnimation.Play(legacyClipName);
                }

                legacyAnimation[legacyClipName].speed = 1f;
                legacyAnimation[legacyClipName].enabled = true;
            }
            else
            {
                legacyAnimation[legacyClipName].speed = 0f;
            }

            return;
        }

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        animator.SetFloat(SpeedHash, isMoving ? inputMagnitude : 0f);

        if (isMoving)
        {
            if (animator.speed < 0.01f)
            {
                animator.speed = 1f;
            }

            animator.Play(0, 0, 0f);
        }
        else
        {
            animator.speed = 0f;
        }
    }
}
