using UnityEngine;

public class LockTransform : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // Store the initial position and rotation when the script starts
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    // LateUpdate is called once per frame, after all Update functions have been called.
    void LateUpdate()
    {
        // Constantly reset the position to the initial value
        transform.position = initialPosition;

        // Get the current Y rotation
        float currentYRotation = transform.eulerAngles.y;

        // Apply the initial X and Z rotations, but use the current Y rotation
        transform.rotation = Quaternion.Euler(initialRotation.eulerAngles.x, currentYRotation, initialRotation.eulerAngles.z);
    }
}
