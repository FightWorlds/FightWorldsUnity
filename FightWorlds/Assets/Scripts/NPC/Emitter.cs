using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emitter : MonoBehaviour
{
    [SerializeField] private Transform[] emitters;
    [SerializeField][Range(0, 100)] private float[] chances;
    [SerializeField] private int startSpawnDelay;
    [SerializeField] private GameObject npcPrefab;
    private const int maxChance = 100;
    private float timePassed;
    private float lastSpawnTime;
    private System.Random random;
    private int len => emitters.Length;

    void Awake()
    {
        timePassed = lastSpawnTime = Time.time; // TODO: new Timer Class
        random = new System.Random();
    }

    void Update()
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
            int currentChance = (int)(chances[i] * maxChance);
            Debug.Log($"rand{currentChance + prevChance}");
            if (rand < currentChance + prevChance)
            {
                dot = i;
                break;
            }
            else
                prevChance = currentChance;
        }
        Instantiate(npcPrefab, emitters[dot].position, Quaternion.identity);
        if (random.Next(0, 3) == 0)
            Instantiate(npcPrefab,
            emitters[dot].position + Vector3.forward * random.Next(-1, 1) * 2 +
            Vector3.right * random.Next(-1, 1) * 2, Quaternion.identity);
        // TODO: replace instantiate with object pool
        lastSpawnTime = timePassed;
    }
}
