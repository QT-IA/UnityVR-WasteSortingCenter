using UnityEngine;

public class TreadmillsController : MonoBehaviour
{
    [SerializeField] float maxTreadmillSpeed = 0.5f;
    [SerializeField, Range(0, 1)] float targetSpeed = 0.5f;
    [SerializeField] Material treadmillMat;
    TreadmillForce[] treadmills;
    const float MATERIAL_SPEED_MULTIPLIER = 1f;

    [Header("Pause")]
    public bool isPaused { get; private set; }
    float currentSpeed, refSpeed;
    const float SPEED_SMOOTH = 0.2f;

    void Start()
    {
        treadmills = FindObjectsByType<TreadmillForce>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    private void Update()
    {
        float effectiveTargetSpeed = targetSpeed;
        if (isPaused)
            effectiveTargetSpeed = 0;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, effectiveTargetSpeed, ref refSpeed, SPEED_SMOOTH);
        SetSpeed(currentSpeed);
    }

    public void SetSpeed(float speed01)
    {
        float speed = speed01 * maxTreadmillSpeed;
        foreach (TreadmillForce t in treadmills)
        {
            t.SetSpeed(speed);
        }

        treadmillMat.SetFloat("_Speed", speed * MATERIAL_SPEED_MULTIPLIER);
    }

    public void SetPaused(bool value)
    {
        isPaused = value;
    }
}
