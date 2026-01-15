using UnityEngine;
using TMPro; // Assurez-vous d'avoir TextMeshPro installé

[RequireComponent(typeof(Collider))]
public class WasteDestroyerUI : MonoBehaviour
{
    [Header("Game Rules")]
    [Tooltip("Est-ce la poubelle de recyclage (Verte) ?\nTRUE = Attend objets sans tag ('Untagged')\nFALSE = Attend objets taggés 'dechet_pas_recyclable'")]
    public bool isRecyclingBin = true;

    [Tooltip("Délai avant destruction (en secondes). 0 = immédiat.")]
    public float destroyDelay = 0f;

    [Header("Identité Visuelle (Anneau)")]
    public float ringRadius = 0.5f;
    public float ringWidth = 0.05f;
    public float baseHeight = 0.0f; // Hauteur de base par rapport au pivot
    public float verticalRange = 0.3f;
    public float verticalSpeed = 2.0f;
    public int segments = 50;
    
    public Color recyclingColor = Color.green;
    public Color wasteColor = Color.red;

    private LineRenderer lineRenderer;
    private GameObject ringObject;

    [Header("Visual FX")]
    [Tooltip("Prefab de particules à instancier lors de la destruction (ex: feu/fumée/confettis)")]
    public GameObject destroyParticles;

    [Header("UI Feedback")]
    [Tooltip("ASSIGNER ICI LE TEXTE 'SCORE +1' (celui au dessus de la poubelle verte)")]
    public TMP_Text successTextUI;

    [Tooltip("ASSIGNER ICI LE TEXTE 'SCORE -1' (celui au dessus de la poubelle rouge)")]
    public TMP_Text errorTextUI;

    [Tooltip("Durée d'affichage du message en secondes")]
    public float messageDuration = 1.0f;

    [Tooltip("Couleur par défaut du texte (quand rien n'est affiché)")]
    public Color defaultColor = Color.white;

    [Header("Feedback Succès (Bon Tag)")]
    public string successMessage = "+1";
    public Color successColor = Color.green;

    [Header("Feedback Erreur (Mauvais Tag)")]
    public string errorMessage = "-1";
    public Color errorColor = Color.red;

    [Header("Audio Feedback")]
    public AudioClip successSound;
    public AudioClip errorSound;
    [Range(0f, 1f)] public float soundVolume = 1.0f;
    private AudioSource audioSource;

    [Header("Animation Feedback")]
    [Tooltip("Objet visuel à faire rebondir quand un objet entre. Si vide, utilise ce Transform.")]
    public Transform binVisual;
    [Tooltip("Force du rebond (scale mul)")]
    public float bounceStrength = 0.1f;
    [Tooltip("Durée du rebond")]
    public float bounceDuration = 0.3f;

    [Header("Debug")]
    [Tooltip("Activer les logs pour le debug")]
    public bool debugLog = false;

    private void Start()
    {
        SetupRing();

        // Vérifier que le collider est bien en mode Trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("WasteDestroyerUI: Le Collider doit être en mode Trigger. Activation automatique.");
            col.isTrigger = true;
        }

        HideMessage();

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Configure AudioSource pour feedback UI : non-spatialisé (2D) -> volume constant
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = soundVolume;
        audioSource.spatialBlend = 0f;
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
        
        // Shader simple (Legacy Particules ou similaire fonctionne bien sans texture)
        // On essaie de trouver un shader de base Unity qui supporte les couleurs vertex
        Shader shader = Shader.Find("Sprites/Default"); 
        if(shader == null) shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
        
        lineRenderer.material = new Material(shader);

