using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FightWorlds.Controllers;

namespace FightWorlds.Combat
{
    [RequireComponent(typeof(CharacterController))]
    public class NPC : Damageable
    {
        [SerializeField] private float speed;
        [SerializeField] private int experienceForKill;

        private const float searchDelay = 1f;

        private Action<GameObject> DeadAction;
        private CharacterController character;
        public NPC Init(Action<GameObject> action)
        {
            DeadAction = action;
            return this;
        }

        protected override void Awake()
        {
            base.Awake();
            searchCoroutine = StartCoroutine(SearchTarget());
            character = gameObject.GetComponent<CharacterController>();
        }

        protected override List<Collider> Detections()
        {
            if (placement == null)
                return null;
            return placement.GetBuildingsColliders();
        }

        private void Update()
        {
            if (target != null)
                if (target.enabled)
                    MoveToTarget();
                else
                    target = null;
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
                FindTargetInDetections();
                yield return new WaitForSeconds(searchDelay);
            }
        }

        protected override void Die()
        {
            base.Die();
            Process();
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
            searchCoroutine = StartCoroutine(SearchTarget());
        }
    }
}