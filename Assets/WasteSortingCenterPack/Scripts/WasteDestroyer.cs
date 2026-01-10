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

    [Header("Visual FX")]
    [Tooltip("Prefab de particules à instancier lors de la destruction (ex: feu/fumée)")]
    public GameObject burnParticlesPrefab;
    [Tooltip("Effet de réduction (scale down) avant destruction")]
    public bool shrinkEffect = true;
    [Tooltip("Durée de l'effet de reduction")]
    public float shrinkDuration = 0.3f;

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

        StartCoroutine(DestroySequence(other.gameObject));
    }

    private System.Collections.IEnumerator DestroySequence(GameObject obj)
    {
        // 1. Instancier les particules
        if (burnParticlesPrefab != null)
        {
            Instantiate(burnParticlesPrefab, obj.transform.position, Quaternion.identity);
        }

        // 2. Désactiver la physique pour qu'il ne bouge plus pendant l'anim
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 3. Effet de shrink (scale down)
        if (shrinkEffect && shrinkDuration > 0f)
        {
            float timer = 0f;
            Vector3 startScale = obj.transform.localScale;

            while (timer < shrinkDuration)
            {
                if (obj == null) yield break; // Sécurité si détruit ailleurs
                timer += Time.deltaTime;
                float progress = timer / shrinkDuration;
                obj.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
                yield return null;
            }
        }
        else if (destroyDelay > 0f)
        {
            yield return new WaitForSeconds(destroyDelay);
        }

        // 4. Destruction finale
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}

