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

    [Header("Audio FX")]
    [Tooltip("Son d'ambiance à jouer en boucle (ex: feu)")]
    public AudioClip loopingSound;
    [Range(0f, 1f)] public float soundVolume = 0.5f;

    private AudioSource audioSource;

    void Start()
    {
        // Vérifier que le collider est bien en mode Trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("WasteDestroyer: Le Collider doit être en mode Trigger. Activation automatique.");
            col.isTrigger = true;
        }

        // Configuration Audio
        if (loopingSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.clip = loopingSound;
            audioSource.loop = true;
            audioSource.volume = soundVolume;
            audioSource.spatialBlend = 1.0f; // Son en 3D
            audioSource.playOnAwake = true;

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
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
