using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emitter : MonoBehaviour
{
    [SerializeField] private Transform[] emitters;
    [Tooltip("Total sum of chances should be equal 100")]
    [SerializeField][Range(0, 100)] private int[] chances;
    [SerializeField] private int startSpawnDelay;
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private PlacementSystem placement;
    private const int maxChance = 100;
    private float timePassed;
    private float lastSpawnTime;
    private System.Random random;
    private int len => emitters.Length;

    private void Awake()
    {
        timePassed = lastSpawnTime = Time.time; // TODO: new Timer Class
        random = new System.Random();
    }

    private void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed - lastSpawnTime > startSpawnDelay)
            SpawnNpc();
        //Debug.Log(timePassed);
    }

    private void SpawnNpc()
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
        for (int i = 0; i < random.Next(0, 4); i++)
        {
            var unit = Instantiate(npcPrefab,
                emitters[dot].position +
                Vector3.forward * random.Next(-1, 1) * 2 +
                Vector3.right * random.Next(-1, 1) * 2,
                Quaternion.identity);
            unit.GetComponent<Damageable>().placement = placement;
        }
        // TODO: replace instantiate with object pool
        lastSpawnTime = timePassed;
    }
}
