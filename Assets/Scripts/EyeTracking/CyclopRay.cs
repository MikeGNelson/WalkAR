using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyclopRay : MonoBehaviour
{
    public Transform REye, LEye;

    [SerializeField]
    private float rayDistance = 1.0f;
    [SerializeField]
    private float rayWidth = 0.01f;
    [SerializeField]
    private LayerMask layersToInclude;
    [SerializeField]
    private Color rayColorDefaultState = Color.yellow;

    private LineRenderer lineRenderer;
    private Vector3 currentGazePoint;
    private string currentGazeGameObject;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupRay();
    }

    void SetupRay()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.startColor = rayColorDefaultState;
        lineRenderer.endColor = rayColorDefaultState;

        // Set the starting point to the average position of the eyes
        lineRenderer.SetPosition(0, GetAveragePosition());

        // Set the ray to extend in front of the average position of the eyes
        lineRenderer.SetPosition(1, GetAveragePosition() + transform.TransformDirection(Vector3.forward) * rayDistance);
    }


    // Update is called once per frame
    void Update()
    {
        transform.position = GetAveragePosition();
        transform.eulerAngles = GetAverageRotation();
    }

    private void FixedUpdate()
    {
        RaycastHit hit;

        // Use the average position of the eyes to cast the ray
        Vector3 rayCastDirection = transform.TransformDirection(Vector3.forward) * rayDistance;

        if (Physics.Raycast(GetAveragePosition(), rayCastDirection, out hit, Mathf.Infinity))
        {
            currentGazePoint = hit.point;

            if (hit.transform != null)
            {
                currentGazeGameObject = hit.transform.gameObject.name;
            }
        }

        // Update the ray in the line renderer to reflect the gaze point
        lineRenderer.SetPosition(0, GetAveragePosition());
        lineRenderer.SetPosition(1, currentGazePoint);
    }


    public Vector3 GetAveragePosition()
    {
        return (REye.position + LEye.position) / 2.0f;
    }

    public Vector3 GetAverageRotation()
    {
        return (REye.eulerAngles + LEye.eulerAngles) / 2.0f;
    }

    public Vector3 GetCurrentGazePoint()
    {
        return currentGazePoint;
    }

    public string GetCurrentGazeGameObject()
    {
        return currentGazeGameObject;
    }
    
    public Vector3 Direction()
    {
        return transform.TransformDirection(Vector3.forward);
    }
}
