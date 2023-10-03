using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FightWorlds.Placement;
using Unity.VisualScripting;
using UnityEngine;

namespace FightWorlds.Controllers
{
    public class Bot : MonoBehaviour
    {
        [SerializeField] private int buildingTime;
        [SerializeField] private int heightOffset;
        [SerializeField] private Material unActive;
        private const float extraSpeed = 1f;
        private const float dockHeight = 1.4f;
        private const float width = 4f;
        private float timeAtDock => buildingTime / 20f;
        private float timeFlying => buildingTime / 5f;
        private float timeProcess => buildingTime / 2f;
        // 0.5 + 0.2 * 2 + 0.05 * 2 = 1

        private Dictionary<Transform, Material> parts;
        private Vector3 dock;
        private Vector3 aboveDock;
        private Vector3 destination;
        private Vector3 direction;
        public bool IsBusy { get; private set; }

        private void Awake()
        {
            transform.position += Vector3.up * dockHeight;
            dock = transform.position;
            aboveDock = dock + Vector3.up * (heightOffset - dockHeight);
            parts = new();
            foreach (Transform child in transform.GetChild(0))
                if (child.TryGetComponent(out Renderer renderer))
                    parts.Add(child, renderer.material);
        }

        private IEnumerator MoveTowards()
        {
            direction = (destination - dock).normalized;
            destination += Vector3.up * heightOffset - direction * width;
            Quaternion rotation = Quaternion.LookRotation(direction);
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, 0);
            transform.rotation = rotation;
            float elapsedTime = 0;
            while (elapsedTime < timeAtDock)
            {
                transform.position =
                Vector3.Lerp(dock, aboveDock, elapsedTime / timeAtDock);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            elapsedTime = 0;
            while (elapsedTime < timeFlying)
            {
                transform.position =
                Vector3.Lerp(aboveDock, destination, elapsedTime / timeFlying);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator Process()
        {
            //start processing animation
            yield return new WaitForSeconds(timeProcess);
            //finish anim
        }

        private IEnumerator MoveBackwards(bool extra)
        {
            direction = -direction;
            Quaternion rotation = Quaternion.LookRotation(direction);
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, 0);
            transform.rotation = rotation;
            float flying, landing, elapsedTime = 0;
            if (extra)
            {
                flying = extraSpeed;
                landing = extraSpeed;
            }
            else
            {
                flying = timeFlying;
                landing = timeAtDock;
            }
            while (elapsedTime < flying)
            {
                transform.position =
                Vector3.Lerp(destination, aboveDock, elapsedTime / flying);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            elapsedTime = 0;
            while (elapsedTime < landing)
            {
                transform.position =
                Vector3.Lerp(aboveDock, dock, elapsedTime / landing);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator Operate()
        {
            IsBusy = true;
            yield return MoveTowards();
            yield return Process();
            yield return MoveBackwards(false);
            IsBusy = false;
        }

        public void StartOperation(Vector3 target)
        {
            destination = target;
            StartCoroutine(Operate());
        }

        public void ReturnAtDock()
        {
            StopAllCoroutines();
            destination = transform.position;
            StartCoroutine(MoveBackwards(true));
            IsBusy = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out Building building) ||
                building.BuildingType != BuildingType.RepairDock)
                for (int i = 0; i < parts.Count; i++)
                    parts.ElementAt(i).Key.GetComponent<Renderer>()
                    .material = unActive;
        }


        private void OnTriggerExit(Collider other)
        {
            for (int i = 0; i < parts.Count; i++)
                parts.ElementAt(i).Key.GetComponent<Renderer>()
                .material = parts.ElementAt(i).Value;
        }
    }
}
