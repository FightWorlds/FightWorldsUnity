using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    private const int instBuildCost = 1;
    protected override List<Collider> Detections() =>
        Physics.OverlapCapsule(currentPosition + Vector3.up,
        currentPosition + Vector3.up, attackRadius, mask).ToList();

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
        UpdateStats(placement.GetTurretsFiringStats());
        //TODO: make UpdateStats onNewLevel event
        placement.ui.CloseBuildingMenu();
        placement.ui.RemoveProcess(gameObject);
        State = BuildingState.Default;
        damage = (int)(damage * placement.player.VipMultiplier);
        if (buildingType == BuildingType.Defense)
        {
            searchCoroutine = StartCoroutine(SearchTarget());
        }
        else if (buildingType == BuildingType.Storage)
            placement.player.NewStorage(resourceType);
    }

    private void Update()
    {
        //  TODO: upgrade to FSM
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
                if (target.enabled)
                    RotateIntoTarget();
                else target = null;
    }

    protected override IEnumerator SearchTarget()
    {
        yield return null;
        destination = Vector3.positiveInfinity;
        while (!inAttackRadius)
        {
            FindTargetInDetections();
            yield return new WaitForSeconds(produceTime);
        }
        if (!isAttacking)
            StartCoroutine(AttackTarget(
                () => placement.player.UseResources
                (damage, ResourceType.Energy, false)));
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
            placement.evacuation.FinishGame();
        placement.DestroyObj(currentPosition, this);
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
    }

    private IEnumerator ProduceResources()
    {
        isProducing = true;
        yield return new WaitForSeconds(produceTime);
        Process();
        isProducing = false;
    }

    public bool TryRepair(bool instant)
    {
        if (!placement.player.UseResources(BuildingData.Cost *
        currentHp / startHp, ResourceType.Metal, true, PermanentRepair))
            return false;
        if (instant)
        {
            if (placement.player.UseResources(instBuildCost,
            ResourceType.Credits, true))
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
        placement.ui.NewActiveProcess(gameObject, ProcessType.Repairing);
        isProducing = true;
        yield return new WaitForSeconds((startHp - currentHp) / 100);
        isProducing = false;
        PermanentRepair();
    }

    private void PermanentRepair()
    {
        placement.ui.CloseBuildingMenu();
        placement.ui.RemoveProcess(gameObject);
        placement.ui.RemoveFromUnderAttack(this);
        State = BuildingState.Default;
        placement.DamageBase(currentHp - startHp);//rename method
        currentHp = startHp;
    }

    public bool TryUpgrade(bool instant)
    {
        if (BuildingLvl >= buildingMaxLvl)
            return false;
        if (!placement.player.UseResources(BuildingData.Cost,
        ResourceType.Metal, true, PermanentUpgrade))
            return false;
        if (instant)
        {
            if (placement.player.UseResources(instBuildCost,
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
        placement.ui.NewActiveProcess(gameObject, ProcessType.Upgrading);
        isProducing = true;
        yield return new WaitForSeconds(buildingTime);
        isProducing = false;
        PermanentUpgrade();
    }

    private void PermanentUpgrade()
    {
        placement.ui.CloseBuildingMenu();
        placement.ui.RemoveProcess(gameObject);
        State = BuildingState.Default;
        BuildingLvl++;
        placement.DamageBase(startHp);//rename method
    }
}
