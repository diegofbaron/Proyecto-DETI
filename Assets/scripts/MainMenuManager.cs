using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public string gameSceneName = "GameScene";

    public GameObject instructionsPanel;

    [Header("Música de Fondo del Menú")]
    public AudioClip menuBackgroundMusic;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = menuBackgroundMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.3f;
    }

    void Start()
    {
        if (audioSource != null && menuBackgroundMusic != null)
        {
            audioSource.Play();
        }
    }
    public void OpenInstructions()
    {
        Debug.Log("Abriendo instrucciones...");
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }

    }
    public void CloseInstructions()
    {
        Debug.Log("Cerrando instrucciones y regresando al menú...");
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        Debug.Log("Cargando juego...");
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}