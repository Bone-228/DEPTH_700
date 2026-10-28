using System;
using UnityEngine;
using Unity.Services.Multiplayer;

public class SessionManager : MonoBehaviour
{
    private ISession currentSession;
    private bool joinInProgress;

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

            EnterGameplay();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create session: {e}");
        }
    }

    public async void JoinSession(string sessionCode)
    {
        if (joinInProgress)
            return;

        string normalizedCode = sessionCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(normalizedCode))
        {
            Debug.LogWarning("Enter a session code before joining.");
            return;
        }

        joinInProgress = true;

        try
        {
            ISession joinedSession =
                await MultiplayerService.Instance.JoinSessionByCodeAsync(normalizedCode);

            currentSession = joinedSession;

            Debug.Log("Successfully joined session!");
            Debug.Log($"Joined Session Code: {currentSession.Code}");

            EnterGameplay();
        }
        catch (SessionException e) when (
            e.Error == SessionError.SessionNotFound ||
            e.Error == SessionError.InvalidParameter)
        {
            Debug.LogWarning("Could not join: the session code is incorrect, expired, or the session is closed.");
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to join session ({e.Error}): {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join session: {e}");
        }
        finally
        {
            joinInProgress = false;
        }
    }

    public void EnterGameplay()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public async void LeaveSession()
    {
        if (currentSession == null)
            return;

        try
        {
            await currentSession.LeaveAsync();

            currentSession = null;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("Successfully left session!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to leave session: {e}");
        }
    }
}
