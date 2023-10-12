using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using FightWorlds.Placement;
using FightWorlds.UI;

namespace FightWorlds.Combat
{
    public class Emitter : MonoBehaviour
    {
        [SerializeField] private Vector3[] emitters;
        [Tooltip("Total sum of chances should be equal 100")]
        [SerializeField][Range(0, 100)] private int[] chances;
        [SerializeField] private float startSpawnDelay;
        [SerializeField] private int oneWaveNpcCount;
        [SerializeField] private int maxSpawnSize;
        [SerializeField] private int bottomSpawnRate;
        [SerializeField] private float spawnRadius;
        [SerializeField] private GameObject npcPrefab;
        [SerializeField] private GameObject unitPrefab;
        [SerializeField] private GameObject buildingsExplosion;
        [SerializeField] private GameObject npcsExplosion;
        [SerializeField] private PlacementSystem placement;

        public int DestroyedCount { get; private set; }

        private ObjectPool<GameObject> poolOfNpc;
        private ObjectPool<GameObject> poolOfBuildingsExplosion;
        private ObjectPool<GameObject> poolOfNPCsExplosion;
        private const int maxChance = 100;
        private float timePassed;
        private float lastSpawnTime; // TODO: maybe switch to coroutine?
        private int spawnedCounter;
        private Vector3 putAwayPosition = new Vector3(100, 100, 100);
        private Vector3 destinationOfNpc;
        private System.Random random;
        private FiringStats firingStats;
        private int len => emitters.Length;

        public GameObject GetBoomExplosion(bool isNpc) =>
        isNpc ? poolOfNPCsExplosion.Get() : poolOfBuildingsExplosion.Get();

        public void SetupForAttack(Vector3 pos, Vector3 destination)
        {
            maxSpawnSize = placement.player.resourceSystem
                .Resources[Controllers.ResourceType.Units];
            if (maxSpawnSize == 0)
            {
                AttackManagementUI.GameShouldFinish = true;
                return;
            }
            bottomSpawnRate = 0;
            chances = new int[1] { 100 };
            float planeHeight = transform.GetChild(1).position.y;
            if (pos.y < planeHeight)
                pos = new Vector3(pos.x, planeHeight, pos.z);
            emitters = new Vector3[1] { pos };
            destinationOfNpc = destination;
            Awake();
        }

        private void OnDestroy() => placement.player.NewLevel -= OnNewLevel;

        private void OnNewLevel()
        {
            oneWaveNpcCount = placement.GetTurretsLimit() +
            Mathf.CeilToInt(placement.player.Level() / 10f);
            firingStats = placement.GetNPCFiringStats();
        }

        private void Awake()
        {
            timePassed = lastSpawnTime = Time.time; // TODO: new Timer Class
            random = new System.Random();
            poolOfNpc = new ObjectPool<GameObject>(CreateNpc, OnGetNpc, OnReleaseNpc, OnDestroyNpc, false, maxSpawnSize / 5, maxSpawnSize);
            StartCoroutine(Subscribe());
            poolOfBuildingsExplosion = new ObjectPool<GameObject>(
                CreateBuildingExplosion, OnGetExplosion, OnReleaseExplosion,
                OnDestroyExplosion, maxSize: maxSpawnSize);
            poolOfNPCsExplosion = new ObjectPool<GameObject>(
                CreateNPCExplosion, OnGetExplosion, OnReleaseExplosion,
                OnDestroyExplosion, maxSize: maxSpawnSize);
            placement.GetBoomExplosion = GetBoomExplosion;
            StartCoroutine(Subscribe());
        }

        private GameObject CreateNpc()
        {
            var obj = Instantiate(PlacementSystem.AttackMode ?
                unitPrefab : npcPrefab, transform);
            obj.GetComponent<NPC>().Init(KillNpc).placement = placement;
            return obj;
        }

        private void OnGetNpc(GameObject npc)
        {
            spawnedCounter++;
            npc.transform.position = GetRandomSpawnPos();
            npc.SetActive(true);
            npc.GetComponent<CharacterController>().enabled = true;
            NPC logic = npc.GetComponent<NPC>();
            logic.ResetLogic();
            if (PlacementSystem.AttackMode)
            {
                logic.SetMainDestination(destinationOfNpc);
                var stats =
                    UnitsMenu.DefaultFiringStats * placement.player.UnitsLevel;
                firingStats =
                    new(stats.Damage, stats.Rate, stats.Strength, stats.Range);
            }
            logic.UpdateStats(firingStats);
        }

        private void OnReleaseNpc(GameObject npc)
        {
            npc.SetActive(false);
            npc.GetComponent<CharacterController>().enabled = false;
            npc.transform.position = putAwayPosition;
            DestroyedCount++;
            if (PlacementSystem.AttackMode)
            {
                placement.player
                .UseResources(1, Controllers.ResourceType.Units, false);
            }
        }

        private void OnDestroyNpc(GameObject npc) => Destroy(npc);

        private GameObject CreateNPCExplosion()
        {
            GameObject explosion = Instantiate(npcsExplosion, transform);
            explosion.GetComponent<Explosion>().UnActive = poolOfNPCsExplosion.Release;
            return explosion;
        }

        private GameObject CreateBuildingExplosion()
        {
            GameObject explosion = Instantiate(buildingsExplosion, transform);
            explosion.GetComponent<Explosion>().UnActive =
            poolOfBuildingsExplosion.Release;
            return explosion;
        }

        private void OnGetExplosion(GameObject explosion) =>
            explosion.SetActive(true);

        private void OnReleaseExplosion(GameObject explosion) =>
            explosion.SetActive(false);

        private void OnDestroyExplosion(GameObject explosion) =>
            Destroy(explosion);

        private void Update()
        {
            timePassed += Time.deltaTime;
            if (timePassed - lastSpawnTime > startSpawnDelay)
                SpawnNpc();
        }

        private IEnumerator Subscribe()
        {
            yield return null;
            OnNewLevel();
            placement.player.NewLevel += OnNewLevel;
        }

        private Vector3 GetRandomSpawnPos()
        {
            int dot = -1, prevChance = 0;
            int rand = random.Next(0, maxChance);
            for (int i = 0; i < len; i++)
            {
                int currentChance = chances[i];
                if (rand < currentChance + prevChance)
                {
                    dot = i;
                    break;
                }
                else
                    prevChance = currentChance;
            }
            Vector3 dotPos = emitters[dot];
            Vector3 spawnPos = dotPos +
            UnityEngine.Random.insideUnitSphere * spawnRadius;
            spawnPos.y = dotPos.y;
            return spawnPos;
        }

        private void KillNpc(GameObject npc) => poolOfNpc.Release(npc);

        private void SpawnNpc()
        {
            if (PlacementSystem.AttackMode &&
            !AttackManagementUI.IsAttackStarted)
                return;
            if (spawnedCounter >= maxSpawnSize)
            {
                if (poolOfNpc.CountActive <= bottomSpawnRate)
                {
                    if (PlacementSystem.AttackMode)
                        AttackManagementUI.GameShouldFinish = true;
                    else
                        spawnedCounter = 0;
                }

            }
            else
                for (int i = 0; i < oneWaveNpcCount; i++)
                    if (spawnedCounter < maxSpawnSize)
                        poolOfNpc.Get();
            lastSpawnTime = timePassed;
        }
    }
}