using Unity.Collections;
using Unity.Netcode;

using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{

    [SerializeField] Transform spawnShpere;
    Transform spawnSphereTrasform;

    NetworkVariable<myCustomData> randomNumber = new NetworkVariable<myCustomData>(
        (new myCustomData
        {
            _int = 56,
            _bool = true,
        }), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct myCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (myCustomData perviousValue, myCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + " :" + newValue._int + ":" + newValue._bool + ":" + newValue.message);
        };

    }
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }


        if (Input.GetKeyUp(KeyCode.T))
        {
            spawnSphereTrasform = Instantiate(spawnShpere);
            spawnSphereTrasform.GetComponent<NetworkObject>().Spawn(true);


            // TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } });
            /* randomNumber.Value = new myCustomData {
                 _int = 10,
                 _bool = true,
                 message = " all are wecome here"
             };*/
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            spawnSphereTrasform.GetComponent<NetworkObject>().Despawn();
        }



        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) { moveDir.z = -1f; }
        if (Input.GetKey(KeyCode.S)) { moveDir.z = +1f; }
        if (Input.GetKey(KeyCode.A)) { moveDir.x = +1f; }
        if (Input.GetKey(KeyCode.D)) { moveDir.x = -1f; }

        float moveSpeed = 3f;
        transform.position += (moveDir * moveSpeed) * Time.deltaTime;

    }

    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams)
    {
        Debug.Log("TestServerRpc" + OwnerClientId + ":" + serverRpcParams.Receive.SenderClientId);
    }
    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams)
    {
        Debug.Log("ClentServer");
    }


}





