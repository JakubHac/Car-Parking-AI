using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SceneReset : MonoBehaviour
{
    [SerializeField] private PrometeoCarController _carController;
    [SerializeField] private Rigidbody _carRigidbody;
    [SerializeField] private WheelCollider[] _wheels;
    [SerializeField] private Transform _designatedParkingSpot;
    [SerializeField] private ParkedCarsSpawner _carsSpawner;
    [SerializeField] private Vector2 _minMaxCarX = new Vector2(-5, 5);
    [SerializeField] private Vector2 _minMaxCarZ = new Vector2(1, 14);

    private void Start()
    {
        ResetScene();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }
    }

    public void ResetScene()
    {
        ResetMainCar();
        _carsSpawner.ClearSpawnedCars();
        _carsSpawner.SpawnCars();

        var possiblePoints = _carsSpawner.GetFreeSpawnPoints.ToArray();
        var randomPoint = possiblePoints[Random.Range(0, possiblePoints.Length)];
        _designatedParkingSpot.position = randomPoint.Position;
        _designatedParkingSpot.rotation = randomPoint.Rotation;
    }

    private void ResetMainCar()
    {
        _carRigidbody.linearVelocity = Vector3.zero;
        _carRigidbody.angularVelocity = Vector3.zero;
        var initialCarPosition = new Vector3(Random.Range(_minMaxCarX.x, _minMaxCarX.y), 0f, Random.Range(_minMaxCarZ.x, _minMaxCarZ.y));
        _carController.transform.position = initialCarPosition;
        _carController.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
        foreach (var wheel in _wheels)
        {
            wheel.motorTorque = 0;
            wheel.brakeTorque = 0;
            wheel.rotationSpeed = 0;
        }
    }
}
