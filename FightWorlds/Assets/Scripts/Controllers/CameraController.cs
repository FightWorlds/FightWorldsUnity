using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private int boundary;
    [SerializeField] private LayerMask selectionMask;
    [SerializeField] private GridInitializer grid;

    private float zOffset;
    private Vector3 newPosition;
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;
    private Ray mouseRay;

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
        mouseRay = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            FindTarget();
            dragStartPosition = DragOnPlane(dragStartPosition);
        }


        if (Input.GetMouseButton(0))
        {
            dragCurrentPosition = DragOnPlane(dragCurrentPosition);
            newPosition = transform.position + dragStartPosition -
                dragCurrentPosition;
            newPosition.x = Mathf.Clamp(newPosition.x, -boundary, boundary);
            newPosition.z = Mathf.Clamp(newPosition.z, -boundary,
                boundary + zOffset);
        }

        transform.position = Vector3.Lerp(transform.position, newPosition,
            Time.deltaTime);
    }

    private bool FindTarget()
    {
        if (Physics.Raycast(mouseRay, out RaycastHit hit, selectionMask))
        {
            grid.gridManager.TapOnHex(hit.collider.transform.position);
            return true;
        }
        return false;
    }

    Vector3 DragOnPlane(Vector3 defaultVector)
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(mouseRay, out float entry))
            return mouseRay.GetPoint(entry);
        else
            return defaultVector;
    }
}
