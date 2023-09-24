using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FightWorlds.UI;
using FightWorlds.Controllers;
using FightWorlds.Combat;

namespace FightWorlds.Placement
{
    public class Building : Damageable
    {
        [SerializeField] private BuildingType buildingType;
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private int produceTime;
        [SerializeField] private int buildingTime;
        [SerializeField] private int resourcesPerOperation;
        [SerializeField] private Material unActive;
        public BuildingData BuildingData;
        public BuildingState State;
        public int BuildingLvl { get; private set; }
        public bool IsProducing { get; private set; }
        public BuildingType BuildingType { get { return buildingType; } }
        public Action<Building> OnBuilded;

        private const int buildingMaxLvl = 3;
        private const int instBuildCost = 1;

        private List<Material> defaultMaterials;
        private Collider modelCollider;

        protected override List<Collider> Detections() =>
            Physics.OverlapCapsule(currentPosition + Vector3.up,
            currentPosition + Vector3.up, attackRadius, mask).ToList();

        protected override void Awake()
        {
            base.Awake();
            BuildingLvl = 1;
            State = BuildingState.Building;
            defaultMaterials = new();
            foreach (Transform child in transform.GetChild(0))
            {
                if (child.TryGetComponent(out Renderer renderer))
                {
                    defaultMaterials.Add(renderer.material);
                    renderer.material = unActive;
                }
            }
            modelCollider = GetComponent<Collider>();
            modelCollider.isTrigger = true;
        }

        private IEnumerator Build()
        {
            IsProducing = true;
            yield return new WaitForSeconds(buildingTime);
            IsProducing = false;
            PermanentBuild();
        }

        public void PermanentBuild()
        {
            int counter = 0;
            UpdateStats(placement.GetTurretsFiringStats());
            OnBuilded(this);
            placement.ui.RemoveProcess(gameObject, true);
            placement.ResetSelectedBuilding();
            modelCollider.isTrigger = false;
            foreach (Transform child in transform.GetChild(0))
            {
                if (child.TryGetComponent(out Renderer renderer))
                {
                    renderer.material = defaultMaterials[counter];
                    counter++;
                }
            }
            State = BuildingState.Default;
            if (buildingType == BuildingType.Defense)
                searchCoroutine = StartCoroutine(SearchTarget());
            else if (buildingType == BuildingType.Storage)
                placement.player.NewStorage(resourceType);
            else if (buildingType == BuildingType.Dock)
                placement.ui.AddRepairBot(this);
        }

        private void Update()
        {
            //  TODO: upgrade to FSM
            if (State == BuildingState.Building) return;
            if (!IsProducing &&
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
            placement.UpdateBaseHp(damage);
            if (currentHp <= 0) return;
            State = BuildingState.Damaged;
            placement.ui.AddBuildUnderAttack(this);
        }

        protected override void Die()
        {
            base.Die();
            if (BuildingData.ID == 1)
                placement.evacuation.FinishGame();
            var boom = placement.GetBoomExplosion(false);
            boom.transform.position = currentPosition;
            placement.DestroyObj(currentPosition, this);
            placement.ui.RemoveFromUnderAttack(this);
            placement.ui.RemoveProcess(gameObject, false);
            if (buildingType == BuildingType.Storage)
                placement.player.DestroyStorage(resourceType);
            else if (buildingType == BuildingType.Dock)
                placement.ui.RemoveRepairBot(this);
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
            IsProducing = true;
            yield return new WaitForSeconds(produceTime);
            Process();
            IsProducing = false;
        }

        public bool TryBuild(bool instant)
        {
            placement.ui.CloseBuildingMenu();
            if (instant)
            {
                if (placement.player.UseResources(instBuildCost,
                ResourceType.Credits, true))
                {
                    PermanentBuild();
                    return true;
                }
            }
            if (!placement.ui.NewActiveProcess(
            gameObject, ProcessType.Building))
                return false;
            StartCoroutine(Build());
            return true;
        }

        public bool TryRepair(bool instant)
        {
            placement.ui.CloseBuildingMenu();
            int cost = (int)(BuildingData.Cost * currentHp / (float)startHp);
            if (!placement.player.UseResources(cost, ResourceType.Metal, true, PermanentRepair))
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
            if (!placement.ui.NewActiveProcess(
            gameObject, ProcessType.Repairing))
            {
                placement.player.TakeResources(cost, ResourceType.Metal);
                return false;
            }
            StartCoroutine(Repair());
            return true;
        }

        private IEnumerator Repair()
        {
            IsProducing = true;
            yield return new WaitForSeconds(buildingTime);
            //new WaitForSeconds((float)currentHp / startHp * buildingTime);
            IsProducing = false;
            PermanentRepair();
        }

        private void PermanentRepair()
        {
            placement.ui.RemoveProcess(gameObject, true);
            placement.ui.RemoveFromUnderAttack(this);
            State = BuildingState.Default;
            placement.UpdateBaseHp(currentHp - startHp);
            currentHp = startHp;
        }

        public bool TryUpgrade(bool instant)
        {
            if (BuildingLvl >= buildingMaxLvl)
                return false;
            placement.ui.CloseBuildingMenu();
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
            if (!placement.ui.NewActiveProcess(
            gameObject, ProcessType.Upgrading))
            {
                placement.player.TakeResources(
                BuildingData.Cost, ResourceType.Metal);
                return false;
            }
            StartCoroutine(Upgrade());
            return true;
        }

        private IEnumerator Upgrade()
        {
            placement.ui.NewActiveProcess(gameObject, ProcessType.Upgrading);
            IsProducing = true;
            yield return new WaitForSeconds(buildingTime);
            IsProducing = false;
            PermanentUpgrade();
        }

        private void PermanentUpgrade()
        {
            placement.ui.RemoveProcess(gameObject, true);
            State = BuildingState.Default;
            // TODO: add stats changing?
            BuildingLvl++;
            placement.UpdateBaseHp(startHp);//rename method
        }
    }
}