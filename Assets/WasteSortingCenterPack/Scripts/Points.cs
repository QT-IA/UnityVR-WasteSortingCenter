using UnityEngine;
using TMPro; // Assurez-vous d'avoir TextMeshPro installé

[RequireComponent(typeof(Collider))]
public class WasteDestroyerUI : MonoBehaviour
{
    [Header("Destruction settings")]
    [Tooltip("Tag des objets à détruire (laissez vide pour détruire tout ce qui entre)")]
    public string targetTag = "";

    [Tooltip("Délai avant destruction (en secondes). 0 = immédiat.")]
    public float destroyDelay = 0f;

    [Header("UI Feedback")]
    [Tooltip("Le TextMeshPro UI qui affichera le message de destruction")]
    public TextMeshProUGUI destructionText;

    [Tooltip("Afficher '+1' au lieu d'un message personnalisé")]
    public bool showPlusOne = true;

    [Tooltip("Message affiché si showPlusOne est false (utilisez {0} pour le nom de l'objet)")]
    public string destructionMessage = "Objet détruit : {0}";

    [Tooltip("Durée d'affichage du message en secondes")]
    public float messageDuration = 0.5f;

    [Tooltip("Couleur du texte pendant l'affichage")]
    public Color messageColor = Color.green;

    [Tooltip("Couleur par défaut du texte (quand rien n'est affiché)")]
    public Color defaultColor = Color.white;

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

        // Initialiser le texte si assigné
        if (destructionText != null)
        {
            destructionText.text = "";
            destructionText.color = defaultColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Vérifier le tag si spécifié
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
        {
            return;
        }

        string objectName = other.gameObject.name;

        if (debugLog)
        {
            Debug.Log($"WasteDestroyerUI: Destruction de {objectName}");
        }

        // Afficher le message UI
        ShowDestructionMessage(objectName);

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

    private void ShowDestructionMessage(string objectName)
    {
        if (destructionText == null) return;

        // Choisir le message à afficher
        string message;
        if (showPlusOne)
        {
            message = "+1";
        }
        else
        {
            message = string.Format(destructionMessage, objectName);
        }

        // Afficher le message
        destructionText.text = message;
        destructionText.color = messageColor;

        // Cacher le message après la durée
        Invoke(nameof(HideMessage), messageDuration);
    }

    private void HideMessage()
    {
        if (destructionText != null)
        {
            destructionText.text = "";
            destructionText.color = defaultColor;
        }
    }
}