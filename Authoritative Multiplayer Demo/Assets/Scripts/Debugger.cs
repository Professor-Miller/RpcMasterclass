using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Debugger : MonoBehaviour
{
    [SerializeField] private ScrollRect debuggerScroll;
    [SerializeField] private GameObject contentDisplay;
    [SerializeField] private GameObject textPrefab;

    void OnEnable()
    {
        NetworkRoleDemo.DebugStartMessage += AddDebugMessage;
        RpcLabDemo.OnButtonPressed += AddDebugMessage;
        Player.OnGiftReceived += AddDebugMessage;
        Player.OnAttackRequested += AddDebugMessage;
        Player.OnPlayerDeath += AddDebugMessage;
    }

    void OnDisable()
    {
        NetworkRoleDemo.DebugStartMessage -= AddDebugMessage;
        RpcLabDemo.OnButtonPressed -= AddDebugMessage;
        Player.OnGiftReceived -= AddDebugMessage;
        Player.OnAttackRequested -= AddDebugMessage;
        Player.OnPlayerDeath -= AddDebugMessage;
    }


    private void AddDebugMessage(string speaker, string message)
    {
        var newMessage = Instantiate(textPrefab, contentDisplay.transform);

        newMessage.TryGetComponent(out TextMeshProUGUI text);
        text.text = $"{speaker}: {message}";

        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();

        debuggerScroll.verticalNormalizedPosition = 0.0f;
    }
}
