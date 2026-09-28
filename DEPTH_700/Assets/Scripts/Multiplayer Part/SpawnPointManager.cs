using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager Instance;

    public Transform[] spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    public Transform GetSpawnPoint(ulong clientId)
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