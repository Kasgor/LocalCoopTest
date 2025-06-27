using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private int damage = 10;
    [SerializeField]
    private float lifetime = 5f;

    private NetworkVariable<Vector3> direction = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private float spawnTime;

    public override void OnNetworkSpawn()
    {
        spawnTime = Time.time;

        if (IsServer)
        {
            networkPosition.Value = transform.position;
        }
    }

    public void Initialize(Vector3 dir)
    {
        if (IsServer)
        {
            direction.Value = dir.normalized;
            networkPosition.Value = transform.position;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            Vector3 movement = direction.Value * speed * Time.deltaTime;
            transform.Translate(movement, Space.World);
            networkPosition.Value = transform.position;

            if (Time.time - spawnTime >= lifetime)
            {
                DestroyBullet();
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 20f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamageServerRpc(damage);
            DestroyBullet();
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}