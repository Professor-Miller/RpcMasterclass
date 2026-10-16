using UnityEngine;
using Unity.Netcode;

public class NetworkRoleDemo : NetworkBehaviour
{
    public static event System.Action<string, string> DebugStartMessage;

    private string _speakerText = "";
    private string _messageText = "";

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SetDebugStartMessage("[THIS IS A SERVER]", "running Server Logic");
        }

        if (IsClient)
        {
            SetDebugStartMessage("[THIS IS A CLIENT]", "running Client Logic");
        }
    }

    private void SetDebugStartMessage(string speaker, string message)
    {
        _speakerText = speaker;
        _messageText = message;

        DebugStartMessage?.Invoke(_speakerText, _messageText);
    }
}
