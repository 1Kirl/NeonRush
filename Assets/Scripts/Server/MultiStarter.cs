using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared.Network;
using System;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
public class MultiStarter : MonoBehaviour
{
    private ScoreManager scoreManager;
    private ScoreSender scoreSender;
    private CameraManager cameraManager;
    private FlipScoreManager flipScoreManager;
    public Transform reStartTransform;
    private int totalPlayer = 0;
    private GameObject myCar;
    [SerializeField] private List<GameObject> CarKindPrefabs = new();
    [SerializeField] private List<GameObject> CarKindPrefabs_Other = new();
    [SerializeField] private List<GameObject> DieEffectPrefabs = new();
    [SerializeField] private List<GameObject> TrailEffectPrefabs = new();
    [SerializeField] private Dictionary<int ,GameObject> Players = new(); //clientId, GameObject
    [SerializeField] private List<Transform> spawnTransform = new();
    [SerializeField] private Dictionary<int, clientData> inGameClients = new();
    private PlayableDirector introDirector;
    public event Action InitializeComplete;
    void Start()
    {
        Debug.Log("[MultiStarter] totalPlayer number received: " + totalPlayer);
    }
    public void MultiStarterInit()
    {
        scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        scoreSender = GameObject.Find("ScoreManager").GetComponent<ScoreSender>();
        cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        flipScoreManager = GameObject.Find("ScoreManager").GetComponent<FlipScoreManager>();
        introDirector = GameObject.Find("TrackIntroDirector").GetComponent<PlayableDirector>();
        LiteNetLibManager.Instance.introDirector = introDirector;
        LiteNetLibManager.Instance.OnGameStart += GameStart;
        LiteNetLibManager.Instance.OnStareCamOff += StareCamOff;
        totalPlayer = LiteNetLibManager.Instance.totalPlayer;
        Debug.Log("[MultiStarter] totalPlayer number received: " + totalPlayer);

    }
    private void StareCamOff()
    {
        cameraManager.StareCamOff(); // stareCam unlive -> possibly normal cam
    }
    private void GameStart()
    {
        Debug.Log("[MultiStarter] Invoke Received, GameStart, made Kinematic false");
        //Bind player to ScoreManager
        scoreManager.Initializing(myCar);
        scoreManager.StartTracking();
        myCar.GetComponent<CarEffectController>().ResetTrail();
        myCar.GetComponent<CarDieCheck>().isGameStarted = true;
        cameraManager.GameStart();
        myCar.GetComponent<PlayerInput>().enabled = true;
    }
    private void BuildACar(GameObject playerObject, int dieEffectId, int trailId, bool isOther)
    {
        Transform death;
        Transform trail;
        if (!isOther)
        {
            death = playerObject.transform.Find("Death");
            trail = playerObject.transform.Find("Trail");
        }
        else
        {
            Transform effect = playerObject.transform.Find("Effects");
            death = effect.transform.Find("Death");
            trail = effect.transform.Find("Trail");
        }

        var deathEffect = Instantiate(DieEffectPrefabs[dieEffectId], death);
        if (trailId != 0)
        {
            playerObject.GetComponentInChildren<TrailRenderer>().enabled = false;
            if (!isOther)
            {
                playerObject.GetComponent<CarEffectController>().isTrailDefault = false;
            }
            else
            {
                playerObject.GetComponent<OtherCarEffectController>().isTrailDefault = false;
            }
            var trailEffect = Instantiate(TrailEffectPrefabs[trailId], trail);
        }
    }
    private void InitializeCars(Dictionary<int, clientData> clients)
    {

        inGameClients = clients;
        int i = 0;
        
        foreach (KeyValuePair<int, clientData> client in clients)
        {
            //Instantiate prefabs and Set transform
            if (client.Value.isMe)
            {
                var playerObject = Instantiate(CarKindPrefabs[client.Value.carKind],
                spawnTransform[i].position,
                spawnTransform[i].rotation);
                Players.Add(client.Key, playerObject);
                //Bind Particle Effects
                BuildACar(playerObject, client.Value.DieEffect, client.Value.Trail, false);
                playerObject.GetComponent<CarEffectController>().BindingEffects();
            }
            else
            {
                var playerObject = Instantiate(CarKindPrefabs_Other[client.Value.carKind],
                spawnTransform[i].position,
                spawnTransform[i].rotation);
                Players.Add(client.Key, playerObject);
                //Bind Particle Effects
                BuildACar(playerObject, client.Value.DieEffect, client.Value.Trail, true);
                playerObject.GetComponent<OtherCarEffectController>().BindingEffects();
            }

            if (client.Value.isMe)
            {
                Debug.Log($"[MultiStarter] identify myCar, id: {client.Key}");
                myCar = Players[client.Key];
                LiteNetLibManager.Instance.local_inputReceiver = myCar.GetComponent<Local_InputReceiver>();
                LiteNetLibManager.Instance.carTransform = myCar.transform;
                // Die Check Event Binding
                var dieCheck = myCar.GetComponent<CarDieCheck>();
                dieCheck.isMulti = true;
                dieCheck.flipScoreManager = flipScoreManager;
                dieCheck.flipText = GameObject.Find("ScoreManager").GetComponent<FlipText>();
                dieCheck.OnResetCamToStart += cameraManager.ResetToStart;
                dieCheck.OnCamToStart += cameraManager.GameStart;
                dieCheck.OnDieCam += cameraManager.SwitchToDeathCam;
                dieCheck.OnResetCarPos += myCar.GetComponent<Car1Controller>().ResetCarPos;
                GameObject resqawn = GameObject.Find("RespawnPoints");
                dieCheck.reSpawnPoints = resqawn.transform.GetComponentsInChildren<Transform>()
                                                .Where(t => t != resqawn.transform)
                                                .ToArray();
                myCar.GetComponent<PlayerInput>().enabled = false;                                
            }
            else
            {
                Debug.Log($"[MultiStarter] identify otherCar, id: {client.Key}");
                //Link Input Receiver
                Players[client.Key].GetComponent<InputReceiver>().ClientId = client.Key;
                Players[client.Key].GetComponent<OtherCarEffectController>().ClientId = client.Key;

                Players[client.Key].GetComponent<TransformUpdateController>().ClientId = client.Key;
                Players[client.Key].GetComponent<WheelAnimationController>().BindInputReceiver();
            }
            i++;
        }
        //unlock Kinematic
        foreach (KeyValuePair<int, GameObject> car in Players)
        {
            if (car.Value.GetComponent<Rigidbody>() != null)
            {
                car.Value.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
        //Bind player to CameraManager ->WheelColliders, Car, Car1Controller
        //Bind player to Cams -> NormalCarFollowCam ~ ... including fog
        cameraManager.InitializeCarAndCamManager(myCar, true); // stareCam live
        flipScoreManager.InitFilpScoreManager(myCar);
        myCar.GetComponent<SkidMarker>().InitSkidMarker();
        LiteNetLibManager.Instance.OnInitializeComplete();
    }
    public void InitializeMultiGame(Dictionary<int, clientData> clients)
    {
        Debug.Log("[MultiStarter] Initialize Start");
        InitializeCars(clients);
        Debug.Log("[MultiStarter] Initial complete");
        LiteNetLibManager.Instance.isGameStarted = true;
    }
}
