using System.Collections;
using UnityEngine;

public class Building : Damageable
{
    [SerializeField] private bool IsTurret; // TODO change to enum of build type
    [SerializeField] private bool IsResourceProducer;
    [SerializeField] private int produceTime;
    [SerializeField] private int resourcesPerOperation;
    private const int xpMultiplier = 5;
    private bool isProducing;
    protected override Collider[] Detections() =>
        Physics.OverlapCapsule(currentPosition + Vector3.up,
        currentPosition + Vector3.up, attackRadius, mask);

    private void Update()
    {
        if (IsResourceProducer && !isProducing)
            StartCoroutine(ProduceResources());
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

    protected override void GetBenefits()
    {
        placement.player.TakeXp(resourcesPerOperation / xpMultiplier);
        placement.player.TakeResources(resourcesPerOperation);
        // TODO remove from here to other cs
    }

    private IEnumerator ProduceResources()
    {
        isProducing = true;
        yield return new WaitForSeconds(produceTime);
        GetBenefits();
        isProducing = false;
    }
}
