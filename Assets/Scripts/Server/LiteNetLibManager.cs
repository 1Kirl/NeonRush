using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;
using System.Net.Sockets;
using System;
using Shared.Network;
using Shared.Protocol;
using Shared.Bits;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;
using System.Linq;
using BackEnd;
using UnityEngine.PlayerLoop;
using UnityEngine.Playables;
using System.Data;

public class LiteNetLibManager : MonoBehaviour, INetEventListener
{
    public static LiteNetLibManager Instance;
    public Dictionary<int, clientData> clientsDic = new(); //client id, clientData
    private MainSceneManager mainSceneManager;
    private MultiStarter multiStarter;
    private WaitingUIManager waitingUIManager;
    private RankingUIManager rankingUIManager;
    public int totalPlayer = 0;
    private NetManager client;
    private NetPeer serverPeer;
    private NetDataWriter writer;
    public Local_InputReceiver local_inputReceiver;
    public Transform carTransform;
    [SerializeField] public string serverSceneName = "";
    [SerializeField] private GameObject Button_StopWaiting;
    [SerializeField] public int inGameClientId = 0; // my Client Id
    [SerializeField] public float matchFoundDelay = 2f;
    [SerializeField] private bool isLocalTest = false;
    public bool connectionStart = false;
    private string myNickname = "test";
    private string serverAddress = "158.247.255.112";
    private int serverPort = 7777;
    private string connectionKey = "hsdbpc";
    private byte inputBits;
    private int input_tickCounter = 0;
    private int transform_tickCounter = 0;
    private float transform_tickTimer = 0f;
    private float input_tickTimer = 0f;
    public bool isGameStarted = false;
    private float _horizontalInput;
    private float _verticalInput;
    private const float INPUT_TICK_RATE = 1f / 20f; // 20Hz
    private const float TRANSFORM_TICK_RATE = 1f / 50f; // 50Hz 중간중간 스무딩 필요? 이전 속도 측정 후 스무딩 속도 결정로직
    private MapType mapType;
    public event Action<Dictionary<int, clientData>> OnInitializeMultiGame;
    public event Action OnGameStart;
    public event Action OnStareCamOff;
    public PlayableDirector introDirector;
    public List<int> myRanksPerRound = new List<int>();
    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start() {
        mainSceneManager = GameObject.Find("SceneManager").GetComponent<MainSceneManager>();
        waitingUIManager = GameObject.Find("UIManager").GetComponent<WaitingUIManager>();
        writer = new NetDataWriter();
        client = new NetManager(this);
        if (isLocalTest) {
            serverAddress = "localhost";
        }
    }

    void Update() {
        if (!connectionStart) {
            return;
        }
        client.PollEvents();
        if (isGameStarted) {
            transform_tickTimer += Time.deltaTime;
            input_tickTimer += Time.deltaTime;

            TickingInputSender();
            TickingTransformSender();
        }
    }

