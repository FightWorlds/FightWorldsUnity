using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace FightWorlds.Controllers
{
    public class Bot : MonoBehaviour
    {
        [SerializeField] private int buildingTime;
        [SerializeField] private int heightOffset;
        private const float dockHeight = 1.4f;
        private const float width = 4f;
        private float timeAtDock => buildingTime / 20f;
        private float timeFlying => buildingTime / 5f;
        private float timeMoving => timeFlying + timeAtDock;
        private float timeProcess => buildingTime / 2f;
        // 0.5 + 0.2 * 2 + 0.05 * 2 = 1
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

        private IEnumerator MoveBackwards()
        {
            direction = -direction;
            Quaternion rotation = Quaternion.LookRotation(direction);
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, 0);
            transform.rotation = rotation;
            float elapsedTime = 0;
            while (elapsedTime < timeFlying)
            {
                transform.position =
                Vector3.Lerp(destination, aboveDock, elapsedTime / timeFlying);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            elapsedTime = 0;
            while (elapsedTime < timeAtDock)
            {
                transform.position =
                Vector3.Lerp(aboveDock, dock, elapsedTime / timeAtDock);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator Operate()
        {
            IsBusy = true;
            yield return MoveTowards();
            yield return Process();
            yield return MoveBackwards();
            IsBusy = false;
        }

        public void StartOperation(Vector3 target)
        {
            destination = target;
            StartCoroutine(Operate());
        }
    }
}
