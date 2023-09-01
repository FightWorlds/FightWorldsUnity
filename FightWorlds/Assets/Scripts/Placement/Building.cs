using System.Collections;
using UnityEngine;

public class Building : Damageable
{
    [SerializeField] private BuildingType buildingType;
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int produceTime;
    [SerializeField] private int resourcesPerOperation;
    private const float xpMultiplier = 0.2f;
    private bool isProducing;
    protected override Collider[] Detections() =>
        Physics.OverlapCapsule(currentPosition + Vector3.up,
        currentPosition + Vector3.up, attackRadius, mask);

    private void Update()
    {
        if (buildingType == BuildingType.Default ||
            buildingType == BuildingType.Storage)
            return;
        else if (!isProducing &&
            (buildingType == BuildingType.Mine ||
            buildingType == BuildingType.Recycle))
            StartCoroutine(ProduceResources());
        else if (buildingType == BuildingType.Defense)
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
        if (target != null)
            UseEnergy();
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

    protected override void Process()
    {
        if (buildingType == BuildingType.Mine)
            placement.player
            .TakeResources(resourcesPerOperation, resourceType);
        else
        {
            ResourceType type = resourceType == ResourceType.Ore ?
            ResourceType.Metal : ResourceType.Energy;
            bool possible = placement.player
            .UseResources(resourcesPerOperation, resourceType);
            if (!possible)
                return;
            placement.player
            .TakeResources(resourcesPerOperation, type);
            placement.player
            .TakeXp((int)(resourcesPerOperation * xpMultiplier));
        }
        // TODO remove from here to other cs
    }

    private IEnumerator ProduceResources()
    {
        isProducing = true;
        yield return new WaitForSeconds(produceTime);
        Process();
        isProducing = false;
    }

    private bool UseEnergy() =>
    placement.player.UseResources(resourcesPerOperation, ResourceType.Energy);
}
