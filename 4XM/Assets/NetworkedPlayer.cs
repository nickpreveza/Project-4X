using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using SignedInitiative;

public class NetworkedPlayer : NetworkBehaviour
{
    public int playerIndex;
    public Player player;
    public NetworkTurnData localTurnData;
    public NetworkVariable<NetworkTurnData> turnData = new NetworkVariable<NetworkTurnData>(
        new NetworkTurnData
        {
            turnCount = 1,
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer && IsOwner) //Only send an RPC to the server on the client that owns the NetworkObject that owns this NetworkBehaviour instance
        {
            TestServerRpc(0, NetworkObjectId);
        }

        if (Initializer.Instance != null)
        {
            Initializer.Instance.Subscribe(this);
        }
    }

    public void Update()
    {
        if (!IsOwner) { return; }

        if (Input.GetKeyDown(KeyCode.A))
        {
            //turnData.Value.turnCount = 1;
            Debug.Log(turnData);
        }
    }

    [ClientRpc]
    void TestClientRpc(int value, ulong sourceNetworkObjectId)
    {
        Debug.Log($"Client Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
        if (IsOwner) //Only send an RPC to the server on the client that owns the NetworkObject that owns this NetworkBehaviour instance
        {
            TestServerRpc(value + 1, sourceNetworkObjectId);
        }
    }

    [ServerRpc]
    void TestServerRpc(int value, ulong sourceNetworkObjectId)
    {
        Debug.Log($"Server Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
        TestClientRpc(value, sourceNetworkObjectId);
    }
}

public struct NetworkTurnData : INetworkSerializable
{
    public int turnCount;
    public int testInt;
    public bool testBool;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T: IReaderWriter
    {
        serializer.SerializeValue(ref turnCount);
        serializer.SerializeValue(ref testInt);
        serializer.SerializeValue(ref testBool);
    }
}
