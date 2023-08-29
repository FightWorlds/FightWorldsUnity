using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Damageable
{
    [SerializeField] private bool IsTurret;
    protected override Collider[] Detections() =>
        Physics.OverlapCapsule(currentPosition + Vector3.up,
        currentPosition + Vector3.up, attackRadius, mask);

    private void Update()
    {
        if (!IsTurret)
            return;
        if (target == null)
            if (Detections().Length != 0)
                StartCoroutine(SearchTarget());
            else return;
        else
            RotateIntoTarget();
    }

    protected override IEnumerator SearchTarget()
    {
        Collider[] hitColliders = Detections();
        foreach (var collider in hitColliders)
        {
            Vector3 colPos = collider.transform.position;
            if (Vector3.Distance(colPos, currentPosition)
                <
                Vector3.Distance(destination, currentPosition))
            {
                destination = colPos;
                target = collider;
            }
        }
        if (!isAttacking)
            StartCoroutine(AttackTarget());
        yield return null;
    }

    protected override void OnDamageTaken(int damage)
    {
        base.OnDamageTaken(damage);
        placement.DamageBase(damage,
        currentPosition - Vector3.down * 1.6f, isDead);
    }

    protected override void Die()
    {
        base.Die();
        isDead = true;
        Destroy(GetComponent<Collider>());
        gameObject.SetActive(false);
    }
}
