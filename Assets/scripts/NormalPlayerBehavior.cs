using UnityEngine;

public class NormalPlayerBehavior : IPlayerBehaviorStrategy
{
    private GameObject currentGazedObject = null;
    private float footstepTimer;
    [Tooltip("Tiempo entre cada paso en segundos (ajusta esto a tu velocidad)")]
    private float footstepDelay = 0.4f;

    public void HandleMovement(CharacterController controller, Transform playerTransform, float moveSpeed, AudioSource audioSource, AudioClip[] footstepSounds)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = playerTransform.right * horizontal + playerTransform.forward * vertical;

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (moveDirection.magnitude > 0.1f)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepDelay)
            {
                PlayFootstepSound(audioSource, footstepSounds);
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    public void HandleInteraction(Camera playerCamera, float interactionDistance, LayerMask interactableLayer)
    {

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
            {
                InteractableButton_V2 button = hit.collider.GetComponent<InteractableButton_V2>();
                if (button != null)
                {
                    button.Interact();
                }
            }
        }
    }

    private void PlayFootstepSound(AudioSource audioSource, AudioClip[] footstepSounds)
    {
        if (audioSource != null && footstepSounds != null && footstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            audioSource.PlayOneShot(footstepSounds[randomIndex]);
        }
    }
}
