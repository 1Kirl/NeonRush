using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class ServerTestClient : MonoBehaviour
{
    private NetworkDriver driver;
    private NetworkConnection connection;
    private bool isConnected = false;
    private float sendTimer = 0f;
    private float sendInterval = 1f / 40f; //

    public string serverIP = "158.247.255.112";
    public ushort serverPort = 7777;

    void Start()
    {
        driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.Parse(serverIP, serverPort);
        connection = driver.Connect(endpoint);
        Debug.Log("서버에 연결 시도 중...");
    }

    void Update()
    {
        driver.ScheduleUpdate().Complete();

        if (!connection.IsCreated)
            return;

        NetworkEvent.Type cmd;
        DataStreamReader stream;

        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            switch (cmd)
            {
                case NetworkEvent.Type.Connect:
                    Debug.Log("Connected to Server Successfully!");
                    //Connection
                    SendByteData(0x42);
                    break;

                case NetworkEvent.Type.Data:
                    HandleReceivedData(stream);
                    break;

                case NetworkEvent.Type.Disconnect:
                    Debug.Log("Disconnect to Server..");
                    connection = default;
                    break;
            }
        }
        if (isConnected)
        {
            sendTimer += Time.deltaTime;

            if (sendTimer >= sendInterval)
            {
                SendByteData(0x42);
                sendTimer = 0f;
            }
        }
    }

    void SendByteData(byte value)
    {
        DataStreamWriter writer;
        if (driver.BeginSend(NetworkPipeline.Null, connection, out writer) == 0)
        {
            writer.WriteByte(value);
            driver.EndSend(writer);
            Debug.Log("Send Data: " + value);
        }
        else
        {
            Debug.LogWarning("failed to send data (BeginSend fail)");
        }
    }

    void HandleReceivedData(DataStreamReader stream)
    {
        byte received = stream.ReadByte();
        Debug.Log("Receive from server: " + received);
    }

    void OnDestroy()
    {
        driver.Dispose();
    }
}
