using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPC : Damageable
{
    [SerializeField] private CharacterController character;
    [SerializeField] private float speed;
    [SerializeField] private int artifactsAfterDrop;
    [SerializeField] private int experienceForKill;

    private const float searchDelayMultiplier = 0.1f;

    private bool inAttackRadius =>
        Vector3.Distance(destination, currentPosition) < attackRadius;
    private float distance => Vector3.Distance(destination, currentPosition);

    protected override Collider[] Detections()
    {
        if (placement == null)
            return null;
        return placement.GetBuildingsColliders();
    }


    private void Update()
    {
        if (target == null)
        {
            var detections = Detections();
            if (detections != null && detections.Length != 0)
                StartCoroutine(SearchTarget());
            else return;
        }
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
        destination = Vector3.positiveInfinity;
        while (!inAttackRadius)
        {
            Collider[] hitColliders = Detections();
            if (hitColliders.Length == 0)
            {
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
        GetBenefits();
        Destroy(gameObject);
    }

    protected override void GetBenefits()
    {
        placement.player.TakeXp(experienceForKill);
        placement.player.TakeArtifacts(artifactsAfterDrop);
    }
}
