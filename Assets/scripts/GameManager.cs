using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referencias de Objetos")]
    public Incendiable baseDeDatosIncendiable;
    public GameObject cajaRoja;
    public GameObject botonAlarma;
    public GameObject botonGas;

    [Header("Referencias del Jugador y Cámara")]
    public PlayerController playerController;
    public Transform playerSpawnPoint;
    public Camera playerCamera;

    [Header("Alarma y Luces")]
    public AudioSource alarmAudioSource;
    public Light[] alarmLights;
    public Color alarmColor = Color.red;
    public float lightFlashInterval = 0.5f;
    private bool isAlarmFlashing = false;
    private Color[] originalLightColors;

    [Header("Protocolo de Gas")]
    public ParticleSystem gasParticlesPrefab;
    public float gasDuration = 10f;
    public float gasExtinguishTime = 2f;
    public AudioClip gasSoundClip;
    private AudioSource gasAudioSource;
    [Tooltip("La velocidad de reproducción de las partículas de gas. 1 es normal, 0.20 es más lento.")]
    public float gasPlaybackSpeed = 0.20f;

    private ParticleSystem currentGasParticlesInstance;
    private bool isGasActive = false;

    [Header("Temporizador")]
    public float tiempoLimiteSimulacro = 60f;
    public TextMeshProUGUI timerText;
    public GameObject temporizadorUI;
    public AudioClip timerSoundEffect;
    private AudioSource timerAudioSource;

    [Header("Estado del Simulacro")]
    public bool simulacroActivo = false;
    private float tiempoRestante;
    private bool simulacroFailed = false;

    [Header("Objetivos del Simulacro")]
    public bool alarmaActivada = false;
    public bool gasActivado = false;
    public bool gasReseteado = false;

    private InteractableButton_V2 botonAlarmaInteractable;
    private InteractableButton_V2 botonGasInteractable;
    private InteractableButton_V2 botonResetInteractable;

    [Header("UI del Simulacro")]
    public GameObject simulacroStartButton;

    [Header("UI de Resultado del Protocolo")]
    public GameObject protocolFailedPanel;
    public GameObject protocolSuccessPanel;

    [Header("Configuración de Escenas")]
    public string mainMenuSceneName = "MainMenu";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        tiempoRestante = tiempoLimiteSimulacro;
        if (temporizadorUI != null) temporizadorUI.SetActive(false);

        if (botonAlarma != null) botonAlarmaInteractable = botonAlarma.GetComponent<InteractableButton_V2>();
        if (botonGas != null) botonGasInteractable = botonGas.GetComponent<InteractableButton_V2>();
        if (cajaRoja != null) botonResetInteractable = cajaRoja.GetComponentInChildren<InteractableButton_V2>();

        SetInteractionButtonsActive(false);

        if (temporizadorUI != null)
        {
            timerAudioSource = temporizadorUI.GetComponent<AudioSource>();
            if (timerAudioSource == null)
            {
                Debug.LogWarning("El GameObject 'temporizadorUI' no tiene un componente AudioSource. Asegúrate de añadir uno para el sonido del temporizador.");
            }
        }

        originalLightColors = new Color[alarmLights.Length];
        for (int i = 0; i < alarmLights.Length; i++)
        {
            if (alarmLights[i] != null)
            {
                originalLightColors[i] = alarmLights[i].color;
            }
        }

        gasAudioSource = GetComponent<AudioSource>();
        if (gasAudioSource == null)
        {
            gasAudioSource = gameObject.AddComponent<AudioSource>();
        }
        gasAudioSource.loop = false;
        gasAudioSource.playOnAwake = false;

        if (protocolFailedPanel != null)
        {
            protocolFailedPanel.SetActive(false);
        }
        if (protocolSuccessPanel != null)
        {
            protocolSuccessPanel.SetActive(false);
        }
    }

    void Start()
    {
        if (playerController != null)
        {
            if (playerSpawnPoint != null)
            {
                playerController.transform.position = playerSpawnPoint.position;
                playerController.transform.rotation = playerSpawnPoint.rotation;
            }
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        if (simulacroStartButton != null)
        {
            simulacroStartButton.SetActive(true);
        }
    }

    void Update()
    {
        if (simulacroActivo && !simulacroFailed)
        {
            tiempoRestante -= Time.deltaTime;
            UpdateTimerUI();

            if (tiempoRestante <= 0)
            {
                tiempoRestante = 0;
                UpdateTimerUI();
                Debug.Log("¡Tiempo agotado! El simulacro ha fallado.");
                EndSimulacro(false);
            }
            else
            {
                CheckWinCondition();
            }
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(tiempoRestante / 60);
            int seconds = Mathf.FloorToInt(tiempoRestante % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void StartSimulacro()
    {
        if (!simulacroActivo)
        {
            Debug.Log("¡Simulacro Iniciado!");
            simulacroActivo = true;
            simulacroFailed = false;
            tiempoRestante = tiempoLimiteSimulacro;

            if (temporizadorUI != null)
            {
                temporizadorUI.SetActive(true);
                Debug.Log("Temporizador UI (contenedor) activado.");
            }

            if (timerAudioSource != null && timerSoundEffect != null)
            {
                timerAudioSource.clip = timerSoundEffect;
                timerAudioSource.Play();
                Debug.Log("Sonido del temporizador iniciado.");
            }

            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
                Debug.Log("TimerText (GameObject del texto) activado explícitamente.");
            }

            UpdateTimerUI();
            Debug.Log("Primer update del TimerText ejecutado.");

            if (baseDeDatosIncendiable != null)
            {
                baseDeDatosIncendiable.SetOnFire(true);
                Debug.Log("Fuego iniciado en " + baseDeDatosIncendiable.name);
            }

            SetInteractionButtonsActive(true);

            alarmaActivada = false;
            gasActivado = false;
            gasReseteado = false;
            isGasActive = false;
            if (currentGasParticlesInstance != null)
            {
                currentGasParticlesInstance.Stop();
                currentGasParticlesInstance.gameObject.SetActive(false);
            }

            if (playerController != null)
            {
                playerController.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (simulacroStartButton != null)
            {
                simulacroStartButton.SetActive(false);
            }
            if (protocolFailedPanel != null) protocolFailedPanel.SetActive(false);
            if (protocolSuccessPanel != null) protocolSuccessPanel.SetActive(false);
        }
    }

    public void ObjetivoAlarmaActivada()
    {
        if (simulacroActivo && !alarmaActivada)
        {
            alarmaActivada = true;
            Debug.Log("Objetivo: ¡Alarma Activada!");
            if (botonAlarmaInteractable != null) botonAlarmaInteractable.SetInteractable(false);
            StartAlarm();
        }
    }

    public void ObjetivoGasActivado()
    {
        if (simulacroActivo && alarmaActivada && !isGasActive && baseDeDatosIncendiable != null && baseDeDatosIncendiable.IsOnFire())
        {
            gasActivado = true;
            isGasActive = true;
            Debug.Log("Objetivo: ¡Sistema de Gas Activado!");
            if (botonGasInteractable != null) botonGasInteractable.SetInteractable(false);

            StartGasProtocol();
        }
        else if (!simulacroActivo)
        {
            Debug.Log("Gas: El simulacro no está activo.");
        }
        else if (!alarmaActivada)
        {
            Debug.Log("Gas: La alarma debe estar activada primero para usar el gas.");
        }
        else if (isGasActive)
        {
            Debug.Log("Gas: El gas ya está activo.");
        }
        else if (baseDeDatosIncendiable == null || !baseDeDatosIncendiable.IsOnFire())
        {
            Debug.Log("Gas: No hay fuego que apagar o la base de datos no es incendiable.");
        }
    }

    public void ObjetivoGasReseteado()
    {
        if (simulacroActivo && !gasReseteado)
        {
            gasReseteado = true;
            Debug.Log("Objetivo: ¡Sistema de Gas Reseteado!");
            if (botonResetInteractable != null) botonResetInteractable.SetInteractable(false);

            StopAlarm();

            EndSimulacro(true);
        }
    }

    void SetInteractionButtonsActive(bool active)
    {
        if (botonAlarmaInteractable != null) botonAlarmaInteractable.SetInteractable(active);
        if (botonGasInteractable != null) botonGasInteractable.SetInteractable(active);
        if (botonResetInteractable != null) botonResetInteractable.SetInteractable(active);
    }

    void CheckWinCondition()
    {
        if (alarmaActivada && gasActivado && gasReseteado)
        {
            Debug.Log("¡Todos los objetivos completados! Simulacro exitoso.");
            EndSimulacro(true);
        }
    }

    void EndSimulacro(bool success)
    {
        simulacroActivo = false;
        if (temporizadorUI != null) temporizadorUI.SetActive(false);

        if (baseDeDatosIncendiable != null)
        {
            baseDeDatosIncendiable.SetOnFire(false);
        }

        if (timerAudioSource != null && timerAudioSource.isPlaying)
        {
            timerAudioSource.Stop();
            Debug.Log("Sonido del temporizador detenido.");
        }

        StopAlarm();
        StopGasProtocol();

        SetInteractionButtonsActive(false);

        if (success)
        {
            Debug.Log("¡FELICITACIONES! Simulacro de incendio completado con éxito.");
            if (protocolSuccessPanel != null)
            {
                protocolSuccessPanel.SetActive(true);
            }
            if (playerController != null)
            {
                playerController.enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            simulacroFailed = true;
            Debug.Log("SIMULACRO FALLIDO. Inténtalo de nuevo.");
            if (protocolFailedPanel != null)
            {
                protocolFailedPanel.SetActive(true);
            }

            if (playerController != null)
            {
                playerController.enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    void StartAlarm()
    {
        if (alarmAudioSource != null && !alarmAudioSource.isPlaying)
        {
            alarmAudioSource.Play();
            Debug.Log("Sonido de alarma iniciado.");
        }

        if (alarmLights.Length > 0 && !isAlarmFlashing)
        {
            isAlarmFlashing = true;
            StartCoroutine(FlashAlarmLights());
            Debug.Log("Luces de alarma rojas y parpadeando.");
        }
    }

    public void StopAlarm()
    {
        if (alarmAudioSource != null && alarmAudioSource.isPlaying)
        {
            alarmAudioSource.Stop();
            Debug.Log("Sonido de alarma detenido.");
        }

        if (isAlarmFlashing)
        {
            isAlarmFlashing = false;
            StopCoroutine("FlashAlarmLights");
            RestoreOriginalLightColors();
            Debug.Log("Luces de alarma restauradas.");
        }
    }

    IEnumerator FlashAlarmLights()
    {
        while (isAlarmFlashing)
        {
            foreach (Light light in alarmLights)
            {
                if (light != null)
                {
                    light.color = alarmColor;
                }
            }
            yield return new WaitForSeconds(lightFlashInterval);

            foreach (Light light in alarmLights)
            {
                if (light != null)
                {
                    light.color = originalLightColors[System.Array.IndexOf(alarmLights, light)];
                }
            }
            yield return new WaitForSeconds(lightFlashInterval);
        }
        RestoreOriginalLightColors();
    }

    void RestoreOriginalLightColors()
    {
        for (int i = 0; i < alarmLights.Length; i++)
        {
            if (alarmLights[i] != null && originalLightColors.Length > i)
            {
                alarmLights[i].color = originalLightColors[i];
            }
        }
    }

    void StartGasProtocol()
    {
        if (gasAudioSource != null && gasSoundClip != null && !gasAudioSource.isPlaying)
        {
            gasAudioSource.clip = gasSoundClip;
            gasAudioSource.Play();
            Debug.Log("Sonido de gas iniciado.");
        }

        if (gasParticlesPrefab != null)
        {
            Vector3 gasSpawnPos = baseDeDatosIncendiable != null ? baseDeDatosIncendiable.transform.position : transform.position;

            if (currentGasParticlesInstance == null)
            {
                currentGasParticlesInstance = Instantiate(gasParticlesPrefab, gasSpawnPos, Quaternion.identity);
            }
            else
            {
                currentGasParticlesInstance.transform.position = gasSpawnPos;
                currentGasParticlesInstance.gameObject.SetActive(true);
            }

            var mainModule = currentGasParticlesInstance.main;
            mainModule.simulationSpeed = gasPlaybackSpeed;

            currentGasParticlesInstance.Play();
            Debug.Log("Partículas de gas activadas en " + gasSpawnPos + " con velocidad: " + gasPlaybackSpeed);
        }

        StartCoroutine(ExtinguishFireOverTime());
        StartCoroutine(StopGasAfterDuration(gasDuration));
    }

    IEnumerator ExtinguishFireOverTime()
    {
        float timer = 0f;
        while (timer < gasExtinguishTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (baseDeDatosIncendiable != null && baseDeDatosIncendiable.IsOnFire())
        {
            baseDeDatosIncendiable.SetOnFire(false);
            Debug.Log(baseDeDatosIncendiable.name + " fuego apagado por gas.");
        }
    }

    IEnumerator StopGasAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopGasProtocol();
    }

    void StopGasProtocol()
    {
        if (currentGasParticlesInstance != null)
        {
            currentGasParticlesInstance.Stop();
            currentGasParticlesInstance.gameObject.SetActive(false);
        }
        if (gasAudioSource != null && gasAudioSource.isPlaying)
        {
            gasAudioSource.Stop();
        }
        isGasActive = false;
        Debug.Log("Partículas y sonido de gas detenidos.");
    }

    public void RestartSimulacro()
    {
        Debug.Log("Reiniciando Simulacro...");
        simulacroActivo = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Debug.Log("Cargando la escena del menú principal...");
        simulacroActivo = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}