using Unity.Netcode;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    private Transform cameraPosition;

    private void Update()
    {
        if (cameraPosition == null)
        {
            FindLocalPlayer();
            return;
        }

        transform.position = cameraPosition.position;
    }

    private void FindLocalPlayer()
    {
        if (!NetworkManager.Singleton.IsClient)
            return;

        NetworkObject localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();

        if (localPlayer == null)
            return;

        Transform playerCameraPosition = localPlayer.transform.Find("CameraPos");

        if (playerCameraPosition == null)
        {
            Debug.LogError("CameraPos was not found on the local Player!");
            return;
        }

        cameraPosition = playerCameraPosition;
    }
}