using UnityEngine;

public class FireProtocolBehavior : IPlayerBehaviorStrategy
{
    private float footstepTimer;
    [Tooltip("Tiempo entre cada paso en segundos en modo protocolo de incendio.")]
    private float footstepDelay = 0.2f; // Puede ser más lento que el normal

    // ¡IMPORTANTE! La firma de este método debe coincidir con IPlayerBehaviorStrategy
    public void HandleMovement(CharacterController controller, Transform playerTransform, float moveSpeed, AudioSource audioSource, AudioClip[] footstepSounds)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = playerTransform.right * horizontal + playerTransform.forward * vertical;

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // En modo protocolo de incendio, el movimiento es más lento (0.5f)
        controller.Move(moveDirection * (moveSpeed * 0.5f) * Time.deltaTime);
        // Debug.Log("Movimiento en modo protocolo de incendio."); // Puedes comentar esto si no lo necesitas

        // --- Lógica de Pasos (copia de NormalPlayerBehavior) ---
        if (moveDirection.magnitude > 0.1f) // Si el jugador se está moviendo
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepDelay)
            {
                PlayFootstepSound(audioSource, footstepSounds);
                footstepTimer = 0f;
            }
        }
        else // Si el jugador no se mueve, resetea el timer
        {
            footstepTimer = 0f;
        }
    }

    public void HandleInteraction(Camera playerCamera, float interactionDistance, LayerMask interactableLayer)
    {
        // Debug.DrawRay para el modo protocolo de incendio (Color.red para diferenciar)
        // Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance, Color.red, 0.1f);

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
            {
                // Primero, intentamos obtener el componente InteractableButton_V2
                InteractableButton_V2 button = hit.collider.GetComponent<InteractableButton_V2>();

                if (button != null)
                {
                    // Debug.Log("Raycast golpeó un InteractableButton_V2: " + hit.collider.name); // Puedes comentar esto
                    button.Interact(); // Llama al método Interact() del botón
                }
                // Luego, comprobamos los tags específicos si no es un InteractableButton_V2
                else if (hit.collider.CompareTag("FireExtinguisher") || hit.collider.CompareTag("AlarmPanel"))
                {
                    Debug.Log("Interactuando con elemento del protocolo: " + hit.collider.name);
                    // Aquí puedes añadir lógica específica para estos tags
                }
                // else { Debug.Log("Este objeto no es parte del protocolo de incendio o no tiene un InteractableButton_V2: " + hit.collider.name + ". Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer)); } // Puedes comentar esto
            }
            // else { Debug.Log("Raycast no golpeó nada interactuable en el modo protocolo."); } // Puedes comentar esto
        }
    }

    // Método privado para reproducir un sonido de paso (copia de NormalPlayerBehavior)
    private void PlayFootstepSound(AudioSource audioSource, AudioClip[] footstepSounds)
    {
        if (audioSource != null && footstepSounds != null && footstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            audioSource.PlayOneShot(footstepSounds[randomIndex]);
        }
    }
}