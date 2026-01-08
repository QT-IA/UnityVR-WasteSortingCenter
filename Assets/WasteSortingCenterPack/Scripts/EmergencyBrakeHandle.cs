using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class EmergencyBrakeHandle : MonoBehaviour
{
    [SerializeField] Transform handleTransform;
    [SerializeField] float pullThreshold = 0.1f; // Distance de traction nécessaire pour activer le frein
    [SerializeField] float maxPullDistance = 0.1f; // Distance maximale de traction autorisée
    [SerializeField] Vector3 pullDirection = Vector3.down; // Direction dans laquelle le handle peut être tiré

    private Vector3 initialPosition;
    private XRGrabInteractable grabInteractable;
    private bool isBrakeActivated = false;
    private TreadmillForce[] treadmills;

    void Start()
    {
        // Récupère le composant XRGrabInteractable sur le handle
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            Debug.LogError("EmergencyBrakeHandle nécessite un composant XRGrabInteractable sur le GameObject !");
            return;
        }

        if (handleTransform == null)
        {
            handleTransform = transform;
        }

        initialPosition = handleTransform.localPosition;

        // Trouve tous les tapis roulants dans la scène
        treadmills = FindObjectsByType<TreadmillForce>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        // S'abonne aux événements de grab avec la nouvelle API
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void Update()
    {
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            ConstrainHandlePosition();
            CheckPullDistance();
        }
    }

    void ConstrainHandlePosition()
    {
        // Calcule le vecteur de déplacement depuis la position initiale
        Vector3 offset = handleTransform.localPosition - initialPosition;

        // Projette le déplacement sur la direction autorisée
        float distanceAlongAxis = Vector3.Dot(offset, pullDirection.normalized);

        // Limite la distance entre 0 (position initiale) et maxPullDistance
        distanceAlongAxis = Mathf.Clamp(distanceAlongAxis, 0f, maxPullDistance);

        // Applique la position contrainte
        handleTransform.localPosition = initialPosition + pullDirection.normalized * distanceAlongAxis;
    }

    void CheckPullDistance()
    {
        // Calcule la distance entre la position actuelle et la position initiale
        float pullDistance = Vector3.Distance(handleTransform.localPosition, initialPosition);

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

        // Arrête tous les tapis roulants
        foreach (TreadmillForce treadmill in treadmills)
        {
            treadmill.SetSpeed(0f);
        }

        Debug.Log("Frein d'urgence activé - Tapis roulants arrêtés");
    }

    void DeactivateBrake()
    {
        isBrakeActivated = false;

        // Note : Les tapis roulants resteront à 0 jusqu'à ce que le contrôleur de vitesse les remette en marche
        Debug.Log("Frein d'urgence désactivé - Le contrôleur de vitesse peut redémarrer les tapis");
    }
    void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("Handle saisi");
    }

    void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log("Handle relâché");

        // Optionnel : Réinitialise la position du handle
        // handleTransform.localPosition = initialPosition;
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