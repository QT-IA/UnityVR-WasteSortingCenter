using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Tooltip("Le texte qui affiche le score total (ex: au dessus de la poubelle jaune)")]
    public TMP_Text totalScoreText;

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
    }

    private void Start()
    {
        UpdateScoreDisplay();
    }

    public void AddPoints(int amount)
    {
        currentScore += amount;
        UpdateScoreDisplay();
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
