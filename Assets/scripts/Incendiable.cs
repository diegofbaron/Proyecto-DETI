using UnityEngine;

public class Incendiable : MonoBehaviour
{
    [Tooltip("Referencia al sistema de partículas de fuego.")]
    public ParticleSystem fireParticles;
    [Tooltip("Sonido del incendio (opcional).")]
    public AudioClip fireSound;

    private AudioSource audioSource;
    private bool isOnFire = false;

    void Awake()
    {
        if (fireParticles == null)
        {
            Debug.LogWarning("El sistema de partículas de fuego no está asignado en " + gameObject.name);
            fireParticles = GetComponentInChildren<ParticleSystem>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;
        if (fireSound != null)
        {
            audioSource.clip = fireSound;
        }

        SetOnFire(false);
    }

    public void SetOnFire(bool state)
    {
        if (isOnFire == state) return;

        isOnFire = state;
        if (fireParticles != null)
        {
            if (state)
            {
                fireParticles.Play();
                if (audioSource != null && fireSound != null)
                {
                    audioSource.Play();
                }
                Debug.Log(gameObject.name + " ¡Se está incendiando!");
            }
            else
            {
                fireParticles.Stop();
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
                Debug.Log(gameObject.name + " El incendio ha sido sofocado.");
            }
        }
    }

    public bool IsOnFire()
    {
        return isOnFire;
    }
}