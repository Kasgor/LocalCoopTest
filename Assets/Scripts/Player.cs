using UnityEngine;

using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [Header("Settings")]

    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private int maxHealth = 100;

    [Header("Camera")]
    [SerializeField]
    private Camera playerCamera;

    public NetworkVariable<int> health = new NetworkVariable<int>(100);
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();
    private CharacterController characterController;
    private Vector3 velocity;
    private float gravity = -9.81f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
            characterController = gameObject.AddComponent<CharacterController>();
    }


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);
            else
                CreatePlayerCamera();
        }
        else
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);
        }

        health.Value = maxHealth;
    }

    private void CreatePlayerCamera()
    {
        GameObject cameraObj = new GameObject("PlayerCamera");
        playerCamera = cameraObj.AddComponent<Camera>();
        cameraObj.transform.SetParent(transform);
        cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
    }

    private void Update()
    {
        if (IsOwner)
        {
            HandleMovement();
            HandleCameraFollow();
        }
        else
        {
            UpdateRemotePlayerPosition();
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up * mouseX);
        UpdatePositionServerRpc(transform.position, transform.rotation);
    }

    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
    {
        networkPosition.Value = position;
        networkRotation.Value = rotation;
    }

    private void UpdateRemotePlayerPosition()
    {
        transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 15f);
        transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation.Value, Time.deltaTime * 15f);
    }

    private void HandleCameraFollow()
    {
        if (playerCamera != null)
        {
            Vector3 cameraPosition = transform.position + Vector3.up * 1.6f + transform.forward * -2f;
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, cameraPosition, Time.deltaTime * 5f);
            playerCamera.transform.LookAt(transform.position + Vector3.up * 1.6f);
        }
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        health.Value = Mathf.Max(0, health.Value - damage);

        if (health.Value <= 0)
        {
            Debug.Log($"Player {OwnerClientId} died!");
        }
    }

    public int GetHealth()
    {
        return health.Value;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

}
