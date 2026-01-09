using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Tooltip("Le texte qui affiche le score total (ex: au dessus de la poubelle jaune)")]
    public TMP_Text totalScoreText;

    [Header("Rules")]
    [SerializeField] TreadmillsController treadmillsController;
    [Tooltip("Multiplicateur de score quand le tapis est en pause")]
    [SerializeField] float pausedMultiplier = 0f; 
    [Tooltip("Multiplicateur de score à vitesse min (0)")]
    [SerializeField] float minSpeedMultiplier = 1f;
    [Tooltip("Multiplicateur de score à vitesse max (100%)")]
    [SerializeField] float maxSpeedMultiplier = 3f;
    [Tooltip("A partir de quel pourcentage de vitesse (0-1) on atteint le multiplicateur max (ex: 0.8 = 80%)")]
    [SerializeField, Range(0.1f, 1f)] float maxScoreSpeedThreshold = 0.8f;
    [Tooltip("A partir de quel pourcentage de vitesse (0-1) on quitte le multiplicateur min (ex: 0.2 = 20%)")]
    [SerializeField, Range(0f, 0.5f)] float minScoreSpeedThreshold = 0.2f;

    private int currentScore = 0;

    private void Awake()
    {
        // Singleton pattern pour y accéder facilement depuis les autres scripts
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (treadmillsController == null)
            treadmillsController = FindFirstObjectByType<TreadmillsController>();
    }

    private void Start()
    {
        UpdateScoreDisplay();
    }

    public int AddPoints(int amount)
    {
        int finalAmount = amount;

        // Applique le multiplicateur seulement sur les gains de points
        if (amount > 0 && treadmillsController != null)
        {
            if (treadmillsController.isPaused)
            {
                finalAmount = Mathf.RoundToInt(amount * pausedMultiplier);
            }
            else
            {
                float ratio = treadmillsController.GetSpeedRatio();
                
                // Remap ratio avec un seuil min et max
                // Si ratio < minThreshold -> 0
                // Si ratio > maxThreshold -> 1
                float scoreRatio = Mathf.InverseLerp(minScoreSpeedThreshold, maxScoreSpeedThreshold, ratio);

                // Utilise RoundToInt pour avoir des entiers stricts (1, 2 ou 3)
                float multiplier = Mathf.Lerp(minSpeedMultiplier, maxSpeedMultiplier, scoreRatio);
                finalAmount = Mathf.RoundToInt(amount * multiplier);
            }
        }

        currentScore += finalAmount;
        UpdateScoreDisplay();
        return finalAmount;
    }

    private void UpdateScoreDisplay()
    {
        if (totalScoreText != null)
        {
            // Affiche juste le chiffre ou "Score: X" selon vos préférences
            totalScoreText.text = $"{currentScore}"; 
        }
    }
}