    private void TickingInputSender() {
        while (input_tickTimer >= INPUT_TICK_RATE) {
            input_tickTimer -= INPUT_TICK_RATE;

            PackingInput(
                local_inputReceiver.packet_horizontalInput,
                local_inputReceiver.packet_verticalInput,
                out inputBits);

            ClientMessageSender.SendPlayerInput(serverPeer, inputBits);
            input_tickCounter++;
        }
    }
    private void TickingTransformSender() {
        while (transform_tickTimer >= TRANSFORM_TICK_RATE) {
            transform_tickTimer -= TRANSFORM_TICK_RATE;
            ClientMessageSender.SendPlayerTransform(serverPeer, carTransform);
            transform_tickCounter++;
        }
    }
    public void StartConnection() {
        connectionStart = true;
        client.Start();
        client.Connect(serverAddress, serverPort, connectionKey);
        //waitingUIManager.PopUpWait();
    }
    public void StopFindingMatch() {
        ClientMessageSender.SendStopFinding(serverPeer);
    }
    public void ScoreUpdate(int currentScore) {
        ClientMessageSender.SendScoreToServer(currentScore);
    }
    // ----- INetEventListener 필수 메서드 구현 -----
    public void OnPeerConnected(NetPeer peer) {
        serverPeer = peer;

        // 유저의 장착 정보 모두 가져오기
        var (nickname, equippedCarId, equippedDieEffectId, equippedTrailId) = UserDataManager.Instance.GetUserMultiplayInfo();

        // 서버에 모든 정보 전달
        ClientMessageSender.SendLetsStartPvPGame(
            serverPeer,
            (ushort)equippedCarId,
            (ushort)equippedDieEffectId,
            (ushort)equippedTrailId,
            nickname
        );

        Debug.Log($"[Client] Connected to server: {nickname} / Car: {equippedCarId} / Effect: {equippedDieEffectId} / Trail: {equippedTrailId}");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
        // 1. 상태 플래그 리셋
        connectionStart = false;
        isGameStarted = false;
        inGameClientId = -1;

        // 2. 연결 종료
        client.Stop();

        // 3. 데이터 정리
        clientsDic.Clear();
        Debug.Log($"[Client] Disconnected. Reason: {disconnectInfo.Reason}");
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) {
        Debug.LogError($"[Client] Network Error: {socketError}");
    }
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod) {
        int start = reader.Position;
        int length = reader.AvailableBytes;

        if (length <= 0 || start + length > reader.RawData.Length) {
            Debug.Log("[Error] Invalid slice range: Position=" + start + ", Available=" + length);
            return;
        }

        byte[] data = new byte[length];
        Array.Copy(reader.RawData, start, data, 0, length);

        var bitReader = new BitReader(data);
        PacketType flag = (PacketType)bitReader.ReadBits(4);
        Debug.Log("Received: flag: " + flag + " / start = " + start + " / length = " + length);

        switch (flag) {
            case PacketType.MyInfo:
                inGameClientId = (int)bitReader.ReadBits(4);
                break;

            case PacketType.MatchFound:
                Button_StopWaiting.SetActive(false);
                Debug.Log("[LNLM] MatchFound");

                totalPlayer = (int)bitReader.ReadBits(4);

                reader.GetByte(); //dump
                clientData player = new clientData();
                player.clientId = (int)reader.GetUShort();
                player.carKind = (int)reader.GetUShort();
                player.DieEffect = (int)reader.GetUShort();
                player.Trail = (int)reader.GetUShort();
                player.name = reader.GetString();
                mapType = (MapType)reader.GetByte();
                if (player.clientId == inGameClientId) {
                    player.isMe = true;
                    myNickname = player.name;
                    Debug.Log($"[LNLM] Found my Id{player.isMe}");
                }
                clientsDic.Add(player.clientId, player);
                Debug.Log($"Game Member: total - {totalPlayer} / id-{player.clientId} / carKind - {player.carKind} / name - {player.name}");
                if (totalPlayer <= clientsDic.Count) {
                    //씬 구성 시작
                    Debug.Log("[LNLM] Change To PvP Scene");
                    StartCoroutine(ChangeSceneAfterDelay(mapType));
                }
                break;
            case PacketType.WaitingMember:
                totalPlayer = (int)bitReader.ReadBits(4);
                waitingUIManager.UpdateCurrentMembers(totalPlayer);
                break;

            case PacketType.PlayerInput:
                int clientId = (int)bitReader.ReadBits(4);
                DecodeAndSaveInput(clientId, (byte)bitReader.ReadBits(3));
                //Debug.Log($"[LNLM] Received: playerInput of clientId: {clientId}");
                break;
            case PacketType.Effect:
                int effect_clientId = (int)bitReader.ReadBits(4);
                DecodeAndSaveEffect(effect_clientId, (byte)bitReader.ReadBits(3));
                Debug.Log($"[LNLM] Received: Effect of clientId: {effect_clientId}");
                break;

            case PacketType.TransformUpdate:
                int tranfrom_clientId = (int)bitReader.ReadBits(4);
                reader.GetByte(); //dump
                Vector3 pos = new Vector3(
                reader.GetFloat(),
                reader.GetFloat(),
                reader.GetFloat()
                );

                Quaternion rot = new Quaternion(
                    reader.GetFloat(),
                    reader.GetFloat(),
                    reader.GetFloat(),
                    reader.GetFloat()
                );
                UpdateTransform(tranfrom_clientId, pos, rot);
                //Debug.Log($"[LNLM] Received: playerInput of clientId: {clientId}");
                break;

            case PacketType.GameStart:
                Debug.Log("[LNLM] Received: GameStart");
                reader.GetByte(); //dump

                long receivedStartTime = reader.GetLong();
                int playerCount = reader.GetInt();

                RankingUIManager.Instance.InitRankingUI(playerCount); // 초기화 호출

                var entryList = clientsDic
                    .Select(kvp => new RankingEntry(
                            (ushort)kvp.Value.clientId,
                            kvp.Value.name,
                            0))
                    .ToList();

                RankingUIManager.Instance.ShowInitialRanking(entryList);
                long localTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long delta = receivedStartTime - localTime;
                float delay = delta / 1000f;
                Debug.Log($"[LNLM] Delay is : {delay}");
                StartCoroutine(StartAfterDelay(delay));

                break;
            case PacketType.RankingsUpdate:
                reader.GetByte(); // dump
                HandleRankingUpdate(reader);
                break;

            case PacketType.ServerResultSummary:
                Debug.Log("[LNLM] Received: ServerResultSummary");
                reader.GetByte(); // dump padding if any
                HandleServerResultSummary(reader);
                break;

            case PacketType.CountdownStart:
                Debug.Log("[LNLM] Received: CountdownStart");
                reader.GetByte();
                long serverStartTime = reader.GetLong();
                long clientNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                float delayBeforeCountdown = (serverStartTime - clientNow) / 1000f;
                StartCoroutine(FinishCountdownManager.Instance.StartCountdownSequence(delayBeforeCountdown));
                break;

            default:

                break;
        }

        reader.Recycle();

    }

    public NetPeer GetServerPeer() {
        return serverPeer;
    }

    IEnumerator StartAfterDelay(float delay) {
        yield return StartCoroutine(WaitRealSeconds(delay));
        //cam move shots
        FadeManager.Instance.SequenceBeforeCountdown();
        if (introDirector != null) {
            introDirector.Play();
            Debug.Log("[LNLM] Timeline played.");
            yield return WaitRealSeconds((float)introDirector.duration);
        }
        OnStareCamOff?.Invoke();
        yield return StartCoroutine(FinishCountdownManager.Instance.StartStartCountdown()); // 3,2,1 카운트다운 실행
        Debug.Log("[LNLM] delay passed, GameStart Invoke");
        OnGameStart?.Invoke();
        isGameStarted = true; // start tick count
    }
    IEnumerator ChangeSceneAfterDelay(MapType mapType) {
        waitingUIManager.UpdateMatchFound();
        yield return StartCoroutine(WaitRealSeconds(matchFoundDelay));
        serverSceneName = mainSceneManager.ChangeToMulti(mapType);
        SceneManager.sceneLoaded += OnMultiSceneLoaded;
    }

    private void OnMultiSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == serverSceneName) {
            Debug.Log("[LNLM] MultiScene Loaded");
            multiStarter = GameObject.Find("MultiGameManager").GetComponent<MultiStarter>();
            rankingUIManager = GameObject.Find("RankingManager_Multi").GetComponent<RankingUIManager>();
            multiStarter.MultiStarterInit();
            multiStarter.InitializeMultiGame(clientsDic);
            //OnInitializeMultiGame.Invoke(clientsDic);
            // 이벤트 중복 방지를 위해 꼭 해제
            SceneManager.sceneLoaded -= OnMultiSceneLoaded;
        }
    }
    public void OnInitializeComplete() {
        Debug.Log("[LNLM] Received OnInitializeComplete, Send ClientIsReady! ");
        ClientMessageSender.SendClientIsReady(serverPeer);
    }
    public void SendEffectSignal(EffectType effectType) {
        ClientMessageSender.SendPlayerEffect(serverPeer, effectType);
    }
    private void PackingInput(float horizontalInput, float verticalInput, out byte inputBits) {
        inputBits = 0;
        // V / H : input  
        // [7] [6] [5] [4] [3] [2] [1] [0]
        //  0   0   0   0   0   H   H   V
        inputBits |= (byte)(EncodeInput(true, horizontalInput) << 1);
        inputBits |= (byte)(EncodeInput(false, verticalInput));
        Debug.Log("inputBits: " + (int)inputBits);
    }
    private byte EncodeInput(bool hOrV, float input) {
        if (hOrV) {
            if (input < -0.5f) return 0b10;
            if (input > 0.5f) return 0b01;
            return 0b00;
        }
        else {
            if (input > 0.5f) return 0b1;
            return 0b0;
        }

    }
    private void DecodeAndSaveInput(int clientId, byte inputBits) {

        // 마지막 1비트 (0): vertical
        clientsDic[clientId].verticalInput = ((inputBits & 0b1) == 1) ? 1f : 0f;

        // 하위 2비트 (1~2): horizontal
        byte hBits = (byte)((inputBits >> 1) & 0b11);
        clientsDic[clientId].horizontalInput = DecodeDirection(hBits);
        clientsDic[clientId].InvokeInputEvent();
        //Debug.Log($"[LNLM] Received: PlayerInput id: {clientId} / h: {clientsDic[clientId].horizontalInput}");
    }
    private void DecodeAndSaveEffect(int clientId, byte effectData) {
        clientsDic[clientId].InvokeEffectEvent((EffectType)effectData);
        Debug.Log($"[LNLM] Received: Effect player id: {clientId} / effect: {(EffectType)effectData}");
    }
    private void UpdateTransform(int clientId, Vector3 pos, Quaternion rot) {
        clientsDic[clientId].pos = pos;
        clientsDic[clientId].rot = rot;
        clientsDic[clientId].InvokeTransformEvent();
        Debug.Log($"[LNLM] Received: TransformUpdate id: {clientId}");
    }

    private float DecodeDirection(byte value) {
        return value switch {
            0b10 => -1f,
            0b00 => 0f,
            0b01 => 1f,
            _ => 0f // _ -> rest of the cases
        };
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) {
        // 사용 안 함
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) {
        //Debug.Log($"[Client] Latency: {latency}ms");
    }

    public void OnConnectionRequest(ConnectionRequest request) {
        // 클라이언트에서는 안 씀
    }

    private void HandleRankingUpdate(NetPacketReader reader) {
        int count = reader.GetInt(); // 플레이어 수
        List<RankingEntry> list = new();

        for (int i = 0; i < count; i++) {
            ushort clientId = reader.GetUShort();
            ushort score = reader.GetUShort();
            string name = reader.GetString();

            list.Add(new RankingEntry(clientId, name, score));
        }
        rankingUIManager.UpdateRankingUI(list);
    }

    private void HandleServerResultSummary(NetPacketReader reader) {
        int count = reader.GetInt();
        List<ResultEntry> results = new();

        for (int i = 0; i < count; i++) {
            ushort clientId = reader.GetUShort();
            string name = reader.GetString();
            ushort baseScore = reader.GetUShort();
            byte arrivalRank = reader.GetByte();
            ushort finalScore = reader.GetUShort();
            var entry = new ResultEntry(clientId, name, baseScore, finalScore, arrivalRank);
            results.Add(entry); // 보너스는 잠시 뒤에 계산
        }

        RankingUIManager.Instance.SetInitialResultUI(results);
        // 2. 약간의 텀 후에 보너스 계산 + 애니메이션 + Gameover 연동!
        StartCoroutine(HandleResultSequenceWithGameover(results));
    }


    private IEnumerator HandleResultSequenceWithGameover(List<ResultEntry> results) {

        yield return StartCoroutine(WaitRealSeconds(2f));

        // 정렬(등수, 점수 등) 후 1등 판별
        var sortedResults = results.OrderByDescending(r => r.BonusScore).ToList();

        if (sortedResults.Count > 0 && sortedResults[0].ClientId == inGameClientId) {
            int wins = PlayerPrefs.GetInt("win_count", 0) + 1;
            PlayerPrefs.SetInt("win_count", wins);
            PlayerPrefs.Save();

            Debug.Log($"[업적] 1등 승리 업적 증가 → {wins}");
            UpdateWinCountToBackend(wins);
        }

        // UI 애니메이션/보상 등 처리
        RankingUIManager.Instance.ShowResultWithBonus(sortedResults);
        yield return StartCoroutine(WaitRealSeconds(5f));
        ShowMyTierScoreResult(sortedResults);
    }

    void OnDestroy() {
        client.Stop();
        clientsDic.Clear();           // CLient inform initialize
        isGameStarted = false;        // Game Start State Reset.
        inGameClientId = -1;          // My Client ID initialize.
    }


    IEnumerator WaitRealSeconds(float seconds) {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < seconds)
            yield return null;
    }
    private void UpdateWinCountToBackend(int winCount) {
        Param param = new Param();
        param.Add("win_count", winCount);

        Where where = new Where();
        where.Equal("owner_inDate", BackEnd.Backend.UserInDate);

        BackEnd.Backend.GameData.Update("user_data", where, param, callback => {
            if (callback.IsSuccess())
                Debug.Log("[서버] win_count 저장 완료");
            else
                Debug.LogWarning("[서버] win_count 저장 실패: " + callback);
        });
    }
    private void ShowMyTierScoreResult(List<ResultEntry> results) {
        int myId = inGameClientId;
        // BonusScore로 내림차순 정렬 (최종 점수 랭킹)
        var sortedResults = results.OrderByDescending(r => r.BonusScore).ToList();

        // 내 순위(1등=1)
        int myRank = sortedResults.FindIndex(r => r.ClientId == myId) + 1;
        if (myRank > 0) {
            TierScoreResultUI.Instance.AnimateAndUpdateTierScore(myRank);
        }
    }
}
