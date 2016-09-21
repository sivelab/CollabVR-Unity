using UnityEngine;
using UnityEngine.Networking;
using VoiceChat.Demo;
using VoiceChat.Demo.HLAPI;
using VoiceChat.Networking;
using System.Linq;

// See link below for muptiple player prefab setup
// http://forum.unity3d.com/threads/how-to-set-individual-playerprefab-form-client-in-the-networkmanger.348337/#post-2256378
public class AppNetworkManager : VoiceChatNetworkManager
{
    public class PrefabMessage : MessageBase
    {
        public short prefabIndex;
    }

    [Header("UI")]
    [SerializeField]
    private bool m_useServerUI = false;
    [SerializeField]
    private bool m_useClientUI = false;

    private const short PlayerPrefabMessageType = MsgType.Highest + 1;

    private short m_chosenPrefabIndex;
    public void SetPlayerPrefab(GameObject prefab)
    {
        // find the prefab index from the gameobject
        m_chosenPrefabIndex = (short)spawnPrefabs.FindIndex(item => item.name == prefab.name);
        //Debug.Log("Using index: " + m_chosenPrefabIndex);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        var message = extraMessageReader.ReadMessage<PrefabMessage>();
        //NetworkServer.SendToClient(conn.connectionId, PlayerPrefabMessageType, message);
        var player = Instantiate(spawnPrefabs[message.prefabIndex]) as GameObject;
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    public override void OnClientConnect(NetworkConnection connection)
    {
        var message = new PrefabMessage();
        message.prefabIndex = m_chosenPrefabIndex;
        ClientScene.AddPlayer(connection, 0, message);
    }

    public override void OnStartClient(NetworkClient client)
    {
        VoiceChatNetworkProxy.OnManagerStartClient(client);

        if (m_useClientUI)
        {
            gameObject.AddComponent<VoiceChatUi>();
        }
    }

    public override void OnStopClient()
    {
        VoiceChatNetworkProxy.OnManagerStopClient();

        if (client != null && m_useClientUI)
        {
            Destroy(GetComponent<VoiceChatUi>());
        }
    }

    public override void OnStartServer()
    {
        // setup voice chat
        VoiceChatNetworkProxy.OnManagerStartServer();
        // setup UI
        if (m_useServerUI)
        {
            gameObject.AddComponent<VoiceChatServerUi>();
        }
    }

    public override void OnStopServer()
    {
        // stop voice chat
        VoiceChatNetworkProxy.OnManagerStopServer();

        // remove UI
        if (m_useServerUI)
        {
            Destroy(GetComponent<VoiceChatServerUi>());
        }
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        //base.OnClientSceneChanged(conn);
    }

    public void StartGame()
    {
        singleton.StartHost();
    }

    public void JoinGame()
    {
        singleton.StartClient();
    }

    public void ServeGame()
    {
        singleton.StartServer();
    }
}