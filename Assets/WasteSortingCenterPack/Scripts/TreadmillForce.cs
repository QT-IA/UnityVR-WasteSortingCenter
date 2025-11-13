using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TreadmillForce : MonoBehaviour
{
    [SerializeField] private float currentSpeed = 5f;

    private void OnCollisionStay(Collision collision)
    {
        Rigidbody body = collision.rigidbody;
        if (body != null && !body.isKinematic)
        {
            Vector3 forceDirection = transform.right;
            Vector3 force = forceDirection * currentSpeed * body.mass;
            body.AddForce(force, ForceMode.Force);
        }
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }
}