using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class InteractableButton_V2 : MonoBehaviour
{
    [Header("Configuración del Botón")]
    [Tooltip("La cantidad que el botón se moverá hacia adentro cuando se presione.")]
    public float pressDistance = 0.05f;
    [Tooltip("El tiempo que tarda el botón en presionarse/soltarse.")]
    public float animationDuration = 0.1f;
    [Tooltip("Tiempo de retraso antes de que el botón se reestablezca a su posición original (si IsToggle es falso).")]
    public float resetDelay = 0.2f;

    [Tooltip("La dirección en el espacio local del botón que representa 'hacia adentro' o 'hacia abajo' cuando se presiona.")]
    public Vector3 pressDirection = Vector3.forward;

    [Tooltip("Si el botón es un interruptor (Toggle) o solo se presiona una vez y se reestablece.")]
    public bool isToggle = false;

    [Tooltip("Si el botón es actualmente interactuable (clicable).")]
    public bool isInteractable = true;

    [Header("Sonidos")]
    public AudioClip pressSound;
    public AudioClip releaseSound;
    private AudioSource audioSource;

    [Header("Eventos")]
    public UnityEvent OnButtonPressed;

    private Vector3 initialPosition;
    private Vector3 pressedPosition;
    private bool isPressed = false;
    private Coroutine currentButtonAnimation;

    void Awake()
    {
        initialPosition = transform.localPosition;
        pressedPosition = initialPosition - pressDirection.normalized * pressDistance;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void Interact()
    {
        if (!isInteractable)
        {
            Debug.Log($"Botón {gameObject.name} no es interactuable en este momento.");
            return;
        }

        if (currentButtonAnimation != null)
        {
            StopCoroutine(currentButtonAnimation);
        }

        if (isToggle)
        {
            if (isPressed)
            {
                currentButtonAnimation = StartCoroutine(ReleaseButtonRoutine());
            }
            else
            {
                currentButtonAnimation = StartCoroutine(PressButtonRoutine());
            }
            isPressed = !isPressed;
        }
        else
        {
            if (!isPressed)
            {
                currentButtonAnimation = StartCoroutine(PressAndReleaseButtonRoutine());
            }
        }
    }
    IEnumerator PressAndReleaseButtonRoutine()
    {
        isPressed = true;
        SetInteractable(false);

        yield return StartCoroutine(AnimateButtonToPosition(pressedPosition));

        if (audioSource != null && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
        }

        OnButtonPressed.Invoke();

        yield return new WaitForSeconds(resetDelay);

        yield return StartCoroutine(AnimateButtonToPosition(initialPosition));

        if (audioSource != null && releaseSound != null)
        {
            audioSource.PlayOneShot(releaseSound);
        }

        isPressed = false;
        SetInteractable(true);
    }

    IEnumerator PressButtonRoutine()
    {
        SetInteractable(false);
        yield return StartCoroutine(AnimateButtonToPosition(pressedPosition));

        if (audioSource != null && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
        }

        OnButtonPressed.Invoke();
    }

    IEnumerator ReleaseButtonRoutine()
    {
        SetInteractable(false);
        yield return StartCoroutine(AnimateButtonToPosition(initialPosition));

        if (audioSource != null && releaseSound != null)
        {
            audioSource.PlayOneShot(releaseSound);
        }

        SetInteractable(true);
    }
    IEnumerator AnimateButtonToPosition(Vector3 targetPos)
    {
        Vector3 startPos = transform.localPosition;
        float timer = 0f;

        while (timer < animationDuration)
        {
            transform.localPosition = Vector3.Lerp(startPos, targetPos, timer / animationDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPos;
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;
    }
}