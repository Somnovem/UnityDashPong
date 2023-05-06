using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private NetworkManagerPong networkManager = null;

    void Update()
    {
        NetworkManagerPong obj = FindObjectOfType<NetworkManagerPong>();
        if(obj.Equals(networkManager))return;
        networkManager = obj;
        networkManager.ClientReturnedToMenu += RestartLobby;
    }

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;

    private void RestartLobby()
    {
        landingPagePanel.SetActive(true);
    }
}
