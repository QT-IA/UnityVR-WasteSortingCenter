using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WasteDestroyer : MonoBehaviour
{
    [Tooltip("Tag des objets à détruire (laissez vide pour détruire tout ce qui entre)")]
    public string targetTag = "";

    [Tooltip("Activer les logs pour le debug")]
    public bool debugLog = false;

    [Tooltip("Délai avant destruction (en secondes). 0 = immédiat.")]
    public float destroyDelay = 0f;

    void Start()
    {
        // Vérifier que le collider est bien en mode Trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("WasteDestroyer: Le Collider doit être en mode Trigger. Activation automatique.");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Vérifier le tag si spécifié
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
        {
            return;
        }

        if (debugLog)
        {
            Debug.Log($"WasteDestroyer: Destruction de {other.gameObject.name}");
        }

        if (destroyDelay > 0f)
        {
            Destroy(other.gameObject, destroyDelay);
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}
