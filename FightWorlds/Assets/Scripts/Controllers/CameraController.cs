using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private int boundary;
    [SerializeField] private float dragScaler;

    public float zOffset { get; private set; }
    public float yOffset { get; private set; }

    private Vector3 newPosition;
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;
    private new Camera camera;

    public Ray MouseRay() =>
        camera.ScreenPointToRay(Input.mousePosition);

    private void Awake()
    {
        camera = gameObject.GetComponent<Camera>();
        newPosition = transform.position;
        zOffset = transform.position.z;
        yOffset = transform.position.y;
    }

    public void HandlePress(Ray mouseRay)
    {
        dragStartPosition =
            CastOnPlane(dragStartPosition, mouseRay);
    }

    public void HandleDrag(Ray mouseRay)
    {
        dragCurrentPosition = CastOnPlane(dragCurrentPosition, mouseRay);
        MoveToNewPosition(transform.position + dragScaler *
        (dragStartPosition - dragCurrentPosition), false);
    }

    public void MoveToNewPosition(Vector3 pos, bool instant)
    {
        newPosition = pos;
        newPosition.x = Mathf.Clamp(newPosition.x, -boundary, boundary);
        newPosition.z = Mathf.Clamp(newPosition.z, -boundary,
            boundary + zOffset);
        transform.position = instant ? newPosition :
        Vector3.Lerp(transform.position, newPosition, Time.deltaTime);
    }
    private Vector3 CastOnPlane(Vector3 defaultVector, Ray mouseRay)
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(mouseRay, out float entry))
            return mouseRay.GetPoint(entry);
        else
            return defaultVector;
    }
}