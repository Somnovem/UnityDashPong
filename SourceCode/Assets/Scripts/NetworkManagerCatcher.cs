using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerCatcher : MonoBehaviour
{
    public static NetworkManagerPong networkManager;
    void Start()
    {
        networkManager = FindObjectOfType<NetworkManagerPong>();
    }

    public void ReturnToMenu()
    {
        networkManager.ServerChangeScene("LobbyScene");
    }
}
