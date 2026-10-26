using UnityEngine;
using Unity.Netcode;

public class PlayerCam : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    public float sensX;
    public float sensY;

    private Transform orientation;
    private Transform playerRoot;

    float _xRotation;
    float _yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (orientation == null || playerRoot == null)
        {
            FindLocalPlayerOrientation();
            return;
        }

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        _yRotation += mouseX;
        _xRotation -= mouseY;

        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);

        Quaternion yawRotation = Quaternion.Euler(0, _yRotation, 0);
        playerRoot.rotation = yawRotation;
        orientation.rotation = yawRotation;
    }

    private void FindLocalPlayerOrientation()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsClient)
            return;

        NetworkObject localPlayer =
            NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();

        if (localPlayer == null)
            return;

        PlayerMovement playerMovement =
            localPlayer.GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement was not found on the local Player!");
            return;
        }

        playerRoot = localPlayer.transform;
        orientation = playerMovement.orientation;

        if (orientation == null)
        {
            Debug.LogError(
                "Orientation is not assigned in PlayerMovement!"
            );
            playerRoot = null;
            return;
        }

        // Start from the player's spawn heading rather than snapping to world zero.
        _yRotation = playerRoot.eulerAngles.y;
    }
}
