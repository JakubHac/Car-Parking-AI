using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ParkedCarsSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] Prefabs;
    [SerializeField] private int FreeSpots = 1;

    public IEnumerable<SpawnPoint> GetFreeSpawnPoints => _spawnPoints[FreeSpots..];
    private SpawnPoint[] _spawnPoints;

    public record SpawnPoint
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public SpawnPoint(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    private void Start()
    {
        _spawnPoints = new SpawnPoint[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            _spawnPoints[i] = new SpawnPoint(position: child.position, rotation: child.rotation);
            Destroy(child.gameObject);
        }

        SpawnCars();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearSpawnedCars();
            SpawnCars();
        }
    }

    public void ClearSpawnedCars()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void SpawnCars()
    {
        var toSpawn = _spawnPoints.Length - FreeSpots;
        _spawnPoints = _spawnPoints.Select(x => (point: x, random: Random.value)).OrderBy(x => x.random).Select(x => x.point).ToArray();
        for (int i = 0; i < toSpawn; i++)
        {
            var prefab = Prefabs[Random.Range(0, Prefabs.Length)];
            var spawnPoint = _spawnPoints[i + FreeSpots];
            Instantiate(prefab, spawnPoint.Position, spawnPoint.Rotation, transform);
        }
    }
}
