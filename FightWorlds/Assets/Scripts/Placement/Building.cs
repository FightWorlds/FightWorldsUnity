using System;
using System.Collections;
using UnityEngine;

public class Building : Damageable
{
    [SerializeField] private BuildingType buildingType;
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int produceTime;
    [SerializeField] private int buildingTime;
    [SerializeField] private int resourcesPerOperation;
    public bool IsCompleted;
    private bool isProducing;
    protected override Collider[] Detections() =>
        Physics.OverlapCapsule(currentPosition + Vector3.up,
        currentPosition + Vector3.up, attackRadius, mask);

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(Build());
    }

    private IEnumerator Build()
    {
        yield return null;
        placement.ui.ShowBuildingMenu(this);
        placement.ui.NewActiveProcess(gameObject);
        yield return new WaitForSeconds(buildingTime);
        PermanentBuild(true);
    }

    public void PermanentBuild()
    {
        StopAllCoroutines();
        PermanentBuild(true);
    }

    private void PermanentBuild(bool flag)
    {
        placement.ui.CloseBuildingMenu();
        placement.ui.RemoveProcess(gameObject);
        IsCompleted = true;
        if (buildingType == BuildingType.Defense)
            searchCoroutine = StartCoroutine(SearchTarget());
    }

    private void Update()
    {
        if (!IsCompleted) return;
        if (buildingType == BuildingType.Default ||
            buildingType == BuildingType.Storage)
            return;
        else if (!isProducing &&
            (buildingType == BuildingType.Mine ||
            buildingType == BuildingType.Recycle))
            StartCoroutine(ProduceResources());
        else if (buildingType == BuildingType.Defense)
            if (target != null)
                RotateIntoTarget();
    }

    protected override IEnumerator SearchTarget()
    {
        while (target == null)
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
            yield return new WaitForSeconds(produceTime);
            UseEnergy();
        }
        if (!isAttacking)
            StartCoroutine(AttackTarget());
    }

    protected override void OnDamageTaken(int damage)
    {
        base.OnDamageTaken(damage);
        if (currentHp <= 0) return;
        placement.ui.AddBuildUnderAttack(this);
        placement.DamageBase(damage);
    }

    protected override void Die()
    {
        base.Die();
        placement.DestroyObj(currentPosition);
        placement.ui.RemoveFromUnderAttack(this);
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
            .IsPossibleToConvert(resourcesPerOperation, resourceType, type);
            if (!possible)
                return;
            placement.player.UseResources(resourcesPerOperation, resourceType);
            placement.player.TakeResources(resourcesPerOperation, type);
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
