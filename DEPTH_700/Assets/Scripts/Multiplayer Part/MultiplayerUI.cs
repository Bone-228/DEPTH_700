using TMPro;
using UnityEngine;

public class MultiplayerUI : MonoBehaviour
{
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private TMP_InputField joinCodeInput;

    private void OnEnable()
    {
        if (joinCodeInput == null)
        {
            Debug.LogError("MultiplayerUI: Join Code Input is not assigned.", this);
            return;
        }

        joinCodeInput.onValueChanged.AddListener(ConvertJoinCodeToUppercase);
        ConvertJoinCodeToUppercase(joinCodeInput.text);
    }

    private void OnDisable()
    {
        if (joinCodeInput != null)
            joinCodeInput.onValueChanged.RemoveListener(ConvertJoinCodeToUppercase);
    }

    public void CreateGame()
    {
        if (sessionManager != null)
            sessionManager.CreateSession();
        else
            Debug.LogError("MultiplayerUI: Session Manager is not assigned.", this);
    }

    public void JoinGame()
    {
        if (sessionManager == null || joinCodeInput == null)
        {
            Debug.LogError("MultiplayerUI: Session Manager or Join Code Input is not assigned.", this);
            return;
        }

        string code = joinCodeInput.text.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Enter a session code before joining.", this);
            return;
        }

        sessionManager.JoinSession(code);
    }

    private void ConvertJoinCodeToUppercase(string value)
    {
        if (joinCodeInput == null)
            return;

        string uppercaseValue = value.ToUpperInvariant();
        if (value == uppercaseValue)
            return;

        int caretPosition = joinCodeInput.caretPosition;
        joinCodeInput.SetTextWithoutNotify(uppercaseValue);
        joinCodeInput.caretPosition = Mathf.Min(caretPosition, uppercaseValue.Length);
    }
}
