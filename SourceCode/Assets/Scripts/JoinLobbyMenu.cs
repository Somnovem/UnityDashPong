using System.Net;
using UnityEngine;
using UnityEngine.UI;
public class JoinLobbyMenu : MonoBehaviour
{
    private NetworkManagerPong networkManager = null;

    void Start()
    {
        ipAddressInputField.text = AuthorizationClient.serverIP.Substring(0,AuthorizationClient.serverIP.LastIndexOf('.')+1);
    }

    void Update()
    {
        NetworkManagerPong obj = FindObjectOfType<NetworkManagerPong>();
        if(obj.Equals(networkManager))return;
        networkManager = obj;
    }

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private TMPro.TMP_InputField ipAddressInputField = null;
    [SerializeField] private Button joinButton = null;

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

    public void JoinLobby()
    {
        string ipAddress = ipAddressInputField.text;
        IPAddress temp;
        if(!IPAddress.TryParse(ipAddress,out temp))return;
        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();
        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;
        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
