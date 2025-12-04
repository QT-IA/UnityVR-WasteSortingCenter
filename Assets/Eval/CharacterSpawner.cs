using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSpawnerFinal : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject characterPrefab; 
    
    private GameObject currentCharacter; 
    private Vector3 lookTargetPoint;     
    private bool isPlacing = false;      

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            TrySpawnCharacter();
        }

  
        if (Keyboard.current.gKey.isPressed && currentCharacter != null)
        {
            isPlacing = true;
            RotateOnFlatPlane();
        }

        if (Keyboard.current.gKey.wasReleasedThisFrame)
        {
            isPlacing = false;
            currentCharacter = null;
        }
    }

    void TrySpawnCharacter()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit))
        {

            currentCharacter = Instantiate(characterPrefab, hit.point, Quaternion.identity);
        }
    }

    void RotateOnFlatPlane()
    {
  
        Plane flatPlane = new Plane(Vector3.up, currentCharacter.transform.position);

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        float enterDistance;

 
        if (flatPlane.Raycast(ray, out enterDistance))
        {
            lookTargetPoint = ray.GetPoint(enterDistance);
            currentCharacter.transform.LookAt(lookTargetPoint);
        }
    }

   
    void OnDrawGizmos()
    {
        if (isPlacing && currentCharacter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(currentCharacter.transform.position, lookTargetPoint);
            Gizmos.DrawSphere(lookTargetPoint, 0.05f); 
        }
    }
}