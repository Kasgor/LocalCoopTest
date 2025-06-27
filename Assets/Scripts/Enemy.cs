using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class Enemy : NetworkBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField]
    private float detectionRange = 10f;
    [SerializeField]
    private float shootInterval = 2f;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Transform firePoint;


    private NetworkVariable<Vector3> targetPosition = new NetworkVariable<Vector3>();
    private Player[] players;
    private float lastShootTime;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = Vector3.forward;
            firePoint = firePointObj.transform;
        }

        StartCoroutine(UpdateTargeting());
    }

    private IEnumerator UpdateTargeting()
    {
        while (IsServer)
        {
            UpdateTarget();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (targetPosition.Value != Vector3.zero)
        {
            Vector3 direction = (targetPosition.Value - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
            }
        }
        if (Time.time - lastShootTime >= shootInterval)
        {
            TryShoot();
        }
    }

    private void UpdateTarget()
    {
        players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        Player closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (Player player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= detectionRange && distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            targetPosition.Value = closestPlayer.transform.position;
        }
        else
        {
            targetPosition.Value = Vector3.zero;
        }
    }

    private void TryShoot()
    {
        if (targetPosition.Value == Vector3.zero) return;

        Vector3 direction = (targetPosition.Value - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        bullet.GetComponent<NetworkObject>().Spawn();

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction);
        }

        lastShootTime = Time.time;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}