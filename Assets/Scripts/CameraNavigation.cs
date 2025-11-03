using UnityEngine;

public class CameraNavigation : MonoBehaviour
{
    public Transform[] positions;  // Array of positions for the camera to move to
    public float moveSpeed = 2f;   // Speed at which the camera moves
    public float lookSpeed = 2f;   // Speed at which the camera rotates to look at the origin
    private float t = 0f;          // Parameter for spline interpolation
    private int currentPositionIndex = 0;  // Index of the current position in the array
    private bool isMoving = true;  // Flag to stop movement at the last position

    private void Start()
    {
        Vector3 directionToOrigin = Vector3.zero - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToOrigin);
        transform.rotation = targetRotation;
    }
    void Update()
    {
        // Ensure we have at least 2 positions
        if (positions.Length < 2)
        {
            Debug.LogError("Need at least 2 positions for the camera to move between.");
            return;
        }

        // Stop moving when the last node is reached
        if (!isMoving)
            return;

        // Calculate the next four control points, with clamping for the final segments
        Vector3 p0 = positions[Mathf.Max(currentPositionIndex - 1, 0)].position;
        Vector3 p1 = positions[currentPositionIndex].position;
        Vector3 p2 = positions[Mathf.Min(currentPositionIndex + 1, positions.Length - 1)].position;
        Vector3 p3 = positions[Mathf.Min(currentPositionIndex + 2, positions.Length - 1)].position;

        // Move along the spline
        Vector3 newPosition = CatmullRom(p0, p1, p2, p3, t);
        transform.position = newPosition;

        // Rotate to look at the origin
        Vector3 directionToOrigin = Vector3.zero - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToOrigin);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);

        // Increment t to progress along the spline
        t += moveSpeed * Time.deltaTime;

        // If t reaches 1, move to the next segment
        if (t >= 1f)
        {
            t = 0f;  // Reset t for the next segment
            currentPositionIndex++;

            // Stop moving if we reach the final position
            if (currentPositionIndex >= positions.Length - 1)
            {
                isMoving = false;  // Stop the movement when reaching the last position
                transform.position = positions[positions.Length - 1].position;  // Ensure we snap to the last position
            }
        }
    }

    // Catmull-Rom spline interpolation
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }
}