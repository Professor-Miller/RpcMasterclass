using UnityEngine;
using Unity.Netcode;

public class RpcLabDemo : NetworkBehaviour
{
    public static event System.Action<string, string> OnButtonPressed;

    private string _speakerText = "";
    private string _messageText = "";

    [Rpc(SendTo.Me)]
    public void MeRpc()
    {
        SetDebugMessage("[ME]", "Probably just use a local function instead.");
    }

    [Rpc(SendTo.Server)]
    public void ServerRpc()
    {
        SetDebugMessage("[SERVER]", "Requesting change to server's authoritative clock!");
    }

    [Rpc(SendTo.Everyone)]
    public void EveryoneRpc()
    {
        SetDebugMessage("[EVERYONE]", "Send Happy New Year's message to everyone!");
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ClientsAndHostRpc()
    {
        SetDebugMessage("[CLIENTS AND HOST]", "Trigger fireworks for all players!");
    }

    public void SendGiftToPlayer()
    {
        Player[] players = FindObjectsByType<Player>();

        if (players.Length == 1)
        {
            SetDebugMessage("[WHOOPS]", "No one to send gifts to :(");
            return;
        }


        foreach (Player player in players)
        {
            if (player.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                player.OwnerRpc(NetworkManager.Singleton.LocalClientId);
                return;
            }
        }
    }

    public void SendPrivateMessage()
    {
        SpecifiedInParamsRpc(RpcTarget.Single(0, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SpecifiedInParamsRpc(RpcParams rpcParams = default)
    {
        SetDebugMessage("[SPECIFIED IN PARAMS]", "Send a personalized message to the player who logged the most hours in the year.");
    }

    private void SetDebugMessage(string speaker, string message)
    {
        _speakerText = speaker;
        _messageText = message;

        OnButtonPressed?.Invoke(_speakerText, _messageText);
    }
}
