using System.Collections;
using UnityEngine;

public class RandomMultiSpawner : MonoBehaviour
{
    [Header("Spawn Edilecek Prefablar")]
    public GameObject[] spawnPrefabs; // Farklı objeler

    [Header("Spawn Noktaları")]
    public Transform[] spawnPoints; // Belirli noktalar

    [Header("Zaman Aralığı")]
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 4f;

    [Header("Toplam Spawn Sayısı (0 = sınırsız)")]
    public int maxSpawnCount = 0;

    private int spawnedCount = 0;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (maxSpawnCount == 0 || spawnedCount < maxSpawnCount)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            GameObject randomPrefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Length)];
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Instantiate(randomPrefab, randomPoint.position, Quaternion.identity);
            spawnedCount++;
        }
    }
}
 