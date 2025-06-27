using UnityEngine;

using Unity.Netcode;
using System.Collections;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private float spawnInterval = 5f;
    [SerializeField]
    private int maxEnemies = 10;
    [SerializeField]
    private float spawnRadius = 15f;
    [SerializeField]
    private int currentEnemyCount = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        currentEnemyCount = 0;
        StopAllCoroutines();
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (IsServer)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.GetComponent<NetworkObject>().Spawn();
        currentEnemyCount++;
        NetworkObject networkObject = enemy.GetComponent<NetworkObject>();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        Collider[] colliders = Physics.OverlapSphere(spawnPosition, 1f);
        if (colliders.Length > 0)
        {
            return GetRandomSpawnPosition();
        }

        return spawnPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
    public override void OnNetworkDespawn()
    {
        StopAllCoroutines();
        currentEnemyCount = 0;
    }

}
