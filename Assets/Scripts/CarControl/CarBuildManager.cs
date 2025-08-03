using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientInfo;
using Unity.VisualScripting;
public class CarBuildManager : MonoBehaviour
{
    public bool isCarInitComplete = false;
    private ScoreManager scoreManager;
    private ScoreSender scoreSender;
    private CameraManager cameraManager;
    private FlipScoreManager flipScoreManager;
    private GameManager_main gameManager_Main;
    private DeathHeightController deathHeightController;
    private DeathHeightController trackFogController;
    private InfiniteFloorManager infiniteFloorManager;
    private GameObject myCar;
    private string nickname;
    private int equippedCarId;
    private int equippedDieEffectId;
    private int equippedTrailId;
    [SerializeField] private Transform startPoint;
    [SerializeField] private GameObject startCylinder;
    [SerializeField] private List<GameObject> CarKindPrefabs = new();
    [SerializeField] private List<GameObject> DieEffectPrefabs = new();
    [SerializeField] private List<GameObject> TrailEffectPrefabs = new();
    void Start()
    {
        isCarInitComplete = false;

        // 오직 초기화 완료 시점에만 Car 빌드하게 유지
        ItemManagerInitializer.OnAllItemManagersInitialized += HandleItemDataReady;
        BaseItemManager<int>.OnItemEquippedGlobal += HandleItemEquipped;

        scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        scoreSender = GameObject.Find("ScoreManager").GetComponent<ScoreSender>();
        cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        gameManager_Main = GameObject.Find("GameManager").GetComponent<GameManager_main>();
        flipScoreManager = GameObject.Find("ScoreManager").GetComponent<FlipScoreManager>();
        deathHeightController = GameObject.Find("DeathHeight").GetComponent<DeathHeightController>();
        trackFogController = GameObject.Find("Track_Fog").GetComponent<DeathHeightController>();
        infiniteFloorManager = GameObject.Find("InfiniteFloorManager").GetComponent<InfiniteFloorManager>();
    }
    public void DownloadUserInfo()
    {

    }
    private void BuildACar(GameObject playerObject, int dieEffectId, int trailId, bool isCarKind)
    {
        Transform death;
        Transform trail;
        death = playerObject.transform.Find("Death");
        trail = playerObject.transform.Find("Trail");
        if (!isCarKind)
        {
            foreach (Transform child in death)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in trail)
            {
                Destroy(child.gameObject);
            }
        }
        var deathEffect = Instantiate(DieEffectPrefabs[dieEffectId], death);
        if (trailId != 0)
        {
            playerObject.GetComponentInChildren<TrailRenderer>().enabled = false;
            playerObject.GetComponent<CarEffectController>().isTrailDefault = false;
            var trailEffect = Instantiate(TrailEffectPrefabs[trailId], trail);
        }
        else
        {
            playerObject.GetComponent<CarEffectController>().isTrailDefault = true;
            playerObject.GetComponent<TrailRenderer>().enabled = true;
        }
    }

    private void HandleItemEquipped(ShopItemType type, int id)
    {
        isCarInitComplete = false;
        var (nickname, equippedCarId, equippedDieEffectId, equippedTrailId) = UserDataManager.Instance.GetUserMultiplayInfo();

        switch (type)
        {
            case ShopItemType.CarKind:
                Debug.Log($"Car 장착됨: {id}");
                StartCoroutine(ReplaceCarAfterDestroy(id, equippedDieEffectId, equippedTrailId));
                break;

            case ShopItemType.Trail:
                Debug.Log($"Trail 장착됨: {id}");
                BuildACar(myCar, equippedDieEffectId, id, false);
                myCar.GetComponent<CarEffectController>().BindingEffects();
                isCarInitComplete = true;
                break;

            case ShopItemType.DieEffect:
                Debug.Log($"DieEffect 장착됨: {id}");
                BuildACar(myCar, id, equippedTrailId, false);
                myCar.GetComponent<CarEffectController>().BindingEffects();
                isCarInitComplete = true;
                break;
        }
    }
    private IEnumerator ReplaceCarAfterDestroy(int id, int dieId, int trailId)
    {
        if (myCar != null)
        {
            Destroy(myCar);
            while (myCar != null)
                yield return null; // 완전히 파괴될 때까지 기다림
            Debug.Log("[CBM] Destroy 완료됨");
        }

        myCar = Instantiate(CarKindPrefabs[id], startPoint);
        Debug.Log($"[CBM] Instantiate Car id:{id}");

        BuildACar(myCar, dieId, trailId, true);
        myCar.GetComponent<CarEffectController>().BindingEffects();
        myCar.GetComponent<Car1Controller>().StartPoint = startPoint;
        myCar.GetComponent<Car1Controller>().StartCylinder = startCylinder;
        BindingManagers();

        isCarInitComplete = true;
    } 
    private void BindingManagers()
    {
        gameManager_Main.CarRigidBody = myCar.GetComponent<Rigidbody>();
        gameManager_Main.myCar = myCar;
        infiniteFloorManager.Car = myCar.transform;
        deathHeightController.car = myCar;
        trackFogController.car = myCar;
        myCar.GetComponent<SkidMarker>().InitSkidMarker();

        var dieCheck = myCar.GetComponent<CarDieCheck>();
        dieCheck.isMulti = false;
        dieCheck.canvas = GameObject.Find("Canvas");
        dieCheck.flipScoreManager = flipScoreManager;
        dieCheck.trackManager = GameObject.Find("TrackManager").GetComponent<TrackManager>();
        dieCheck.flipText = GameObject.Find("ScoreManager").GetComponent<FlipText>();
        dieCheck.OnResetCamToStart += cameraManager.ResetToStart;
        dieCheck.OnCamToStart += cameraManager.GameStart;
        dieCheck.OnDieCam += cameraManager.SwitchToDeathCam;
        dieCheck.OnResetCarPos += myCar.GetComponent<Car1Controller>().ResetCarPos;
        dieCheck.OnResetFloor += infiniteFloorManager.Init;

        cameraManager.InitializeCarAndCamManager(myCar, false);
        scoreManager.Initializing(myCar);
        flipScoreManager.InitFilpScoreManager(myCar);

    }
    private void HandleItemDataReady()
    {
        var (nickname, equippedCarId, equippedDieEffectId, equippedTrailId) = UserDataManager.Instance.GetUserMultiplayInfo();
        HandleItemEquipped(ShopItemType.CarKind, equippedCarId);
        Debug.Log($"[CBM] Car 정보: carKind: {equippedCarId} / die: {equippedDieEffectId} / trail: {equippedTrailId}");

        // 이벤트 등록 제거 (한 번만 작동)
        ItemManagerInitializer.OnAllItemManagersInitialized -= HandleItemDataReady;
    }
    void OnDestroy()
    {
        BaseItemManager<int>.OnItemEquippedGlobal -= HandleItemEquipped;
    }
}
