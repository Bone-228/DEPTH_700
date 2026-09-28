using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints;

    private int nextSpawnIndex = 0;

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager was not found!");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Player connected! Client ID: {clientId}");
    }

    public Transform GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return null;
        }

        Transform spawnPoint = spawnPoints[nextSpawnIndex];

        nextSpawnIndex++;

        if (nextSpawnIndex >= spawnPoints.Length)
        {
            nextSpawnIndex = 0;
        }

        return spawnPoint;
    }
}