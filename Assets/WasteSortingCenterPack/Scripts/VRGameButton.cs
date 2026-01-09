using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRGameButton : MonoBehaviour
{
    [Tooltip("Reference à l'interactable (si vide, cherche sur l'objet)")]
    public XRBaseInteractable interactable;

    [Header("Actions")]
    [Tooltip("Fonction à appeler quand le bouton est activé (ex: GameManager.StartGame)")]
    public UnityEvent onPressed;

    [Header("Animation (Optionnel)")]
    [Tooltip("La partie mobile du bouton qui s'enfonce")]
    public Transform buttonMesh;
    [Tooltip("Profondeur d'enfoncement (en mètres local)")]
    public float pressDepth = 0.02f;
    public float returnSpeed = 5f;

    private Vector3 initialLocalPos;
    private bool isPressed = false;
    private AudioSource audioSource;
    public AudioClip pressSound;

    void Start()
    {
        if (interactable == null)
            interactable = GetComponent<XRBaseInteractable>();

        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnPress);
            interactable.selectExited.AddListener(OnRelease);
        }
        else
        {
            Debug.LogError("VRGameButton: Aucun XRBaseInteractable trouvé ! Ajoutez un XRSimpleInteractable.");
        }

        if (buttonMesh != null)
            initialLocalPos = buttonMesh.localPosition;
            
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Animation simple d'enfoncement
        if (buttonMesh != null)
        {
            Vector3 targetPos = initialLocalPos;
            if (isPressed)
            {
                // Descend sur l'axe Y local (ou Z selon modélisation, ici Y supposé)
                targetPos.y -= pressDepth;
            }
            
            buttonMesh.localPosition = Vector3.Lerp(buttonMesh.localPosition, targetPos, Time.deltaTime * returnSpeed);
        }
    }

    private void OnPress(SelectEnterEventArgs args)
    {
        isPressed = true;
        onPressed.Invoke();
        
        if (audioSource != null && pressSound != null)
            audioSource.PlayOneShot(pressSound);
            
        Debug.Log($"Bouton {name} pressé !");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isPressed = false;
    }
}
