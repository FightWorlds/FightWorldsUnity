using System;
using UnityEngine;
using UnityEngine.EventSystems;
using SystemInfo = UnityEngine.Device.SystemInfo;
using FightWorlds.Placement;

namespace FightWorlds.Controllers
{
    [RequireComponent(typeof(CameraController))]
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlacementSystem placement;
        [SerializeField] private LayerMask selectionMask;

        private const float mouseMinMove = 5f;

        private CameraController cameraController;
        private Ray mouseRay;
        private Vector3 lastPosition;
        private Func<bool> PointerOverUi;
        private LayerMask terrainLayer;

        private void Awake()
        {
            cameraController = gameObject.GetComponent<CameraController>();
            lastPosition = new();
            PointerOverUi = SystemInfo.deviceType == DeviceType.Desktop ?
            () => EventSystem.current.IsPointerOverGameObject() :
            () => EventSystem.current.IsPointerOverGameObject(0);
            terrainLayer = LayerMask.NameToLayer("Terrain");
        }

        private void Update()
        {
            mouseRay = cameraController.MouseRay();
            bool isOverUi = PointerOverUi();
            Action OnDown;
            Action OnToggle;
            Action OnUp;
            if (PlacementSystem.AttackMode)
            {
                OnDown = () => LocateAtLand();
                OnToggle = () => cameraController.HandleDrag(mouseRay);
                OnUp = () => { };
            }
            else
            {
                OnDown = () => FindTarget();
                OnToggle = () =>
                {
                    placement.ui.CloseBuildingMenu();
                    if (!DragTarget())
                        cameraController.HandleDrag(mouseRay);
                };
                OnUp = () => placement.ResetSelectedBuilding();
            }
            if (Input.GetMouseButtonDown(0) && !isOverUi)
            {
                lastPosition = Input.mousePosition;
                cameraController.HandlePress(mouseRay);
                if (Vector3.Distance(lastPosition, Input.mousePosition) <
                mouseMinMove)
                    OnDown();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                lastPosition = Vector3.positiveInfinity;
                OnUp();
            }
            else if (Input.GetMouseButton(0) && !isOverUi)
            {
                OnToggle();
            }
        }

        public bool IsPointerOverUI()
            => EventSystem.current.IsPointerOverGameObject();

        private bool LocateAtLand()
        {
            var hits = Physics.RaycastAll(mouseRay, 100f);
            if (hits.Length > 0)
            {
                var hit = hits[0];
                bool isLand = (hits.Length == 1) &&
                    (hit.transform.gameObject.layer == terrainLayer);
                placement.TapOnLand(hit.point, isLand);
                return true;
            }
            return false;
        }

        private bool FindTarget()
        {
            if (Physics.Raycast(mouseRay, out RaycastHit hit, selectionMask))
            {
                placement.TapOnHex(hit.collider.transform.position);
                return true;
            }
            return false;
        }

        private bool DragTarget()
        {
            if (Physics.Raycast(mouseRay, out RaycastHit hit, selectionMask))
                return placement.DragOnHex(hit.collider.transform.position);
            return false;
        }
    }
}