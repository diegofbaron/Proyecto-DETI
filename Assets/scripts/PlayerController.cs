using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Camera playerCamera;
    public float moveSpeed = 5f;
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    private IPlayerBehaviorStrategy currentBehavior;
    private CharacterController characterController;

    [Header("Sonidos de Pasos")]
    public AudioSource playerAudioSource;
    public AudioClip[] footstepSounds;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("PlayerController: ¡ERROR! No se encontró CharacterController en el GameObject.");
        }

        if (playerAudioSource == null)
        {
            playerAudioSource = GetComponent<AudioSource>();
            if (playerAudioSource == null)
            {
                Debug.LogError("PlayerController: ¡ERROR! No se encontró AudioSource en el GameObject. Añádelo o asigna uno.");
            }
        }

        SetBehavior(new NormalPlayerBehavior());

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PlayerController: playerCamera es NULL en Update. ¡Necesita ser asignada!");
            return;
        }

        if (currentBehavior == null)
        {
            Debug.LogError("PlayerController: ¡ERROR! currentBehavior es NULL en Update() antes de HandleMovement. ¿Se perdió la referencia?");
            return;
        }

        currentBehavior.HandleMovement(characterController, transform, moveSpeed, playerAudioSource, footstepSounds);
        currentBehavior.HandleInteraction(playerCamera, interactionDistance, interactableLayer);

        HandleCameraRotation();

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (currentBehavior.GetType() == typeof(NormalPlayerBehavior))
            {
                SetBehavior(new FireProtocolBehavior());
                Debug.Log("Cambiando a modo Protocolo de Incendio.");
            }
            else
            {
                SetBehavior(new NormalPlayerBehavior());
                Debug.Log("Cambiando a modo Normal.");
            }
        }
    }

    public void SetBehavior(IPlayerBehaviorStrategy newBehavior)
    {
        currentBehavior = newBehavior;
    }

    void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}