using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField]
    private Button hostButton;
    [SerializeField]
    private Button clientButton;
    [SerializeField]
    private Button shutdownButton;
    [SerializeField]
    private TextMeshProUGUI statusText;
    [SerializeField]
    private GameObject panel;

    private void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
        shutdownButton.onClick.AddListener(Shutdown);

        UpdateUI();
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        UpdateUI();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        UpdateUI();
    }

    private void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool isConnected = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient;

        hostButton.gameObject.SetActive(!isConnected);
        clientButton.gameObject.SetActive(!isConnected);
        shutdownButton.gameObject.SetActive(isConnected);
        panel.SetActive(!isConnected);

        if (NetworkManager.Singleton.IsHost)
            statusText.text = "Host Active";
        else if (NetworkManager.Singleton.IsClient)
            statusText.text = "Client Connected";
        else
            statusText.text = "Not Connected";
    }

    private void Update()
    {
        UpdateUI();
    }
}