        UpdateRingColor();
    }

    private void UpdateRingColor()
    {
        if (lineRenderer != null)
        {
            Color targetColor = isRecyclingBin ? recyclingColor : wasteColor;
            lineRenderer.startColor = targetColor;
            lineRenderer.endColor = targetColor;
        }
    }

    private void AnimateRing()
    {
        if (lineRenderer == null) return;

        // On peut mettre à jour la couleur si on change en runtime
        // Note: Pour optimiser, ne le faire que si isRecyclingBin change
        // Mais ici c'est léger
        UpdateRingColor();

        float angleStep = 360f / segments;
        float yOffset = baseHeight + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;

        // On s'assure que le collider est pris en compte pour la hauteur de base si besoin
        // Ici on prend juste le centre local (0,0,0) + oscillation Y
        
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
        string objectName = other.gameObject.name;
        string tag = other.tag;

        if (debugLog)
        {
            Debug.Log($"WasteDestroyerUI: Objet entré: {objectName} | Tag: {tag} | Mode Recyclage: {isRecyclingBin}");
        }

        // Animation de rebond
        StartCoroutine(AnimateBounce());

        // Particules (Depop)
        if (destroyParticles != null)
        {
            Instantiate(destroyParticles, other.transform.position, Quaternion.identity);
        }

        // Si c'est un ballon de basket, on le détruit mais on met 0 points (ni bonus, ni malus) du point de vue de CETTE poubelle
        // car le ballon doit aller dans le panier
        if (tag == "Basketball")
        {
             if (destroyDelay > 0f) Destroy(other.gameObject, destroyDelay);
             else Destroy(other.gameObject);
             return;
        }

        // Afficher le message UI (Succès ou Erreur selon la logique)
        ShowDestructionMessage(tag);


        // Détruire l'objet
        if (destroyDelay > 0f)
        {
            Destroy(other.gameObject, destroyDelay);
        }
        else
        {
            Destroy(other.gameObject);
        }
    }

    private void ShowDestructionMessage(string objectTag)
    {
        bool isSuccess = false;

        if (isRecyclingBin)
        {
            // Poubelle Verte : Succès si l'objet n'a PAS de tag (ou "Untagged")
            isSuccess = (objectTag == "Untagged");
        }
        else
        {
            // Poubelle Rouge : Succès si l'objet est "dechet_pas_recyclable"
            isSuccess = (objectTag == "dechet_pas_recyclable");
        }

        if (isSuccess)
        {
            // AUDIO
            if (audioSource != null && successSound != null)
            {
                audioSource.PlayOneShot(successSound, soundVolume);
            }

            // AJOUT DE POINTS
            int pointsAdded = 1;

            if (ScoreManager.Instance != null)
            {
                pointsAdded = ScoreManager.Instance.AddPoints(1);
            }

            if (successTextUI != null)
            {
                // Affiche les points réélement gagnés (ex: +1, +2, +3 ou +0)
                SpawnFloatingText(successTextUI, "+" + pointsAdded, successColor);
            }
        }
        else
        {
            // AUDIO
            if (audioSource != null && errorSound != null)
            {
                audioSource.PlayOneShot(errorSound, soundVolume);
            }

            // RETRAIT DE POINTS
            int pointsRemoved = -1;

            if (ScoreManager.Instance != null)
            {
                pointsRemoved = ScoreManager.Instance.AddPoints(-1);
            }

            if (errorTextUI != null)
            {
                SpawnFloatingText(errorTextUI, pointsRemoved.ToString(), errorColor);
            }
        }
        // Plus besoin de HideMessage car les textes volants se détruisent tout seuls
    }

    private void SpawnFloatingText(TMP_Text template, string text, Color color)
    {
        // On crée une copie du texte en gardant le MÊME PARENT pour conserver la taille/échelle locale correcte
        GameObject clone = Instantiate(template.gameObject, template.transform.position, template.transform.rotation, template.transform.parent);
        
        // On s'assure qu'il est bien visible et configuré
        clone.SetActive(true);
        
        // On lui met le bon texte et la bonne couleur
        TMP_Text cloneText = clone.GetComponent<TMP_Text>();
        cloneText.text = text;
        cloneText.color = color;

        // On ajoute le script d'animation
        clone.AddComponent<FloatingScore>();
    }

    private void HideMessage()
    {
        // On vide juste les textes templates pour qu'ils restent invisibles
        if (successTextUI != null) successTextUI.text = "";
        if (errorTextUI != null) errorTextUI.text = "";
    }

    private System.Collections.IEnumerator AnimateBounce()
    {
        Transform target = binVisual != null ? binVisual : transform;
        Vector3 initialScale = target.localScale;
        
        // On évite d'empiler les coroutines qui modifient le scale en même temps si possible, mais ici c'est rapide
        // Effet de squash/stretch simple : on réduit la hauteur (Y) et augmente la largeur (X/Z)
        Vector3 squatScale = new Vector3(initialScale.x * (1 + bounceStrength), initialScale.y * (1 - bounceStrength), initialScale.z * (1 + bounceStrength));

        float timer = 0f;
        while(timer < bounceDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / bounceDuration;
            
            // Rebond élastique : 0 -> 1 -> 0 avec un dépassement possible
            // Utilisons une courbe sinus pour l'aller-retour simple
            float curve = Mathf.Sin(progress * Mathf.PI);

            target.localScale = Vector3.Lerp(initialScale, squatScale, curve);
            yield return null;
        }
        target.localScale = initialScale;
    }
}