using UnityEngine;
using TMPro;

public class FloatingScore : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Vitesse de montée")]
    public float moveSpeed = 1.5f;
    
    [Tooltip("Durée de vie avant disparition")]
    public float lifeTime = 1.5f;

    private TMP_Text textMesh;
    private Color startColor;
    private float timer;

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
        if (textMesh != null)
        {
            startColor = textMesh.color;
        }
        else
        {
            // Sécurité si pas de texte
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 1. Monter vers le haut (World Space pour éviter d'être influencé par la rotation locale bizarre)
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. Fondu transparent (Fade out)
        timer += Time.deltaTime;
        if (textMesh != null)
        {
            // Calcule l'alpha (de 1 à 0)
            float alpha = Mathf.Lerp(1f, 0f, timer / lifeTime);
            
            // Applique la nouvelle couleur avec transparence
            Color newColor = startColor;
            newColor.a = alpha;
            textMesh.color = newColor;
        }

        // 3. Destruction à la fin
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
