using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Emitter : MonoBehaviour
{
    [SerializeField] private Transform[] emitters;
    [Tooltip("Total sum of chances should be equal 100")]
    [SerializeField][Range(0, 100)] private int[] chances;
    [SerializeField] private float startSpawnDelay;
    [SerializeField] private int oneWaveNpcCount;
    [SerializeField] private int maxSpawnSize;
    [SerializeField] private int bottomSpawnRate;
    [SerializeField] private float spawnRadius;
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private PlacementSystem placement;
    private ObjectPool<GameObject> poolOfNpc;
    private const int maxChance = 100;
    private float timePassed;
    private float lastSpawnTime; // TODO: maybe switch to coroutine?
    private bool isWaveStopped;
    private Vector3 putAwayPosition = new Vector3(100, 100, 100);
    private System.Random random;
    private FiringStats firingStats;
    private int len => emitters.Length;

    private void OnDestroy() => placement.player.NewLevel -= OnNewLevel;

    private void OnNewLevel()
    {
        oneWaveNpcCount = placement.GetTurretsLimit() +
        Mathf.CeilToInt(placement.player.Level() / 10);
        firingStats = placement.GetNPCFiringStats();
    }
    private void Awake()
    {
        timePassed = lastSpawnTime = Time.time; // TODO: new Timer Class
        random = new System.Random();
        poolOfNpc = new ObjectPool<GameObject>(CreateNpc, OnGetNpc, OnReleaseNpc, OnDestroyNpc, false, maxSpawnSize / 10, maxSpawnSize);
        StartCoroutine(Subscribe());
    }

    private GameObject CreateNpc()
    {
        var obj = Instantiate(npcPrefab, transform);
        obj.GetComponent<NPC>().Init(KillNpc).placement = placement;
        return obj;
    }

    private void OnGetNpc(GameObject npc)
    {
        npc.transform.position = GetRandomSpawnPos();
        npc.SetActive(true);
        npc.GetComponent<CharacterController>().enabled = true;
        NPC logic = npc.GetComponent<NPC>();
        //logic.UpdateLevel(levelModifier); // TODO: move from OnGet
        logic.ResetLogic();
        logic.UpdateStats(firingStats);
    }

    private void OnReleaseNpc(GameObject npc)
    {
        npc.SetActive(false);
        npc.GetComponent<CharacterController>().enabled = false;
        npc.transform.position = putAwayPosition;
    }

    private void OnDestroyNpc(GameObject npc)
    {
        Destroy(npc);
    }

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
        Vector3 dotPos = emitters[dot].position;
        Vector3 spawnPos = dotPos +
        UnityEngine.Random.insideUnitSphere * spawnRadius;
        spawnPos.y = dotPos.y;
        return spawnPos;
    }

    private void KillNpc(GameObject npc) => poolOfNpc.Release(npc);

    private void SpawnNpc()
    {
        for (int i = 0; i < oneWaveNpcCount; i++)
        {
            if (isWaveStopped)
            {
                if (poolOfNpc.CountActive < bottomSpawnRate)
                    isWaveStopped = false;
                else
                    return;
            }
            else
            {
                if (poolOfNpc.CountActive < maxSpawnSize)
                    poolOfNpc.Get();
                else
                    isWaveStopped = true;
            }
        }
        lastSpawnTime = timePassed;
    }
}
