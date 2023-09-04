using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPC : Damageable
{
    [SerializeField] private CharacterController character;
    [SerializeField] private float speed;
    [SerializeField] private int artifactsAfterDrop;
    [SerializeField] private int experienceForKill;

    private const float searchDelay = 1f;

    private bool inAttackRadius =>
        Vector3.Distance(destination, currentPosition) < attackRadius;
    private float distance => Vector3.Distance(destination, currentPosition);

    protected override void Awake()
    {
        base.Awake();
        searchCoroutine = StartCoroutine(SearchTarget());
    }

    protected override Collider[] Detections()
    {
        if (placement == null)
            return null;
        return placement.GetBuildingsColliders();
    }

    private void Update()
    {
        if (target != null)
            MoveToTarget();
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
        yield return new WaitForSeconds(1f);
        destination = Vector3.positiveInfinity;
        while (!inAttackRadius)
        {
            Collider[] hitColliders = Detections();
            if (hitColliders == null)
            {
                destination = Vector3.positiveInfinity;
            }
            foreach (var collider in hitColliders)
            {
                if (collider == null) continue;
                if (Vector3.Distance(collider.transform.position,
                    transform.position) < distance)
                {
                    target = collider;
                    destination = target.transform.position;
                }
            }
            yield return
            new WaitForSeconds(searchDelay);
        }
    }

    protected override void Die()
    {
        base.Die();
        Process();
        Destroy(gameObject);
    }

    protected override void Process()
    {
        placement.player.TakeXp(experienceForKill);
        placement.player.TakeArtifacts(artifactsAfterDrop);
    }

    public override void UpdateLevel(float levelModifier)
    {
        base.UpdateLevel(levelModifier);
        speed *= levelModifier;
    }
}
