using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private LayerMask selectionMask;

    private CameraController cameraController;
    private Ray mouseRay;
    private Vector3 lastPosition;
    //public event Action OnClicked, OnExit;

    private void Awake()
    {
        cameraController = gameObject.GetComponent<CameraController>();
    }

    private void Update()
    {
        mouseRay = cameraController.MouseRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            //if (IsPointerOverUI)
            //    return;
            cameraController.HandlePress(mouseRay);
            FindTarget();
        }
        if (Input.GetMouseButton(0))
            cameraController.HandleDrag(mouseRay);
        //if (Input.GetKeyDown(KeyCode.Escape))
        //    OnExit?.Invoke();
    }

    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();

    public Vector3 GetSelectedMapPosition()
    {
        //Vector3 mousePos = Input.mousePosition;
        //mousePos.z = camera.nearClipPlane;
        //Ray ray = camera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 100, selectionMask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }

    private bool FindTarget()
    {
        if (Physics.Raycast(mouseRay, out RaycastHit hit, selectionMask))
        {
            gridManager.TapOnHex(hit.collider.transform.position);
            return true;
        }
        return false;
    }
}