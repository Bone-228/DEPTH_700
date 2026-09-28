using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints;

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

        StartCoroutine(SpawnPlayerWhenReady(clientId));
    }

    private System.Collections.IEnumerator SpawnPlayerWhenReady(ulong clientId)
    {
        while (true)
        {
            if (NetworkManager.Singleton == null)
                yield break;

            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(
                    clientId,
                    out NetworkClient client))
            {
                yield return null;
                continue;
            }

            if (client.PlayerObject != null)
                break;

            yield return null;
        }

        NetworkObject playerObject =
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if (playerObject == null)
        {
            Debug.LogError(
                $"PlayerObject not found for Client ID {clientId}"
            );

            yield break;
        }

        // Работаем только со своим Player
        if (!playerObject.IsOwner)
            yield break;

        Transform spawnPoint = GetSpawnPoint(clientId);

        if (spawnPoint == null)
            yield break;

        playerObject.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log(
            $"LOCAL Player {clientId} spawned at " +
            $"{spawnPoint.name} | " +
            $"Position: {spawnPoint.position}"
        );
    }

    private Transform GetSpawnPoint(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return null;
        }

        int index = (int)clientId % spawnPoints.Length;

        return spawnPoints[index];
    }
}