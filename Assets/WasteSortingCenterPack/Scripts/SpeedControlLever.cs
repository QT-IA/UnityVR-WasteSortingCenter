using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SpeedControlLever : MonoBehaviour
{
    [Header("Controller Reference")]
    [SerializeField] TreadmillsController treadmillsController;

    [Header("Configuration")]
    [Tooltip("Axe de rotation du levier (dans l'espace local)")]
    [SerializeField] Vector3 rotationAxis = Vector3.right; 
    
    [Tooltip("Axe pointant vers le haut du levier (dans l'espace local) pour le calcul de l'angle")]
    [SerializeField] Vector3 leverUpAxis = Vector3.up;    

    [SerializeField] float minAngle = -45f;
    [SerializeField] float maxAngle = 45f;
    
    [Tooltip("Vitesse initiale (et position du levier) entre 0 et 1")]
    [SerializeField, Range(0f, 1f)] float initialSpeedRatio = 0.5f;

    [Tooltip("Vitesse minimale (ratio 0-1) quand le levier est au minimum")]
    [SerializeField, Range(0f, 1f)] float minOutputSpeed = 0.2f;

    private XRBaseInteractable grabInteractable;
    private Rigidbody rb;
    
    // Etat interne
    private Quaternion neutralRotation; // La rotation de base (correspondant au préfab posé dans la scène)
    private float currentAngle = 0f;

    void Start()
    {
        // Récupère les composants nécessaires
        grabInteractable = GetComponent<XRBaseInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grabInteractable == null)
        {
            Debug.LogError("SpeedControlLever: XRBaseInteractable manquant !");
            return;
        }

        // Configuration Rigidbody (Kinematic pour contrôle manuel complet)
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // On assume que la rotation actuelle dans la scène est la rotation "neutre" (0 degré de décalage)
        neutralRotation = transform.localRotation;

        // Calcule l'angle initial basé sur le ratio demandé
        currentAngle = Mathf.Lerp(minAngle, maxAngle, initialSpeedRatio);
        
        // Applique la rotation initiale
        UpdateLeverRotation();

        // Trouve le contrôleur si non assigné
        if (treadmillsController == null)
        {
            treadmillsController = FindFirstObjectByType<TreadmillsController>();
        }

        // Applique la vitesse initiale
        UpdateSpeed();
    }

    void LateUpdate()
    {
        // Si la manette est saisie, elle suit la main via rotation
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            RotateLeverToInteractor();
        }
    }

    void RotateLeverToInteractor()
    {
        if (grabInteractable.interactorsSelecting.Count == 0) return;
        var interactor = grabInteractable.interactorsSelecting[0];

        // 1. Récupère la position de la main
        Vector3 handWorldPos = interactor.GetAttachTransform(grabInteractable).position;
        
        // 2. Calcule le vecteur [Pivot -> Main]
        // On effectue les calculs dans l'espace du PARENT pour être indépendant de la rotation actuelle du levier
        // mais respectueux de l'orientation globale du parent (ex: pupitre incliné).
        Transform parent = transform.parent;
        
        Vector3 vecToHand; // Vecteur du pivot vers la main (en orientation parent/world)
        
        if (parent != null)
        {
            Vector3 localHand = parent.InverseTransformPoint(handWorldPos);
            Vector3 localPivot = transform.localPosition;
            vecToHand = localHand - localPivot;
        }
        else
        {
            vecToHand = handWorldPos - transform.position;
        }

        // 3. Prépare les axes de référence transformés par la rotation neutre
        // (Cela permet de garder les axes corrects même si le levier a été placé avec une rotation bizarre)
        Vector3 effectiveRotationAxis = neutralRotation * rotationAxis; 
        Vector3 effectiveLeverUp = neutralRotation * leverUpAxis;

        // 4. Projette le vecteur main sur le plan de rotation (enlève la composante de l'axe)
        Vector3 projectedVec = Vector3.ProjectOnPlane(vecToHand, effectiveRotationAxis);

        // 5. Calcule l'angle entre le "Haut" neutre et le vecteur main projeté
        float angle = Vector3.SignedAngle(effectiveLeverUp, projectedVec, effectiveRotationAxis);

        // 6. Contraint l'angle
        currentAngle = Mathf.Clamp(angle, minAngle, maxAngle);

        // 7. Applique
        UpdateLeverRotation();
        UpdateSpeed();
    }

    void UpdateLeverRotation()
    {
        // La rotation finale est : RotationNeutre * Rotation(angle autour de l'axe)
        transform.localRotation = neutralRotation * Quaternion.AngleAxis(currentAngle, rotationAxis);
    }

    void UpdateSpeed()
    {
        if (treadmillsController == null) return;

        // Convertit l'angle en valeur 0-1 (position physique du levier)
        float t = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);

        // Inversion : Levier en bas (minAngle -> t=0) = Vitesse Max
        // Levier en haut (maxAngle -> t=1) = Vitesse Min
        float invertedT = 1f - t;

        // Mappe la valeur inversée entre le minimum autorisé et 1
        float finalSpeed = Mathf.Lerp(minOutputSpeed, 1f, invertedT);
        
        treadmillsController.SetTargetSpeed(finalSpeed);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        // Logique optionnelle au grab
    }

    void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log($"Levier relâché à {currentAngle:F1} degrés. Speed Ratio: {Mathf.InverseLerp(minAngle, maxAngle, currentAngle):F2}");
        // Le levier reste à sa position actuelle (currentAngle n'est pas réinitialisé)
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}
