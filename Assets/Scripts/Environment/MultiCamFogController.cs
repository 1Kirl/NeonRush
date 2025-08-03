using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class MultiCamFogController : MonoBehaviour
{
    public GameObject cam;
    [SerializeField] private float _zOffset;
    [SerializeField] private float _yOffset;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (cam != null)
        {
            this.transform.position = new Vector3(
            cam.transform.position.x
            , cam.transform.position.y + _yOffset
            , cam.transform.position.z + _zOffset);
        }
    }
    // [SerializeField] private float _zOffset;
    // [SerializeField] private float _yOffset;
    // [SerializeField] Transform camTransform;

    // private CinemachineBrain brain;

    // void Start()
    // {
    //     brain = Camera.main.GetComponent<CinemachineBrain>();
    //     ICinemachineCamera initialCam = brain.ActiveVirtualCamera;
    //     camTransform = initialCam.VirtualCameraGameObject.transform;
    //     if (brain != null)
    //     {
    //         brain.m_CameraActivatedEvent.AddListener(OnCameraActivated);
    //     }
    // }

    // void OnDestroy()
    // {
    //     if (brain != null)
    //     {
    //         brain.m_CameraActivatedEvent.RemoveListener(OnCameraActivated);
    //     }
    // }

    // private void OnCameraActivated(ICinemachineCamera fromCam, ICinemachineCamera toCam)
    // {
    //     StartCoroutine(UpdateCamTransformNextFrame());
    // }

    // private IEnumerator UpdateCamTransformNextFrame()
    // {
    //     // 한 프레임 대기
    //     yield return null;

    //     var activeCam = brain.ActiveVirtualCamera;
    //     if (activeCam != null)
    //     {
    //         camTransform = activeCam.VirtualCameraGameObject.transform;
    //         Debug.Log($"[Cinemachine] 정확히 전환된 카메라: {activeCam.Name}, Transform: {camTransform.position}");
    //     }
    // }

    // void FixedUpdate()
    // {
    //     if (camTransform != null)
    //     {
    //         this.transform.position = new Vector3(
    //         camTransform.transform.position.x
    //         , camTransform.transform.position.y + _yOffset
    //         , camTransform.transform.position.z + _zOffset);
    //     }
    // }
}
