using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private int boundary;

    private float zOffset;
    private Vector3 newPosition;
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;

    void Start()
    {
        newPosition = transform.position;
        zOffset = transform.position.z;
    }

    void Update()
    {
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            dragStartPosition = DragOnPlane(dragStartPosition);


        if (Input.GetMouseButton(0))
        {
            dragCurrentPosition = DragOnPlane(dragCurrentPosition);
            newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            newPosition.x = Mathf.Clamp(newPosition.x, -boundary, boundary);
            newPosition.z = Mathf.Clamp(newPosition.z, -boundary, boundary + zOffset);
        }

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime);

    }

    Vector3 DragOnPlane(Vector3 defaultVector)
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float entry))
            return ray.GetPoint(entry);
        else
            return defaultVector;
    }
}
