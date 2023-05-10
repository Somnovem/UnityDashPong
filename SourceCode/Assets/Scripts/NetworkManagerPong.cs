using Mirror;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerPong : NetworkManager
{   
    public static NetworkManagerPong Instance { get; private set; }

    public override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    [SerializeField] private int minPlayers = 2;
    [Scene] [SerializeField] private string menuScene = string.Empty;

    [SerializeField] private GameObject playerSpawnSystem = null;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayer roomPlayerPrefab = null;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayer gamePlayerPrefab = null;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;
    public static event Action OnServerStopped;

    public event Action ClientReturnedToMenu;

    public List<NetworkRoomPlayer> RoomPlayers { get; } = new List<NetworkRoomPlayer>();
    public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();

    private bool SceneOnChange = false;
    private bool ReturningToMenu = false;

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (numPlayers >= maxConnections || SceneManager.GetActiveScene().path != menuScene)
        {
            conn.Disconnect();
            return;
         }
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayer>();
            RoomPlayers.Remove(player);
            if (SceneManager.GetActiveScene().path == menuScene)NotifyPlayersOfReadyState();
        }
        if (SceneManager.GetActiveScene().path != menuScene && !SceneOnChange)
        {
            SceneOnChange = true;
            ServerChangeScene("LobbyScene");
        }
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        OnServerStopped?.Invoke();
        RoomPlayers.Clear();
        GamePlayers.Clear();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if(ReturningToMenu)
        {
            ReturningToMenu = false;
            return;
        }
        SceneManager.LoadScene("LobbyScene");
    }

    public void RestartMenu()
    {
        ReturningToMenu = true;
        ClientReturnedToMenu?.Invoke();
        NetworkRoomPlayer[] players = FindObjectsOfType<NetworkRoomPlayer>();
        for(int i = 0; i < players.Length;++i)
        {
            NetworkServer.Destroy(players[i].gameObject);
        }
        StopHost();
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) { return false; }
        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) { return false; }
        }
        return true;
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            if (!IsReadyToStart()) { return; }
            ServerChangeScene("GameScene");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            NetworkGamePlayer.ResetStaticVariables();
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab);
                gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
                NetworkServer.Destroy(conn.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
                StartCoroutine(DestroyCopy(gameplayerInstance.gameObject));
            }
        }
        base.ServerChangeScene(newSceneName);
        if (SceneManager.GetActiveScene().path != menuScene)
        {
            StopHost();
        }
        SceneOnChange = false;
    }
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        OnServerReadied?.Invoke(conn);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName.Equals("GameScene"))
        {
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            NetworkServer.Spawn(playerSpawnSystemInstance);
        }
    }
    
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            bool isLeader = RoomPlayers.Count == 0;
            NetworkRoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);
            roomPlayerInstance.IsLeader = isLeader;
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    private IEnumerator DestroyCopy(GameObject copy)
    {
        yield return new WaitForSeconds(1f);
        NetworkServer.Destroy(copy);
    }
}