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
    public BuildingData BuildingData;
    public BuildingState State;

    public int BuildingLvl { get; private set; }
    private bool isProducing;
    private const int buildingMaxLvl = 3;
    private const int instBuildCost = 100;
    protected override Collider[] Detections() =>
        Physics.OverlapCapsule(currentPosition + Vector3.up,
        currentPosition + Vector3.up, attackRadius, mask);

    protected override void Awake()
    {
        base.Awake();
        BuildingLvl = 1;
        StartCoroutine(Build());
    }

    private IEnumerator Build()
    {
        State = BuildingState.Building;
        yield return null;
        placement.ui.ShowBuildingMenu(this);
        placement.ui.NewActiveProcess(gameObject, ProcessType.Building);
        yield return new WaitForSeconds(buildingTime);
        PermanentBuild();
    }

    public void PermanentBuild(bool instant, bool flag)
    {
        if (!instant) return;
        if (flag)
        {
            StopAllCoroutines();
            PermanentBuild();
            return;
        }
        if (placement.player.UseResources(instBuildCost,
        ResourceType.Credits, true))
            PermanentBuild();
    }

    private void PermanentBuild()
    {
        placement.ui.CloseBuildingMenu();
        placement.ui.RemoveProcess(gameObject);
        State = BuildingState.Default;
        damage = (int)(damage * placement.player.VipMultiplier);
        if (buildingType == BuildingType.Defense)
            searchCoroutine = StartCoroutine(SearchTarget());
        else if (buildingType == BuildingType.Storage)
            placement.player.NewStorage(resourceType);
    }

    private void Update()
    {
        if (State == BuildingState.Building) return;
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

    protected override void OnDamageTaken(int damage, Vector3 fromPos)
    {
        base.OnDamageTaken(damage, fromPos);
        placement.DamageBase(damage);
        if (currentHp <= 0) return;
        State = BuildingState.Damaged;
        placement.ui.AddBuildUnderAttack(this);
    }

    protected override void Die()
    {
        base.Die();
        if (BuildingData.ID == 1)
            placement.FinishGame();
        placement.DestroyObj(currentPosition);
        placement.ui.RemoveFromUnderAttack(this);
        placement.ui.RemoveProcess(gameObject);
        if (buildingType == BuildingType.Storage)
            placement.player.DestroyStorage(resourceType);
        Destroy(gameObject);
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
            placement.player.UseResources(resourcesPerOperation,
            resourceType, false);
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
    placement.player.UseResources(resourcesPerOperation,
    ResourceType.Energy, false);

    public bool TryRepair(bool instant)
    {
        if (!placement.player.UseResources(BuildingData.Cost *
        currentHp / startHp, ResourceType.Metal, true))
            return false;
        if (instant)
        {
            if (placement.player.UseResources(10, ResourceType.Credits, true))
            {
                PermanentRepair();
                return true;
            }
        }
        StartCoroutine(Repair());
        return true;
    }

    private IEnumerator Repair()
    {
        isProducing = true;
        placement.ui.NewActiveProcess(gameObject, ProcessType.Repairing);
        yield return new WaitForSeconds((startHp - currentHp) / 100);
        PermanentRepair();
    }

    private void PermanentRepair()
    {
        placement.ui.CloseBuildingMenu();
        placement.ui.RemoveProcess(gameObject);
        State = BuildingState.Default;
        currentHp = startHp;
        isProducing = false;
    }

    public bool TryUpgrade(bool instant)
    {
        if (BuildingLvl >= buildingMaxLvl)
            return false;
        if (!placement.player.UseResources(BuildingData.Cost,
        ResourceType.Metal, true))
            return false;
        if (instant)
        {
            if (placement.player.UseResources(instBuildCost * BuildingLvl,
            ResourceType.Credits, true))
            {
                PermanentUpgrade();
                return true;
            }
        }
        StartCoroutine(Upgrade());
        return true;
    }

    private IEnumerator Upgrade()
    {
        isProducing = true;
        placement.ui.NewActiveProcess(gameObject, ProcessType.Upgrading);
        yield return new WaitForSeconds(buildingTime);
        PermanentUpgrade();
    }

    private void PermanentUpgrade()
    {
        placement.ui.CloseBuildingMenu();
        placement.ui.RemoveProcess(gameObject);
        State = BuildingState.Default;
        BuildingLvl++;
        damage += damage;
        startHp += startHp;
        currentHp = startHp;
        isProducing = false;
    }
}
