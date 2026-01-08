using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class EmergencyBrakeHandle : MonoBehaviour
{
    [SerializeField] TreadmillsController treadmillsController;
    [SerializeField] Spawner spawner;
    [SerializeField] float pullThreshold = 0.08f; // Distance de traction nécessaire pour activer le frein
    [SerializeField] float maxPullDistance = 0.1f; // Distance maximale de traction autorisée
    [SerializeField] Vector3 pullDirection = Vector3.down; // Direction dans laquelle le handle peut être tiré

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private XRBaseInteractable grabInteractable;
    private bool isBrakeActivated = false;
    private Rigidbody rb;

    void Start()
    {
        // Récupère le composant XRBaseInteractable sur le handle
        grabInteractable = GetComponent<XRBaseInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grabInteractable == null)
        {
            Debug.LogError("EmergencyBrakeHandle nécessite un composant XRBaseInteractable sur le GameObject !");
            return;
        }

        // Configure le Rigidbody pour être kinematic
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            // Crée un Rigidbody si inexistant
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        // Sauvegarde la position et rotation initiales
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        // Trouve le TreadmillsController dans la scène si non assigné
        if (treadmillsController == null)
        {
            treadmillsController = FindFirstObjectByType<TreadmillsController>();
            if (treadmillsController == null)
            {
                Debug.LogError("EmergencyBrakeHandle: TreadmillsController introuvable ! Le frein ne pourra pas arrêter le tapis.");
            }
        }

        if (spawner == null)
        {
            spawner = FindFirstObjectByType<Spawner>();
        }

        // S'abonne aux événements de select avec la nouvelle API
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void LateUpdate()
    {
        // Force toujours la rotation à rester constante
        transform.localRotation = initialRotation;

        if (grabInteractable != null && grabInteractable.isSelected)
        {
            // Déplace manuellement le handle vers l'interactor
            MoveHandleToInteractor();
        }
    }

    void MoveHandleToInteractor()
    {
        if (grabInteractable.interactorsSelecting.Count == 0) return;

        var interactor = grabInteractable.interactorsSelecting[0];

        // Position de la main en world space
        Vector3 handWorldPos = interactor.GetAttachTransform(grabInteractable).position;

        // Convertit en local space par rapport au parent (ou world si pas de parent)
        Transform parentTransform = transform.parent;
        Vector3 handLocalPos;
        
        handLocalPos = parentTransform.InverseTransformPoint(handWorldPos);

        // Calcule l'offset depuis la position initiale
        Vector3 offset = handLocalPos - initialPosition;

        // Projette sur l'axe autorisé
        float distanceAlongAxis = Vector3.Dot(offset, pullDirection.normalized);

        // Limite la distance
        distanceAlongAxis = Mathf.Clamp(distanceAlongAxis, 0f, maxPullDistance);

        // Applique la position contrainte
        Vector3 targetLocalPos = initialPosition + pullDirection.normalized * distanceAlongAxis;
        transform.localPosition = targetLocalPos;

        // Debug pour voir la distance
        Debug.Log($"Hand: {handLocalPos}, Initial: {initialPosition}, Distance: {distanceAlongAxis:F3}m / Threshold: {pullThreshold}m");

        // Vérifie et active le frein
        CheckPullDistance(distanceAlongAxis);
    }

    void CheckPullDistance(float pullDistance)
    {
        if (pullDistance >= pullThreshold && !isBrakeActivated)
        {
            ActivateBrake();
        }
        else if (pullDistance < pullThreshold && isBrakeActivated)
        {
            DeactivateBrake();
        }
    }

    void ActivateBrake()
    {
        isBrakeActivated = true;

        if (treadmillsController != null)
        {
            treadmillsController.SetPaused(true);
        }

        if (spawner != null)
        {
            spawner.enabled = false;
        }
        
        Debug.Log("Frein d'urgence activé - Tapis et Spawner arrêtés");
    }

    void DeactivateBrake()
    {
        isBrakeActivated = false;

        if (treadmillsController != null)
        {
            treadmillsController.SetPaused(false);
        }

        if (spawner != null)
        {
            spawner.enabled = true;
        }

        Debug.Log("Frein d'urgence désactivé - Tapis et Spawner redémarrés");
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("Handle saisi");
    }

    void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log("Handle relâché");
        // Le handle reste à sa dernière position
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
 