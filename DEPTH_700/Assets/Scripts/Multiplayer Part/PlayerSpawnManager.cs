using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints;

    private int nextSpawnIndex = 0;

    public Transform GetNextSpawnPoint()
    {
        if (spawnPoints.Length == 0)
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