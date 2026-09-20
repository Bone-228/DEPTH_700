using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class UGSInitializer : MonoBehaviour
{
    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("Unity Gaming Services initialized!");
            Debug.Log($"Player ID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"UGS initialization failed: {e}");
        }
    }
}