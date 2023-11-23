using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FightWorlds.Controllers;
using FightWorlds.Placement;
using System.Linq;

namespace FightWorlds.Combat
{
    [RequireComponent(typeof(CharacterController))]
    public class NPC : Damageable
    {
        [SerializeField] private float speed;
        [SerializeField] private int experienceForKill;

        private const float searchDelay = 0.5f;

        private Action<GameObject> DeadAction;
        private CharacterController character;
        protected bool isMainDestinationReached;

        public NPC Init(Action<GameObject> action)
        {
            DeadAction = action;
            return this;
        }

        public void SetMainDestination(Vector3 pos) => mainDestination = pos;

        protected override void Awake() { }

        protected override List<Collider> Detections()
        {
            if (placement == null)
                return null;
            if (PlacementSystem.AttackMode && !isMainDestinationReached)
                return Physics.OverlapCapsule(currentPosition - Vector3.up,
                currentPosition + Vector3.up, attackRadius, mask).ToList();
            return placement.GetBuildingsColliders();
        }

        private void Update()
        {
            if (target != null)
                if (target.enabled)
                    MoveToTarget();
                else
                    target = null;
            if (PlacementSystem.AttackMode)
            {
                FindTargetInDetections();
                if (isMainDestinationReached)
                    return;
                MoveToTarget();
                if (Vector3.Distance(mainDestination, currentPosition) < attackRadius + 1f)
                {
                    isMainDestinationReached = true;
                    mainDestination = Vector3.positiveInfinity;
                }
            }
        }

        private void MoveToTarget()
        {
            Vector3 direction = RotateIntoTarget();
            if (inAttackRadius)
                if (!isAttacking)
                    StartCoroutine(AttackTarget());
                else return;
            else
                character.Move(direction * speed * Time.deltaTime);
        }

        protected override IEnumerator SearchTarget()
        {
            yield return null;
            destination = Vector3.positiveInfinity;
            while (!inAttackRadius)
            {
                if (target == null)
                {
                    FindTargetInDetections();
                    if (target != null)
                        target.GetComponent<Building>().AttachUnit();
                }
                yield return new WaitForSeconds(searchDelay);
            }
        }

        protected override void Die()
        {
            if (target != null && target.TryGetComponent(out Building building))
                building.DetachUnit();
            base.Die();
            StopAllCoroutines();
            if (!PlacementSystem.AttackMode) Process();
            var boom = placement.GetBoomExplosion(true);
            boom.transform.position = currentPosition;
            DeadAction(gameObject);
        }

        protected override void Process()
        {
            placement.player.TakeXp(experienceForKill);
            placement.player.TakeResources(startHp,
            ResourceType.Artifacts);
        }

        public void ResetLogic()
        {
            base.Awake();
            isMainDestinationReached = false;
            searchCoroutine = StartCoroutine(SearchTarget());
            character = gameObject.GetComponent<CharacterController>();
        }
    }
}