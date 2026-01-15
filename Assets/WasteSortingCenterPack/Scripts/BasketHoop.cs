using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class BasketHoop : MonoBehaviour
{
    [Tooltip("Tag du ballon de basket")]
    public string basketballTag = "Basketball";

    [Tooltip("Points gagnés quand on marque")]
    public int scorePoints = 10;

    [Header("UI Feedback")]
    [Tooltip("Prefab du texte flottant à instancier")]
    public GameObject floatingTextPrefab;
    [Tooltip("Couleur du texte")]
    public Color scoreColor = new Color(1f, 0.5f, 0f); // Orange

    [Header("Identité Visuelle (Anneau)")]
    public float ringRadius = 0.5f;
    public float ringWidth = 0.05f;
    public float baseHeight = 0.0f; // Hauteur de base par rapport au pivot
    public float verticalRange = 0.3f;
    public float verticalSpeed = 2.0f;
    public int segments = 50;
    public Color ringColor = new Color(1f, 0.5f, 0f); // Orange par défaut

    private LineRenderer lineRenderer;
    private GameObject ringObject;

    [Header("Audio")]
    public AudioClip scoreSound;
    [Range(0f, 1f)]
    public float soundVolume = 1.0f;
    
    private AudioSource audioSource;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configuration Audio 2D pour être sûr qu'on l'entende peu importe la distance ou la position du filet
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = soundVolume;
        audioSource.spatialBlend = 0f;

        SetupRing();
    }

    private void Update()
    {
        AnimateRing();
    }

    private void SetupRing()
    {
        // Création d'un objet enfant pour l'anneau si non existant
        if (ringObject == null)
        {
            ringObject = new GameObject("VisualRing");
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localPosition = Vector3.zero;
        }

        lineRenderer = ringObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = ringObject.AddComponent<LineRenderer>();
        }

        // Configuration du LineRenderer
        lineRenderer.useWorldSpace = false; // Important pour suivre le mouvement local
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = ringWidth;
        lineRenderer.endWidth = ringWidth;
        
        Shader shader = Shader.Find("Sprites/Default"); 
        if(shader == null) shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
        
        lineRenderer.material = new Material(shader);

        UpdateRingColor();
    }

    private void UpdateRingColor()
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = ringColor;
            lineRenderer.endColor = ringColor;
        }
    }

    private void AnimateRing()
    {
        if (lineRenderer == null) return;
        
        UpdateRingColor();

        float angleStep = 360f / segments;
        float yOffset = baseHeight + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
        
        Vector3[] positions = new Vector3[segments];

        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * ringRadius;
            float z = Mathf.Sin(angle) * ringRadius;

            positions[i] = new Vector3(x, yOffset, z);
        }

        lineRenderer.SetPositions(positions);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(basketballTag))
        {
            // Marquer des points
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPoints(scorePoints);
            }

            // Audio
            if (audioSource != null && scoreSound != null)
            {
                audioSource.PlayOneShot(scoreSound, soundVolume);
            }

            // UI
            if (floatingTextPrefab != null)
            {
                GameObject ft = Instantiate(floatingTextPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                TMP_Text txt = ft.GetComponent<TMP_Text>();
                if (txt != null)
                {
                    txt.text = "+" + scorePoints;
                    txt.color = scoreColor;
                }
                
                // Ajouter le script FloatingScore si pas présent (optionnel si le prefab l'a déjà)
                if (ft.GetComponent<FloatingScore>() == null)
                    ft.AddComponent<FloatingScore>();
            }

            // Détruire le ballon
            Destroy(other.gameObject);
        }
    }
}
