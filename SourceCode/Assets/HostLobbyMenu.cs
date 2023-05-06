using UnityEngine;
using UnityEngine.UI;
using System.Net;
public class HostLobbyMenu : MonoBehaviour
{
   private NetworkManagerPong networkManager = null;
    void Update()
    {
        NetworkManagerPong obj = FindObjectOfType<NetworkManagerPong>();
        if(obj.Equals(networkManager))return;
        networkManager = obj;
    }

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMPro.TMP_InputField ipAddressInputField = null;
    [SerializeField] private Button hostButton = null;
    private void OnEnable()
    {
        NetworkManagerPong.OnClientConnected += HandleClientConnected;
        NetworkManagerPong.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkManagerPong.OnClientConnected -= HandleClientConnected;
        NetworkManagerPong.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void HostLobby()
    {
        string ipAddress = ipAddressInputField.text;
        IPAddress temp;
        if(!IPAddress.TryParse(ipAddress,out temp))return;
        networkManager.networkAddress = ipAddress;
        networkManager.StartHost();
        hostButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        hostButton.interactable = true;
        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        hostButton.interactable = true;
    }
}
