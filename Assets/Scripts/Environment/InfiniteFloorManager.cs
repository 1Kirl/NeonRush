using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteFloorManager : MonoBehaviour
{
    [SerializeField] private float thresholdDistance = 30f;     // 자동차가 벗어나야 할 거리
    [SerializeField] private float spawnOffset = 100f;          // 생성될 바닥과의 거리
    [SerializeField] private float groundHeight = 0f;           // 바닥 y 고정값
    [SerializeField] public Transform center;                     // 초기 바닥 생성 위치
    private Vector3 lastDirection = Vector3.forward; // 아무 값으로 초기화

    private Transform _car;
    public Transform Car
    {
        get => _car;
        set
        {
            _car = value;
            Init(); // 외부에서 설정 시 초기화
        }
    }

    private List<GameObject> currentFloors = new List<GameObject>();
    private Vector3 lastFloorCenter;

    public void Init()
    {
        ResetFloorToOrigin();
    }

    void Update()
    {
        if (_car == null) return;

        Vector3 carPos = _car.position;
        Vector3 offset = carPos - lastFloorCenter;

        if (offset.magnitude >= thresholdDistance)
        {
            Vector3 direction = offset.normalized;

            Vector3 spawnPosition = lastFloorCenter + direction * spawnOffset;
            spawnPosition.y = groundHeight;

            GameObject newFloor = FloorPoolManager.Instance.GetFloor(spawnPosition);
            currentFloors.Add(newFloor);

            if (currentFloors.Count > 2)
            {
                FloorPoolManager.Instance.ReturnFloor(currentFloors[0]);
                currentFloors.RemoveAt(0);
            }

            lastFloorCenter = spawnPosition;
            lastDirection = direction;
        }
    }

    public void ResetFloorToOrigin()
    {
        foreach (var fl in currentFloors)
        {
            FloorPoolManager.Instance.ReturnFloor(fl);
        }

        currentFloors.Clear();

        lastFloorCenter = center.position;
        Vector3 spawnPosition = new Vector3(center.position.x, groundHeight, center.position.z);
        GameObject floor = FloorPoolManager.Instance.GetFloor(spawnPosition);
        currentFloors.Add(floor);
    }
}
