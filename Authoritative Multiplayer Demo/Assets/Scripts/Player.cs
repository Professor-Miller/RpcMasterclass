using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class Player : NetworkBehaviour
{
    private static int nextPlayerNumber = 1;

    [SerializeField] private TextMeshProUGUI nameDisplay;

    private const int AttackDamage = 10;

    // SERVER AUTHORITY:
    // Everyone can read these values, but only the server can change them.
    public NetworkVariable<int> Health = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<FixedString64Bytes> Username = new(new FixedString64Bytes(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public static event System.Action<string, string> OnGiftReceived;
    public static event System.Action<string, string> OnAttackRequested;
    public static event System.Action<string, string> OnPlayerDeath;


    // CLIENT: Request an attack against another player.
    void OnMouseDown()
    {
        if (!IsClient || !IsSpawned)
            return;

        if (IsLocalPlayer)
            return;

        RequestAttackRpc();
    }


    public override void OnNetworkSpawn()
    {
        Username.OnValueChanged += OnUsernameChanged;

        if (IsServer)
        {
            Username.Value = $"Player {nextPlayerNumber}";
            nextPlayerNumber++;
        }

        if (IsClient)
        {
            UpdateUsernameText(Username.Value);
        }
    }


    public override void OnNetworkDespawn()
    {
        Username.OnValueChanged -= OnUsernameChanged;

        if (IsServer)
            nextPlayerNumber = 1;
    }


    // SERVER AUTHORITY:
    // Only the server is allowed to trigger gift notifications.
    [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
    public void OwnerRpc(ulong senderID)
    {
        OnGiftReceived?.Invoke("[OWNER]", $"received gift from Player {senderID + 1}");
    }


    // CLIENT REQUEST -> SERVER DECISION
    // Anyone can request an attack against this player.
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestAttackRpc(RpcParams rpcParams = default)
    {
        // SERVER: Identify who actually sent the request.
        ulong attackerId = rpcParams.Receive.SenderClientId;

        // SERVER: Validate the request.
        if (attackerId == OwnerClientId)
            return;

        if (Health.Value <= 0)
            return;

        // SERVER: Determine and apply damage.
        Health.Value = Mathf.Max(0, Health.Value - AttackDamage);

        OnAttackRequested?.Invoke("[SERVER]", $"Client {attackerId} attacked {Username.Value}. Health: {Health.Value}");

        if (Health.Value <= 0)
        {
            KillPlayer();
        }
    }


    // OWNERSHIP:
    // Only the owner can request their name to be changed.
    // The server still performs the actual change.
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestNameChangeRpc(string value)
    {
        // SERVER: Validate the requested name.
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (value.Length > 30)
            return;

        // SERVER: Authorize the change.
        Username.Value = new FixedString64Bytes(value);
    }


    // SERVER -> EVERYONE
    // Only the server can announce a player's death.
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server)]
    private void NotifyPlayersOfDeathRpc()
    {
        OnPlayerDeath?.Invoke("[CLIENTS AND HOST]", $"{Username.Value} has perished a painful death!");
    }


    private void OnUsernameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        UpdateUsernameText(newValue);
    }


    private void UpdateUsernameText(FixedString64Bytes username)
    {
        nameDisplay.text = username.ToString();
    }


    // SERVER AUTHORITY:
    // Only the server can remove a player from the network.
    private void KillPlayer()
    {
        if (!IsServer)
            return;

        NotifyPlayersOfDeathRpc();

        NetworkObject.Despawn();
    }
}
