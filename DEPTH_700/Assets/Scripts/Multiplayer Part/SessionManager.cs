using System;
using UnityEngine;
using Unity.Services.Multiplayer;

public class SessionManager : MonoBehaviour
{
    private ISession currentSession;

    public async void CreateSession()
    {
        try
        {
            var options = new SessionOptions
            {
                MaxPlayers = 4
            }.WithRelayNetwork();

            currentSession =
                await MultiplayerService.Instance.CreateSessionAsync(options);

            Debug.Log("Session created!");
            Debug.Log($"Session Code: {currentSession.Code}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create session: {e}");
        }
    }

    public async void JoinSession(string sessionCode)
    {
        try
        {
            currentSession =
                await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);

            Debug.Log("Successfully joined session!");
            Debug.Log($"Joined Session Code: {currentSession.Code}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join session: {e}");
        }
    }
}
