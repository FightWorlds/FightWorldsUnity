using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CameraController))]
public class InputManager : MonoBehaviour
{
    [SerializeField] private PlacementSystem placement;
    [SerializeField] private LayerMask selectionMask;

    private const float mouseMinMove = 5f;

    private CameraController cameraController;
    private Ray mouseRay;
    private Vector3 lastPosition;

    private void Awake()
    {
        cameraController = gameObject.GetComponent<CameraController>();
        lastPosition = new();
    }

    private void Update()
    {
        if (IsPointerOverUI())
            return;
        mouseRay = cameraController.MouseRay();
        if (Input.GetMouseButtonDown(0))
        {
            lastPosition = Input.mousePosition;
            cameraController.HandlePress(mouseRay);
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (Vector3.Distance
                (lastPosition, Input.mousePosition)
                < mouseMinMove)
                FindTarget();
            lastPosition = Vector3.positiveInfinity;
        }
        if (Input.GetMouseButton(0))
            cameraController.HandleDrag(mouseRay);
    }

    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();

    private bool FindTarget()
    {
        if (Physics.Raycast(mouseRay, out RaycastHit hit, selectionMask))
        {
            placement.TapOnHex(hit.collider.transform.position);
            return true;
        }
        return false;
    }
}