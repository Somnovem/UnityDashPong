using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private GameObject ballPrefab = null;
    private static List<Transform> spawnPoints = new List<Transform>();
    public static Transform ballSpawnPoint;
    private int nextIndex = 0;
    private int playerCount = 0;
    private GameObject ball;
    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }
    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);
    public override void OnStartServer() => NetworkManagerPong.OnServerReadied += SpawnPlayer;

    [ServerCallback]
    private void OnDestroy() => NetworkManagerPong.OnServerReadied -= SpawnPlayer;

    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {   
        ++playerCount;
        if(playerCount > 2)return;
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(nextIndex);
        if (spawnPoint == null)
        {
            Debug.LogError($"Missing spawn point for player {nextIndex}");
            return;
        }
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);
        NetworkServer.Spawn(playerInstance, conn);
        if(playerCount == 2)
        {
            ball = Instantiate(ballPrefab,ballSpawnPoint.position,ballSpawnPoint.rotation);
            NetworkServer.Spawn(ball);
        }
        nextIndex++;
    }
}
