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

    [Header("Debug")]
    [Tooltip("Activer les logs pour le debug")]
    public bool debugLog = false;

    private void Start()
    {
        // Vérifier que le collider est bien en mode Trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("WasteDestroyerUI: Le Collider doit être en mode Trigger. Activation automatique.");
            col.isTrigger = true;
        }

        HideMessage();
    }

    private void OnTriggerEnter(Collider other)
    {
        string objectName = other.gameObject.name;
        string tag = other.tag;

        if (debugLog)
        {
            Debug.Log($"WasteDestroyerUI: Objet entré: {objectName} | Tag: {tag} | Mode Recyclage: {isRecyclingBin}");
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
}