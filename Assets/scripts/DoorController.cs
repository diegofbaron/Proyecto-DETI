using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Configuración de la Puerta")]
    [Tooltip("El ángulo final de apertura de la puerta (por ejemplo, 90 para abrir hacia la derecha, -90 para la izquierda).")]
    public float openAngle = 90f;
    [Tooltip("El tiempo que tarda la puerta en abrirse/cerrarse.")]
    public float animationDuration = 1f;
    [Tooltip("Si la puerta se cierra automáticamente después de un tiempo.")]
    public bool autoClose = true;
    [Tooltip("Tiempo en segundos antes de que la puerta se cierre automáticamente si autoClose es true.")]
    public float autoCloseDelay = 3f;

    [Header("Efectos Sonoros (Opcional)")]
    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource audioSource;

    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private bool isOpen = false;
    private Coroutine currentDoorRoutine;

    void Awake()
    {
        initialRotation = transform.localRotation;
        targetRotation = initialRotation * Quaternion.Euler(0, openAngle, 0);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void ToggleDoor()
    {
        if (currentDoorRoutine != null)
        {
            StopCoroutine(currentDoorRoutine);
        }

        if (isOpen)
        {
            currentDoorRoutine = StartCoroutine(CloseDoor());
        }
        else
        {
            currentDoorRoutine = StartCoroutine(OpenDoor());
        }
    }

    IEnumerator OpenDoor()
    {
        isOpen = true;
        float timer = 0f;
        Quaternion startRot = transform.localRotation;

        if (openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        while (timer < animationDuration)
        {
            transform.localRotation = Quaternion.Slerp(startRot, targetRotation, timer / animationDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = targetRotation;

        if (autoClose)
        {
            currentDoorRoutine = StartCoroutine(AutoCloseDoorAfterDelay());
        }
    }

    IEnumerator CloseDoor()
    {
        isOpen = false;
        float timer = 0f;
        Quaternion startRot = transform.localRotation;

        if (closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        while (timer < animationDuration)
        {
            transform.localRotation = Quaternion.Slerp(startRot, initialRotation, timer / animationDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = initialRotation;
    }

    IEnumerator AutoCloseDoorAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        if (isOpen)
        {
            ToggleDoor();
        }
    }

    public void Open()
    {
        if (!isOpen) ToggleDoor();
    }

    public void Close()
    {
        if (isOpen) ToggleDoor();
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}