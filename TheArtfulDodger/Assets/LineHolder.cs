using UnityEngine;

public class LineHolder : MonoBehaviour
{
    public Transform targetPoint; // Assign the target point in the Inspector

    void Update()
    {
        if (targetPoint != null)
        {
            // 1. Calculate the direction vector to the target point
            Vector3 directionToTarget = targetPoint.position - transform.position;

            // 2. Zero out the Y component of the direction to restrict rotation to the XZ plane
            directionToTarget.y = 0;
            directionToTarget.z = 0;

            // 3. Ensure the direction is not zero to avoid errors
            if (directionToTarget != Vector3.zero)
            {
                // 4. Calculate the desired rotation using Quaternion.LookRotation
                // This creates a rotation that looks along the directionToTarget vector.
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                // 5. Apply only the Y-axis rotation from the targetRotation
                // We extract the Y-component of the Euler angles from the target rotation
                // and combine it with the current X and Z rotations of the object.
                transform.rotation = Quaternion.Euler(targetRotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            }
        }
    }
}
