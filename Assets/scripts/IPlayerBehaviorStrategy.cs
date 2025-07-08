using UnityEngine;

public interface IPlayerBehaviorStrategy
{
    void HandleMovement(CharacterController controller, Transform playerTransform, float moveSpeed, AudioSource audioSource, AudioClip[] footstepSounds);
    void HandleInteraction(Camera playerCamera, float interactionDistance, LayerMask interactableLayer);
}