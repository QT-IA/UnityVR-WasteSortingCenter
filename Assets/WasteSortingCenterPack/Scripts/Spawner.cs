using System.Collections;
using UnityEngine;

/// <summary>
/// Script pour spawn des déchets Carton0, Carton1 et Bottle selon des probabilités configurables.
/// Usage : attacher ce script à un GameObject vide dans la scène, assigner les prefabs dans
/// l'inspector et régler les probabilités (elles seront normalisées automatiquement).
/// </summary>
/// 
public class Spawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Prefab pour Apple")]
    public GameObject Apple;
    [Tooltip("Prefab pour Bottle")]
    public GameObject Bottle;
    [Tooltip("Prefab pour Carrot")]
    public GameObject Carrot;
    [Tooltip("Prefab pour Carton0")]
    public GameObject Carton0;
    [Tooltip("Prefab pour Carton1")]
    public GameObject Carton1;
    [Tooltip("Prefab pour Pizza")]
    public GameObject Pizza;
    [Tooltip("Prefab pour PizzaBox")]
    public GameObject PizzaBox;
    [Tooltip("Prefab pour SodaCan")]
    public GameObject SodaCan;
    [Tooltip("Prefab pour SodaCup")]
    public GameObject SodaCup;
    [Tooltip("Prefab pour Water0")]
    public GameObject Water0;
    [Tooltip("Prefab pour Water1")]
    public GameObject Water1;

    [Header("Probabilités (les valeurs seront normalisées)")]
    [Tooltip("Poids relatif pour Apple")] [Range(0f, 1f)]
    public float weightApple = 0.09f;
    [Tooltip("Poids relatif pour Bottle")] [Range(0f, 1f)]
    public float weightBottle = 0.09f;
    [Tooltip("Poids relatif pour Carrot")] [Range(0f, 1f)]
    public float weightCarrot = 0.09f;
    [Tooltip("Poids relatif pour Carton0")] [Range(0f, 1f)]
    public float weightCarton0 = 0.09f;
    [Tooltip("Poids relatif pour Carton1")] [Range(0f, 1f)]
    public float weightCarton1 = 0.09f;
    [Tooltip("Poids relatif pour Pizza")] [Range(0f, 1f)]
    public float weightPizza = 0.09f;
    [Tooltip("Poids relatif pour PizzaBox")] [Range(0f, 1f)]
    public float weightPizzaBox = 0.09f;
    [Tooltip("Poids relatif pour SodaCan")] [Range(0f, 1f)]
    public float weightSodaCan = 0.09f;
    [Tooltip("Poids relatif pour SodaCup")] [Range(0f, 1f)]
    public float weightSodaCup = 0.09f;
    [Tooltip("Poids relatif pour Water0")] [Range(0f, 1f)]
    public float weightWater0 = 0.09f;
    [Tooltip("Poids relatif pour Water1")] [Range(0f, 1f)]
    public float weightWater1 = 0.10f;

    [Header("Spawn settings")]
    [Tooltip("Points où les objets peuvent apparaître. Si vide, utilise la position du GameObject contenant ce script.")]
    public Transform[] spawnPoints;
    [Tooltip("Intervalle entre chaque spawn en secondes")]
    public float spawnInterval = 1f;
    [Tooltip("Activer/désactiver le spawn automatique en boucle")]
    public bool loop = true;

    [Header("Physics / projection")]
    [Tooltip("Force appliquée le long de l'axe local Right (X) du point de spawn lors de l'instanciation. 0 = pas de force.")]
    public float spawnForce = 1f;
    [Tooltip("Mode d'application de la force (Impulse, Force, VelocityChange, Acceleration)")]
    public ForceMode spawnForceMode = ForceMode.Impulse;
    [Tooltip("Si vrai, ajoute automatiquement un Rigidbody si le prefab instancié n'en a pas.")]
    public bool addRigidbodyIfMissing = false;

    [Header("Random rotation")]
    [Tooltip("Activer la rotation aléatoire appliquée au prefab instancié.")]
    public bool randomizeRotation = true;
    [Tooltip("Si vrai, la rotation aléatoire affectera aussi l'axe Y. Sinon Y restera celui du spawnPoint.")]
    public bool randomizeY = false;
    [Tooltip("Angles minimaux (Euler) pour la rotation aléatoire")] 
    public Vector3 randomRotationMin = new Vector3(0f, 0f, 0f);
    [Tooltip("Angles maximaux (Euler) pour la rotation aléatoire")] 
    public Vector3 randomRotationMax = new Vector3(0f, 360f, 0f);

    // Coroutine instance
    private Coroutine spawnRoutine;

    void OnValidate()
    {
        // Ensure non-negative weights
        if (weightApple < 0f) weightApple = 0f;
        if (weightBottle < 0f) weightBottle = 0f;
        if (weightCarrot < 0f) weightCarrot = 0f;
        if (weightCarton0 < 0f) weightCarton0 = 0f;
        if (weightCarton1 < 0f) weightCarton1 = 0f;
        if (weightPizza < 0f) weightPizza = 0f;
        if (weightPizzaBox < 0f) weightPizzaBox = 0f;
        if (weightSodaCan < 0f) weightSodaCan = 0f;
        if (weightSodaCup < 0f) weightSodaCup = 0f;
        if (weightWater0 < 0f) weightWater0 = 0f;
        if (weightWater1 < 0f) weightWater1 = 0f;
        if (spawnInterval < 0.01f) spawnInterval = 0.01f;
    }

    void Start()
    {
        if (loop)
        {
            spawnRoutine = StartCoroutine(SpawnLoop());
        }
    }

    void OnDisable()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    /// <summary>
    /// Boucle de spawn qui instancie un objet toutes les spawnInterval secondes.
    /// </summary>
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// Choisit au hasard un prefab selon les probabilités et l'instancie à un point de spawn aléatoire.
    /// </summary>
    public void SpawnOne()
    {
        GameObject prefab = ChoosePrefabByProbability();
        if (prefab == null)
        {
            Debug.LogWarning("Spawner: aucun prefab assigné ou poids nuls — rien à spawn.");
            return;
        }

        Transform spawnTransform = ChooseSpawnTransform();
        Vector3 pos = spawnTransform != null ? spawnTransform.position + Vector3.up * 2f + Vector3.right * 0.3f : transform.position + Vector3.up * 2f + Vector3.right * 0.3f;

        // Base rotation (from spawn point)
        Quaternion baseRot = spawnTransform != null ? spawnTransform.rotation : Quaternion.identity;
        Quaternion rot = baseRot;

        // Appliquer une rotation aléatoire si activée (les angles sont en Euler)
        if (randomizeRotation)
        {
            float rx = Random.Range(randomRotationMin.x, randomRotationMax.x);
            float ry = randomizeY ? Random.Range(randomRotationMin.y, randomRotationMax.y) : 0f;
            float rz = Random.Range(randomRotationMin.z, randomRotationMax.z);
            Quaternion randomQ = Quaternion.Euler(rx, ry, rz);
            rot = baseRot * randomQ;
        }

        GameObject go = Instantiate(prefab, pos, rot);

        // Appliquer une force le long de l'axe local Right (X) du point de spawn ou du spawner
        if (spawnForce != 0f)
        {
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null && addRigidbodyIfMissing)
            {
                rb = go.AddComponent<Rigidbody>();
            }

            if (rb != null)
            {
                Vector3 direction = (spawnTransform != null) ? -spawnTransform.right : transform.right;
                rb.AddForce(direction.normalized * spawnForce * 1.5f, spawnForceMode);
            }
            else
            {
                Debug.LogWarning($"Spawner: spawnForce != 0 mais l'objet '{go.name}' n'a pas de Rigidbody. Activer addRigidbodyIfMissing pour l'ajouter automatiquement.");
            }
        }
    }

    private Transform ChooseSpawnTransform()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int idx = Random.Range(0, spawnPoints.Length);
            return spawnPoints[idx];
        }
        return this.transform;
    }

    private GameObject ChoosePrefabByProbability()
    {
        float total = weightApple + weightBottle + weightCarrot + weightCarton0 + weightCarton1 + 
                      weightPizza + weightPizzaBox + weightSodaCan + weightSodaCup + weightWater0 + weightWater1;
        
        if (total <= 0f)
        {
            // fallback: choose uniformly among assigned prefabs
            var list = new System.Collections.Generic.List<GameObject>();
            if (Apple != null) list.Add(Apple);
            if (Bottle != null) list.Add(Bottle);
            if (Carrot != null) list.Add(Carrot);
            if (Carton0 != null) list.Add(Carton0);
            if (Carton1 != null) list.Add(Carton1);
            if (Pizza != null) list.Add(Pizza);
            if (PizzaBox != null) list.Add(PizzaBox);
            if (SodaCan != null) list.Add(SodaCan);
            if (SodaCup != null) list.Add(SodaCup);
            if (Water0 != null) list.Add(Water0);
            if (Water1 != null) list.Add(Water1);
            if (list.Count == 0) return null;
            return list[Random.Range(0, list.Count)];
        }

        float r = Random.value * total;
        
        if (r < weightApple)
            return Apple != null ? Apple : FallbackPrefabExcept(Apple);
        r -= weightApple;
        
        if (r < weightBottle)
            return Bottle != null ? Bottle : FallbackPrefabExcept(Bottle);
        r -= weightBottle;
        
        if (r < weightCarrot)
            return Carrot != null ? Carrot : FallbackPrefabExcept(Carrot);
        r -= weightCarrot;
        
        if (r < weightCarton0)
            return Carton0 != null ? Carton0 : FallbackPrefabExcept(Carton0);
        r -= weightCarton0;
        
        if (r < weightCarton1)
            return Carton1 != null ? Carton1 : FallbackPrefabExcept(Carton1);
        r -= weightCarton1;
        
        if (r < weightPizza)
            return Pizza != null ? Pizza : FallbackPrefabExcept(Pizza);
        r -= weightPizza;
        
        if (r < weightPizzaBox)
            return PizzaBox != null ? PizzaBox : FallbackPrefabExcept(PizzaBox);
        r -= weightPizzaBox;
        
        if (r < weightSodaCan)
            return SodaCan != null ? SodaCan : FallbackPrefabExcept(SodaCan);
        r -= weightSodaCan;
        
        if (r < weightSodaCup)
            return SodaCup != null ? SodaCup : FallbackPrefabExcept(SodaCup);
        r -= weightSodaCup;
        
        if (r < weightWater0)
            return Water0 != null ? Water0 : FallbackPrefabExcept(Water0);
        
        // else Water1
        return Water1 != null ? Water1 : FallbackPrefabExcept(Water1);
    }

    private GameObject FallbackPrefabExcept(GameObject except)
    {
        // Return any assigned prefab that's not null (simple fallback)
        if (Apple != null && Apple != except) return Apple;
        if (Bottle != null && Bottle != except) return Bottle;
        if (Carrot != null && Carrot != except) return Carrot;
        if (Carton0 != null && Carton0 != except) return Carton0;
        if (Carton1 != null && Carton1 != except) return Carton1;
        if (Pizza != null && Pizza != except) return Pizza;
        if (PizzaBox != null && PizzaBox != except) return PizzaBox;
        if (SodaCan != null && SodaCan != except) return SodaCan;
        if (SodaCup != null && SodaCup != except) return SodaCup;
        if (Water0 != null && Water0 != except) return Water0;
        if (Water1 != null && Water1 != except) return Water1;
        return null;
    }
}
