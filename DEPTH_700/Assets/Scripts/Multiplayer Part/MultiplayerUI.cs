using UnityEngine;
using TMPro;
public class MultiplayerUI : MonoBehaviour
{
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private TMP_InputField joinCodeInput;

    public void CreateGame()
    {
        sessionManager.CreateSession();
    }

    public void JoinGame()
    {
        string code = joinCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Session code is empty!");
            return;
        }

        sessionManager.JoinSession(code);
    }
}
