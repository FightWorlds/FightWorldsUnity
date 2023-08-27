using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class NPC : Damageable
{
    [SerializeField] private CharacterController character;
    [SerializeField] private float speed;

    private const float searchDelayMultiplier = 0.1f;

    private bool inAttackRadius =>
        Vector3.Distance(destination, currentPosition) < attackRadius;
    private float distance => Vector3.Distance(destination, currentPosition);
    // move realization out of class:
    protected new Collider[] detections => Physics.OverlapBox(Vector3.zero,
                new Vector3(40, 4, 40), Quaternion.identity, mask);


    private void Update()
    {
        if (target == null)
            if (detections.Length != 0)
                StartCoroutine(SearchTarget());
            else return;
        else
            MoveToTarget();
    }

    private void MoveToTarget()
    {
        if (inAttackRadius)
            return;
        Vector3 direction = RotateIntoTarget();
        if (direction == Vector3.positiveInfinity)
            return;
        character.Move(direction * speed * Time.deltaTime);
    }

    protected override IEnumerator SearchTarget()
    {
        while (!inAttackRadius)
        {
            Collider[] hitColliders = detections;
            if (hitColliders.Length == 0)
            {
                Debug.Log("GG YOU LOOSE");
                destination = Vector3.positiveInfinity;
                yield break;
            }
            foreach (var collider in hitColliders)
            {
                if (Vector3.Distance(collider.transform.position,
                    transform.position) < distance)
                {
                    target = collider;
                    destination = target.transform.position;
                }
            }
            yield return
            new WaitForSeconds(distance * searchDelayMultiplier);
        }
        if (!isAttacking)
            StartCoroutine(AttackTarget());
    }

    protected override void Die()
    {
        base.Die();
        Destroy(gameObject);
    }
}
