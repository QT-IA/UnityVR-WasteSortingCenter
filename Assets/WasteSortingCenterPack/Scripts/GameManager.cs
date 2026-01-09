using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public Spawner spawner;
    public TreadmillsController treadmills;
    public ScoreManager scoreManager;

    [Header("Game Settings")]
    public bool startOnAwake = false;
    
    // Si vrai, les points ne sont pas comptabilisés
    [HideInInspector] public bool isDemoMode = false;
    
    // Etat du jeu
    [HideInInspector] public bool isGameRunning = false;

    [Header("Events")]
    public UnityEvent onGameStart;
    public UnityEvent onDemoStart;
    public UnityEvent onGameStop;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (spawner == null) spawner = FindFirstObjectByType<Spawner>();
        if (treadmills == null) treadmills = FindFirstObjectByType<TreadmillsController>();
        if (scoreManager == null) scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void Start()
    {
        if (!startOnAwake)
        {
            StopGame();
        }
    }

    [ContextMenu("Start Game")]
    public void StartGame()
    {
        isGameRunning = true;
        isDemoMode = false;
        ResetAndRun();
        onGameStart.Invoke();
        Debug.Log("GameManager: Lancement du jeu (Mode Normal)");
    }

    [ContextMenu("Toggle Game")]
    public void ToggleGame()
    {
        if (isGameRunning)
            StopGame();
        else
            StartGame();
    }

    [ContextMenu("Start Demo")]
    public void StartDemo()
    {
        isGameRunning = true;
        isDemoMode = true;
        ResetAndRun();
        onDemoStart.Invoke();
        Debug.Log("GameManager: Lancement du jeu (Mode Démo - Pas de score)");
    }

    [ContextMenu("Toggle Demo")]
    public void ToggleDemo()
    {
        if (isGameRunning)
            StopGame();
        else
            StartDemo();
    }

    private void ResetAndRun()
    {
        if (scoreManager != null) scoreManager.ResetScore();
        
        if (treadmills != null)
        {
            treadmills.SetPaused(false);
        }

        if (spawner != null)
        {
            spawner.enabled = true;
        }
    }

    [ContextMenu("Stop Game")]
    public void StopGame()
    {
        isGameRunning = false;
        if (spawner != null) spawner.enabled = false;
        if (treadmills != null) treadmills.SetPaused(true);
        
        onGameStop.Invoke();
        Debug.Log("GameManager: Jeu Arrêté");
    }
}
